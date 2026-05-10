using System;
using System.Windows.Forms;

namespace InactivityLocker
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Garantir apenas uma instância rodando
            bool createdNew;
            using (var mutex = new System.Threading.Mutex(true, "InactivityLockerMutex", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "O InactivityLocker já está em execução.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );
                    return;
                }

                Application.Run(new TrayApplicationContext());
            }
        }
    }
}
