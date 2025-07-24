using System;
using GlitchOverlay.Core;
using GlitchOverlay.Models;

namespace GlitchOverlay.Effects
{
    /// <summary>
    /// Effect that creates CRT-style scanlines
    /// </summary>
    public class ScanlineEffect : IEffect
    {
        #region Properties

        public string Name => "Scanlines";
        public bool IsEnabled { get; private set; }

        #endregion

        #region Fields

        private ScanlineSettings _settings;
        private Random _random;
        private double _timeSinceLastFlicker;
        private double _flickerInterval;
        private bool _isFlickering;
        private double _flickerDuration;

        #endregion

        #region Constructor

        public ScanlineEffect()
        {
            _random = new Random();
        }

        #endregion

        #region IEffect Implementation

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _settings = config?.Scanlines;
            IsEnabled = _settings?.IsEnabled == true && config?.IsEnabled == true;
            
            if (_settings != null)
            {
                // Calculate flicker interval based on flicker rate
                _flickerInterval = Math.Max(0.05, 1.0 - _settings.FlickerRate);
            }
        }

        public void Apply(GraphicsEngine graphics, double deltaTime)
        {
            if (!IsEnabled || _settings == null) return;

            UpdateFlicker(deltaTime);
            DrawScanlines(graphics);
        }

        public void Reset()
        {
            _timeSinceLastFlicker = 0;
            _isFlickering = false;
            _flickerDuration = 0;
        }

        #endregion

        #region Private Methods

        private void UpdateFlicker(double deltaTime)
        {
            _timeSinceLastFlicker += deltaTime;

            if (_isFlickering)
            {
                _flickerDuration -= deltaTime;
                if (_flickerDuration <= 0)
                {
                    _isFlickering = false;
                }
            }
            else if (_timeSinceLastFlicker >= _flickerInterval)
            {
                if (_random.NextDouble() < _settings.FlickerRate)
                {
                    _isFlickering = true;
                    _flickerDuration = 0.1 + (_random.NextDouble() * 0.2); // 0.1-0.3 seconds
                }
                _timeSinceLastFlicker = 0;
            }
        }

        private void DrawScanlines(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var spacing = _settings.LineSpacing;
            
            // Adjust intensity based on flicker state
            if (_isFlickering)
            {
                intensity *= 0.3 + (_random.NextDouble() * 0.7); // Flicker between 30-100%
            }

            var alpha = (byte)(intensity * 150);
            var scanlineColor = GraphicsEngine.CreateColor(alpha, 0, 255, 0); // Green scanlines

            switch (_settings.Pattern)
            {
                case ScanlinePattern.Horizontal:
                    DrawHorizontalScanlines(graphics, spacing, scanlineColor);
                    break;
                case ScanlinePattern.Vertical:
                    DrawVerticalScanlines(graphics, spacing, scanlineColor);
                    break;
                case ScanlinePattern.Grid:
                    DrawHorizontalScanlines(graphics, spacing, scanlineColor);
                    DrawVerticalScanlines(graphics, spacing, scanlineColor);
                    break;
            }
        }

        private void DrawHorizontalScanlines(GraphicsEngine graphics, int spacing, uint color)
        {
            for (int y = 0; y < graphics.Height; y += spacing)
            {
                // Add some randomness to line thickness and position
                var lineThickness = 1;
                if (_settings.Intensity > 0.7)
                {
                    lineThickness = _random.Next(1, 3);
                }

                var yOffset = _isFlickering ? _random.Next(-1, 2) : 0;
                var actualY = Math.Max(0, Math.Min(graphics.Height - 1, y + yOffset));

                for (int t = 0; t < lineThickness && actualY + t < graphics.Height; t++)
                {
                    graphics.DrawLine(0, actualY + t, graphics.Width - 1, actualY + t, color);
                }
            }
        }

        private void DrawVerticalScanlines(GraphicsEngine graphics, int spacing, uint color)
        {
            for (int x = 0; x < graphics.Width; x += spacing)
            {
                // Add some randomness to line thickness and position
                var lineThickness = 1;
                if (_settings.Intensity > 0.7)
                {
                    lineThickness = _random.Next(1, 3);
                }

                var xOffset = _isFlickering ? _random.Next(-1, 2) : 0;
                var actualX = Math.Max(0, Math.Min(graphics.Width - 1, x + xOffset));

                for (int t = 0; t < lineThickness && actualX + t < graphics.Width; t++)
                {
                    graphics.DrawLine(actualX + t, 0, actualX + t, graphics.Height - 1, color);
                }
            }
        }

        #endregion
    }
}
