using System;
using System.Windows;
using GlitchOverlay.Core;
using GlitchOverlay.Services;

namespace GlitchOverlay
{
    /// <summary>
    /// Main application class for the Glitch Overlay
    /// </summary>
    public partial class App : Application
    {
        private OverlayManager _overlayManager;
        private SystemTrayManager _systemTrayManager;
        private ConfigurationService _configService;
        private GlobalHotkeyService _hotkeyService;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // Initialize core services
                System.Diagnostics.Debug.WriteLine("Creating ConfigurationService...");
                _configService = new ConfigurationService();

                System.Diagnostics.Debug.WriteLine("Creating OverlayManager...");
                _overlayManager = new OverlayManager(_configService);

                System.Diagnostics.Debug.WriteLine("Creating SystemTrayManager...");
                _systemTrayManager = new SystemTrayManager(_overlayManager, _configService);

                // Create and initialize the main window
                System.Diagnostics.Debug.WriteLine("Creating MainWindow...");
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;

                System.Diagnostics.Debug.WriteLine("Initializing MainWindow...");
                mainWindow.Initialize(_configService);

                // Hide the main window initially - we'll use system tray
                System.Diagnostics.Debug.WriteLine("Hiding MainWindow...");
                mainWindow.WindowState = WindowState.Minimized;
                mainWindow.ShowInTaskbar = false;
                mainWindow.Visibility = Visibility.Hidden;

                // Start the overlay system
                System.Diagnostics.Debug.WriteLine("Initializing OverlayManager...");
                _overlayManager.Initialize();

                System.Diagnostics.Debug.WriteLine("Initializing SystemTrayManager...");
                _systemTrayManager.Initialize();

                // Initialize global hotkeys
                System.Diagnostics.Debug.WriteLine("Initializing GlobalHotkeyService...");
                _hotkeyService = new GlobalHotkeyService(mainWindow);
                _hotkeyService.ToggleEffectsRequested += OnToggleEffectsHotkey;
                _hotkeyService.ShowPanelRequested += OnShowPanelHotkey;
                _hotkeyService.Initialize();

                // Handle application shutdown
                this.SessionEnding += OnSessionEnding;
                this.Exit += OnApplicationExit;

                System.Diagnostics.Debug.WriteLine("Initialization complete!");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to initialize Glitch Overlay: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                MessageBox.Show(errorMessage, "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown(1);
            }
        }

        private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            CleanupResources();
        }

        private void OnApplicationExit(object sender, ExitEventArgs e)
        {
            CleanupResources();
        }

        private void OnToggleEffectsHotkey(object sender, EventArgs e)
        {
            try
            {
                _overlayManager?.ToggleOverlay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling effects via hotkey: {ex.Message}");
            }
        }

        private void OnShowPanelHotkey(object sender, EventArgs e)
        {
            try
            {
                var mainWindow = MainWindow as MainWindow;
                mainWindow?.ShowFromTray();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing panel via hotkey: {ex.Message}");
            }
        }

        private void CleanupResources()
        {
            try
            {
                _hotkeyService?.Dispose();
                _overlayManager?.Dispose();
                _systemTrayManager?.Dispose();
                _configService?.SaveConfiguration();
            }
            catch (Exception ex)
            {
                // Log error but don't prevent shutdown
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }
    }
}
