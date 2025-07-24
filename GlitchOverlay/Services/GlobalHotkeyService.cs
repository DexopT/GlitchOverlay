using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace GlitchOverlay.Services
{
    /// <summary>
    /// Service for managing global system hotkeys
    /// </summary>
    public class GlobalHotkeyService : IDisposable
    {
        #region Windows API

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_HOTKEY = 0x0312;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_SHIFT = 0x0004;
        private const uint MOD_WIN = 0x0008;

        #endregion

        #region Fields

        private readonly Window _window;
        private HwndSource _source;
        private bool _disposed;

        // Hotkey IDs
        private const int TOGGLE_EFFECTS_ID = 1;
        private const int SHOW_PANEL_ID = 2;

        #endregion

        #region Events

        public event EventHandler ToggleEffectsRequested;
        public event EventHandler ShowPanelRequested;

        #endregion

        #region Constructor

        public GlobalHotkeyService(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));
        }

        #endregion

        #region Public Methods

        public void Initialize()
        {
            try
            {
                if (_window.IsLoaded)
                {
                    SetupHotkeys();
                }
                else
                {
                    _window.Loaded += OnWindowLoaded;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing global hotkeys: {ex.Message}");
            }
        }

        public bool RegisterToggleEffectsHotkey(ModifierKeys modifiers, Key key)
        {
            try
            {
                var hwnd = new WindowInteropHelper(_window).Handle;
                if (hwnd == IntPtr.Zero) return false;

                // Unregister existing hotkey
                UnregisterHotKey(hwnd, TOGGLE_EFFECTS_ID);

                // Register new hotkey
                var modifierFlags = ConvertModifiers(modifiers);
                var virtualKey = KeyInterop.VirtualKeyFromKey(key);
                
                return RegisterHotKey(hwnd, TOGGLE_EFFECTS_ID, modifierFlags, (uint)virtualKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering toggle effects hotkey: {ex.Message}");
                return false;
            }
        }

        public bool RegisterShowPanelHotkey(ModifierKeys modifiers, Key key)
        {
            try
            {
                var hwnd = new WindowInteropHelper(_window).Handle;
                if (hwnd == IntPtr.Zero) return false;

                // Unregister existing hotkey
                UnregisterHotKey(hwnd, SHOW_PANEL_ID);

                // Register new hotkey
                var modifierFlags = ConvertModifiers(modifiers);
                var virtualKey = KeyInterop.VirtualKeyFromKey(key);
                
                return RegisterHotKey(hwnd, SHOW_PANEL_ID, modifierFlags, (uint)virtualKey);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error registering show panel hotkey: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region Private Methods

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _window.Loaded -= OnWindowLoaded;
            SetupHotkeys();
        }

        private void SetupHotkeys()
        {
            try
            {
                var hwnd = new WindowInteropHelper(_window).Handle;
                _source = HwndSource.FromHwnd(hwnd);
                _source.AddHook(WndProc);

                // Register default hotkeys
                RegisterToggleEffectsHotkey(ModifierKeys.Control | ModifierKeys.Shift, Key.G);
                RegisterShowPanelHotkey(ModifierKeys.Control | ModifierKeys.Shift, Key.P);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting up hotkeys: {ex.Message}");
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY)
            {
                try
                {
                    var hotkeyId = wParam.ToInt32();
                    
                    switch (hotkeyId)
                    {
                        case TOGGLE_EFFECTS_ID:
                            ToggleEffectsRequested?.Invoke(this, EventArgs.Empty);
                            handled = true;
                            break;
                        case SHOW_PANEL_ID:
                            ShowPanelRequested?.Invoke(this, EventArgs.Empty);
                            handled = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error handling hotkey: {ex.Message}");
                }
            }

            return IntPtr.Zero;
        }

        private uint ConvertModifiers(ModifierKeys modifiers)
        {
            uint result = 0;
            
            if (modifiers.HasFlag(ModifierKeys.Alt))
                result |= MOD_ALT;
            
            if (modifiers.HasFlag(ModifierKeys.Control))
                result |= MOD_CONTROL;
            
            if (modifiers.HasFlag(ModifierKeys.Shift))
                result |= MOD_SHIFT;
            
            if (modifiers.HasFlag(ModifierKeys.Windows))
                result |= MOD_WIN;
            
            return result;
        }

        #endregion

        #region Disposal

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                if (_source != null)
                {
                    var hwnd = new WindowInteropHelper(_window).Handle;
                    if (hwnd != IntPtr.Zero)
                    {
                        UnregisterHotKey(hwnd, TOGGLE_EFFECTS_ID);
                        UnregisterHotKey(hwnd, SHOW_PANEL_ID);
                    }
                    
                    _source.RemoveHook(WndProc);
                    _source = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error disposing global hotkey service: {ex.Message}");
            }
            finally
            {
                _disposed = true;
            }
        }

        #endregion
    }
}
