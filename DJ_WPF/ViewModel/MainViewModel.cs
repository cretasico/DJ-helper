using DJ_WPF.Commands;
using DJ_WPF.Model;
using DJ_WPF.View;
using DJ_WPF.ViewModel;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using System.Windows.Input;



public class MainViewModel : ViewModelBase
{
    #region Private Fields

    private int _progress;
    private string _richTextContent;
    private bool _isScanning;
    private ProgressWindow _progressWindow;
    private object _currentView;

    #endregion

    #region Public Properties

    public int Progress
    {
        get => _progress;
        set { _progress = value; OnPropertyChanged(); }
    }

    public string RichTextContent
    {
        get => _richTextContent;
        set { _richTextContent = value; OnPropertyChanged(); }
    }

    public bool IsScanning
    {
        get => _isScanning;
        set { _isScanning = value; OnPropertyChanged(); }
    }

    public object CurrentView
    {
        get => _currentView;
        set { _currentView = value; OnPropertyChanged(); }
    }

    #endregion

    #region Commands

    public ICommand ScanTagsCommand { get; }
    public ICommand ScanSongsCommand { get; }
    public ICommand UpdateFromExcelCommand { get; }
    public ICommand ShowConfigurationViewCommand { get; }
    public ICommand ShowAddScoreViewCommand { get; }

    #endregion

    #region Constructor

    public MainViewModel()
    {
        ScanTagsCommand = new RelayCommand(async () => await ScanTagsAsync(), () => !IsScanning);
        ScanSongsCommand = new RelayCommand(async () => await ScanSongsAsync(), () => !IsScanning);
        UpdateFromExcelCommand = new RelayCommand(async () => await UpdateFromExcelAsync(), () => !IsScanning);

        ShowConfigurationViewCommand = new RelayCommand(() => CurrentView = new ConfigurationView());
        ShowAddScoreViewCommand = new RelayCommand(() => CurrentView = new AddScoreView());
    }

    #endregion

    #region Private Methods (UI Helpers)

    private void ShowProgressWindow(string initialText)
    {
        IsScanning = true;
        Progress = 0;
        RichTextContent = initialText;

        _progressWindow = new ProgressWindow { DataContext = this };
        _progressWindow.Show();
    }

    private void CloseProgressWindow()
    {
        IsScanning = false;
        _progressWindow?.Close();
        _progressWindow = null;
    }

    // Evita update de UI desde background thread
    private void UpdateProgressSafe(int value)
    {
        if (System.Windows.Application.Current?.Dispatcher?.CheckAccess() == true)
        {
            Progress = value;
        }
        else
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(() => Progress = value);
        }
    }

    private void ShowErrorAndStop(string message)
    {
        System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        CloseProgressWindow();
    }

    #endregion

    #region Private Methods (Config / Validation)

    private bool TryGetMusicPath(out string musicPath)
    {
        musicPath = string.Empty;
        var config = Config.Load();
        musicPath = config.Source;

        if (string.IsNullOrEmpty(musicPath) || !Directory.Exists(musicPath))
            return false;

        return true;
    }

    private bool TryGetExcelPaths(out string musicPath, out string excelPath)
    {
        musicPath = string.Empty;
        excelPath = string.Empty;

        var config = Config.Load();
        musicPath = config.Source;
        excelPath = config.ExcelPath;

        if (string.IsNullOrEmpty(musicPath) || !Directory.Exists(musicPath))
            return false;

        if (string.IsNullOrEmpty(excelPath))
            return false;

        return true;
    }

    private bool TryGetUpdateExcelPaths(out string newExcelPath, out string oldExcelPath)
    {
        newExcelPath = string.Empty;
        oldExcelPath = string.Empty;

        var config = Config.Load();
        newExcelPath = config.ExcelPath;
        oldExcelPath = config.ExcelPathOld;

        if (string.IsNullOrEmpty(newExcelPath) || string.IsNullOrEmpty(oldExcelPath))
            return false;

        if (!File.Exists(newExcelPath) || !File.Exists(oldExcelPath))
            return false;

        return true;
    }

    #endregion

    #region Private Methods (File Scanning)

    private List<string> GetMp3FilesSafe(string rootPath)
    {
        var files = new List<string>();

        try
        {
            // incluir también los mp3 directos en root
            try
            {
                files.AddRange(Directory.GetFiles(rootPath, "*.mp3", SearchOption.TopDirectoryOnly));
            }
            catch (UnauthorizedAccessException) { }

            foreach (var dir in Directory.EnumerateDirectories(rootPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    files.AddRange(Directory.GetFiles(dir, "*.mp3", SearchOption.TopDirectoryOnly));
                }
                catch (UnauthorizedAccessException)
                {
                    // carpeta protegida → skip
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // root no accesible
        }

        return files;
    }

    #endregion

    #region Private Methods (Scan / Update)

    private async Task ScanTagsAsync()
    {
        ShowProgressWindow("Scanning MP3 files...\n");

        if (!TryGetMusicPath(out var musicPath))
        {
            ShowErrorAndStop("Invalid path in config.xml");
            return;
        }

        var files = GetMp3FilesSafe(musicPath);
        int totalFiles = files.Count;

        if (totalFiles == 0)
        {
            RichTextContent = "No MP3 files found.";
            CloseProgressWindow();
            return;
        }

        var tagCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        await Task.Run(() =>
        {
            for (int i = 0; i < totalFiles; i++)
            {
                try
                {
                    using (var file = TagLib.File.Create(files[i]))
                    {
                        foreach (var prop in file.Tag.GetType().GetProperties())
                        {
                            var tagName = prop.Name;
                            if (!tagCounts.ContainsKey(tagName))
                                tagCounts[tagName] = 0;
                            tagCounts[tagName]++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {files[i]}: {ex.Message}");
                }

                UpdateProgressSafe((i + 1) * 100 / totalFiles);
            }
        });

        RichTextContent = $"Scan Completed!\nTotal Files: {totalFiles}\n\n";
        foreach (var tag in tagCounts.OrderByDescending(x => x.Value))
            RichTextContent += $"{tag.Key}: {tag.Value} files\n";

        CloseProgressWindow();
    }

    private async Task ScanSongsAsync()
    {
        ShowProgressWindow("Scanning MP3 files and generating Excel...\n");

        if (!TryGetExcelPaths(out var musicPath, out var excelFilePath))
        {
            ShowErrorAndStop("Invalid path in config.xml");
            return;
        }

        var files = GetMp3FilesSafe(musicPath);
        int totalFiles = files.Count;

        if (totalFiles == 0)
        {
            RichTextContent = "No MP3 files found.";
            CloseProgressWindow();
            return;
        }

        var songs = new List<Song>(capacity: totalFiles);

        await Task.Run(() =>
        {
            for (int i = 0; i < totalFiles; i++)
            {
                try
                {
                    using (var file = TagLib.File.Create(files[i]))
                    {
                        var song = new Song(
                            title: file.Tag.Title ?? string.Empty,
                            artist: file.Tag.FirstPerformer ?? string.Empty,
                            bpm: (int)(file.Tag.BeatsPerMinute > 0 ? file.Tag.BeatsPerMinute : 0),
                            genre: file.Tag.FirstGenre ?? string.Empty,
                            year: (int)(file.Tag.Year > 0 ? file.Tag.Year : 0),
                            energy: 0,
                            key: string.Empty,
                            popularity: 0,
                            fileName: Path.GetFileName(files[i]),
                            filePath: Path.GetDirectoryName(files[i]),
                            country: string.Empty,
                            myScore: string.Empty,
                            comment: string.Empty,
                            danceability: 0,
                            loudness: 0,
                            speechiness: 0,
                            acousticness: 0,
                            instrumentalness: 0,
                            liveness: 0,
                            valence: 0,
                            durationMs: 0,
                            mode: -1,
                            timeSignature: 0,
                            isSearchedOnSpotify: false,
                            noMatchOnSpotify: false
                        );

                        songs.Add(song);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {files[i]}: {ex.Message}");
                }

                UpdateProgressSafe((i + 1) * 100 / totalFiles);
            }
        });

        try
        {
            ExcelPackage.License.SetNonCommercialPersonal("<LaPausa.org>");

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Songs");

                // headers
                string[] headers =
                {
                    "Title","Artist","BPM","Genre","Year","Energy","Key","Popularity",
                    "FileName","FilePath","Country","MyScore","Comment","Danceability",
                    "Loudness","Speechiness","Acousticness","Instrumentalness","Liveness",
                    "Valence","DurationMs","Mode","TimeSignature","IsSearchedOnSpotify","NoMatchOnSpotify"
                };

                for (int c = 0; c < headers.Length; c++)
                    worksheet.Cells[1, c + 1].Value = headers[c];

                for (int i = 0; i < songs.Count; i++)
                {
                    var s = songs[i];
                    int r = i + 2;

                    worksheet.Cells[r, 1].Value = s.Title;
                    worksheet.Cells[r, 2].Value = s.Artist;
                    worksheet.Cells[r, 3].Value = s.BPM;
                    worksheet.Cells[r, 4].Value = s.Genre;
                    worksheet.Cells[r, 5].Value = s.Year;
                    worksheet.Cells[r, 6].Value = s.Energy;
                    worksheet.Cells[r, 7].Value = s.Key;
                    worksheet.Cells[r, 8].Value = s.Popularity;
                    worksheet.Cells[r, 9].Value = s.FileName;
                    worksheet.Cells[r, 10].Value = s.FilePath;
                    worksheet.Cells[r, 11].Value = s.Country;
                    worksheet.Cells[r, 12].Value = s.MyScore;
                    worksheet.Cells[r, 13].Value = s.Comment;
                    worksheet.Cells[r, 14].Value = s.Danceability;
                    worksheet.Cells[r, 15].Value = s.Loudness;
                    worksheet.Cells[r, 16].Value = s.Speechiness;
                    worksheet.Cells[r, 17].Value = s.Acousticness;
                    worksheet.Cells[r, 18].Value = s.Instrumentalness;
                    worksheet.Cells[r, 19].Value = s.Liveness;
                    worksheet.Cells[r, 20].Value = s.Valence;
                    worksheet.Cells[r, 21].Value = s.DurationMs;
                    worksheet.Cells[r, 22].Value = s.Mode;
                    worksheet.Cells[r, 23].Value = s.TimeSignature;
                    worksheet.Cells[r, 24].Value = s.IsSearchedOnSpotify;
                    worksheet.Cells[r, 25].Value = s.NoMatchOnSpotify;
                }

                package.SaveAs(new FileInfo(excelFilePath));
            }

            RichTextContent += $"Excel file created successfully at {excelFilePath}\n";
        }
        catch (Exception ex)
        {
            RichTextContent += $"Error creating Excel file: {ex.Message}\n";
        }

        CloseProgressWindow();
    }

    private async Task UpdateFromExcelAsync()
    {
        ShowProgressWindow("Updating songs from old Excel...\n");

        if (!TryGetUpdateExcelPaths(out var newExcelPath, out var oldExcelPath))
        {
            ShowErrorAndStop("Invalid Excel paths in config.xml");
            return;
        }

        try
        {
            ExcelPackage.License.SetNonCommercialPersonal("<LaPausa.org>");

            var newSongs = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);
            var oldSongs = new Dictionary<string, Dictionary<string, object>>(StringComparer.OrdinalIgnoreCase);

            // 1) new excel
            await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(newExcelPath));
                var ws = package.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new Exception("New Excel file is empty.");

                int rows = ws.Dimension.Rows;
                int cols = ws.Dimension.Columns;

                int titleCol = FindColumnIndex(ws, cols, "Title");
                for (int row = 2; row <= rows; row++)
                {
                    var title = ws.Cells[row, titleCol].Text;
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var songData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int col = 1; col <= cols; col++)
                    {
                        var header = ws.Cells[1, col].Text;
                        songData[header] = ws.Cells[row, col].Value;
                    }
                    newSongs[title] = songData;

                    UpdateProgressSafe((row - 1) * 33 / Math.Max(1, rows - 1));
                }
            });

            // 2) old excel
            await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(oldExcelPath));
                var ws = package.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new Exception("Old Excel file is empty.");

                int rows = ws.Dimension.Rows;
                int cols = ws.Dimension.Columns;

                int titleCol = FindColumnIndex(ws, cols, "Title");
                for (int row = 2; row <= rows; row++)
                {
                    var title = ws.Cells[row, titleCol].Text;
                    if (string.IsNullOrWhiteSpace(title)) continue;

                    var songData = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    for (int col = 1; col <= cols; col++)
                    {
                        var header = ws.Cells[1, col].Text;
                        songData[header] = ws.Cells[row, col].Value;
                    }
                    oldSongs[title] = songData;

                    UpdateProgressSafe(33 + (row - 1) * 33 / Math.Max(1, rows - 1));
                }
            });

            // 3) merge into new excel
            await Task.Run(() =>
            {
                using var package = new ExcelPackage(new FileInfo(newExcelPath));
                var ws = package.Workbook.Worksheets.FirstOrDefault()
                         ?? throw new Exception("New Excel file is empty.");

                int rows = ws.Dimension.Rows;
                int cols = ws.Dimension.Columns;

                int titleCol = FindColumnIndex(ws, cols, "Title");

                for (int row = 2; row <= rows; row++)
                {
                    var title = ws.Cells[row, titleCol].Text;
                    if (string.IsNullOrWhiteSpace(title)) continue;
                    if (!oldSongs.TryGetValue(title, out var oldSongData)) continue;

                    for (int col = 1; col <= cols; col++)
                    {
                        var header = ws.Cells[1, col].Text;
                        if (!oldSongData.TryGetValue(header, out var oldValue)) continue;

                        var newValue = ws.Cells[row, col].Value;

                        bool isNullOrZero =
                            newValue == null ||
                            (newValue is double d && d == 0) ||
                            (newValue is string s && string.IsNullOrWhiteSpace(s));

                        if (isNullOrZero && oldValue != null)
                            ws.Cells[row, col].Value = oldValue;
                    }

                    UpdateProgressSafe(66 + (row - 1) * 34 / Math.Max(1, rows - 1));
                }

                package.Save();
            });

            RichTextContent += "Update completed successfully.\n";
        }
        catch (Exception ex)
        {
            RichTextContent += $"Error updating songs: {ex.Message}\n";
        }

        Progress = 100;
        CloseProgressWindow();
    }

    private static int FindColumnIndex(ExcelWorksheet ws, int cols, string headerName)
    {
        for (int col = 1; col <= cols; col++)
        {
            var header = ws.Cells[1, col].Text.Trim();
            if (header.Equals(headerName, StringComparison.OrdinalIgnoreCase))
                return col;
        }
        throw new Exception($"No '{headerName}' column found.");
    }

    #endregion
}



