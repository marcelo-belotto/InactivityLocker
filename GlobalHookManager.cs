using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace InactivityLocker
{
    /// <summary>
    /// Gerencia os hooks globais de mouse e teclado via WinAPI.
    /// Dispara o evento ActivityDetected sempre que há interação do usuário.
    /// </summary>
    public class GlobalHookManager : IDisposable
    {
        // ── WinAPI ──────────────────────────────────────────────────────────
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL    = 14;
        private const int WM_KEYDOWN     = 0x0100;
        private const int WM_SYSKEYDOWN  = 0x0104;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private delegate IntPtr LowLevelProc(int nCode, IntPtr wParam, IntPtr lParam);

        // ── Campos ──────────────────────────────────────────────────────────
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private IntPtr _mouseHookId    = IntPtr.Zero;

        // Mantemos referências para evitar que o GC colete os delegates
        private readonly LowLevelProc _keyboardProc;
        private readonly LowLevelProc _mouseProc;

        public event EventHandler ActivityDetected;

        // ── Construtor ──────────────────────────────────────────────────────
        public GlobalHookManager()
        {
            _keyboardProc = KeyboardHookCallback;
            _mouseProc    = MouseHookCallback;
        }

        // ── Público ─────────────────────────────────────────────────────────
        public void Install()
        {
            _keyboardHookId = SetHook(WH_KEYBOARD_LL, _keyboardProc);
            _mouseHookId    = SetHook(WH_MOUSE_LL,    _mouseProc);
        }

        public void Uninstall()
        {
            if (_keyboardHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_keyboardHookId);
                _keyboardHookId = IntPtr.Zero;
            }

            if (_mouseHookId != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_mouseHookId);
                _mouseHookId = IntPtr.Zero;
            }
        }

        // ── Privado ─────────────────────────────────────────────────────────
        private IntPtr SetHook(int hookType, LowLevelProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (var curModule  = curProcess.MainModule)
            {
                return SetWindowsHookEx(
                    hookType,
                    proc,
                    GetModuleHandle(curModule.ModuleName),
                    0
                );
            }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                OnActivityDetected();
            }

            return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                OnActivityDetected();
            }

            return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        private void OnActivityDetected()
        {
            ActivityDetected?.Invoke(this, EventArgs.Empty);
        }

        // ── IDisposable ─────────────────────────────────────────────────────
        public void Dispose()
        {
            Uninstall();
        }
    }
}
