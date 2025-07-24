using System;
using GlitchOverlay.Core;
using GlitchOverlay.Models;

namespace GlitchOverlay.Effects
{
    /// <summary>
    /// Effect that creates pixel displacement and shifting
    /// </summary>
    public class PixelShiftEffect : IEffect
    {
        #region Properties

        public string Name => "Pixel Shift";
        public bool IsEnabled { get; private set; }

        #endregion

        #region Fields

        private PixelShiftSettings _settings;
        private Random _random;
        private double _timeSinceLastShift;
        private double _shiftInterval;

        #endregion

        #region Constructor

        public PixelShiftEffect()
        {
            _random = new Random();
            _shiftInterval = 0.1; // Default shift every 100ms
        }

        #endregion

        #region IEffect Implementation

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _settings = config?.PixelShift;
            IsEnabled = _settings?.IsEnabled == true && config?.IsEnabled == true;
            
            if (_settings != null)
            {
                // Calculate shift interval based on frequency
                _shiftInterval = Math.Max(0.01, 1.0 - _settings.Frequency);
            }
        }

        public void Apply(GraphicsEngine graphics, double deltaTime)
        {
            if (!IsEnabled || _settings == null) return;

            _timeSinceLastShift += deltaTime;

            // Check if it's time for a new shift
            if (_timeSinceLastShift >= _shiftInterval)
            {
                ApplyPixelShift(graphics);
                _timeSinceLastShift = 0;
            }
        }

        public void Reset()
        {
            _timeSinceLastShift = 0;
        }

        #endregion

        #region Private Methods

        private void ApplyPixelShift(GraphicsEngine graphics)
        {
            var intensity = _settings.Intensity;
            var maxDistance = (int)(intensity * _settings.MaxShiftDistance);
            
            if (maxDistance <= 0) return;

            // Create different types of shifts based on block size
            if (_settings.BlockSize <= 1)
            {
                ApplyIndividualPixelShift(graphics, maxDistance);
            }
            else
            {
                ApplyBlockShift(graphics, maxDistance);
            }
        }

        private void ApplyIndividualPixelShift(GraphicsEngine graphics, int maxDistance)
        {
            var shiftProbability = _settings.Intensity * 0.1; // 10% max probability
            
            for (int y = 0; y < graphics.Height; y++)
            {
                for (int x = 0; x < graphics.Width; x++)
                {
                    if (_random.NextDouble() < shiftProbability)
                    {
                        var distance = _random.Next(1, maxDistance + 1);
                        int newX = x, newY = y;

                        switch (_settings.Direction)
                        {
                            case ShiftDirection.Horizontal:
                                newX = x + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                            case ShiftDirection.Vertical:
                                newY = y + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                            case ShiftDirection.Both:
                                newX = x + (_random.Next(2) == 0 ? distance : -distance);
                                newY = y + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                        }

                        // Ensure coordinates are within bounds
                        newX = Math.Max(0, Math.Min(graphics.Width - 1, newX));
                        newY = Math.Max(0, Math.Min(graphics.Height - 1, newY));

                        // Get the pixel at the original location and move it
                        var pixel = graphics.GetPixel(x, y);
                        if (pixel != 0) // Only move non-transparent pixels
                        {
                            graphics.SetPixel(newX, newY, pixel);
                            graphics.SetPixel(x, y, 0); // Clear original location
                        }
                    }
                }
            }
        }

        private void ApplyBlockShift(GraphicsEngine graphics, int maxDistance)
        {
            var blockSize = _settings.BlockSize;
            var blocksX = graphics.Width / blockSize;
            var blocksY = graphics.Height / blockSize;
            var shiftProbability = _settings.Intensity * 0.2; // 20% max probability for blocks

            for (int by = 0; by < blocksY; by++)
            {
                for (int bx = 0; bx < blocksX; bx++)
                {
                    if (_random.NextDouble() < shiftProbability)
                    {
                        var distance = _random.Next(1, maxDistance + 1);
                        int newBx = bx, newBy = by;

                        switch (_settings.Direction)
                        {
                            case ShiftDirection.Horizontal:
                                newBx = bx + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                            case ShiftDirection.Vertical:
                                newBy = by + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                            case ShiftDirection.Both:
                                newBx = bx + (_random.Next(2) == 0 ? distance : -distance);
                                newBy = by + (_random.Next(2) == 0 ? distance : -distance);
                                break;
                        }

                        // Ensure block coordinates are within bounds
                        newBx = Math.Max(0, Math.Min(blocksX - 1, newBx));
                        newBy = Math.Max(0, Math.Min(blocksY - 1, newBy));

                        // Move the entire block
                        MoveBlock(graphics, bx * blockSize, by * blockSize, 
                                 newBx * blockSize, newBy * blockSize, blockSize);
                    }
                }
            }
        }

        private void MoveBlock(GraphicsEngine graphics, int srcX, int srcY, int dstX, int dstY, int size)
        {
            // Store the block pixels
            var blockPixels = new uint[size * size];
            int index = 0;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pixelX = srcX + x;
                    var pixelY = srcY + y;
                    
                    if (pixelX < graphics.Width && pixelY < graphics.Height)
                    {
                        blockPixels[index] = graphics.GetPixel(pixelX, pixelY);
                        graphics.SetPixel(pixelX, pixelY, 0); // Clear source
                    }
                    index++;
                }
            }

            // Place the block at the new location
            index = 0;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    var pixelX = dstX + x;
                    var pixelY = dstY + y;
                    
                    if (pixelX < graphics.Width && pixelY < graphics.Height)
                    {
                        graphics.SetPixel(pixelX, pixelY, blockPixels[index]);
                    }
                    index++;
                }
            }
        }

        #endregion
    }
}
