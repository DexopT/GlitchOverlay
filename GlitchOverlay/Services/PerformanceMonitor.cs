using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace GlitchOverlay.Services
{
    /// <summary>
    /// Monitors application performance metrics
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        #region Fields

        private readonly DispatcherTimer _updateTimer;
        private readonly PerformanceCounter _cpuCounter;
        private readonly Process _currentProcess;
        private readonly Stopwatch _frameStopwatch;
        
        private double _frameRate;
        private double _cpuUsage;
        private long _memoryUsage;
        private int _frameCount;
        private double _lastFrameTime;
        private bool _disposed;

        #endregion

        #region Properties

        public double FrameRate => _frameRate;
        public double CpuUsage => _cpuUsage;
        public long MemoryUsageMB => _memoryUsage / (1024 * 1024);
        public int ActiveEffects { get; set; }
        public int TotalEffects { get; set; }

        #endregion

        #region Events

        public event EventHandler<PerformanceData> PerformanceUpdated;

        #endregion

        #region Constructor

        public PerformanceMonitor()
        {
            try
            {
                _currentProcess = Process.GetCurrentProcess();
                _frameStopwatch = Stopwatch.StartNew();
                
                // Initialize CPU counter
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue(); // First call returns 0, so we call it once
                
                // Setup update timer
                _updateTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(500) // Update every 500ms
                };
                _updateTimer.Tick += OnUpdateTick;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing performance monitor: {ex.Message}");
            }
        }

        #endregion

        #region Public Methods

        public void Start()
        {
            try
            {
                _updateTimer?.Start();
                _frameStopwatch?.Restart();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error starting performance monitor: {ex.Message}");
            }
        }

        public void Stop()
        {
            try
            {
                _updateTimer?.Stop();
                _frameStopwatch?.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping performance monitor: {ex.Message}");
            }
        }

        public void RecordFrame()
        {
            try
            {
                _frameCount++;
                var currentTime = _frameStopwatch.Elapsed.TotalSeconds;
                
                // Calculate frame rate every second
                if (currentTime - _lastFrameTime >= 1.0)
                {
                    _frameRate = _frameCount / (currentTime - _lastFrameTime);
                    _frameCount = 0;
                    _lastFrameTime = currentTime;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error recording frame: {ex.Message}");
            }
        }

        #endregion

        #region Private Methods

        private void OnUpdateTick(object sender, EventArgs e)
        {
            try
            {
                UpdateMetrics();
                
                var data = new PerformanceData
                {
                    FrameRate = _frameRate,
                    CpuUsage = _cpuUsage,
                    MemoryUsageMB = MemoryUsageMB,
                    ActiveEffects = ActiveEffects,
                    TotalEffects = TotalEffects
                };
                
                PerformanceUpdated?.Invoke(this, data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating performance metrics: {ex.Message}");
            }
        }

        private void UpdateMetrics()
        {
            try
            {
                // Update CPU usage
                _cpuUsage = _cpuCounter?.NextValue() ?? 0;
                
                // Update memory usage
                _currentProcess?.Refresh();
                _memoryUsage = _currentProcess?.WorkingSet64 ?? 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating metrics: {ex.Message}");
            }
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                Stop();
                _updateTimer?.Stop();
                _cpuCounter?.Dispose();
                _currentProcess?.Dispose();
                _frameStopwatch?.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing performance monitor: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion
    }

    /// <summary>
    /// Performance data container
    /// </summary>
    public class PerformanceData
    {
        public double FrameRate { get; set; }
        public double CpuUsage { get; set; }
        public long MemoryUsageMB { get; set; }
        public int ActiveEffects { get; set; }
        public int TotalEffects { get; set; }
        
        public string FrameRateText => $"{FrameRate:F1} FPS";
        public string CpuUsageText => $"{CpuUsage:F1}%";
        public string MemoryUsageText => $"{MemoryUsageMB:F1} MB";
        public string ActiveEffectsText => $"{ActiveEffects} / {TotalEffects}";
    }
}
