using Microsoft.EntityFrameworkCore;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Services.Interfaces;

namespace WebApplication1.Services.Implementations
{
    public class RegistrationConfigService : IRegistrationConfigService
    {
        private readonly ApplicationDbContext _context;

        public RegistrationConfigService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<RegistrationConfig> GetConfigAsync()
        {
            var config = await _context.RegistrationConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new RegistrationConfig { IsOpen = true };
                _context.RegistrationConfigs.Add(config);
                await _context.SaveChangesAsync();
                return config;
            }

            // ── تطبيق الفتح/الإغلاق التلقائي بناءً على التواريخ ──
            bool changed = false;
            var now = DateTime.UtcNow;

            // إذا حان وقت الفتح التلقائي وكان التسجيل مغلقًا
            if (config.OpenAt.HasValue && now >= config.OpenAt.Value && !config.IsOpen)
            {
                config.IsOpen = true;
                config.OpenAt = null;   // مسح التاريخ بعد تطبيقه
                changed = true;
            }

            // إذا حان وقت الإغلاق التلقائي وكان التسجيل مفتوحًا
            if (config.CloseAt.HasValue && now >= config.CloseAt.Value && config.IsOpen)
            {
                config.IsOpen = false;
                config.CloseAt = null;  // مسح التاريخ بعد تطبيقه
                changed = true;
            }

            if (changed)
                await _context.SaveChangesAsync();

            return config;
        }

        public async Task UpdateConfigAsync(RegistrationConfig config)
        {
            _context.RegistrationConfigs.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsRegistrationOpenAsync()
        {
            var config = await GetConfigAsync();
            return config.IsRegistrationOpen();
        }
    }
}
