# 🎮 Glitch Overlay

**Professional Desktop Visual Effects System for Windows**

A powerful, real-time visual effects application that applies customizable glitch effects to your entire desktop. Perfect for content creators, streamers, developers, and anyone looking to add a cyberpunk aesthetic to their workflow.

![Glitch Overlay](https://img.shields.io/badge/Version-1.0-red?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-blue?style=for-the-badge)
![Framework](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

## ✨ Features

### 🎯 **Core Visual Effects**
- **Pixel Shift**: Digital displacement artifacts with customizable intensity and direction
- **Color Inversion**: Random color corruption effects with multiple inversion types
- **Scanlines**: Retro CRT monitor simulation with adjustable spacing and flicker
- **Digital Breakdown**: Severe corruption effects with pixelation and data loss simulation

### 🎬 **Live Preview System**
- Real-time effect preview at 20 FPS
- Independent operation from main effects
- Robust error handling and recovery
- Visual feedback and troubleshooting tools

### 📊 **Performance Overlay**
- Customizable desktop performance metrics
- Adjustable font size (8-16px)
- Multiple positioning options (4 corners)
- Opacity control (50%-100%)
- 3 professional color schemes
- Individual metric toggles (FPS, CPU, Memory, Effects)

### 💾 **Presets System**
- Save and load custom effect configurations
- 5 built-in professional presets
- JSON-based storage with automatic backup
- Easy preset management and sharing

### ⚙️ **Advanced Configuration**
- JSON-based settings with automatic persistence
- Comprehensive debug logging with rotation
- Global hotkey support (Ctrl+Shift+G)
- System tray integration
- Startup options and auto-launch

## 🚀 Quick Start

### System Requirements
- **OS**: Windows 10/11 (64-bit)
- **Runtime**: .NET 8.0 (auto-installed if missing)
- **Graphics**: DirectX 11 compatible card
- **Memory**: 4GB RAM minimum, 8GB recommended

### Installation
1. Download the latest release from the [Releases](../../releases) page
2. Extract to your preferred directory
3. Run `GlitchOverlay.exe` as Administrator (recommended)
4. Access the control panel via the system tray icon

### First Launch
1. Enable **Master Enable** to activate effects
2. Start with low intensity values (0.1-0.3)
3. Use **Live Preview** to test effects safely
4. Save your favorite settings as presets

## 🎛️ Usage Guide

### Basic Controls
- **Master Enable**: Global on/off switch for all effects
- **Global Intensity**: Overall strength of effects (0.0-1.0)
- **Global Frequency**: How often effects trigger (0.0-1.0)

### Effect Configuration
Each effect has individual controls:
- **Intensity**: Effect strength
- **Frequency**: Trigger rate
- **Duration**: How long effects last
- **Type/Pattern**: Effect variation options

### Hotkeys
- `Ctrl+Shift+G`: Toggle all effects on/off
- `Ctrl+Shift+P`: Show/hide control panel

## 🏗️ Technical Details

### Built With
- **.NET 8.0**: Modern cross-platform framework
- **WPF**: Rich desktop application framework
- **DirectX Graphics**: Hardware-accelerated rendering
- **JSON.NET**: Configuration and preset management

### Architecture
```
GlitchOverlay/
├── Core/              # Graphics engine and rendering
├── Effects/           # Individual effect implementations
├── Models/            # Data models and configuration
├── Services/          # Configuration and performance monitoring
├── UI/                # User interface components
└── Resources/         # Application assets
```

### Configuration Files
The application creates configuration files in `%AppData%\GlitchOverlay\`:
- **settings.json**: Application settings and effect parameters
- **presets.json**: Saved effect presets with metadata
- **debug.log**: Comprehensive logging with automatic rotation

## 📁 Project Structure

```
GlitchOverlay/
├── App.xaml                    # Application entry point
├── MainWindow.xaml             # Main UI window
├── GlitchOverlay.csproj        # Project configuration
├── favicon.ico                 # Application icon
├── Core/
│   └── GraphicsEngine.cs       # DirectX rendering engine
├── Effects/
│   ├── PixelShiftEffect.cs     # Pixel displacement
│   ├── ColorInversionEffect.cs # Color corruption
│   ├── ScanlinesEffect.cs      # CRT simulation
│   └── DigitalBreakdownEffect.cs # Data corruption
├── Models/
│   └── GlitchConfiguration.cs  # Configuration models
├── Services/
│   ├── ConfigurationService.cs # Settings management
│   ├── FileConfigurationService.cs # JSON file handling
│   └── PerformanceMonitor.cs   # Performance tracking
└── UI/
    └── PerformanceOverlay.xaml # Desktop overlay
```

## 🤝 Contributing

We welcome contributions! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Setup
1. Clone the repository
2. Open `GlitchOverlay.sln` in Visual Studio 2022
3. Restore NuGet packages
4. Build and run in Debug mode

### Code Style
- Follow C# naming conventions
- Use XML documentation for public APIs
- Maintain consistent indentation (4 spaces)
- Add unit tests for new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👨‍💻 Author

**DexopT**
- Professional software developer
- Specializing in desktop applications and visual effects
- Passionate about cyberpunk aesthetics and real-time graphics

## 🙏 Acknowledgments

- Built with modern .NET 8.0 and WPF technologies
- Inspired by cyberpunk and glitch art aesthetics
- Thanks to the open-source community for tools and libraries

## 📞 Support

- **Issues**: Report bugs via [GitHub Issues](../../issues)
- **Discussions**: Join conversations in [GitHub Discussions](../../discussions)
- **Documentation**: Check the built-in Help system in the application

---

**Made with ❤️ by DexopT** | **© 2025 All Rights Reserved**
