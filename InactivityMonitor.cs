using System;
using System.Windows.Forms;

namespace InactivityLocker
{
    /// <summary>
    /// Monitora inatividade: se nenhuma atividade for detectada dentro do
    /// intervalo configurado, dispara o evento InactivityTimeout.
    /// </summary>
    public class InactivityMonitor : IDisposable
    {
        private readonly System.Windows.Forms.Timer _timer;
        private int _intervalMinutes;

        public event EventHandler InactivityTimeout;

        public int IntervalMinutes
        {
            get { return _intervalMinutes; }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("value", "O intervalo mínimo é 1 minuto.");

                _intervalMinutes = value;
                _timer.Interval   = value * 60 * 1000;
            }
        }

        public bool IsRunning { get { return _timer.Enabled; } }

        public InactivityMonitor(int defaultIntervalMinutes = 1)
        {
            _timer          = new System.Windows.Forms.Timer();
            _timer.Tick    += OnTimerTick;
            IntervalMinutes = defaultIntervalMinutes;
        }

        public void Start()
        {
            _timer.Stop();
            _timer.Start();
        }

        /// <summary>
        /// Reinicia a contagem — deve ser chamado sempre que houver atividade.
        /// </summary>
        public void Reset()
        {
            _timer.Stop();
            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _timer.Stop(); // Evita disparos repetidos até a próxima atividade
            InactivityTimeout?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
