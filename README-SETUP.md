# Summer Courses Portal — إعداد Visual Studio + SQL Server

## المتطلبات
- Visual Studio 2022 أو أحدث
- .NET 10 SDK
- SQL Server (أي إصدار من 2017 فصاعداً)

## خطوات الإعداد

### 1. فتح المشروع
افتح ملف `WebApplication1.csproj` في Visual Studio.

### 2. ضبط الاتصال بـ SQL Server
افتح `appsettings.json` وعدّل سطر الاتصال:

```json
"SqlServerConnection": "Server=اسم_السيرفر;Database=SummerCoursesPortal;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

مثال إذا كان اسم السيرفر `.\SQLEXPRESS`:
```json
"SqlServerConnection": "Server=.\\SQLEXPRESS;Database=SummerCoursesPortal;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
```

### 3. تشغيل التطبيق
اضغط **F5** أو **Ctrl+F5** في Visual Studio.  
سيقوم التطبيق تلقائياً بإنشاء قاعدة البيانات وجميع الجداول عند أول تشغيل.

## بيانات الدخول الافتراضية للإدارة
- **المستخدم**: `admin`  
- **كلمة المرور**: `Admin@123`

## هيكل المشروع
```
Areas/
  Admin/   ← لوحة الإدارة (طلاب، منشورات، إعدادات، dashboard)
  Student/ ← واجهة الطلاب (تسجيل، بحث، أخبار)
Models/        ← الكيانات (Student, Level, Division, Post, RegistrationConfig...)
Repositories/  ← Repository Pattern
Services/      ← Business Logic
ViewModels/    ← ViewModels للـ Views
```

## ملاحظات
- كلمة مرور الأدمن مخزّنة بـ BCrypt — تُحدَّث تلقائياً عند أول دخول.
- مجلد `wwwroot/uploads/` يُستخدم لرفع مرفقات المنشورات.
