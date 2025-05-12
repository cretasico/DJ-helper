using System;
using System.Windows.Media;
using System.Windows.Threading;

namespace DJ_WPF.Services
{
    public class AudioPlayerService
    {
        private MediaPlayer _player;
        private DispatcherTimer _timer;

        // Evento para notificar al ViewModel sobre el progreso
        public event Action<double> ProgressUpdated;

        public AudioPlayerService()
        {
            _player = new MediaPlayer();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500) // Actualización cada 500 ms
            };
            _timer.Tick += (s, e) =>
            {
                if (_player.NaturalDuration.HasTimeSpan)
                {
                    ProgressUpdated?.Invoke(_player.Position.TotalSeconds);
                }
            };
        }

        public void Play(string path)
        {
            _player.Open(new Uri(path));
            _player.MediaOpened += (s, e) =>
            {
                ProgressUpdated?.Invoke(_player.Position.TotalSeconds);
            };

            _player.Play();
            _timer.Start();
        }

        public void Pause() => _player.Pause();

        public void Stop()
        {
            _player.Stop();
            _timer.Stop();
        }

        public void Seek(double seconds)
        {
            _player.Position = TimeSpan.FromSeconds(seconds);
        }

        // Acceso a duración total en segundos (para el Slider)
        public double TotalDurationInSeconds =>
            _player.NaturalDuration.HasTimeSpan
                ? _player.NaturalDuration.TimeSpan.TotalSeconds
                : 0;
    }
}
