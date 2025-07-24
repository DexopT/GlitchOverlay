using System;
using System.IO;
using System.ComponentModel;
using System.Windows;
using GlitchOverlay.Services;
using GlitchOverlay.Models;

namespace GlitchOverlay
{
    /// <summary>
    /// Main control panel window for the Glitch Overlay application
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigurationService _configService;
        private PerformanceMonitor _performanceMonitor;
        private UI.PerformanceOverlay _performanceOverlay;
        private Core.GraphicsEngine _previewEngine;
        private System.Windows.Threading.DispatcherTimer _previewTimer;
        private bool _isPreviewRunning = false;
        private bool _previewInitializationFailed = false;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        public void Initialize(ConfigurationService configService)
        {
            _configService = configService;
            _configService.LogInfo("MainWindow: Initializing with ConfigurationService");

            InitializeEventHandlers();
            LoadConfiguration();

            // Initialize performance monitor
            InitializePerformanceMonitor();

            // Refresh preset list (built-in presets are now handled by FileConfigurationService)
            RefreshPresetList();

            _configService.LogInfo("MainWindow: Initialization complete");

            // Subscribe to configuration changes
            _configService.ConfigurationChanged += OnConfigurationChanged;
        }

        private void InitializePerformanceMonitor()
        {
            try
            {
                _performanceMonitor = new PerformanceMonitor();
                _performanceMonitor.PerformanceUpdated += OnPerformanceUpdated;
                _performanceMonitor.Start();

                // Initialize performance overlay
                _performanceOverlay = new UI.PerformanceOverlay();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing performance monitor: {ex.Message}");
            }
        }

        private void InitializeEventHandlers()
        {
            try
            {
                // Global control event handlers
                if (MasterEnableCheckBox != null)
                {
                    MasterEnableCheckBox.Checked += OnMasterEnableChanged;
                    MasterEnableCheckBox.Unchecked += OnMasterEnableChanged;
                }

                if (GlobalIntensitySlider != null)
                    GlobalIntensitySlider.ValueChanged += OnGlobalIntensityChanged;

                if (GlobalFrequencySlider != null)
                    GlobalFrequencySlider.ValueChanged += OnGlobalFrequencyChanged;

                // Pixel Shift controls
                InitializePixelShiftHandlers();

                // Color Inversion controls
                InitializeColorInversionHandlers();

                // Scanline controls
                InitializeScanlineHandlers();

                // Digital Breakdown controls
                InitializeBreakdownHandlers();

                // Preview controls
                InitializePreviewHandlers();

                // Preset controls
                InitializePresetHandlers();

                // Button event handlers
                if (MinimizeButton != null)
                    MinimizeButton.Click += OnMinimizeToTray;

                if (ExitButton != null)
                    ExitButton.Click += OnExitApplication;

                // Window event handlers
                this.Closing += OnWindowClosing;
                this.StateChanged += OnWindowStateChanged;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing event handlers: {ex.Message}");
            }
        }

        private void InitializePixelShiftHandlers()
        {
            if (PixelShiftEnableCheckBox != null)
            {
                PixelShiftEnableCheckBox.Checked += OnPixelShiftEnableChanged;
                PixelShiftEnableCheckBox.Unchecked += OnPixelShiftEnableChanged;
            }

            if (PixelShiftIntensitySlider != null)
                PixelShiftIntensitySlider.ValueChanged += OnPixelShiftIntensityChanged;

            if (PixelShiftFrequencySlider != null)
                PixelShiftFrequencySlider.ValueChanged += OnPixelShiftFrequencyChanged;

            if (PixelShiftDistanceSlider != null)
                PixelShiftDistanceSlider.ValueChanged += OnPixelShiftDistanceChanged;

            if (PixelShiftDirectionCombo != null)
                PixelShiftDirectionCombo.SelectionChanged += OnPixelShiftDirectionChanged;
        }

        private void InitializeColorInversionHandlers()
        {
            if (ColorInversionEnableCheckBox != null)
            {
                ColorInversionEnableCheckBox.Checked += OnColorInversionEnableChanged;
                ColorInversionEnableCheckBox.Unchecked += OnColorInversionEnableChanged;
            }

            if (ColorInversionIntensitySlider != null)
                ColorInversionIntensitySlider.ValueChanged += OnColorInversionIntensityChanged;

            if (ColorInversionFrequencySlider != null)
                ColorInversionFrequencySlider.ValueChanged += OnColorInversionFrequencyChanged;

            if (ColorInversionDurationSlider != null)
                ColorInversionDurationSlider.ValueChanged += OnColorInversionDurationChanged;

            if (ColorInversionTypeCombo != null)
                ColorInversionTypeCombo.SelectionChanged += OnColorInversionTypeChanged;

            if (ColorInversionFullScreenCheckBox != null)
            {
                ColorInversionFullScreenCheckBox.Checked += OnColorInversionFullScreenChanged;
                ColorInversionFullScreenCheckBox.Unchecked += OnColorInversionFullScreenChanged;
            }
        }

        private void InitializeScanlineHandlers()
        {
            if (ScanlineEnableCheckBox != null)
            {
                ScanlineEnableCheckBox.Checked += OnScanlineEnableChanged;
                ScanlineEnableCheckBox.Unchecked += OnScanlineEnableChanged;
            }

            if (ScanlineIntensitySlider != null)
                ScanlineIntensitySlider.ValueChanged += OnScanlineIntensityChanged;

            if (ScanlineFrequencySlider != null)
                ScanlineFrequencySlider.ValueChanged += OnScanlineFrequencyChanged;

            if (ScanlineSpacingSlider != null)
                ScanlineSpacingSlider.ValueChanged += OnScanlineSpacingChanged;

            if (ScanlineFlickerSlider != null)
                ScanlineFlickerSlider.ValueChanged += OnScanlineFlickerChanged;

            if (ScanlinePatternCombo != null)
                ScanlinePatternCombo.SelectionChanged += OnScanlinePatternChanged;
        }

        private void InitializeBreakdownHandlers()
        {
            if (BreakdownEnableCheckBox != null)
            {
                BreakdownEnableCheckBox.Checked += OnBreakdownEnableChanged;
                BreakdownEnableCheckBox.Unchecked += OnBreakdownEnableChanged;
            }

            if (BreakdownIntensitySlider != null)
                BreakdownIntensitySlider.ValueChanged += OnBreakdownIntensityChanged;

            if (BreakdownFrequencySlider != null)
                BreakdownFrequencySlider.ValueChanged += OnBreakdownFrequencyChanged;

            if (BreakdownDurationSlider != null)
                BreakdownDurationSlider.ValueChanged += OnBreakdownDurationChanged;

            if (BreakdownBlockSizeSlider != null)
                BreakdownBlockSizeSlider.ValueChanged += OnBreakdownBlockSizeChanged;

            if (BreakdownTypeCombo != null)
                BreakdownTypeCombo.SelectionChanged += OnBreakdownTypeChanged;
        }

        private void InitializePreviewHandlers()
        {
            if (StartPreviewButton != null)
                StartPreviewButton.Click += OnStartPreview;

            if (StopPreviewButton != null)
                StopPreviewButton.Click += OnStopPreview;
        }

        private void InitializePresetHandlers()
        {
            if (NewPresetButton != null)
                NewPresetButton.Click += OnNewPreset;

            if (LoadPresetButton != null)
                LoadPresetButton.Click += OnLoadPreset;

            if (SavePresetButton != null)
                SavePresetButton.Click += OnSavePreset;

            if (DeletePresetButton != null)
                DeletePresetButton.Click += OnDeletePreset;

            // Performance overlay checkbox
            if (ShowPerformanceOverlayCheckBox != null)
            {
                ShowPerformanceOverlayCheckBox.Checked += OnShowPerformanceOverlayChanged;
                ShowPerformanceOverlayCheckBox.Unchecked += OnShowPerformanceOverlayChanged;
            }

            // Initialize overlay settings handlers
            InitializeOverlaySettingsHandlers();
        }

        private void OnShowPerformanceOverlayChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShowPerformanceOverlayCheckBox?.IsChecked == true)
                {
                    _performanceOverlay?.ShowOverlay();
                }
                else
                {
                    _performanceOverlay?.HideOverlay();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling performance overlay: {ex.Message}");
            }
        }

        private void LoadConfiguration()
        {
            if (_configService?.CurrentConfiguration == null) return;

            try
            {
                var config = _configService.CurrentConfiguration;

                // Global settings
                if (MasterEnableCheckBox != null)
                    MasterEnableCheckBox.IsChecked = config.IsEnabled;

                if (GlobalIntensitySlider != null)
                    GlobalIntensitySlider.Value = config.GlobalIntensity;

                if (GlobalFrequencySlider != null)
                    GlobalFrequencySlider.Value = config.GlobalFrequency;

                // Pixel Shift settings
                LoadPixelShiftConfiguration(config.PixelShift);

                // Color Inversion settings
                LoadColorInversionConfiguration(config.ColorInversion);

                // Scanline settings
                LoadScanlineConfiguration(config.Scanlines);

                // Digital Breakdown settings
                LoadBreakdownConfiguration(config.DigitalBreakdown);

                // Load overlay settings
                LoadOverlaySettings();

                UpdateValueDisplays();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading configuration: {ex.Message}");
            }
        }

        private void LoadPixelShiftConfiguration(PixelShiftSettings settings)
        {
            if (settings == null) return;

            if (PixelShiftEnableCheckBox != null)
                PixelShiftEnableCheckBox.IsChecked = settings.IsEnabled;

            if (PixelShiftIntensitySlider != null)
                PixelShiftIntensitySlider.Value = settings.Intensity;

            if (PixelShiftFrequencySlider != null)
                PixelShiftFrequencySlider.Value = settings.Frequency;

            if (PixelShiftDistanceSlider != null)
                PixelShiftDistanceSlider.Value = settings.MaxShiftDistance;

            if (PixelShiftDirectionCombo != null)
                PixelShiftDirectionCombo.SelectedIndex = (int)settings.Direction;
        }

        private void LoadColorInversionConfiguration(ColorInversionSettings settings)
        {
            if (settings == null) return;

            if (ColorInversionEnableCheckBox != null)
                ColorInversionEnableCheckBox.IsChecked = settings.IsEnabled;

            if (ColorInversionIntensitySlider != null)
                ColorInversionIntensitySlider.Value = settings.Intensity;

            if (ColorInversionFrequencySlider != null)
                ColorInversionFrequencySlider.Value = settings.Frequency;

            if (ColorInversionDurationSlider != null)
                ColorInversionDurationSlider.Value = settings.Duration;

            if (ColorInversionTypeCombo != null)
                ColorInversionTypeCombo.SelectedIndex = (int)settings.Type;

            if (ColorInversionFullScreenCheckBox != null)
                ColorInversionFullScreenCheckBox.IsChecked = settings.AffectFullScreen;
        }

        private void LoadScanlineConfiguration(ScanlineSettings settings)
        {
            if (settings == null) return;

            if (ScanlineEnableCheckBox != null)
                ScanlineEnableCheckBox.IsChecked = settings.IsEnabled;

            if (ScanlineIntensitySlider != null)
                ScanlineIntensitySlider.Value = settings.Intensity;

            if (ScanlineFrequencySlider != null)
                ScanlineFrequencySlider.Value = settings.Frequency;

            if (ScanlineSpacingSlider != null)
                ScanlineSpacingSlider.Value = settings.LineSpacing;

            if (ScanlineFlickerSlider != null)
                ScanlineFlickerSlider.Value = settings.FlickerRate;

            if (ScanlinePatternCombo != null)
                ScanlinePatternCombo.SelectedIndex = (int)settings.Pattern;
        }

        private void LoadBreakdownConfiguration(DigitalBreakdownSettings settings)
        {
            if (settings == null) return;

            if (BreakdownEnableCheckBox != null)
                BreakdownEnableCheckBox.IsChecked = settings.IsEnabled;

            if (BreakdownIntensitySlider != null)
                BreakdownIntensitySlider.Value = settings.Intensity;

            if (BreakdownFrequencySlider != null)
                BreakdownFrequencySlider.Value = settings.Frequency;

            if (BreakdownDurationSlider != null)
                BreakdownDurationSlider.Value = settings.Duration;

            if (BreakdownBlockSizeSlider != null)
                BreakdownBlockSizeSlider.Value = settings.BlockSize;

            if (BreakdownTypeCombo != null)
                BreakdownTypeCombo.SelectedIndex = (int)settings.Type;
        }

        private void LoadOverlaySettings()
        {
            try
            {
                if (_configService?.CurrentConfiguration?.PerformanceOverlay == null) return;

                var settings = _configService.CurrentConfiguration.PerformanceOverlay;

                if (ShowPerformanceOverlayCheckBox != null)
                    ShowPerformanceOverlayCheckBox.IsChecked = settings.IsEnabled;

                if (OverlayFontSizeSlider != null)
                    OverlayFontSizeSlider.Value = settings.FontSize;

                if (OverlayPositionCombo != null)
                    OverlayPositionCombo.SelectedIndex = (int)settings.Position;

                if (OverlayOpacitySlider != null)
                    OverlayOpacitySlider.Value = settings.Opacity;

                if (OverlayColorSchemeCombo != null)
                    OverlayColorSchemeCombo.SelectedIndex = (int)settings.ColorScheme;

                if (ShowFPSCheckBox != null)
                    ShowFPSCheckBox.IsChecked = settings.ShowFPS;

                if (ShowCPUCheckBox != null)
                    ShowCPUCheckBox.IsChecked = settings.ShowCPU;

                if (ShowMemoryCheckBox != null)
                    ShowMemoryCheckBox.IsChecked = settings.ShowMemory;

                if (ShowEffectsCheckBox != null)
                    ShowEffectsCheckBox.IsChecked = settings.ShowEffects;

                // Apply settings to overlay
                _performanceOverlay?.ApplySettings(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading overlay settings: {ex.Message}");
            }
        }

        private void UpdateValueDisplays()
        {
            try
            {
                // Global values
                if (GlobalIntensityValue != null && GlobalIntensitySlider != null)
                    GlobalIntensityValue.Text = GlobalIntensitySlider.Value.ToString("F2");

                if (GlobalFrequencyValue != null && GlobalFrequencySlider != null)
                    GlobalFrequencyValue.Text = GlobalFrequencySlider.Value.ToString("F2");

                // Pixel Shift values
                if (PixelShiftIntensityValue != null && PixelShiftIntensitySlider != null)
                    PixelShiftIntensityValue.Text = PixelShiftIntensitySlider.Value.ToString("F2");

                if (PixelShiftFrequencyValue != null && PixelShiftFrequencySlider != null)
                    PixelShiftFrequencyValue.Text = PixelShiftFrequencySlider.Value.ToString("F2");

                if (PixelShiftDistanceValue != null && PixelShiftDistanceSlider != null)
                    PixelShiftDistanceValue.Text = PixelShiftDistanceSlider.Value.ToString("F0");

                // Color Inversion values
                if (ColorInversionIntensityValue != null && ColorInversionIntensitySlider != null)
                    ColorInversionIntensityValue.Text = ColorInversionIntensitySlider.Value.ToString("F2");

                if (ColorInversionFrequencyValue != null && ColorInversionFrequencySlider != null)
                    ColorInversionFrequencyValue.Text = ColorInversionFrequencySlider.Value.ToString("F2");

                if (ColorInversionDurationValue != null && ColorInversionDurationSlider != null)
                    ColorInversionDurationValue.Text = ColorInversionDurationSlider.Value.ToString("F1") + "s";

                // Scanline values
                if (ScanlineIntensityValue != null && ScanlineIntensitySlider != null)
                    ScanlineIntensityValue.Text = ScanlineIntensitySlider.Value.ToString("F2");

                if (ScanlineFrequencyValue != null && ScanlineFrequencySlider != null)
                    ScanlineFrequencyValue.Text = ScanlineFrequencySlider.Value.ToString("F2");

                if (ScanlineSpacingValue != null && ScanlineSpacingSlider != null)
                    ScanlineSpacingValue.Text = ScanlineSpacingSlider.Value.ToString("F0");

                if (ScanlineFlickerValue != null && ScanlineFlickerSlider != null)
                    ScanlineFlickerValue.Text = ScanlineFlickerSlider.Value.ToString("F2");

                // Breakdown values
                if (BreakdownIntensityValue != null && BreakdownIntensitySlider != null)
                    BreakdownIntensityValue.Text = BreakdownIntensitySlider.Value.ToString("F2");

                if (BreakdownFrequencyValue != null && BreakdownFrequencySlider != null)
                    BreakdownFrequencyValue.Text = BreakdownFrequencySlider.Value.ToString("F2");

                if (BreakdownDurationValue != null && BreakdownDurationSlider != null)
                    BreakdownDurationValue.Text = BreakdownDurationSlider.Value.ToString("F1") + "s";

                if (BreakdownBlockSizeValue != null && BreakdownBlockSizeSlider != null)
                    BreakdownBlockSizeValue.Text = BreakdownBlockSizeSlider.Value.ToString("F0");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating value displays: {ex.Message}");
            }
        }

        private void OnConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            Dispatcher.Invoke(() => LoadConfiguration());
        }

        private void OnMasterEnableChanged(object sender, RoutedEventArgs e)
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.IsEnabled = MasterEnableCheckBox.IsChecked ?? false;
            _configService.UpdateConfiguration(config);

            // Note: Live Preview continues to work independently of Master Enable
            // Master Enable only controls the desktop overlay, not the preview window
        }

        private void OnGlobalIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.GlobalIntensity = e.NewValue;

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }

            UpdateValueDisplays();
        }

        private void OnGlobalFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.GlobalFrequency = e.NewValue;

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }

            UpdateValueDisplays();
        }

        private void OnMinimizeToTray(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visibility = Visibility.Hidden;
        }

        private void OnExitApplication(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OnWindowClosing(object sender, CancelEventArgs e)
        {
            // Minimize to tray instead of closing
            e.Cancel = true;
            OnMinimizeToTray(sender, null);
        }

        private void OnWindowStateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Visibility = Visibility.Hidden;
            }
        }

        public void ShowFromTray()
        {
            this.Visibility = Visibility.Visible;
            this.WindowState = WindowState.Normal;
            this.ShowInTaskbar = true;
            this.Activate();
            this.Focus();
        }

        #region Effect Event Handlers

        // Pixel Shift Event Handlers
        private void OnPixelShiftEnableChanged(object sender, RoutedEventArgs e)
        {
            UpdatePixelShiftConfiguration();
        }

        private void OnPixelShiftIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePixelShiftConfiguration();
            UpdateValueDisplays();
        }

        private void OnPixelShiftFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePixelShiftConfiguration();
            UpdateValueDisplays();
        }

        private void OnPixelShiftDistanceChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePixelShiftConfiguration();
            UpdateValueDisplays();
        }

        private void OnPixelShiftDirectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdatePixelShiftConfiguration();
        }

        private void UpdatePixelShiftConfiguration()
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.PixelShift.IsEnabled = PixelShiftEnableCheckBox?.IsChecked ?? false;
            config.PixelShift.Intensity = PixelShiftIntensitySlider?.Value ?? 0.4;
            config.PixelShift.Frequency = PixelShiftFrequencySlider?.Value ?? 0.2;
            config.PixelShift.MaxShiftDistance = (int)(PixelShiftDistanceSlider?.Value ?? 5);
            config.PixelShift.Direction = (ShiftDirection)(PixelShiftDirectionCombo?.SelectedIndex ?? 2);

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                // Master Enable is OFF - update configuration but don't apply effects
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }
        }

        // Color Inversion Event Handlers
        private void OnColorInversionEnableChanged(object sender, RoutedEventArgs e)
        {
            UpdateColorInversionConfiguration();
        }

        private void OnColorInversionIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColorInversionConfiguration();
            UpdateValueDisplays();
        }

        private void OnColorInversionFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColorInversionConfiguration();
            UpdateValueDisplays();
        }

        private void OnColorInversionDurationChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColorInversionConfiguration();
            UpdateValueDisplays();
        }

        private void OnColorInversionTypeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateColorInversionConfiguration();
        }

        private void OnColorInversionFullScreenChanged(object sender, RoutedEventArgs e)
        {
            UpdateColorInversionConfiguration();
        }

        private void UpdateColorInversionConfiguration()
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.ColorInversion.IsEnabled = ColorInversionEnableCheckBox?.IsChecked ?? false;
            config.ColorInversion.Intensity = ColorInversionIntensitySlider?.Value ?? 0.3;
            config.ColorInversion.Frequency = ColorInversionFrequencySlider?.Value ?? 0.1;
            config.ColorInversion.Duration = ColorInversionDurationSlider?.Value ?? 0.2;
            config.ColorInversion.Type = (InversionType)(ColorInversionTypeCombo?.SelectedIndex ?? 1);
            config.ColorInversion.AffectFullScreen = ColorInversionFullScreenCheckBox?.IsChecked ?? false;

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                // Master Enable is OFF - update configuration but don't apply effects
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }
        }

        #endregion

        #region Preview and Preset Handlers

        private void OnStartPreview(object sender, RoutedEventArgs e)
        {
            try
            {
                _configService?.LogDebug("Live Preview: Start button clicked");

                // Reset failure state
                _previewInitializationFailed = false;
                UpdatePreviewButtonStates();

                // Initialize preview engine if needed
                if (_previewEngine == null)
                {
                    try
                    {
                        _configService?.LogInfo("Live Preview: Initializing graphics engine");
                        _previewEngine = new Core.GraphicsEngine();
                        _previewEngine.Initialize(400, 300);

                        if (PreviewImage != null)
                            PreviewImage.Source = _previewEngine.RenderTarget;

                        _configService?.LogInfo("Live Preview: Graphics engine initialized successfully");
                    }
                    catch (Exception initEx)
                    {
                        _previewInitializationFailed = true;
                        UpdatePreviewButtonStates();
                        _configService?.LogError($"Live Preview: Graphics engine initialization failed: {initEx.Message}");
                        MessageBox.Show($"Failed to initialize preview engine: {initEx.Message}",
                            "Preview Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                // Initialize timer if needed
                if (_previewTimer == null)
                {
                    _previewTimer = new System.Windows.Threading.DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(50) // 20 FPS for preview
                    };
                    _previewTimer.Tick += OnPreviewTimerTick;
                    _configService?.LogDebug("Live Preview: Timer initialized");
                }

                // Start preview
                _previewTimer.Start();
                _isPreviewRunning = true;
                UpdatePreviewButtonStates();

                _configService?.LogInfo("Live Preview: Started successfully");
            }
            catch (Exception ex)
            {
                _previewInitializationFailed = true;
                _isPreviewRunning = false;
                UpdatePreviewButtonStates();

                _configService?.LogError($"Live Preview: Start failed: {ex.Message}");
                MessageBox.Show($"Error starting preview: {ex.Message}", "Preview Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnStopPreview(object sender, RoutedEventArgs e)
        {
            try
            {
                _previewTimer?.Stop();
                _isPreviewRunning = false;
                UpdatePreviewButtonStates();

                // Clear preview
                if (PreviewImage != null)
                    PreviewImage.Source = null;

                System.Diagnostics.Debug.WriteLine("Live Preview stopped successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error stopping preview: {ex.Message}");
            }
        }

        private void UpdatePreviewButtonStates()
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    // Debug: Check if buttons exist
                    System.Diagnostics.Debug.WriteLine($"UpdatePreviewButtonStates: StartButton={StartPreviewButton != null}, StopButton={StopPreviewButton != null}, Running={_isPreviewRunning}, Failed={_previewInitializationFailed}");

                    if (StartPreviewButton != null && StopPreviewButton != null)
                    {
                        if (_previewInitializationFailed)
                        {
                            StartPreviewButton.IsEnabled = true;
                            StartPreviewButton.ToolTip = "Preview initialization failed. Click to retry.";
                            StopPreviewButton.IsEnabled = false;
                            System.Diagnostics.Debug.WriteLine("Preview buttons: Start=ENABLED (retry), Stop=DISABLED");
                        }
                        else if (_isPreviewRunning)
                        {
                            StartPreviewButton.IsEnabled = false;
                            StartPreviewButton.ToolTip = "Preview is currently running";
                            StopPreviewButton.IsEnabled = true;
                            System.Diagnostics.Debug.WriteLine("Preview buttons: Start=DISABLED (running), Stop=ENABLED");
                        }
                        else
                        {
                            StartPreviewButton.IsEnabled = true;
                            StartPreviewButton.ToolTip = "Start live preview of effects";
                            StopPreviewButton.IsEnabled = false;
                            System.Diagnostics.Debug.WriteLine("Preview buttons: Start=ENABLED (ready), Stop=DISABLED");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Preview buttons are null!");
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview button states: {ex.Message}");
            }
        }

        private void OnPreviewTimerTick(object sender, EventArgs e)
        {
            try
            {
                if (_previewEngine == null || _configService?.CurrentConfiguration == null) return;

                _previewEngine.BeginFrame();

                var config = _configService.CurrentConfiguration;

                // Live Preview works independently of Master Enable
                // It shows what the effects WOULD look like based on current settings

                // Apply a simplified version of effects for preview
                if (config.PixelShift.IsEnabled)
                {
                    var distance = (int)(config.PixelShift.Intensity * config.PixelShift.MaxShiftDistance);
                    if (distance > 0)
                        _previewEngine.ApplyPixelShift(distance, config.PixelShift.Direction);
                }

                if (config.ColorInversion.IsEnabled)
                {
                    var offsetX = (int)(config.ColorInversion.Intensity * 10);
                    _previewEngine.ApplyColorShift(offsetX, 0, Core.ColorChannel.Red);
                    _previewEngine.ApplyColorShift(-offsetX, 0, Core.ColorChannel.Blue);
                }

                if (config.Scanlines.IsEnabled)
                {
                    var alpha = (byte)(config.Scanlines.Intensity * 150);
                    var color = Core.GraphicsEngine.CreateColor(alpha, 0, 255, 0);
                    for (int y = 0; y < _previewEngine.Height; y += config.Scanlines.LineSpacing)
                    {
                        _previewEngine.DrawLine(0, y, _previewEngine.Width - 1, y, color);
                    }
                }

                if (config.DigitalBreakdown.IsEnabled)
                {
                    var alpha = (byte)(config.DigitalBreakdown.Intensity * 100);
                    var random = new Random();
                    for (int i = 0; i < 10; i++)
                    {
                        var x = random.Next(_previewEngine.Width - 20);
                        var y = random.Next(_previewEngine.Height - 20);
                        var color = Core.GraphicsEngine.CreateColor(alpha,
                            (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256));
                        _previewEngine.FillRect(x, y, 10, 10, color);
                    }
                }

                // Add some noise
                if (config.GlobalIntensity > 0.1)
                {
                    var noiseIntensity = (byte)(config.GlobalIntensity * 100);
                    _previewEngine.GenerateNoise(noiseIntensity);
                }

                _previewEngine.EndFrame();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating preview: {ex.Message}");
            }
        }

        private void OnNewPreset(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new PresetNameDialog();
                if (dialog.ShowDialog() == true)
                {
                    var presetName = dialog.PresetName;
                    if (!string.IsNullOrWhiteSpace(presetName))
                    {
                        _configService.SavePreset(presetName, _configService.CurrentConfiguration);
                        RefreshPresetList();
                        MessageBox.Show($"Preset '{presetName}' saved successfully!", "Preset Saved",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating preset: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnLoadPreset(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = PresetListBox?.SelectedItem as System.Windows.Controls.ListBoxItem;
                if (selectedItem != null)
                {
                    var presetName = selectedItem.Content.ToString();
                    if (presetName != "Default Settings")
                    {
                        var config = _configService.LoadPreset(presetName);

                        // Check Master Enable state
                        var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;

                        if (masterEnabled)
                        {
                            // Master Enable is ON - load preset and apply effects
                            _configService.UpdateConfiguration(config);
                        }
                        else
                        {
                            // Master Enable is OFF - load preset but don't apply effects
                            // Override the preset's IsEnabled to match Master Enable state
                            config.IsEnabled = false;
                            _configService.UpdateConfigurationSilently(config);

                            // Manually update UI since we used silent update
                            LoadConfiguration();
                        }

                        MessageBox.Show($"Preset '{presetName}' loaded successfully!", "Preset Loaded",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a preset to load.", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading preset: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnSavePreset(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = PresetListBox?.SelectedItem as System.Windows.Controls.ListBoxItem;
                if (selectedItem != null)
                {
                    var presetName = selectedItem.Content.ToString();
                    if (presetName != "Default Settings")
                    {
                        var result = MessageBox.Show($"Overwrite preset '{presetName}'?", "Confirm Overwrite",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            _configService.SavePreset(presetName, _configService.CurrentConfiguration);
                            MessageBox.Show($"Preset '{presetName}' updated successfully!", "Preset Updated",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        OnNewPreset(sender, e); // Create new preset instead
                    }
                }
                else
                {
                    OnNewPreset(sender, e); // Create new preset if none selected
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving preset: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnDeletePreset(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedItem = PresetListBox?.SelectedItem as System.Windows.Controls.ListBoxItem;
                if (selectedItem != null)
                {
                    var presetName = selectedItem.Content.ToString();
                    if (presetName != "Default Settings")
                    {
                        var result = MessageBox.Show($"Delete preset '{presetName}'?", "Confirm Delete",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            _configService.DeletePreset(presetName);
                            RefreshPresetList();
                            MessageBox.Show($"Preset '{presetName}' deleted successfully!", "Preset Deleted",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Cannot delete the default settings.", "Cannot Delete",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a preset to delete.", "No Selection",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting preset: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RefreshPresetList()
        {
            try
            {
                if (PresetListBox != null)
                {
                    PresetListBox.Items.Clear();
                    PresetListBox.Items.Add(new System.Windows.Controls.ListBoxItem { Content = "Default Settings" });

                    var presets = _configService.GetAvailablePresets();
                    foreach (var preset in presets)
                    {
                        PresetListBox.Items.Add(new System.Windows.Controls.ListBoxItem { Content = preset });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing preset list: {ex.Message}");
            }
        }

        #endregion

        #region Scanline and Breakdown Handlers

        private void OnScanlineEnableChanged(object sender, RoutedEventArgs e)
        {
            UpdateScanlineConfiguration();
        }

        private void OnScanlineIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScanlineConfiguration();
            UpdateValueDisplays();
        }

        private void OnScanlineFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScanlineConfiguration();
            UpdateValueDisplays();
        }

        private void OnScanlineSpacingChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScanlineConfiguration();
            UpdateValueDisplays();
        }

        private void OnScanlineFlickerChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateScanlineConfiguration();
            UpdateValueDisplays();
        }

        private void OnScanlinePatternChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateScanlineConfiguration();
        }

        private void UpdateScanlineConfiguration()
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.Scanlines.IsEnabled = ScanlineEnableCheckBox?.IsChecked ?? false;
            config.Scanlines.Intensity = ScanlineIntensitySlider?.Value ?? 0.2;
            config.Scanlines.Frequency = ScanlineFrequencySlider?.Value ?? 0.8;
            config.Scanlines.LineSpacing = (int)(ScanlineSpacingSlider?.Value ?? 4);
            config.Scanlines.FlickerRate = ScanlineFlickerSlider?.Value ?? 0.1;
            config.Scanlines.Pattern = (ScanlinePattern)(ScanlinePatternCombo?.SelectedIndex ?? 0);

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                // Master Enable is OFF - update configuration but don't apply effects
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }
        }

        private void OnBreakdownEnableChanged(object sender, RoutedEventArgs e)
        {
            UpdateBreakdownConfiguration();
        }

        private void OnBreakdownIntensityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBreakdownConfiguration();
            UpdateValueDisplays();
        }

        private void OnBreakdownFrequencyChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBreakdownConfiguration();
            UpdateValueDisplays();
        }

        private void OnBreakdownDurationChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBreakdownConfiguration();
            UpdateValueDisplays();
        }

        private void OnBreakdownBlockSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateBreakdownConfiguration();
            UpdateValueDisplays();
        }

        private void OnBreakdownTypeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateBreakdownConfiguration();
        }

        private void UpdateBreakdownConfiguration()
        {
            if (_configService?.CurrentConfiguration == null) return;

            var config = _configService.CurrentConfiguration.Clone();
            config.DigitalBreakdown.IsEnabled = BreakdownEnableCheckBox?.IsChecked ?? false;
            config.DigitalBreakdown.Intensity = BreakdownIntensitySlider?.Value ?? 0.6;
            config.DigitalBreakdown.Frequency = BreakdownFrequencySlider?.Value ?? 0.05;
            config.DigitalBreakdown.Duration = BreakdownDurationSlider?.Value ?? 0.5;
            config.DigitalBreakdown.BlockSize = (int)(BreakdownBlockSizeSlider?.Value ?? 8);
            config.DigitalBreakdown.Type = (BreakdownType)(BreakdownTypeCombo?.SelectedIndex ?? 3);

            // Respect Master Enable state
            var masterEnabled = MasterEnableCheckBox?.IsChecked ?? false;
            if (masterEnabled)
            {
                _configService.UpdateConfiguration(config);
            }
            else
            {
                // Master Enable is OFF - update configuration but don't apply effects
                config.IsEnabled = false;
                _configService.UpdateConfiguration(config, suppressEffectExecution: true);
            }
        }

        #endregion

        #region Performance Monitoring

        private void OnPerformanceUpdated(object sender, PerformanceData data)
        {
            try
            {
                // Update performance display on UI thread
                Dispatcher.Invoke(() =>
                {
                    if (FrameRateText != null)
                        FrameRateText.Text = data.FrameRateText;

                    if (CpuUsageText != null)
                        CpuUsageText.Text = data.CpuUsageText;

                    if (MemoryUsageText != null)
                        MemoryUsageText.Text = data.MemoryUsageText;

                    if (ActiveEffectsText != null)
                        ActiveEffectsText.Text = data.ActiveEffectsText;
                });

                // Update desktop performance overlay
                _performanceOverlay?.UpdatePerformance(data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating performance display: {ex.Message}");
            }
        }

        public void RecordFrame()
        {
            _performanceMonitor?.RecordFrame();
        }

        #endregion

        #region Overlay Settings

        private void InitializeOverlaySettingsHandlers()
        {
            try
            {
                if (OverlayFontSizeSlider != null)
                    OverlayFontSizeSlider.ValueChanged += OnOverlayFontSizeChanged;

                if (OverlayPositionCombo != null)
                    OverlayPositionCombo.SelectionChanged += OnOverlayPositionChanged;

                if (OverlayOpacitySlider != null)
                    OverlayOpacitySlider.ValueChanged += OnOverlayOpacityChanged;

                if (OverlayColorSchemeCombo != null)
                    OverlayColorSchemeCombo.SelectionChanged += OnOverlayColorSchemeChanged;

                if (ShowFPSCheckBox != null)
                    ShowFPSCheckBox.Click += OnOverlayMetricToggled;

                if (ShowCPUCheckBox != null)
                    ShowCPUCheckBox.Click += OnOverlayMetricToggled;

                if (ShowMemoryCheckBox != null)
                    ShowMemoryCheckBox.Click += OnOverlayMetricToggled;

                if (ShowEffectsCheckBox != null)
                    ShowEffectsCheckBox.Click += OnOverlayMetricToggled;

                if (ResetOverlaySettingsButton != null)
                    ResetOverlaySettingsButton.Click += OnResetOverlaySettings;

                // Configuration file buttons
                if (OpenConfigFolderButton != null)
                    OpenConfigFolderButton.Click += OnOpenConfigFolder;

                if (OpenLogFileButton != null)
                    OpenLogFileButton.Click += OnOpenLogFile;

                // Initialize preview button handlers
                InitializePreviewButtonHandlers();

                // Initialize button states
                UpdatePreviewButtonStates();

                // Update file path displays
                UpdateFilePathDisplays();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing overlay settings handlers: {ex.Message}");
            }
        }

        private void OnOverlayFontSizeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (OverlayFontSizeText != null)
                    OverlayFontSizeText.Text = $"{e.NewValue:F0}px";

                UpdateOverlaySettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating overlay font size: {ex.Message}");
            }
        }

        private void OnOverlayPositionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateOverlaySettings();
        }

        private void OnOverlayOpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                if (OverlayOpacityText != null)
                    OverlayOpacityText.Text = $"{e.NewValue * 100:F0}%";

                UpdateOverlaySettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating overlay opacity: {ex.Message}");
            }
        }

        private void OnOverlayColorSchemeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            UpdateOverlaySettings();
        }

        private void OnOverlayMetricToggled(object sender, RoutedEventArgs e)
        {
            UpdateOverlaySettings();
        }

        private void OnResetOverlaySettings(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset to defaults
                if (OverlayFontSizeSlider != null)
                    OverlayFontSizeSlider.Value = 9;

                if (OverlayPositionCombo != null)
                    OverlayPositionCombo.SelectedIndex = 0; // Top Left

                if (OverlayOpacitySlider != null)
                    OverlayOpacitySlider.Value = 0.8;

                if (OverlayColorSchemeCombo != null)
                    OverlayColorSchemeCombo.SelectedIndex = 0; // Green/Cyan

                if (ShowFPSCheckBox != null)
                    ShowFPSCheckBox.IsChecked = true;

                if (ShowCPUCheckBox != null)
                    ShowCPUCheckBox.IsChecked = true;

                if (ShowMemoryCheckBox != null)
                    ShowMemoryCheckBox.IsChecked = true;

                if (ShowEffectsCheckBox != null)
                    ShowEffectsCheckBox.IsChecked = true;

                UpdateOverlaySettings();

                MessageBox.Show("Overlay settings reset to defaults!", "Settings Reset",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error resetting overlay settings: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateOverlaySettings()
        {
            try
            {
                if (_configService?.CurrentConfiguration == null) return;

                var config = _configService.CurrentConfiguration.Clone();

                // Update overlay settings
                config.PerformanceOverlay.IsEnabled = ShowPerformanceOverlayCheckBox?.IsChecked ?? false;
                config.PerformanceOverlay.FontSize = (int)(OverlayFontSizeSlider?.Value ?? 9);
                config.PerformanceOverlay.Position = (OverlayPosition)(OverlayPositionCombo?.SelectedIndex ?? 0);
                config.PerformanceOverlay.Opacity = OverlayOpacitySlider?.Value ?? 0.8;
                config.PerformanceOverlay.ColorScheme = (OverlayColorScheme)(OverlayColorSchemeCombo?.SelectedIndex ?? 0);
                config.PerformanceOverlay.ShowFPS = ShowFPSCheckBox?.IsChecked ?? true;
                config.PerformanceOverlay.ShowCPU = ShowCPUCheckBox?.IsChecked ?? true;
                config.PerformanceOverlay.ShowMemory = ShowMemoryCheckBox?.IsChecked ?? true;
                config.PerformanceOverlay.ShowEffects = ShowEffectsCheckBox?.IsChecked ?? true;

                _configService.UpdateConfigurationSilently(config);

                // Apply settings to overlay
                _performanceOverlay?.ApplySettings(config.PerformanceOverlay);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating overlay settings: {ex.Message}");
            }
        }

        private void InitializePreviewButtonHandlers()
        {
            try
            {
                if (StartPreviewButton != null)
                {
                    StartPreviewButton.Click -= OnStartPreview; // Remove existing handler
                    StartPreviewButton.Click += OnStartPreview; // Add handler
                    System.Diagnostics.Debug.WriteLine("StartPreviewButton handler initialized");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: StartPreviewButton is null during initialization");
                }

                if (StopPreviewButton != null)
                {
                    StopPreviewButton.Click -= OnStopPreview; // Remove existing handler
                    StopPreviewButton.Click += OnStopPreview; // Add handler
                    System.Diagnostics.Debug.WriteLine("StopPreviewButton handler initialized");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: StopPreviewButton is null during initialization");
                }

                if (RefreshPreviewButton != null)
                {
                    RefreshPreviewButton.Click -= OnRefreshPreview; // Remove existing handler
                    RefreshPreviewButton.Click += OnRefreshPreview; // Add handler
                    System.Diagnostics.Debug.WriteLine("RefreshPreviewButton handler initialized");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: RefreshPreviewButton is null during initialization");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing preview button handlers: {ex.Message}");
            }
        }

        public void ForceRefreshPreviewButtons()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Force refreshing preview button states...");
                _isPreviewRunning = false;
                _previewInitializationFailed = false;
                UpdatePreviewButtonStates();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error force refreshing preview buttons: {ex.Message}");
            }
        }

        private void OnRefreshPreview(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("Manual refresh button clicked");
                ForceRefreshPreviewButtons();
                MessageBox.Show("Preview button states refreshed!", "Debug",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error refreshing preview buttons: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateFilePathDisplays()
        {
            try
            {
                if (_configService != null)
                {
                    if (ConfigDirectoryText != null)
                        ConfigDirectoryText.Text = _configService.GetConfigurationDirectory();
                }
            }
            catch (Exception ex)
            {
                _configService?.LogError($"Error updating file path displays: {ex.Message}");
            }
        }

        private void OnOpenConfigFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configService != null)
                {
                    var configDir = _configService.GetConfigurationDirectory();
                    if (Directory.Exists(configDir))
                    {
                        System.Diagnostics.Process.Start("explorer.exe", configDir);
                        _configService.LogInfo("Opened configuration folder");
                    }
                    else
                    {
                        MessageBox.Show("Configuration directory not found.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        _configService.LogWarning("Configuration directory not found when trying to open");
                    }
                }
            }
            catch (Exception ex)
            {
                _configService?.LogError($"Error opening configuration folder: {ex.Message}");
                MessageBox.Show($"Error opening configuration folder: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnOpenLogFile(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configService != null)
                {
                    var logFile = _configService.GetLogFilePath();
                    if (File.Exists(logFile))
                    {
                        System.Diagnostics.Process.Start("notepad.exe", logFile);
                        _configService.LogInfo("Opened debug log file");
                    }
                    else
                    {
                        MessageBox.Show("Debug log file not found.", "Error",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        _configService.LogWarning("Debug log file not found when trying to open");
                    }
                }
            }
            catch (Exception ex)
            {
                _configService?.LogError($"Error opening log file: {ex.Message}");
                MessageBox.Show($"Error opening log file: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion
    }
}
