using System;
using System.Windows;
using System.Windows.Threading;
using GlitchOverlay.Services;
using GlitchOverlay.Models;

namespace GlitchOverlay.Core
{
    /// <summary>
    /// Core manager for the desktop overlay system
    /// </summary>
    public class OverlayManager : IDisposable
    {
        private readonly ConfigurationService _configService;
        private OverlayWindow _overlayWindow;
        private DispatcherTimer _effectTimer;
        private bool _isInitialized;
        private bool _disposed;

        public bool IsOverlayActive { get; private set; }
        
        public event EventHandler<bool> OverlayStateChanged;

        public OverlayManager(ConfigurationService configService)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _configService.ConfigurationChanged += OnConfigurationChanged;
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                CreateOverlayWindow();
                InitializeEffectTimer();
                
                // Apply initial configuration
                ApplyConfiguration(_configService.CurrentConfiguration);
                
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize overlay manager: {ex.Message}", ex);
            }
        }

        private void CreateOverlayWindow()
        {
            _overlayWindow = new OverlayWindow();
            _overlayWindow.Initialize();
        }

        private void InitializeEffectTimer()
        {
            _effectTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            _effectTimer.Tick += OnEffectTimerTick;
        }

        private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            if (!e.SuppressEffectExecution)
            {
                ApplyConfiguration(e.Configuration);
            }
        }

        private void ApplyConfiguration(GlitchConfiguration config)
        {
            if (!_isInitialized || config == null) return;

            if (config.IsEnabled && !IsOverlayActive)
            {
                StartOverlay();
            }
            else if (!config.IsEnabled && IsOverlayActive)
            {
                StopOverlay();
            }

            // Update overlay window with new configuration
            _overlayWindow?.UpdateConfiguration(config);
        }

        public void StartOverlay()
        {
            if (IsOverlayActive || !_isInitialized) return;

            try
            {
                _overlayWindow.Show();
                _effectTimer.Start();
                IsOverlayActive = true;
                
                OverlayStateChanged?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting overlay: {ex.Message}");
            }
        }

        public void StopOverlay()
        {
            if (!IsOverlayActive) return;

            try
            {
                _effectTimer.Stop();
                _overlayWindow.Hide();
                IsOverlayActive = false;
                
                OverlayStateChanged?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping overlay: {ex.Message}");
            }
        }

        public void ToggleOverlay()
        {
            if (IsOverlayActive)
                StopOverlay();
            else
                StartOverlay();
        }

        private void OnEffectTimerTick(object sender, EventArgs e)
        {
            try
            {
                // Update effects on the overlay window
                _overlayWindow?.UpdateEffects();

                // Notify performance monitor of frame update
                NotifyFrameUpdate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating effects: {ex.Message}");
            }
        }

        private void NotifyFrameUpdate()
        {
            try
            {
                // Find the main window and update its performance monitor
                var mainWindow = System.Windows.Application.Current?.MainWindow as MainWindow;
                mainWindow?.RecordFrame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error notifying frame update: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                StopOverlay();
                
                _effectTimer?.Stop();
                _effectTimer = null;
                
                _overlayWindow?.Close();
                _overlayWindow = null;
                
                if (_configService != null)
                    _configService.ConfigurationChanged -= OnConfigurationChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing overlay manager: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
