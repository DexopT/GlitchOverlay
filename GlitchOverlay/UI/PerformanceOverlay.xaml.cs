using System;
using System.Windows;
using System.Windows.Interop;
using GlitchOverlay.Services;
using GlitchOverlay.Models;

namespace GlitchOverlay.UI
{
    /// <summary>
    /// Desktop performance overlay window
    /// </summary>
    public partial class PerformanceOverlay : Window
    {
        private bool _isInitialized;
        private PerformanceOverlaySettings _settings;

        public PerformanceOverlay()
        {
            InitializeComponent();
            InitializeWindow();
        }

        private void InitializeWindow()
        {
            // Make window click-through
            this.Focusable = false;
            this.IsHitTestVisible = false;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            try
            {
                // Make window click-through
                var hwnd = new WindowInteropHelper(this).Handle;
                WindowsServices.SetWindowExTransparent(hwnd);
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing performance overlay: {ex.Message}");
            }
        }

        public void UpdatePerformance(PerformanceData data)
        {
            if (!_isInitialized || _settings == null) return;

            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Update text based on settings
                    if (_settings.ShowFPS)
                        FpsText.Text = data.FrameRate.ToString("F1");

                    if (_settings.ShowCPU)
                        CpuText.Text = data.CpuUsage.ToString("F1") + "%";

                    if (_settings.ShowMemory)
                        MemText.Text = data.MemoryUsageMB.ToString("F0") + "MB";

                    if (_settings.ShowEffects)
                        EffectsText.Text = $"{data.ActiveEffects}/{data.TotalEffects}";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating performance overlay: {ex.Message}");
            }
        }

        public void ApplySettings(PerformanceOverlaySettings settings)
        {
            _settings = settings;

            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Apply font size
                    var fontSize = settings.FontSize;
                    FpsText.FontSize = fontSize;
                    CpuText.FontSize = fontSize;
                    MemText.FontSize = fontSize;
                    EffectsText.FontSize = fontSize;

                    // Apply visibility
                    FpsText.Visibility = settings.ShowFPS ? Visibility.Visible : Visibility.Collapsed;
                    CpuText.Visibility = settings.ShowCPU ? Visibility.Visible : Visibility.Collapsed;
                    MemText.Visibility = settings.ShowMemory ? Visibility.Visible : Visibility.Collapsed;
                    EffectsText.Visibility = settings.ShowEffects ? Visibility.Visible : Visibility.Collapsed;

                    // Apply opacity
                    this.Opacity = settings.Opacity;

                    // Apply position
                    ApplyPosition(settings.Position);

                    // Apply color scheme
                    ApplyColorScheme(settings.ColorScheme);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error applying overlay settings: {ex.Message}");
            }
        }

        private void ApplyPosition(OverlayPosition position)
        {
            var workingArea = SystemParameters.WorkArea;

            switch (position)
            {
                case OverlayPosition.TopLeft:
                    this.Left = 20;
                    this.Top = 20;
                    break;
                case OverlayPosition.TopRight:
                    this.Left = workingArea.Width - this.Width - 20;
                    this.Top = 20;
                    break;
                case OverlayPosition.BottomLeft:
                    this.Left = 20;
                    this.Top = workingArea.Height - this.Height - 20;
                    break;
                case OverlayPosition.BottomRight:
                    this.Left = workingArea.Width - this.Width - 20;
                    this.Top = workingArea.Height - this.Height - 20;
                    break;
            }
        }

        private void ApplyColorScheme(OverlayColorScheme colorScheme)
        {
            string secondaryColor;

            switch (colorScheme)
            {
                case OverlayColorScheme.GreenCyan:
                    secondaryColor = "#41FFFF";
                    break;
                case OverlayColorScheme.RedOrange:
                    secondaryColor = "#FF8888"; // Bright red for better visibility
                    break;
                case OverlayColorScheme.BlueWhite:
                    secondaryColor = "#FFFFFF";
                    break;
                default:
                    secondaryColor = "#FF8888"; // Default to red theme
                    break;
            }

            // Apply colors to text elements
            var brush = new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(secondaryColor));

            FpsText.Foreground = brush;
            CpuText.Foreground = brush;
            MemText.Foreground = brush;
            EffectsText.Foreground = brush;
        }

        public void ShowOverlay()
        {
            try
            {
                this.Show();
                this.Topmost = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing performance overlay: {ex.Message}");
            }
        }

        public void HideOverlay()
        {
            try
            {
                this.Hide();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error hiding performance overlay: {ex.Message}");
            }
        }
    }
}
