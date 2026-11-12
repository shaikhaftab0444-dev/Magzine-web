using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ApplicationDbContext _context;
        private static readonly ConcurrentDictionary<string, string> _cache = new(StringComparer.OrdinalIgnoreCase);
        private static bool _isLoaded = false;
        private static readonly object _lock = new();

        public SettingsService(ApplicationDbContext context)
        {
            _context = context;
            EnsureLoaded();
        }

        private void EnsureLoaded()
        {
            if (!_isLoaded)
            {
                lock (_lock)
                {
                    if (!_isLoaded)
                    {
                        try
                        {
                            var settings = _context.PlatformSettings.AsNoTracking().ToList();
                            _cache.Clear();
                            foreach (var s in settings)
                            {
                                _cache[s.SettingKey] = s.SettingValue;
                            }
                            _isLoaded = true;
                        }
                        catch
                        {
                            // In case of initial startup before tables are verified
                        }
                    }
                }
            }
        }

        public string GetValue(string key, string defaultValue = "")
        {
            if (_cache.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
            {
                return val;
            }

            // Fallback defaults
            return key.ToLowerInvariant() switch
            {
                "brandname" => "Culture Club",
                "tagline" => "ALL THINGS THAT MATTER.",
                "currency" => "INR",
                "timezone" => "UTC+05:30",
                "logopath" => "/images/system/logo.png",
                _ => defaultValue
            };
        }

        public async Task<string> GetValueAsync(string key, string defaultValue = "")
        {
            if (_cache.TryGetValue(key, out var val) && !string.IsNullOrEmpty(val))
            {
                return val;
            }

            var setting = await _context.PlatformSettings
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SettingKey == key);

            if (setting != null && !string.IsNullOrEmpty(setting.SettingValue))
            {
                _cache[key] = setting.SettingValue;
                return setting.SettingValue;
            }

            return GetValue(key, defaultValue);
        }

        public string GetCurrencySymbol()
        {
            var currency = GetValue("Currency", "INR").Trim();

            return currency.ToUpperInvariant() switch
            {
                "INR" or "₹" => "₹",
                "USD" or "$" => "$",
                "EUR" or "€" => "€",
                "GBP" or "£" => "£",
                "SAR" => "SAR",
                _ => currency
            };
        }

        public async Task SetValueAsync(string key, string value)
        {
            var setting = await _context.PlatformSettings.FirstOrDefaultAsync(s => s.SettingKey == key);
            if (setting != null)
            {
                setting.SettingValue = value;
            }
            else
            {
                _context.PlatformSettings.Add(new PlatformSetting
                {
                    SettingKey = key,
                    SettingValue = value
                });
            }

            await _context.SaveChangesAsync();
            _cache[key] = value;
        }

        public void RefreshCache()
        {
            lock (_lock)
            {
                try
                {
                    var settings = _context.PlatformSettings.AsNoTracking().ToList();
                    _cache.Clear();
                    foreach (var s in settings)
                    {
                        _cache[s.SettingKey] = s.SettingValue;
                    }
                    _isLoaded = true;
                }
                catch
                {
                    _isLoaded = false;
                }
            }
        }
    }
}
