using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using GlitchOverlay.Models;

namespace GlitchOverlay.Services
{
    public class FileConfigurationService
    {
        private readonly string _configDirectory;
        private readonly string _settingsFile;
        private readonly string _presetsFile;
        private readonly string _logFile;
        private readonly JsonSerializerOptions _jsonOptions;

        public FileConfigurationService()
        {
            // Create configuration directory in user's AppData
            _configDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GlitchOverlay"
            );

            _settingsFile = Path.Combine(_configDirectory, "settings.json");
            _presetsFile = Path.Combine(_configDirectory, "presets.json");
            _logFile = Path.Combine(_configDirectory, "debug.log");

            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

            EnsureConfigurationDirectory();
            InitializeLogFile();
        }

        #region Directory and File Management

        private void EnsureConfigurationDirectory()
        {
            try
            {
                if (!Directory.Exists(_configDirectory))
                {
                    Directory.CreateDirectory(_configDirectory);
                    LogInfo($"Created configuration directory: {_configDirectory}");
                }
            }
            catch (Exception ex)
            {
                LogError($"Failed to create configuration directory: {ex.Message}");
            }
        }

        private void InitializeLogFile()
        {
            try
            {
                // Create log file if it doesn't exist
                if (!File.Exists(_logFile))
                {
                    File.WriteAllText(_logFile, $"=== Glitch Overlay Debug Log ===\nInitialized: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n");
                }

                // Rotate log file if it's too large (>5MB)
                var fileInfo = new FileInfo(_logFile);
                if (fileInfo.Exists && fileInfo.Length > 5 * 1024 * 1024)
                {
                    var backupFile = Path.Combine(_configDirectory, $"debug_backup_{DateTime.Now:yyyyMMdd_HHmmss}.log");
                    File.Move(_logFile, backupFile);
                    File.WriteAllText(_logFile, $"=== Glitch Overlay Debug Log ===\nRotated from backup: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to initialize log file: {ex.Message}");
            }
        }

        #endregion

        #region Settings Management

        public GlitchConfiguration LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsFile))
                {
                    LogInfo("Settings file not found, creating default configuration");
                    var defaultConfig = CreateDefaultConfiguration();
                    SaveSettings(defaultConfig);
                    return defaultConfig;
                }

                var jsonContent = File.ReadAllText(_settingsFile);
                var configuration = JsonSerializer.Deserialize<GlitchConfiguration>(jsonContent, _jsonOptions);
                
                if (configuration == null)
                {
                    LogWarning("Failed to deserialize settings, using default configuration");
                    return CreateDefaultConfiguration();
                }

                LogInfo("Settings loaded successfully");
                return configuration;
            }
            catch (Exception ex)
            {
                LogError($"Failed to load settings: {ex.Message}");
                return CreateDefaultConfiguration();
            }
        }

        public bool SaveSettings(GlitchConfiguration configuration)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(configuration, _jsonOptions);
                File.WriteAllText(_settingsFile, jsonContent);
                LogInfo("Settings saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save settings: {ex.Message}");
                return false;
            }
        }

        private GlitchConfiguration CreateDefaultConfiguration()
        {
            return new GlitchConfiguration
            {
                IsEnabled = false,
                GlobalIntensity = 0.5,
                GlobalFrequency = 0.3,
                PixelShift = new PixelShiftSettings
                {
                    IsEnabled = true,
                    Intensity = 0.4,
                    Frequency = 0.2,
                    MaxShiftDistance = 5,
                    Direction = ShiftDirection.Both
                },
                ColorInversion = new ColorInversionSettings
                {
                    IsEnabled = true,
                    Intensity = 0.3,
                    Frequency = 0.1,
                    Duration = 0.2,
                    Type = InversionType.Full
                },
                Scanlines = new ScanlineSettings
                {
                    IsEnabled = true,
                    Intensity = 0.2,
                    Frequency = 0.8,
                    LineSpacing = 4,
                    FlickerRate = 0.1,
                    Pattern = ScanlinePattern.Horizontal
                },
                DigitalBreakdown = new DigitalBreakdownSettings
                {
                    IsEnabled = true,
                    Intensity = 0.6,
                    Frequency = 0.05,
                    Duration = 0.5,
                    BlockSize = 8,
                    Type = BreakdownType.Mixed
                },
                PerformanceOverlay = new PerformanceOverlaySettings
                {
                    IsEnabled = false,
                    FontSize = 9,
                    Position = OverlayPosition.TopLeft,
                    Opacity = 0.8,
                    ColorScheme = OverlayColorScheme.RedOrange,
                    ShowFPS = true,
                    ShowCPU = true,
                    ShowMemory = true,
                    ShowEffects = true
                }
            };
        }

        #endregion

        #region Presets Management

        public List<EffectPreset> LoadPresets()
        {
            try
            {
                if (!File.Exists(_presetsFile))
                {
                    LogInfo("Presets file not found, creating default presets");
                    var defaultPresets = CreateDefaultPresets();
                    SavePresets(defaultPresets);
                    return defaultPresets;
                }

                var jsonContent = File.ReadAllText(_presetsFile);
                var presets = JsonSerializer.Deserialize<List<EffectPreset>>(jsonContent, _jsonOptions);
                
                if (presets == null)
                {
                    LogWarning("Failed to deserialize presets, using default presets");
                    return CreateDefaultPresets();
                }

                LogInfo($"Loaded {presets.Count} presets successfully");
                return presets;
            }
            catch (Exception ex)
            {
                LogError($"Failed to load presets: {ex.Message}");
                return CreateDefaultPresets();
            }
        }

        public bool SavePresets(List<EffectPreset> presets)
        {
            try
            {
                var jsonContent = JsonSerializer.Serialize(presets, _jsonOptions);
                File.WriteAllText(_presetsFile, jsonContent);
                LogInfo($"Saved {presets.Count} presets successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogError($"Failed to save presets: {ex.Message}");
                return false;
            }
        }

        private List<EffectPreset> CreateDefaultPresets()
        {
            return new List<EffectPreset>
            {
                new EffectPreset
                {
                    Name = "Default Settings",
                    Description = "Balanced configuration for general use",
                    Configuration = CreateDefaultConfiguration()
                },
                new EffectPreset
                {
                    Name = "Subtle Glitch",
                    Description = "Light effects for productivity",
                    Configuration = CreateSubtlePreset()
                },
                new EffectPreset
                {
                    Name = "Heavy Corruption",
                    Description = "Intense effects for creative work",
                    Configuration = CreateHeavyPreset()
                },
                new EffectPreset
                {
                    Name = "Retro CRT",
                    Description = "Classic CRT monitor simulation",
                    Configuration = CreateRetroPreset()
                },
                new EffectPreset
                {
                    Name = "Digital Noise",
                    Description = "Modern digital corruption effects",
                    Configuration = CreateDigitalNoisePreset()
                }
            };
        }

        private GlitchConfiguration CreateSubtlePreset()
        {
            var config = CreateDefaultConfiguration();
            config.GlobalIntensity = 0.2;
            config.GlobalFrequency = 0.1;
            config.PixelShift.Intensity = 0.1;
            config.PixelShift.Frequency = 0.05;
            config.ColorInversion.Intensity = 0.1;
            config.ColorInversion.Frequency = 0.02;
            config.Scanlines.Intensity = 0.1;
            config.DigitalBreakdown.Intensity = 0.2;
            config.DigitalBreakdown.Frequency = 0.01;
            return config;
        }

        private GlitchConfiguration CreateHeavyPreset()
        {
            var config = CreateDefaultConfiguration();
            config.GlobalIntensity = 0.8;
            config.GlobalFrequency = 0.6;
            config.PixelShift.Intensity = 0.7;
            config.PixelShift.Frequency = 0.4;
            config.ColorInversion.Intensity = 0.6;
            config.ColorInversion.Frequency = 0.3;
            config.Scanlines.Intensity = 0.5;
            config.DigitalBreakdown.Intensity = 0.8;
            config.DigitalBreakdown.Frequency = 0.2;
            return config;
        }

        private GlitchConfiguration CreateRetroPreset()
        {
            var config = CreateDefaultConfiguration();
            config.GlobalIntensity = 0.4;
            config.GlobalFrequency = 0.3;
            config.PixelShift.IsEnabled = false;
            config.ColorInversion.IsEnabled = false;
            config.Scanlines.Intensity = 0.6;
            config.Scanlines.Frequency = 1.0;
            config.Scanlines.LineSpacing = 2;
            config.Scanlines.FlickerRate = 0.3;
            config.DigitalBreakdown.IsEnabled = false;
            return config;
        }

        private GlitchConfiguration CreateDigitalNoisePreset()
        {
            var config = CreateDefaultConfiguration();
            config.GlobalIntensity = 0.6;
            config.GlobalFrequency = 0.4;
            config.PixelShift.Intensity = 0.5;
            config.PixelShift.MaxShiftDistance = 10;
            config.ColorInversion.IsEnabled = false;
            config.Scanlines.IsEnabled = false;
            config.DigitalBreakdown.Intensity = 0.7;
            config.DigitalBreakdown.Type = BreakdownType.Pixelation;
            config.DigitalBreakdown.BlockSize = 16;
            return config;
        }

        #endregion

        #region Logging

        public void LogInfo(string message)
        {
            WriteLog("INFO", message);
        }

        public void LogWarning(string message)
        {
            WriteLog("WARN", message);
        }

        public void LogError(string message)
        {
            WriteLog("ERROR", message);
        }

        public void LogDebug(string message)
        {
            WriteLog("DEBUG", message);
        }

        private void WriteLog(string level, string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var logEntry = $"[{timestamp}] [{level}] {message}\n";
                
                File.AppendAllText(_logFile, logEntry);
                
                // Also write to debug output
                System.Diagnostics.Debug.WriteLine($"[{level}] {message}");
            }
            catch
            {
                // Silently fail to avoid infinite loops
            }
        }

        public string GetLogFilePath() => _logFile;
        public string GetSettingsFilePath() => _settingsFile;
        public string GetPresetsFilePath() => _presetsFile;
        public string GetConfigurationDirectory() => _configDirectory;

        #endregion
    }

    public class EffectPreset
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ModifiedDate { get; set; } = DateTime.Now;
        public GlitchConfiguration Configuration { get; set; } = new GlitchConfiguration();
    }
}
