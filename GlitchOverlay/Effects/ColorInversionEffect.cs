using System;
using GlitchOverlay.Core;
using GlitchOverlay.Models;

namespace GlitchOverlay.Effects
{
    /// <summary>
    /// Effect that creates color inversions and channel shifts
    /// </summary>
    public class ColorInversionEffect : IEffect
    {
        #region Properties

        public string Name => "Color Inversion";
        public bool IsEnabled { get; private set; }

        #endregion

        #region Fields

        private ColorInversionSettings _settings;
        private Random _random;
        private double _timeSinceLastInversion;
        private double _inversionInterval;
        private double _currentInversionDuration;
        private bool _isInverting;

        #endregion

        #region Constructor

        public ColorInversionEffect()
        {
            _random = new Random();
        }

        #endregion

        #region IEffect Implementation

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _settings = config?.ColorInversion;
            IsEnabled = _settings?.IsEnabled == true && config?.IsEnabled == true;
            
            if (_settings != null)
            {
                // Calculate inversion interval based on frequency
                _inversionInterval = Math.Max(0.1, 2.0 - (_settings.Frequency * 2.0));
            }
        }

        public void Apply(GraphicsEngine graphics, double deltaTime)
        {
            if (!IsEnabled || _settings == null) return;

            _timeSinceLastInversion += deltaTime;

            // Check if we should start a new inversion
            if (!_isInverting && _timeSinceLastInversion >= _inversionInterval)
            {
                StartInversion();
            }

            // Apply inversion if active
            if (_isInverting)
            {
                ApplyColorInversion(graphics);
                
                _currentInversionDuration -= deltaTime;
                if (_currentInversionDuration <= 0)
                {
                    EndInversion();
                }
            }
        }

        public void Reset()
        {
            _timeSinceLastInversion = 0;
            _currentInversionDuration = 0;
            _isInverting = false;
        }

        #endregion

        #region Private Methods

        private void StartInversion()
        {
            _isInverting = true;
            _currentInversionDuration = _settings.Duration;
            _timeSinceLastInversion = 0;
        }

        private void EndInversion()
        {
            _isInverting = false;
        }

        private void ApplyColorInversion(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var alpha = (byte)(intensity * 255);

            switch (_settings.Type)
            {
                case InversionType.Full:
                    ApplyFullInversion(graphics, alpha);
                    break;
                case InversionType.ChannelShift:
                    ApplyChannelShift(graphics, alpha);
                    break;
                case InversionType.Desaturation:
                    ApplyDesaturation(graphics, alpha);
                    break;
                case InversionType.Hue:
                    ApplyHueShift(graphics, alpha);
                    break;
            }
        }

        private void ApplyFullInversion(GraphicsEngine graphics, byte alpha)
        {
            if (_settings.AffectFullScreen)
            {
                // Full screen inversion overlay
                var inversionColor = GraphicsEngine.CreateColor(alpha, 255, 255, 255);
                graphics.FillRect(0, 0, graphics.Width, graphics.Height, inversionColor);
            }
            else
            {
                // Random patches of inversion
                var patchCount = (int)(_settings.Intensity * 10);
                for (int i = 0; i < patchCount; i++)
                {
                    var x = _random.Next(graphics.Width / 4);
                    var y = _random.Next(graphics.Height / 4);
                    var width = _random.Next(50, 200);
                    var height = _random.Next(50, 200);
                    
                    var inversionColor = GraphicsEngine.CreateColor(alpha, 255, 255, 255);
                    graphics.FillRect(x, y, width, height, inversionColor);
                }
            }
        }

        private void ApplyChannelShift(GraphicsEngine graphics, byte alpha)
        {
            var offsetX = (int)(_settings.Intensity * 20);
            var offsetY = (int)(_settings.Intensity * 10);
            
            // Shift red channel
            graphics.ApplyColorShift(offsetX, 0, ColorChannel.Red);
            
            // Shift blue channel in opposite direction
            graphics.ApplyColorShift(-offsetX, 0, ColorChannel.Blue);
            
            // Add some vertical shift to green channel
            graphics.ApplyColorShift(0, offsetY, ColorChannel.Green);
        }

        private void ApplyDesaturation(GraphicsEngine graphics, byte alpha)
        {
            // Create desaturation effect by overlaying gray
            var grayValue = (byte)(128 * _settings.Intensity);
            var desaturationColor = GraphicsEngine.CreateColor(alpha, grayValue, grayValue, grayValue);
            
            if (_settings.AffectFullScreen)
            {
                graphics.FillRect(0, 0, graphics.Width, graphics.Height, desaturationColor);
            }
            else
            {
                // Random desaturated patches
                var patchCount = (int)(_settings.Intensity * 15);
                for (int i = 0; i < patchCount; i++)
                {
                    var x = _random.Next(graphics.Width);
                    var y = _random.Next(graphics.Height);
                    var size = _random.Next(20, 100);
                    
                    graphics.FillRect(x, y, size, size, desaturationColor);
                }
            }
        }

        private void ApplyHueShift(GraphicsEngine graphics, byte alpha)
        {
            // Create colored overlays to simulate hue shift
            var colors = new[]
            {
                GraphicsEngine.CreateColor(alpha, 255, 0, 0),   // Red
                GraphicsEngine.CreateColor(alpha, 0, 255, 0),   // Green
                GraphicsEngine.CreateColor(alpha, 0, 0, 255),   // Blue
                GraphicsEngine.CreateColor(alpha, 255, 255, 0), // Yellow
                GraphicsEngine.CreateColor(alpha, 255, 0, 255), // Magenta
                GraphicsEngine.CreateColor(alpha, 0, 255, 255)  // Cyan
            };

            var stripCount = (int)(_settings.Intensity * 8);
            var stripHeight = graphics.Height / Math.Max(1, stripCount);

            for (int i = 0; i < stripCount; i++)
            {
                var y = i * stripHeight;
                var color = colors[_random.Next(colors.Length)];
                
                graphics.FillRect(0, y, graphics.Width, stripHeight, color);
            }
        }

        #endregion
    }
}
