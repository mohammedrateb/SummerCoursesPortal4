using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using WebApplication1.Data;
using WebApplication1.Models;
using WebApplication1.Repositories.Interfaces;

namespace WebApplication1.Repositories.Implementations
{
    public class AdminRepository : IAdminRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminRepository> _logger;

        // ── Account Lockout (in-memory, resets on app restart) ──
        private static readonly ConcurrentDictionary<string, (int Attempts, DateTime LockoutUntil)> _lockouts = new();
        private const int MaxFailedAttempts = 10;            // زاد من 5 لـ 10 محاولات
        private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(2);  // قلل من 15 لـ دقيقتين

        public AdminRepository(ApplicationDbContext context, ILogger<AdminRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<AdminUser?> GetByUsernameAsync(string username) =>
            await _context.AdminUsers.FirstOrDefaultAsync(a => a.Username == username);

        public async Task<bool> ValidateCredentialsAsync(string username, string password)
        {
            var key = username.ToLowerInvariant();

            // ── Check lockout ──
            if (_lockouts.TryGetValue(key, out var lockoutInfo))
            {
                if (DateTime.Now < lockoutInfo.LockoutUntil)
                {
                    var remaining = (lockoutInfo.LockoutUntil - DateTime.Now).TotalSeconds;
                    _logger.LogWarning("[AUDIT] Login BLOCKED (lockout): {Username} — {Remaining} sec remaining", username, Math.Ceiling(remaining));
                    return false;
                }
                else
                {
                    _lockouts.TryRemove(key, out _);
                }
            }

            var admin = await GetByUsernameAsync(username);
            if (admin == null)
            {
                RecordFailedAttempt(key, username);
                return false;
            }

            bool isValid;

            if (!string.IsNullOrEmpty(admin.Password) && admin.Password.StartsWith("$2"))
            {
                try { isValid = global::BCrypt.Net.BCrypt.Verify(password, admin.Password); }
                catch { isValid = false; }
            }
            else
            {
                var hashedInput = HashSHA256(password);
                isValid = admin.Password == hashedInput;
                if (isValid)
                {
                    admin.Password = global::BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("[AUDIT] Password auto-upgraded to BCrypt for: {Username}", username);
                }
            }

            if (isValid)
            {
                _lockouts.TryRemove(key, out _);
                _logger.LogInformation("[AUDIT] Login SUCCESS: {Username}", username);
            }
            else
            {
                RecordFailedAttempt(key, username);
            }

            return isValid;
        }

        public async Task<bool> ChangePasswordAsync(string username, string currentPassword, string newPassword)
        {
            var admin = await GetByUsernameAsync(username);
            if (admin == null) return false;

            var ok = await ValidateCredentialsAsync(username, currentPassword);
            if (!ok) return false;

            admin.Password = global::BCrypt.Net.BCrypt.HashPassword(newPassword, workFactor: 12);
            await _context.SaveChangesAsync();
            _logger.LogInformation("[AUDIT] Password CHANGED for: {Username}", username);
            return true;
        }

        // ── Helpers ──
        private void RecordFailedAttempt(string key, string username)
        {
            _lockouts.AddOrUpdate(key,
                _ => (1, DateTime.Now.Add(LockoutDuration)),
                (_, existing) =>
                {
                    var newAttempts = existing.Attempts + 1;
                    var lockUntil = newAttempts >= MaxFailedAttempts
                        ? DateTime.Now.Add(LockoutDuration)
                        : existing.LockoutUntil;
                    return (newAttempts, lockUntil);
                });

            if (_lockouts.TryGetValue(key, out var info) && info.Attempts >= MaxFailedAttempts)
                _logger.LogWarning("[AUDIT] Login LOCKED: {Username} — {Max} failed attempts", username, MaxFailedAttempts);
            else
                _logger.LogWarning("[AUDIT] Login FAILED: {Username} — attempt {N}", username,
                    _lockouts.TryGetValue(key, out var i2) ? i2.Attempts : 1);
        }

        private static string HashSHA256(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLower();
        }
    }
}
