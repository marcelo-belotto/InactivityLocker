using System;
using System.Drawing;
using System.Windows.Forms;

namespace InactivityLocker
{
    /// <summary>
    /// Janela simples para o usuário configurar o intervalo de inatividade em minutos.
    /// </summary>
    public class IntervalConfigForm : Form
    {
        private NumericUpDown _numericUpDown;
        private Button        _btnOk;
        private Button        _btnCancel;
        private Label         _lblDescription;

        public int SelectedMinutes { get; private set; }

        public IntervalConfigForm(int currentMinutes)
        {
            SelectedMinutes = currentMinutes;
            InitializeComponents();
            _numericUpDown.Value = currentMinutes;
        }

        private void InitializeComponents()
        {
            // ── Janela ────────────────────────────────────────────────────
            this.Text            = "InactivityLocker — Configurar Intervalo";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox     = false;
            this.MinimizeBox     = false;
            this.StartPosition   = FormStartPosition.CenterScreen;
            this.ClientSize      = new Size(320, 140);
            this.BackColor       = Color.FromArgb(30, 30, 30);
            this.ForeColor       = Color.White;
            this.ShowInTaskbar   = false;

            // ── Label descrição ───────────────────────────────────────────
            _lblDescription = new Label
            {
                Text      = "Bloquear tela após quantos minutos de inatividade?",
                Location  = new Point(12, 15),
                Size      = new Size(296, 32),
                ForeColor = Color.LightGray,
                Font      = new Font("Segoe UI", 9f)
            };

            // ── NumericUpDown ─────────────────────────────────────────────
            _numericUpDown = new NumericUpDown
            {
                Location  = new Point(12, 55),
                Size      = new Size(80, 26),
                Minimum   = 1,
                Maximum   = 480,   // até 8 horas
                Value     = 5,
                Font      = new Font("Segoe UI", 10f),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            var lblMin = new Label
            {
                Text     = "minutos",
                Location = new Point(100, 58),
                Size     = new Size(70, 22),
                ForeColor = Color.LightGray,
                Font     = new Font("Segoe UI", 9f)
            };

            // ── Botões ────────────────────────────────────────────────────
            _btnOk = new Button
            {
                Text        = "OK",
                DialogResult = DialogResult.OK,
                Location    = new Point(152, 100),
                Size        = new Size(72, 28),
                FlatStyle   = FlatStyle.Flat,
                BackColor   = Color.FromArgb(0, 122, 204),
                ForeColor   = Color.White,
                Font        = new Font("Segoe UI", 9f, FontStyle.Bold)
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += OnOkClick;

            _btnCancel = new Button
            {
                Text         = "Cancelar",
                DialogResult = DialogResult.Cancel,
                Location     = new Point(232, 100),
                Size         = new Size(76, 28),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = Color.FromArgb(60, 60, 60),
                ForeColor    = Color.White,
                Font         = new Font("Segoe UI", 9f)
            };
            _btnCancel.FlatAppearance.BorderSize = 0;

            this.AcceptButton = _btnOk;
            this.CancelButton = _btnCancel;

            this.Controls.AddRange(new Control[]
            {
                _lblDescription, _numericUpDown, lblMin, _btnOk, _btnCancel
            });
        }

        private void OnOkClick(object sender, EventArgs e)
        {
            SelectedMinutes = (int)_numericUpDown.Value;
        }
    }
}
