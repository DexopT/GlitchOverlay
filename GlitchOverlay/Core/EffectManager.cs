using System;
using System.Collections.Generic;
using System.Diagnostics;
using GlitchOverlay.Effects;
using GlitchOverlay.Models;

namespace GlitchOverlay.Core
{
    /// <summary>
    /// Manages and coordinates all glitch effects
    /// </summary>
    public class EffectManager : IDisposable
    {
        #region Fields

        private readonly List<IEffect> _effects;
        private GlitchConfiguration _currentConfig;
        private Stopwatch _frameTimer;
        private double _lastFrameTime;
        private bool _disposed;

        #endregion

        #region Properties

        public IReadOnlyList<IEffect> Effects => _effects;
        public bool IsInitialized { get; private set; }

        #endregion

        #region Constructor

        public EffectManager()
        {
            _effects = new List<IEffect>();
            _frameTimer = new Stopwatch();
            InitializeEffects();
        }

        #endregion

        #region Initialization

        private void InitializeEffects()
        {
            // Add all available effects
            _effects.Add(new PixelShiftEffect());
            _effects.Add(new ColorInversionEffect());
            _effects.Add(new ScanlineEffect());
            _effects.Add(new DigitalBreakdownEffect());

            IsInitialized = true;
        }

        #endregion

        #region Configuration

        public void UpdateConfiguration(GlitchConfiguration config)
        {
            _currentConfig = config?.Clone();
            
            // Update all effects with the new configuration
            foreach (var effect in _effects)
            {
                try
                {
                    effect.UpdateConfiguration(_currentConfig);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error updating effect {effect.Name}: {ex.Message}");
                }
            }
        }

        #endregion

        #region Effect Processing

        public void ProcessEffects(GraphicsEngine graphics)
        {
            if (!IsInitialized || _currentConfig == null || !_currentConfig.IsEnabled)
                return;

            try
            {
                // Calculate delta time
                var deltaTime = CalculateDeltaTime();
                
                // Apply each enabled effect
                foreach (var effect in _effects)
                {
                    if (effect.IsEnabled)
                    {
                        try
                        {
                            effect.Apply(graphics, deltaTime);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error applying effect {effect.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error processing effects: {ex.Message}");
            }
        }

        private double CalculateDeltaTime()
        {
            if (!_frameTimer.IsRunning)
            {
                _frameTimer.Start();
                _lastFrameTime = 0;
                return 0.016; // Default to ~60 FPS
            }

            var currentTime = _frameTimer.Elapsed.TotalSeconds;
            var deltaTime = currentTime - _lastFrameTime;
            _lastFrameTime = currentTime;

            // Clamp delta time to prevent issues with very large values
            return Math.Min(deltaTime, 0.1);
        }

        #endregion

        #region Effect Control

        public void ResetAllEffects()
        {
            foreach (var effect in _effects)
            {
                try
                {
                    effect.Reset();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error resetting effect {effect.Name}: {ex.Message}");
                }
            }
        }

        public IEffect GetEffect<T>() where T : class, IEffect
        {
            foreach (var effect in _effects)
            {
                if (effect is T)
                    return effect;
            }
            return null;
        }

        public IEffect GetEffectByName(string name)
        {
            foreach (var effect in _effects)
            {
                if (string.Equals(effect.Name, name, StringComparison.OrdinalIgnoreCase))
                    return effect;
            }
            return null;
        }

        #endregion

        #region Statistics

        public EffectStatistics GetStatistics()
        {
            var stats = new EffectStatistics
            {
                TotalEffects = _effects.Count,
                EnabledEffects = 0,
                FrameTime = _lastFrameTime
            };

            foreach (var effect in _effects)
            {
                if (effect.IsEnabled)
                    stats.EnabledEffects++;
            }

            return stats;
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _frameTimer?.Stop();
                _effects?.Clear();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing effect manager: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Statistics about effect processing
    /// </summary>
    public class EffectStatistics
    {
        public int TotalEffects { get; set; }
        public int EnabledEffects { get; set; }
        public double FrameTime { get; set; }
        public double FrameRate => FrameTime > 0 ? 1.0 / FrameTime : 0;
    }
}
