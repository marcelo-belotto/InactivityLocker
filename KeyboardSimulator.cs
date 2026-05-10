using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace InactivityLocker
{
    /// <summary>
    /// Simula o pressionamento de Win+L usando keybd_event (compatível com .NET 4).
    /// </summary>
    public static class KeyboardSimulator
    {
        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        /// <summary>
        /// Pressiona e solta Win+L, disparando o bloqueio de tela do Windows.
        /// </summary>
        public static void PressWinL()
        {

        LockWorkStation();
        }
    }
}
