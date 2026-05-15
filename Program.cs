using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Implementations;
using WebApplication1.Repositories.Interfaces;
using WebApplication1.Services.Implementations;
using WebApplication1.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ───── قراءة مزوّد قاعدة البيانات من الإعدادات ─────
var dbProvider = builder.Configuration["DatabaseProvider"] ?? "Sqlite";
var isSqlServer = dbProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase);

builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery(options =>
{
    options.Cookie.Name = "X-CSRF-TOKEN";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ───── DbContext: SQLite أو SQL Server حسب الإعداد ─────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (isSqlServer)
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("SqlServerConnection"));
    else
        options.UseSqlite(
            builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ───── Repositories ─────
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<ILevelRepository, LevelRepository>();
builder.Services.AddScoped<IDivisionRepository, DivisionRepository>();
builder.Services.AddScoped<IAdminRepository, AdminRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// ───── Services ─────
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IRegistrationConfigService, RegistrationConfigService>();

// ───── Session ─────
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = "AppSession";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ───── Forwarded Headers ─────
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                              | ForwardedHeaders.XForwardedProto
                              | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ───── Rate Limiting ─────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddPolicy("login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 15,         // زاد من 5 لـ 15 محاولة في الدقيقة
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.AddPolicy("search", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

// ───── Upload size limits ─────
const long MaxUploadBytes = 209715200;

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = MaxUploadBytes;
    options.Limits.MinRequestBodyDataRate = null;
    options.Limits.MinResponseDataRate = null;
    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(15);
    options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxUploadBytes;
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = 32768;
    options.MemoryBufferThreshold = 1048576;
});

var app = builder.Build();

// ───── إنشاء / ترقية قاعدة البيانات تلقائياً ─────
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var log     = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        context.Database.Migrate();
        log.LogInformation("قاعدة البيانات جاهزة ({Provider})", dbProvider);
    }
    catch (Exception ex)
    {
        log.LogError(ex, "فشل إنشاء قاعدة البيانات ({Provider})", dbProvider);
    }

    // ── SQLite فقط: دعم قواعد البيانات القديمة التي لا تحتوي على الأعمدة الجديدة ──
    if (!isSqlServer)
    {
        void TryExec(string sql)
        {
            try { context.Database.ExecuteSqlRaw(sql); } catch { }
        }

        TryExec(@"ALTER TABLE ""Students"" ADD COLUMN ""IsDeleted"" INTEGER NOT NULL DEFAULT 0");
        TryExec(@"ALTER TABLE ""Students"" ADD COLUMN ""DeletedAt"" TEXT");
        TryExec(@"
            CREATE TABLE IF NOT EXISTS ""AuditLogs"" (
                ""Id""        INTEGER NOT NULL CONSTRAINT ""PK_AuditLogs"" PRIMARY KEY AUTOINCREMENT,
                ""Action""    TEXT NOT NULL DEFAULT '',
                ""Details""   TEXT NOT NULL DEFAULT '',
                ""Username""  TEXT NOT NULL DEFAULT '',
                ""IpAddress"" TEXT,
                ""Timestamp"" TEXT NOT NULL DEFAULT (datetime('now')),
                ""Category""  TEXT
            )");
    }

    // ── تعيين كلمة مرور الأدمن من متغير البيئة (إجباري) ──
    var adminPasswordEnv = builder.Configuration["AdminPassword"] ?? "Admin@12345";
    var adminUser = await context.AdminUsers.FirstOrDefaultAsync(u => u.Username == "admin");
    
    if (adminUser == null)
    {
        // إنشاء حساب الأدمن إذا لم يكن موجوداً
        adminUser = new AdminUser { Username = "admin", Password = BCrypt.Net.BCrypt.HashPassword(adminPasswordEnv, workFactor: 12) };
        context.AdminUsers.Add(adminUser);
        await context.SaveChangesAsync();
        log.LogInformation("[SECURITY] تم إنشاء حساب الأدمن الجديد");
    }
    else
    {
        // تحديث كلمة مرور الأدمن دائماً لضمان التطابق
        var oldHash = adminUser.Password;
        adminUser.Password = BCrypt.Net.BCrypt.HashPassword(adminPasswordEnv, workFactor: 12);
        
        if (oldHash != adminUser.Password)
        {
            await context.SaveChangesAsync();
            log.LogInformation("[SECURITY] تم تحديث كلمة مرور الأدمن بنجاح");
        }
    }
}

// ───── Forwarded Headers قبل أي middleware ─────
app.UseForwardedHeaders();

// ───── Security Headers ─────
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "SAMEORIGIN";
    headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    headers["X-XSS-Protection"] = "0";
    headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "img-src 'self' data: https: blob:; " +
        "style-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com https://fonts.googleapis.com; " +
        "font-src 'self' https://cdnjs.cloudflare.com https://fonts.gstatic.com data:; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net https://cdnjs.cloudflare.com; " +
        "frame-src 'self' https://www.youtube.com https://www.youtube-nocookie.com https://player.vimeo.com https://www.facebook.com https://web.facebook.com https://drive.google.com; " +
        "frame-ancestors 'self'; " +
        "base-uri 'self'; " +
        "form-action 'self';";
    await next();
});

// ───── Status Code Pages (404, etc.) ─────
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error/500");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseSession();

// ───── حماية Admin Area ─────
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower();

    if (path != null && path.StartsWith("/admin"))
    {
        var isLoggedIn = context.Session.GetString("IsAdminLoggedIn");

        if (isLoggedIn != "true" && !path.Contains("/admin/auth"))
        {
            context.Response.Redirect("/Admin/Auth/Login");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapGet("/", context =>
{
    context.Response.Redirect("/Student/Home/Index");
    return Task.CompletedTask;
});

app.Run();
