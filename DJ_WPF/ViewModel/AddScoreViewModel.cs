using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DJ_WPF.Model;
using DJ_WPF.ViewModel;
using OfficeOpenXml;
using System.Windows.Forms;
using DJ_WPF.Commands;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows;
using DJ_WPF.Services;

namespace DJ_WPF.ViewModel
{

    public class AddScoreViewModel : INotifyPropertyChanged
    {
        private string _selectedFolder;
        private ObservableCollection<string> _myScoreOptions;
        private ObservableCollection<Song> _filteredSongs;

        private double _progressInSeconds;


        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }



        public double ProgressInSeconds
        {
            get => _progressInSeconds;
            set
            {
                if (Math.Abs(_progressInSeconds - value) > 0.5) // evita rebotes menores
                {
                    _progressInSeconds = value;
                    OnPropertyChanged();
                    SeekToPosition(value); // <- cada vez que se mueva el slider
                }
            }
        }

        private double _durationInSeconds;
        public double DurationInSeconds
        {
            get => _durationInSeconds;
            set
            {
                _durationInSeconds = value;
                OnPropertyChanged();
            }
        }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged();
                
            }
        }

        public ObservableCollection<string> MyScoreOptions
        {
            get => _myScoreOptions;
            set
            {
                _myScoreOptions = value;
                OnPropertyChanged();
            }
        }

        public ICommand BrowseFolderCommand { get; }

        public ObservableCollection<Song> FilteredSongs
        {
            get => _filteredSongs;
            set
            {
                _filteredSongs = value;
                OnPropertyChanged();
            }
        }

        public AddScoreViewModel()
        {
            // Inicializar campos
            _selectedFolder = string.Empty;
            _myScoreOptions = new ObservableCollection<string>();
            _filteredSongs = new ObservableCollection<Song>();
            _selectedSong = null; // Puede ser null si no hay una canción seleccionada inicialmente
            PropertyChanged = null; // El evento no necesita inicialización explícita

            // Inicializar comandos
            BrowseFolderCommand = new RelayCommand(BrowseFolder);
            PlayCommand = new RelayCommand(Play);
            PauseCommand = new RelayCommand(Pause);
            StopCommand = new RelayCommand(Stop);

            // Cargar opciones de MyScore
            LoadMyScoreOptions();

            // Suscribirse al evento de progreso del reproductor
            _audioPlayerService.ProgressUpdated += (progress) =>
            {
                ProgressInSeconds = progress;
                DurationInSeconds = _audioPlayerService.TotalDurationInSeconds;
            };

        }


        private void BrowseFolder()
        {
            // Cargar configuración desde el archivo XML
            //var configFilePath = "config.xml"; // Ruta al archivo de configuración
            var config = Config.Load();

            using var dialog = new FolderBrowserDialog
            {
                Description = "Select a folder inside your configuration Source",
                SelectedPath = config.Source,
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var basePath = config.Source;
                if (dialog.SelectedPath.StartsWith(basePath, StringComparison.OrdinalIgnoreCase))
                {
                    SelectedFolder = dialog.SelectedPath;

                    // Buscar canciones con FilePath == SelectedFolder y MyScore vacío
                    var repo = new SongRepository(config);
                    var matches = repo.FindSongsByTwoColumns("FilePath", SelectedFolder, "MyScore", string.Empty);

                    FilteredSongs = new ObservableCollection<Song>(matches);
                }
                else
                {
                    System.Windows.MessageBox.Show("Please select a folder inside your configuration Source.", "Invalid Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void LoadMyScoreOptions()
        {
            string levelNormal = Config.GetNestedValue("RatingScale", "Level_Normal");
            string levelGood = Config.GetNestedValue("RatingScale", "Level_Good");
            string levelSuper = Config.GetNestedValue("RatingScale", "Level_Super");

            if (!string.IsNullOrEmpty(levelNormal)) MyScoreOptions.Add(levelNormal);
            if (!string.IsNullOrEmpty(levelGood)) MyScoreOptions.Add(levelGood);
            if (!string.IsNullOrEmpty(levelSuper)) MyScoreOptions.Add(levelSuper);
        }


        private readonly AudioPlayerService _audioPlayerService = new AudioPlayerService();
        private Song _selectedSong;

        public Song SelectedSong
        {
            get => _selectedSong;
            set
            {
                _selectedSong = value;
                OnPropertyChanged();
            }
        }



        private void Play()
        {
            if (SelectedSong != null &&
                !string.IsNullOrEmpty(SelectedSong.FilePath) &&
                !string.IsNullOrEmpty(SelectedSong.FileName))
            {
                string fullPath = System.IO.Path.Combine(SelectedSong.FilePath, SelectedSong.FileName);

                _audioPlayerService.Play(fullPath);
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a song to play.", "No Song Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void Pause()
        {
            _audioPlayerService.Pause();
        }

        private void Stop()
        {
            _audioPlayerService.Stop();
        }
        private void SeekToPosition(double seconds)
        {
            _audioPlayerService.Seek(seconds);
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
