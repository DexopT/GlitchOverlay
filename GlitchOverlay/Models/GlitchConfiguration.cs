using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace GlitchOverlay.Models
{
    /// <summary>
    /// Configuration model for glitch effects
    /// </summary>
    public class GlitchConfiguration
    {
        public string Name { get; set; } = "Default";
        public bool IsEnabled { get; set; } = true;
        public double GlobalIntensity { get; set; } = 0.5;
        public double GlobalFrequency { get; set; } = 0.3;
        
        // Effect-specific settings
        public PixelShiftSettings PixelShift { get; set; } = new PixelShiftSettings();
        public ColorInversionSettings ColorInversion { get; set; } = new ColorInversionSettings();
        public ScanlineSettings Scanlines { get; set; } = new ScanlineSettings();
        public DigitalBreakdownSettings DigitalBreakdown { get; set; } = new DigitalBreakdownSettings();
        public PerformanceOverlaySettings PerformanceOverlay { get; set; } = new PerformanceOverlaySettings();
        
        // System settings
        public bool StartWithWindows { get; set; } = false;
        public string GlobalHotkey { get; set; } = "Ctrl+Shift+G";
        public bool MinimizeToTray { get; set; } = true;
        
        public GlitchConfiguration Clone()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<GlitchConfiguration>(json);
        }
    }

    public class PixelShiftSettings
    {
        public bool IsEnabled { get; set; } = true;
        public double Intensity { get; set; } = 0.4;
        public double Frequency { get; set; } = 0.2;
        public int MaxShiftDistance { get; set; } = 5;
        public ShiftDirection Direction { get; set; } = ShiftDirection.Both;
        public int BlockSize { get; set; } = 1;
    }

    public class ColorInversionSettings
    {
        public bool IsEnabled { get; set; } = true;
        public double Intensity { get; set; } = 0.3;
        public double Frequency { get; set; } = 0.1;
        public double Duration { get; set; } = 0.2; // seconds
        public InversionType Type { get; set; } = InversionType.ChannelShift;
        public bool AffectFullScreen { get; set; } = false;
    }

    public class ScanlineSettings
    {
        public bool IsEnabled { get; set; } = true;
        public double Intensity { get; set; } = 0.2;
        public double Frequency { get; set; } = 0.8;
        public int LineSpacing { get; set; } = 4;
        public double FlickerRate { get; set; } = 0.1;
        public ScanlinePattern Pattern { get; set; } = ScanlinePattern.Horizontal;
    }

    public class DigitalBreakdownSettings
    {
        public bool IsEnabled { get; set; } = true;
        public double Intensity { get; set; } = 0.6;
        public double Frequency { get; set; } = 0.05;
        public double Duration { get; set; } = 0.5; // seconds
        public BreakdownType Type { get; set; } = BreakdownType.Mixed;
        public int BlockSize { get; set; } = 8;
    }

    public enum ShiftDirection
    {
        Horizontal,
        Vertical,
        Both
    }

    public enum InversionType
    {
        Full,
        ChannelShift,
        Desaturation,
        Hue
    }

    public enum ScanlinePattern
    {
        Horizontal,
        Vertical,
        Grid
    }

    public enum BreakdownType
    {
        Static,
        Pixelation,
        Tearing,
        Mixed
    }

    public enum OverlayPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public enum OverlayColorScheme
    {
        GreenCyan,
        RedOrange,
        BlueWhite
    }

    public class PerformanceOverlaySettings
    {
        public bool IsEnabled { get; set; } = false;
        public int FontSize { get; set; } = 9;
        public OverlayPosition Position { get; set; } = OverlayPosition.TopLeft;
        public double Opacity { get; set; } = 0.8;
        public OverlayColorScheme ColorScheme { get; set; } = OverlayColorScheme.GreenCyan;
        public bool ShowFPS { get; set; } = true;
        public bool ShowCPU { get; set; } = true;
        public bool ShowMemory { get; set; } = true;
        public bool ShowEffects { get; set; } = true;
    }
}
