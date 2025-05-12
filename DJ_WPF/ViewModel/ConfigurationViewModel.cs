using System.IO;
using System.Xml.Linq;
using DJ_WPF.ViewModel;
using System.Windows.Input;
using DJ_WPF.Commands;
using System.Windows;
using DJ_WPF.Model;



namespace DJ_WPF.ViewModel
{

    public class ConfigurationViewModel : ViewModelBase
{
    private string _excelPath;
    private string _excelPathOld;
    private string _sourcePath;
    private string _spotifyAPIKey;
    private string _levelNormal;
    private string _levelGood;
    private string _levelSuper;

        public string ExcelPath
    {
        get => _excelPath;
        set { _excelPath = value; OnPropertyChanged(); }
    }

    public string ExcelPathOld
    {
        get => _excelPathOld;
        set { _excelPathOld = value; OnPropertyChanged(); }
    }

    public string SourcePath
    {
        get => _sourcePath;
        set { _sourcePath = value; OnPropertyChanged(); }
    }

    public string SpotifyAPIKey
    {
        get => _spotifyAPIKey;
        set { _spotifyAPIKey = value; OnPropertyChanged(); }
    }

    public string LevelNormal
    {
        get => _levelNormal;
        set { _levelNormal = value; OnPropertyChanged(); }
    }

    public string LevelGood
    {
        get => _levelGood;
        set { _levelGood = value; OnPropertyChanged(); }
    }

    public string LevelSuper
    {
        get => _levelSuper;
        set { _levelSuper = value; OnPropertyChanged(); }
    }

    public ICommand SaveConfigurationCommand { get; }
    public ICommand CancelConfigurationCommand { get; }

    public ConfigurationViewModel()
    {
        LoadConfiguration();

        SaveConfigurationCommand = new RelayCommand(SaveConfiguration);
        CancelConfigurationCommand = new RelayCommand(CancelConfiguration);
    }

        private void LoadConfiguration()
        {
            var config = Config.Load();

            ExcelPath = config.ExcelPath;
            ExcelPathOld = config.ExcelPathOld;
            SourcePath = config.Source;
            SpotifyAPIKey = config.SpotifyAPIKey;
            LevelNormal = config.RatingLevelNormal;
            LevelGood = config.RatingLevelGood;
            LevelSuper = config.RatingLevelSuper;
        }



        private void SaveConfiguration()
        {
            try
            {
                var config = new Config
                {
                    ExcelPath = ExcelPath,
                    ExcelPathOld = ExcelPathOld,
                    Source = SourcePath,
                    SpotifyAPIKey = SpotifyAPIKey,
                    RatingLevelNormal = LevelNormal,
                    RatingLevelGood = LevelGood,
                    RatingLevelSuper = LevelSuper
                };

                config.Save();

                System.Windows.MessageBox.Show("Configuration saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving configuration: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        private void CancelConfiguration()
    {
        LoadConfiguration();
        System.Windows.MessageBox.Show("Changes canceled.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    }
}

