using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.VisualBasic;

namespace InactivityLocker
{
    /// <summary>
    /// Contexto principal da aplicação: vive na bandeja do sistema (System Tray).
    /// Sem janela principal — apenas ícone, tooltip e menu de contexto.
    /// </summary>
    public class TrayApplicationContext : ApplicationContext
    {
        private readonly NotifyIcon _trayIcon;
        private readonly GlobalHookManager _hookManager;
        private readonly InactivityMonitor _monitor;
        private const string SENHA = "1234";

        private bool _isSessionLocked = false;

        public TrayApplicationContext()
        {
            _hookManager = new GlobalHookManager();
            _monitor = new InactivityMonitor(defaultIntervalMinutes: 1);

            // ── Ícone da bandeja ──────────────────────────────────────────
            _trayIcon = new NotifyIcon
            {
                Icon = CreateTrayIcon(),
                Text = BuildTooltip(),
                Visible = true,
                ContextMenuStrip = BuildContextMenu()
            };

            // ── Wiring ────────────────────────────────────────────────────
            _hookManager.ActivityDetected += OnActivityDetected;
            _monitor.InactivityTimeout += OnInactivityTimeout;
            SystemEvents.SessionSwitch += OnSessionSwitch;

            _hookManager.Install();
            _monitor.Start();
        }

        // ── Handlers ────────────────────────────────────────────────────────
        private void OnActivityDetected(object sender, EventArgs e)
        {
            // Ignora qualquer atividade capturada durante a tela de bloqueio
            if (!_isSessionLocked)
                _monitor.Reset();
        }

        private void OnInactivityTimeout(object sender, EventArgs e)
        {
            KeyboardSimulator.PressWinL();
            // Não reinicia o monitor aqui — o SessionSwitch cuidará disso ao desbloquear
        }

        /// <summary>
        /// Chamado pelo Windows ao bloquear ou desbloquear a sessão.
        /// </summary>
        private void OnSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (e.Reason == SessionSwitchReason.SessionLock)
            {
                // Tela bloqueada: para o monitor e sinaliza estado
                _isSessionLocked = true;
                _monitor.Stop();
                UpdateTrayStatus("🔒 Sessão bloqueada");
            }
            else if (e.Reason == SessionSwitchReason.SessionUnlock)
            {
                // Usuário desbloqueou: reinicia contagem do zero
                _isSessionLocked = false;
                _monitor.Reset();
                UpdateTrayStatus(null); // volta ao tooltip padrão
            }
        }

        // ── Menu de contexto ─────────────────────────────────────────────────
        private ContextMenuStrip BuildContextMenu()
        {
            var menu = new ContextMenuStrip();

            // --- Status (não clicável) ---
            var statusItem = new ToolStripMenuItem("● Monitorando") { Enabled = false };
            menu.Items.Add(statusItem);

            menu.Items.Add(new ToolStripSeparator());

            // --- Configurar intervalo ---
            var configItem = new ToolStripMenuItem("Configurar intervalo...");
            configItem.Click += OnConfigureInterval;
            menu.Items.Add(configItem);

            menu.Items.Add(new ToolStripSeparator());

            // --- Sair ---
            var exitItem = new ToolStripMenuItem("Sair");
            exitItem.Click += OnExit;
            menu.Items.Add(exitItem);

            return menu;
        }

        private void OnConfigureInterval(object sender, EventArgs e)
        {
            if (showPasswordBox())
            {
                using (var form = new IntervalConfigForm(_monitor.IntervalMinutes))
                {
                    if (form.ShowDialog() == DialogResult.OK)
                    {
                        _monitor.IntervalMinutes = form.SelectedMinutes;
                        _trayIcon.Text = BuildTooltip();
                        _monitor.Reset();
                    }
                }
            }
        }


        private void OnExit(object sender, EventArgs e)
        {
            if (showPasswordBox())
            {
                _trayIcon.Visible = false;
                _hookManager.Uninstall();
                _monitor.Stop();

                Application.Exit();
            }
        }

        private Boolean showPasswordBox()
        {
            string senha = Interaction.InputBox(
            "Digite a senha para fechar a aplicação:",
            "Autenticação",
            "");

            if (senha != SENHA)
            {
                MessageBox.Show(
                    "Senha incorreta.",
                    "Acesso negado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }
            return true;
        }

        // ── Helpers ─────────────────────────────────────────────────────────
        private string BuildTooltip()
        {
            return string.Format("InactivityLocker — Bloqueio em {0} min de inatividade",
                                 _monitor != null ? _monitor.IntervalMinutes : 5);
        }

        private void UpdateTrayStatus(string overrideText)
        {
            // NotifyIcon.Text tem limite de 63 caracteres no Windows
            var text = overrideText ?? BuildTooltip();
            if (text.Length > 63)
                text = text.Substring(0, 63);
            _trayIcon.Text = text;
        }

        /// <summary>
        /// Gera um ícone simples (cadeado) programaticamente — sem arquivo .ico externo.
        /// </summary>
        private static Icon CreateTrayIcon()
        {
            var bmp = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.Transparent);

                // Arco do cadeado
                using (var pen = new Pen(Color.White, 2))
                {
                    g.DrawArc(pen, 4, 1, 8, 7, 180, 180);
                }

                // Corpo do cadeado
                using (var brush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(brush, 2, 7, 12, 8);
                }

                // Buraco da fechadura
                using (var brush = new SolidBrush(Color.DimGray))
                {
                    g.FillEllipse(brush, 6, 9, 4, 4);
                }
            }

            return Icon.FromHandle(bmp.GetHicon());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SystemEvents.SessionSwitch -= OnSessionSwitch;
                _hookManager.Dispose();
                _monitor.Dispose();
                _trayIcon.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}