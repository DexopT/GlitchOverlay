using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GlitchOverlay.Models;
using GlitchOverlay.Services;

namespace GlitchOverlay.Core
{
    /// <summary>
    /// Transparent overlay window that covers the entire desktop
    /// Implements a full-screen, click-through overlay for applying visual effects
    /// </summary>
    public class OverlayWindow : Window
    {
        #region Fields

        private GlitchConfiguration _currentConfig;
        private GraphicsEngine _graphicsEngine;
        private EffectManager _effectManager;
        private System.Windows.Controls.Image _overlayImage;
        private IntPtr _hwnd;
        private bool _isInitialized;

        #endregion

        #region Constructor

        public OverlayWindow()
        {
            try
            {
                InitializeWindow();
                CreateOverlayCanvas();
                _graphicsEngine = new GraphicsEngine();
                _effectManager = new EffectManager();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OverlayWindow constructor: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Initialization

        private void InitializeWindow()
        {
            // Configure window properties for overlay
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;
            WindowStartupLocation = WindowStartupLocation.Manual;

            // Set window to cover entire virtual screen (all monitors)
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            // Prevent window from being activated
            this.Focusable = false;
            this.IsHitTestVisible = false;
        }

        private void CreateOverlayCanvas()
        {
            // Create the image control that will display our effects
            _overlayImage = new System.Windows.Controls.Image
            {
                Stretch = Stretch.Fill,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            this.Content = _overlayImage;
        }

        public void Initialize()
        {
            if (_isInitialized) return;

            try
            {
                // Validate required components
                if (_graphicsEngine == null)
                    throw new InvalidOperationException("Graphics engine is not initialized");

                if (_overlayImage == null)
                    throw new InvalidOperationException("Overlay image is not initialized");

                // Initialize the graphics engine
                var width = (int)SystemParameters.VirtualScreenWidth;
                var height = (int)SystemParameters.VirtualScreenHeight;

                if (width <= 0 || height <= 0)
                    throw new InvalidOperationException($"Invalid screen dimensions: {width}x{height}");

                _graphicsEngine.Initialize(width, height);
                _overlayImage.Source = _graphicsEngine.RenderTarget;

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing overlay window: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Configuration and Effects

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _currentConfig = config?.Clone();

            // Update effect manager with new configuration
            _effectManager?.UpdateConfiguration(_currentConfig);

            // Update overlay visibility based on configuration
            if (_currentConfig != null)
            {
                this.Opacity = _currentConfig.IsEnabled ? 1.0 : 0.0;
            }
        }

        public void UpdateEffects()
        {
            if (!_isInitialized || _currentConfig == null || !_currentConfig.IsEnabled || !_graphicsEngine.IsInitialized)
                return;

            try
            {
                // Begin frame rendering
                _graphicsEngine.BeginFrame();

                // Process all effects through the effect manager
                _effectManager?.ProcessEffects(_graphicsEngine);

                // End frame rendering
                _graphicsEngine.EndFrame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating effects: {ex.Message}");
            }
        }



        #endregion

        #region Window Management

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _hwnd = new WindowInteropHelper(this).Handle;

            // Make window click-through and ensure it stays on top
            WindowsServices.SetWindowExTransparent(_hwnd);
            WindowsServices.SetWindowTopmost(_hwnd);
        }

        protected override void OnClosed(EventArgs e)
        {
            _effectManager?.Dispose();
            _graphicsEngine?.Dispose();
            base.OnClosed(e);
        }

        #endregion
    }
}
