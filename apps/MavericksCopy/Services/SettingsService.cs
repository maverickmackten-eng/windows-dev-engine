using System.IO;
using System.Text.Json;
using MavericksCopy.Models;
using Serilog;

namespace MavericksCopy.Services
{
    /// <summary>
    /// Loads and saves AppSettings to settings.json next to the executable.
    /// Gracefully returns defaults on missing or corrupt file.
    /// Atomic save via .tmp + File.Move to prevent corruption on crash.
    /// </summary>
    public class SettingsService
    {
        private static readonly string _path =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        private static readonly JsonSerializerOptions _opts = new()
        {
            WriteIndented         = true,
            PropertyNamingPolicy  = JsonNamingPolicy.CamelCase,
        };

        public AppSettings Current { get; private set; } = new();

        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(_path))
                {
                    Log.Information("[Settings] No settings.json found — using defaults");
                    await SaveAsync();
                    return;
                }
                var json = await File.ReadAllTextAsync(_path);
                Current = JsonSerializer.Deserialize<AppSettings>(json, _opts)
                          ?? new AppSettings();
                Log.Debug("[Settings] Loaded from {Path}", _path);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[Settings] Load failed — using defaults");
                Current = new AppSettings();
            }
        }

        public async Task SaveAsync()
        {
            try
            {
                var json = JsonSerializer.Serialize(Current, _opts);
                var tmp  = _path + ".tmp";
                await File.WriteAllTextAsync(tmp, json, System.Text.Encoding.UTF8);
                File.Move(tmp, _path, overwrite: true);
                Log.Debug("[Settings] Saved");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[Settings] Save failed");
            }
        }
    }
}
