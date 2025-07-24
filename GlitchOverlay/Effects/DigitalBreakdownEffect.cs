using System;
using GlitchOverlay.Core;
using GlitchOverlay.Models;

namespace GlitchOverlay.Effects
{
    /// <summary>
    /// Effect that creates digital breakdown artifacts like static, pixelation, and tearing
    /// </summary>
    public class DigitalBreakdownEffect : IEffect
    {
        #region Properties

        public string Name => "Digital Breakdown";
        public bool IsEnabled { get; private set; }

        #endregion

        #region Fields

        private DigitalBreakdownSettings _settings;
        private Random _random;
        private double _timeSinceLastBreakdown;
        private double _breakdownInterval;
        private double _currentBreakdownDuration;
        private bool _isBreakingDown;
        private BreakdownType _currentBreakdownType;

        #endregion

        #region Constructor

        public DigitalBreakdownEffect()
        {
            _random = new Random();
        }

        #endregion

        #region IEffect Implementation

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _settings = config?.DigitalBreakdown;
            IsEnabled = _settings?.IsEnabled == true && config?.IsEnabled == true;
            
            if (_settings != null)
            {
                // Calculate breakdown interval based on frequency
                _breakdownInterval = Math.Max(0.5, 3.0 - (_settings.Frequency * 3.0));
            }
        }

        public void Apply(GraphicsEngine graphics, double deltaTime)
        {
            if (!IsEnabled || _settings == null) return;

            _timeSinceLastBreakdown += deltaTime;

            // Check if we should start a new breakdown
            if (!_isBreakingDown && _timeSinceLastBreakdown >= _breakdownInterval)
            {
                StartBreakdown();
            }

            // Apply breakdown if active
            if (_isBreakingDown)
            {
                ApplyBreakdown(graphics);
                
                _currentBreakdownDuration -= deltaTime;
                if (_currentBreakdownDuration <= 0)
                {
                    EndBreakdown();
                }
            }
        }

        public void Reset()
        {
            _timeSinceLastBreakdown = 0;
            _currentBreakdownDuration = 0;
            _isBreakingDown = false;
        }

        #endregion

        #region Private Methods

        private void StartBreakdown()
        {
            _isBreakingDown = true;
            _currentBreakdownDuration = _settings.Duration;
            _timeSinceLastBreakdown = 0;

            // Choose breakdown type
            if (_settings.Type == BreakdownType.Mixed)
            {
                var types = new[] { BreakdownType.Static, BreakdownType.Pixelation, BreakdownType.Tearing };
                _currentBreakdownType = types[_random.Next(types.Length)];
            }
            else
            {
                _currentBreakdownType = _settings.Type;
            }
        }

        private void EndBreakdown()
        {
            _isBreakingDown = false;
        }

        private void ApplyBreakdown(GraphicsEngine graphics)
        {
            switch (_currentBreakdownType)
            {
                case BreakdownType.Static:
                    ApplyStaticEffect(graphics);
                    break;
                case BreakdownType.Pixelation:
                    ApplyPixelationEffect(graphics);
                    break;
                case BreakdownType.Tearing:
                    ApplyTearingEffect(graphics);
                    break;
            }
        }

        private void ApplyStaticEffect(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var staticDensity = (int)(intensity * 1000);
            
            // Generate random static pixels
            for (int i = 0; i < staticDensity; i++)
            {
                var x = _random.Next(graphics.Width);
                var y = _random.Next(graphics.Height);
                
                var brightness = (byte)_random.Next(256);
                var alpha = (byte)(intensity * 200);
                var staticColor = GraphicsEngine.CreateColor(alpha, brightness, brightness, brightness);
                
                graphics.SetPixel(x, y, staticColor);
            }

            // Add some larger static blocks
            var blockCount = (int)(intensity * 20);
            for (int i = 0; i < blockCount; i++)
            {
                var x = _random.Next(graphics.Width - 10);
                var y = _random.Next(graphics.Height - 10);
                var size = _random.Next(2, 8);
                
                var brightness = (byte)_random.Next(256);
                var alpha = (byte)(intensity * 150);
                var blockColor = GraphicsEngine.CreateColor(alpha, brightness, brightness, brightness);
                
                graphics.FillRect(x, y, size, size, blockColor);
            }
        }

        private void ApplyPixelationEffect(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var blockSize = Math.Max(2, _settings.BlockSize);
            var pixelationCount = (int)(intensity * 50);
            
            for (int i = 0; i < pixelationCount; i++)
            {
                var x = _random.Next(0, graphics.Width - blockSize);
                var y = _random.Next(0, graphics.Height - blockSize);
                
                // Create pixelated blocks with random colors
                var red = (byte)_random.Next(256);
                var green = (byte)_random.Next(256);
                var blue = (byte)_random.Next(256);
                var alpha = (byte)(intensity * 180);
                
                var pixelColor = GraphicsEngine.CreateColor(alpha, red, green, blue);
                graphics.FillRect(x, y, blockSize, blockSize, pixelColor);
            }
        }

        private void ApplyTearingEffect(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var tearCount = (int)(intensity * 10);
            
            for (int i = 0; i < tearCount; i++)
            {
                if (_random.Next(2) == 0)
                {
                    // Horizontal tear
                    ApplyHorizontalTear(graphics, intensity);
                }
                else
                {
                    // Vertical tear
                    ApplyVerticalTear(graphics, intensity);
                }
            }
        }

        private void ApplyHorizontalTear(GraphicsEngine graphics, double intensity)
        {
            var y = _random.Next(graphics.Height);
            var tearWidth = _random.Next(50, Math.Min(200, graphics.Width / 4));
            var tearHeight = _random.Next(2, 8);
            var x = _random.Next(graphics.Width - tearWidth);
            
            // Create a "torn" effect with displaced pixels
            var displacement = _random.Next(5, 20);
            var alpha = (byte)(intensity * 120);
            
            for (int ty = 0; ty < tearHeight; ty++)
            {
                for (int tx = 0; tx < tearWidth; tx++)
                {
                    var sourceX = x + tx;
                    var sourceY = y + ty;
                    var targetX = sourceX + displacement;
                    
                    if (sourceX < graphics.Width && sourceY < graphics.Height && 
                        targetX < graphics.Width)
                    {
                        // Create a glitchy color for the tear
                        var tearColor = GraphicsEngine.CreateColor(alpha, 
                            (byte)_random.Next(256), 
                            (byte)_random.Next(100), 
                            (byte)_random.Next(256));
                        
                        graphics.SetPixel(targetX, sourceY, tearColor);
                    }
                }
            }
        }

        private void ApplyVerticalTear(GraphicsEngine graphics, double intensity)
        {
            var x = _random.Next(graphics.Width);
            var tearHeight = _random.Next(50, Math.Min(200, graphics.Height / 4));
            var tearWidth = _random.Next(2, 8);
            var y = _random.Next(graphics.Height - tearHeight);
            
            // Create a "torn" effect with displaced pixels
            var displacement = _random.Next(5, 20);
            var alpha = (byte)(intensity * 120);
            
            for (int tx = 0; tx < tearWidth; tx++)
            {
                for (int ty = 0; ty < tearHeight; ty++)
                {
                    var sourceX = x + tx;
                    var sourceY = y + ty;
                    var targetY = sourceY + displacement;
                    
                    if (sourceX < graphics.Width && sourceY < graphics.Height && 
                        targetY < graphics.Height)
                    {
                        // Create a glitchy color for the tear
                        var tearColor = GraphicsEngine.CreateColor(alpha, 
                            (byte)_random.Next(256), 
                            (byte)_random.Next(100), 
                            (byte)_random.Next(256));
                        
                        graphics.SetPixel(sourceX, targetY, tearColor);
                    }
                }
            }
        }

        #endregion
    }
}
