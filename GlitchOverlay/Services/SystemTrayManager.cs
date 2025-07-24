using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows;
using GlitchOverlay.Core;
using GlitchOverlay.Services;
using Application = System.Windows.Application;

namespace GlitchOverlay.Services
{
    /// <summary>
    /// Manages system tray integration and notifications
    /// </summary>
    public class SystemTrayManager : IDisposable
    {
        private readonly OverlayManager _overlayManager;
        private readonly ConfigurationService _configService;
        private NotifyIcon _notifyIcon;
        private ContextMenuStrip _contextMenu;
        private bool _disposed;

        public SystemTrayManager(OverlayManager overlayManager, ConfigurationService configService)
        {
            _overlayManager = overlayManager ?? throw new ArgumentNullException(nameof(overlayManager));
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
        }

        public void Initialize()
        {
            CreateNotifyIcon();
            CreateContextMenu();
            
            _overlayManager.OverlayStateChanged += OnOverlayStateChanged;
            
            // Show initial notification
            ShowNotification("Glitch Overlay", "Application started with your custom icon! Right-click the tray icon for options.",
                ToolTipIcon.Info, 5000);
        }

        private void CreateNotifyIcon()
        {
            _notifyIcon = new NotifyIcon
            {
                Text = "Glitch Overlay",
                Visible = true,
                Icon = LoadApplicationIcon()
            };

            _notifyIcon.DoubleClick += OnTrayIconDoubleClick;
            _notifyIcon.MouseClick += OnTrayIconClick;
        }

        private Icon LoadApplicationIcon()
        {
            try
            {
                // Try to load the custom favicon.ico
                var iconPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favicon.ico");
                if (System.IO.File.Exists(iconPath))
                {
                    return new Icon(iconPath);
                }

                // Fallback: Create a simple programmatic icon if favicon.ico is not found
                return CreateFallbackIcon();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading application icon: {ex.Message}");
                return CreateFallbackIcon();
            }
        }

        private Icon CreateFallbackIcon()
        {
            // Create a simple 16x16 icon programmatically as fallback
            var bitmap = new Bitmap(16, 16);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Create a glitchy-looking icon
                graphics.Clear(Color.Black);

                // Draw some glitch-style pixels
                var random = new Random();
                for (int i = 0; i < 20; i++)
                {
                    var x = random.Next(16);
                    var y = random.Next(16);
                    var color = random.Next(2) == 0 ? Color.Lime : Color.Red;
                    bitmap.SetPixel(x, y, color);
                }
            }

            return Icon.FromHandle(bitmap.GetHicon());
        }

        private void CreateContextMenu()
        {
            _contextMenu = new ContextMenuStrip();
            
            // Toggle overlay item
            var toggleItem = new ToolStripMenuItem("Enable Overlay")
            {
                CheckOnClick = true,
                Checked = _configService.CurrentConfiguration.IsEnabled
            };
            toggleItem.Click += OnToggleOverlay;
            _contextMenu.Items.Add(toggleItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            // Show control panel
            var controlPanelItem = new ToolStripMenuItem("Show Control Panel");
            controlPanelItem.Click += OnShowControlPanel;
            _contextMenu.Items.Add(controlPanelItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            // Quick presets (placeholder)
            var presetsItem = new ToolStripMenuItem("Quick Presets");
            presetsItem.DropDownItems.Add(new ToolStripMenuItem("Subtle") { Tag = "subtle" });
            presetsItem.DropDownItems.Add(new ToolStripMenuItem("Moderate") { Tag = "moderate" });
            presetsItem.DropDownItems.Add(new ToolStripMenuItem("Intense") { Tag = "intense" });
            _contextMenu.Items.Add(presetsItem);
            
            _contextMenu.Items.Add(new ToolStripSeparator());
            
            // Exit
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += OnExit;
            _contextMenu.Items.Add(exitItem);
            
            _notifyIcon.ContextMenuStrip = _contextMenu;
        }

        private void OnTrayIconDoubleClick(object sender, EventArgs e)
        {
            ShowControlPanel();
        }

        private void OnTrayIconClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                // Left click toggles overlay
                _overlayManager.ToggleOverlay();
            }
        }

        private void OnToggleOverlay(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            if (menuItem == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.IsEnabled = menuItem.Checked;
            _configService.UpdateConfiguration(config);
        }

        private void OnShowControlPanel(object sender, EventArgs e)
        {
            ShowControlPanel();
        }

        private void ShowControlPanel()
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.ShowFromTray();
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnOverlayStateChanged(object sender, bool isActive)
        {
            // Update context menu
            if (_contextMenu.Items.Count > 0 && _contextMenu.Items[0] is ToolStripMenuItem toggleItem)
            {
                toggleItem.Checked = isActive;
            }

            // Update tray icon tooltip
            _notifyIcon.Text = $"Glitch Overlay - {(isActive ? "Active" : "Inactive")}";
        }

        public void ShowNotification(string title, string message, ToolTipIcon icon = ToolTipIcon.Info, int timeout = 3000)
        {
            try
            {
                _notifyIcon?.ShowBalloonTip(timeout, title, message, icon);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing notification: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (_overlayManager != null)
                    _overlayManager.OverlayStateChanged -= OnOverlayStateChanged;

                _contextMenu?.Dispose();
                
                if (_notifyIcon != null)
                {
                    _notifyIcon.Visible = false;
                    _notifyIcon.Dispose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing system tray manager: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}
