using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace InactivityLocker
{
    /// <summary>
    /// Simula o pressionamento de Win+L(compatível com .NET 4).
    /// </summary>
    public static class LockSession
    {
        [DllImport("user32.dll")]
        private static extern bool LockWorkStation();

        /// <summary>
        /// Realiza o bloqueio da sessão  
        /// </summary>
        public static void Lock()
        {
            LockWorkStation();
        }
    }
}
