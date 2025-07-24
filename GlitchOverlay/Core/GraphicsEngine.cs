using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GlitchOverlay.Models;

namespace GlitchOverlay.Core
{
    /// <summary>
    /// High-performance graphics engine for rendering glitch effects
    /// </summary>
    public class GraphicsEngine : IDisposable
    {
        #region Fields

        private WriteableBitmap _renderTarget;
        private int _width;
        private int _height;
        private int _stride;
        private bool _disposed;
        private Random _random;

        // Performance optimization: pre-allocated arrays
        private uint[] _pixelBuffer;
        private byte[] _noiseBuffer;

        #endregion

        #region Properties

        public WriteableBitmap RenderTarget => _renderTarget;
        public int Width => _width;
        public int Height => _height;
        public bool IsInitialized { get; private set; }

        #endregion

        #region Constructor

        public GraphicsEngine()
        {
            _random = new Random();
        }

        #endregion

        #region Initialization

        public void Initialize(int width, int height)
        {
            if (IsInitialized)
                Dispose();

            // Reduce resolution for better memory usage - use 1/4 resolution for effects
            var effectWidth = Math.Max(320, width / 4);
            var effectHeight = Math.Max(240, height / 4);

            _width = effectWidth;
            _height = effectHeight;
            _stride = effectWidth * 4; // 4 bytes per pixel (BGRA)

            // Create the render target with reduced resolution
            _renderTarget = new WriteableBitmap(effectWidth, effectHeight, 96, 96, PixelFormats.Bgra32, null);

            // Pre-allocate smaller buffers for better memory usage
            _pixelBuffer = new uint[effectWidth * effectHeight];
            _noiseBuffer = new byte[effectWidth * effectHeight * 4];

            IsInitialized = true;
        }

        #endregion

        #region Core Rendering

        public void BeginFrame()
        {
            if (!IsInitialized) return;

            _renderTarget.Lock();
            ClearBuffer();
        }

        public void EndFrame()
        {
            if (!IsInitialized) return;

            // Copy our pixel buffer to the bitmap
            unsafe
            {
                var backBuffer = (uint*)_renderTarget.BackBuffer.ToPointer();
                fixed (uint* pixelPtr = _pixelBuffer)
                {
                    for (int i = 0; i < _pixelBuffer.Length; i++)
                    {
                        backBuffer[i] = pixelPtr[i];
                    }
                }
            }

            _renderTarget.AddDirtyRect(new Int32Rect(0, 0, _width, _height));
            _renderTarget.Unlock();
        }

        private void ClearBuffer()
        {
            // Clear to transparent
            Array.Clear(_pixelBuffer, 0, _pixelBuffer.Length);
        }

        #endregion

        #region Basic Drawing Operations

        public void SetPixel(int x, int y, uint color)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return;
            
            var index = y * _width + x;
            _pixelBuffer[index] = color;
        }

        public uint GetPixel(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height) return 0;
            
            var index = y * _width + x;
            return _pixelBuffer[index];
        }

        public void DrawLine(int x1, int y1, int x2, int y2, uint color)
        {
            // Bresenham's line algorithm
            int dx = Math.Abs(x2 - x1);
            int dy = Math.Abs(y2 - y1);
            int sx = x1 < x2 ? 1 : -1;
            int sy = y1 < y2 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                SetPixel(x1, y1, color);

                if (x1 == x2 && y1 == y2) break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x1 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y1 += sy;
                }
            }
        }

        public void FillRect(int x, int y, int width, int height, uint color)
        {
            for (int py = y; py < y + height && py < _height; py++)
            {
                for (int px = x; px < x + width && px < _width; px++)
                {
                    SetPixel(px, py, color);
                }
            }
        }

        #endregion

        #region Noise and Random Generation

        public void GenerateNoise(byte intensity)
        {
            // Generate noise more efficiently without large buffer
            var noisePixelCount = Math.Min(1000, (intensity * _pixelBuffer.Length) / 255);

            for (int i = 0; i < noisePixelCount; i++)
            {
                var x = _random.Next(_width);
                var y = _random.Next(_height);
                var index = y * _width + x;

                var noiseValue = (byte)_random.Next(256);
                var color = (uint)((intensity << 24) | (noiseValue << 16) | (noiseValue << 8) | noiseValue);
                _pixelBuffer[index] = color;
            }
        }

        public void AddRandomPixels(int count, uint color)
        {
            for (int i = 0; i < count; i++)
            {
                var x = _random.Next(_width);
                var y = _random.Next(_height);
                SetPixel(x, y, color);
            }
        }

        #endregion

        #region Advanced Effects

        public void ApplyColorShift(int offsetX, int offsetY, ColorChannel channel)
        {
            if (offsetX == 0 && offsetY == 0) return;

            var tempBuffer = new uint[_pixelBuffer.Length];
            Array.Copy(_pixelBuffer, tempBuffer, _pixelBuffer.Length);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var sourceX = x - offsetX;
                    var sourceY = y - offsetY;
                    
                    if (sourceX >= 0 && sourceX < _width && sourceY >= 0 && sourceY < _height)
                    {
                        var sourceIndex = sourceY * _width + sourceX;
                        var targetIndex = y * _width + x;
                        
                        var sourceColor = tempBuffer[sourceIndex];
                        var targetColor = _pixelBuffer[targetIndex];
                        
                        _pixelBuffer[targetIndex] = BlendColorChannel(targetColor, sourceColor, channel);
                    }
                }
            }
        }

        private uint BlendColorChannel(uint target, uint source, ColorChannel channel)
        {
            switch (channel)
            {
                case ColorChannel.Red:
                    return (target & 0xFF00FFFF) | (source & 0x00FF0000);
                case ColorChannel.Green:
                    return (target & 0xFFFF00FF) | (source & 0x0000FF00);
                case ColorChannel.Blue:
                    return (target & 0xFFFFFF00) | (source & 0x000000FF);
                default:
                    return source;
            }
        }

        public void ApplyPixelShift(int maxDistance, ShiftDirection direction)
        {
            var tempBuffer = new uint[_pixelBuffer.Length];
            Array.Copy(_pixelBuffer, tempBuffer, _pixelBuffer.Length);

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (_random.NextDouble() < 0.1) // 10% chance for each pixel
                    {
                        var distance = _random.Next(1, maxDistance + 1);
                        int newX = x, newY = y;

                        switch (direction)
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

                        if (newX >= 0 && newX < _width && newY >= 0 && newY < _height)
                        {
                            var sourceIndex = y * _width + x;
                            var targetIndex = newY * _width + newX;
                            _pixelBuffer[targetIndex] = tempBuffer[sourceIndex];
                        }
                    }
                }
            }
        }

        #endregion

        #region Utility

        public static uint CreateColor(byte alpha, byte red, byte green, byte blue)
        {
            return (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
        }

        public static uint CreateColor(byte red, byte green, byte blue)
        {
            return CreateColor(255, red, green, blue);
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (_disposed) return;

            _renderTarget = null;
            _pixelBuffer = null;
            _noiseBuffer = null;
            IsInitialized = false;
            _disposed = true;
        }

        #endregion
    }

    public enum ColorChannel
    {
        Red,
        Green,
        Blue
    }
}
