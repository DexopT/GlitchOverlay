using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using GlitchOverlay.Models;

namespace GlitchOverlay.Services
{
    /// <summary>
    /// Service for managing application configuration and presets
    /// </summary>
    public class ConfigurationService
    {
        private readonly FileConfigurationService _fileService;
        private List<EffectPreset> _presets;

        public GlitchConfiguration CurrentConfiguration { get; private set; }
        public List<EffectPreset> Presets => _presets;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        public ConfigurationService()
        {
            try
            {
                _fileService = new FileConfigurationService();
                _fileService.LogInfo("ConfigurationService: Initializing with file-based storage...");

                CurrentConfiguration = _fileService.LoadSettings();
                _presets = _fileService.LoadPresets();

                _fileService.LogInfo("ConfigurationService: Initialization complete!");
                _fileService.LogInfo($"Loaded configuration with {_presets.Count} presets");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ConfigurationService constructor error: {ex.Message}");
                // Create a fallback file service for error logging
                try
                {
                    _fileService = new FileConfigurationService();
                    _fileService.LogError($"ConfigurationService initialization failed: {ex.Message}");
                }
                catch
                {
                    // Silent fallback
                }
                throw;
            }
        }

        // Configuration loading is now handled by FileConfigurationService

        public void SaveConfiguration()
        {
            try
            {
                var success = _fileService.SaveSettings(CurrentConfiguration);
                if (!success)
                {
                    _fileService.LogError("Failed to save configuration to file");
                }
            }
            catch (Exception ex)
            {
                _fileService.LogError($"Error saving configuration: {ex.Message}");
            }
        }

        public void UpdateConfiguration(GlitchConfiguration newConfig, bool suppressEffectExecution = false)
        {
            CurrentConfiguration = newConfig.Clone();
            SaveConfiguration();
            _fileService.LogDebug($"Configuration updated, suppressEffectExecution: {suppressEffectExecution}");
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(CurrentConfiguration, suppressEffectExecution));
        }

        public void UpdateConfigurationSilently(GlitchConfiguration newConfig)
        {
            CurrentConfiguration = newConfig.Clone();
            SaveConfiguration();
            _fileService.LogDebug("Configuration updated silently (UI-only update)");
            // Don't trigger ConfigurationChanged event - this is for UI-only updates
        }

        public void SavePreset(string name, GlitchConfiguration config)
        {
            try
            {
                var existingPreset = _presets.FirstOrDefault(p => p.Name == name);
                if (existingPreset != null)
                {
                    existingPreset.Configuration = config.Clone();
                    existingPreset.ModifiedDate = DateTime.Now;
                    _fileService.LogInfo($"Updated existing preset: {name}");
                }
                else
                {
                    var newPreset = new EffectPreset
                    {
                        Name = name,
                        Description = $"Custom preset created on {DateTime.Now:yyyy-MM-dd}",
                        Configuration = config.Clone()
                    };
                    _presets.Add(newPreset);
                    _fileService.LogInfo($"Created new preset: {name}");
                }

                _fileService.SavePresets(_presets);
            }
            catch (Exception ex)
            {
                _fileService.LogError($"Failed to save preset '{name}': {ex.Message}");
                throw new InvalidOperationException($"Failed to save preset '{name}': {ex.Message}");
            }
        }

        public GlitchConfiguration LoadPreset(string name)
        {
            try
            {
                var preset = _presets.FirstOrDefault(p => p.Name == name);
                if (preset == null)
                {
                    _fileService.LogWarning($"Preset '{name}' not found");
                    throw new FileNotFoundException($"Preset '{name}' not found");
                }

                _fileService.LogInfo($"Loaded preset: {name}");
                return preset.Configuration.Clone();
            }
            catch (Exception ex)
            {
                _fileService.LogError($"Failed to load preset '{name}': {ex.Message}");
                throw new InvalidOperationException($"Failed to load preset '{name}': {ex.Message}");
            }
        }

        public List<string> GetAvailablePresets()
        {
            try
            {
                return _presets.Select(p => p.Name).ToList();
            }
            catch (Exception ex)
            {
                _fileService.LogError($"Error getting available presets: {ex.Message}");
                return new List<string>();
            }
        }

        public void DeletePreset(string name)
        {
            try
            {
                var preset = _presets.FirstOrDefault(p => p.Name == name);
                if (preset != null)
                {
                    _presets.Remove(preset);
                    _fileService.SavePresets(_presets);
                    _fileService.LogInfo($"Deleted preset: {name}");
                }
                else
                {
                    _fileService.LogWarning($"Attempted to delete non-existent preset: {name}");
                }
            }
            catch (Exception ex)
            {
                _fileService.LogError($"Failed to delete preset '{name}': {ex.Message}");
                throw new InvalidOperationException($"Failed to delete preset '{name}': {ex.Message}");
            }
        }

        // Logging methods
        public void LogInfo(string message) => _fileService.LogInfo(message);
        public void LogWarning(string message) => _fileService.LogWarning(message);
        public void LogError(string message) => _fileService.LogError(message);
        public void LogDebug(string message) => _fileService.LogDebug(message);

        // File path access methods
        public string GetLogFilePath() => _fileService.GetLogFilePath();
        public string GetSettingsFilePath() => _fileService.GetSettingsFilePath();
        public string GetPresetsFilePath() => _fileService.GetPresetsFilePath();
        public string GetConfigurationDirectory() => _fileService.GetConfigurationDirectory();

        // Default configuration is now handled by FileConfigurationService

        // Built-in presets are now handled by FileConfigurationService
    }

    /// <summary>
    /// Event arguments for configuration changes
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public GlitchConfiguration Configuration { get; }
        public bool SuppressEffectExecution { get; }

        public ConfigurationChangedEventArgs(GlitchConfiguration configuration, bool suppressEffectExecution = false)
        {
            Configuration = configuration;
            SuppressEffectExecution = suppressEffectExecution;
        }
    }
}
