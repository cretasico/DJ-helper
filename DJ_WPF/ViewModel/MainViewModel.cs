using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TagLib;
using System.Collections.ObjectModel;
using DJ_WPF.ViewModel;
using DJ_WPF.Commands;
using DJ_WPF.View;
using DJ_WPF.Model;
using OfficeOpenXml;


public class MainViewModel : ViewModelBase
{
    private int _progress;
    private string _richTextContent;
    private bool _isScanning;
    private ProgressWindow _progressWindow;
    private object _currentView;

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

    public ICommand ScanTagsCommand { get; }
    public ICommand ScanSongsCommand { get; }
    public ICommand UpdateFromExcelCommand { get; }
    public ICommand ShowConfigurationViewCommand { get; } // Declarar la propiedad
    public ICommand ShowAddScoreViewCommand { get; }


    public MainViewModel()
    {
        ScanTagsCommand = new RelayCommand(async () => await ScanTagsAsync(), () => !IsScanning);
        ScanSongsCommand = new RelayCommand(async () => await ScanSongsAsync(), () => !IsScanning);
        UpdateFromExcelCommand = new RelayCommand(async () => await UpdateFromExcelAsync(), () => !IsScanning);

        ShowConfigurationViewCommand = new RelayCommand(() => CurrentView = new ConfigurationView());
        ShowAddScoreViewCommand = new RelayCommand(() => CurrentView = new AddScoreView());

        //ShowConfigurationViewCommand = new RelayCommand(() => CurrentView = new ConfigurationViewModel());


    }






    private async Task ScanTagsAsync()
    {
        IsScanning = true;
        Progress = 0;
        RichTextContent = "Scanning MP3 files...\n";

        _progressWindow = new ProgressWindow
        {
            DataContext = this
        };
        _progressWindow.Show();


        Config config = Config.Load();
        string musicPath = config.Source;


        if (string.IsNullOrEmpty(musicPath) || !Directory.Exists(musicPath))
        {
            System.Windows.MessageBox.Show("Invalid path in config.xml", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            IsScanning = false;
            _progressWindow.Close();
            return;
        }

        var files = Directory.GetFiles(musicPath, "*.mp3", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        if (totalFiles == 0)
        {
            RichTextContent = "No MP3 files found.";
            IsScanning = false;
            _progressWindow.Close();
            return;
        }

        var tagCounts = new Dictionary<string, int>();

        await Task.Run(() =>
        {
            for (int i = 0; i < totalFiles; i++)
            {
                try
                {
                    using (var file = TagLib.File.Create(files[i]))
                    {
                        foreach (var tag in file.Tag.GetType().GetProperties())
                        {
                            string tagName = tag.Name;
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

                Progress = (i + 1) * 100 / totalFiles;
            }
        });

        RichTextContent = $"Scan Completed!\nTotal Files: {totalFiles}\n\n";
        foreach (var tag in tagCounts)
        {
            RichTextContent += $"{tag.Key}: {tag.Value} files\n";
        }

        IsScanning = false;
        _progressWindow.Close(); // Cerrar la ventana de progreso
    }

    private async Task ScanSongsAsync()
    {
        IsScanning = true;
        Progress = 0;
        RichTextContent = "Scanning MP3 files and generating Excel...\n";

        // Mostrar la ventana de progreso
        _progressWindow = new ProgressWindow
        {
            DataContext = this
        };
        _progressWindow.Show();


        Config config = Config.Load();
        string musicPath = config.Source;
        string excelFilePath = config.ExcelPath;


        if (string.IsNullOrEmpty(musicPath) || !Directory.Exists(musicPath))
        {
            System.Windows.MessageBox.Show("Invalid path in config.xml", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            IsScanning = false;
            _progressWindow.Close();
            return;
        }

        var files = Directory.GetFiles(musicPath, "*.mp3", SearchOption.AllDirectories);
        int totalFiles = files.Length;
        if (totalFiles == 0)
        {
            RichTextContent = "No MP3 files found.";
            IsScanning = false;
            _progressWindow.Close();
            return;
        }

        var songs = new List<Song>();

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
                            energy: 0, // No disponible en los tags
                            key: string.Empty, // No disponible en los tags
                            popularity: 0, // No disponible en los tags
                            fileName: Path.GetFileName(files[i]),
                            filePath: Path.GetDirectoryName(files[i]),
                            country: string.Empty, // No disponible en los tags
                            myScore: string.Empty, // Valor predeterminado
                            comment: string.Empty, // Valor predeterminado
                            danceability: 0, // Valor predeterminado
                            loudness: 0, // Valor predeterminado
                            speechiness: 0, // Valor predeterminado
                            acousticness: 0, // Valor predeterminado
                            instrumentalness: 0, // Valor predeterminado
                            liveness: 0, // Valor predeterminado
                            valence: 0, // Valor predeterminado
                            durationMs: 0, // Valor predeterminado
                            mode: -1, // Valor predeterminado (mayor)
                            timeSignature: 0, // Valor predeterminado
                            isSearchedOnSpotify: false, // No buscado aún
                            noMatchOnSpotify: false // No se ha intentado buscar
                        );

                        songs.Add(song);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {files[i]}: {ex.Message}");
                }

                Progress = (i + 1) * 100 / totalFiles;
            }
        });


        // Generar el archivo Excel
        try
        {
            ExcelPackage.License.SetNonCommercialPersonal("<LaPausa.org>");


            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Songs");

                // Escribir encabezados
                worksheet.Cells[1, 1].Value = "Title";
                worksheet.Cells[1, 2].Value = "Artist";
                worksheet.Cells[1, 3].Value = "BPM";
                worksheet.Cells[1, 4].Value = "Genre";
                worksheet.Cells[1, 5].Value = "Year";
                worksheet.Cells[1, 6].Value = "Energy";
                worksheet.Cells[1, 7].Value = "Key";
                worksheet.Cells[1, 8].Value = "Popularity";
                worksheet.Cells[1, 9].Value = "FileName";
                worksheet.Cells[1, 10].Value = "FilePath";
                worksheet.Cells[1, 11].Value = "Country";
                worksheet.Cells[1, 12].Value = "MyScore";
                worksheet.Cells[1, 13].Value = "Comment";
                worksheet.Cells[1, 14].Value = "Danceability";
                worksheet.Cells[1, 15].Value = "Loudness";
                worksheet.Cells[1, 16].Value = "Speechiness";
                worksheet.Cells[1, 17].Value = "Acousticness";
                worksheet.Cells[1, 18].Value = "Instrumentalness";
                worksheet.Cells[1, 19].Value = "Liveness";
                worksheet.Cells[1, 20].Value = "Valence";
                worksheet.Cells[1, 21].Value = "DurationMs";
                worksheet.Cells[1, 22].Value = "Mode";
                worksheet.Cells[1, 23].Value = "TimeSignature";
                worksheet.Cells[1, 24].Value = "IsSearchedOnSpotify";
                worksheet.Cells[1, 25].Value = "NoMatchOnSpotify";

                // Escribir datos
                for (int i = 0; i < songs.Count; i++)
                {
                    var song = songs[i];
                    worksheet.Cells[i + 2, 1].Value = song.Title;
                    worksheet.Cells[i + 2, 2].Value = song.Artist;
                    worksheet.Cells[i + 2, 3].Value = song.BPM;
                    worksheet.Cells[i + 2, 4].Value = song.Genre;
                    worksheet.Cells[i + 2, 5].Value = song.Year;
                    worksheet.Cells[i + 2, 6].Value = song.Energy;
                    worksheet.Cells[i + 2, 7].Value = song.Key;
                    worksheet.Cells[i + 2, 8].Value = song.Popularity;
                    worksheet.Cells[i + 2, 9].Value = song.FileName;
                    worksheet.Cells[i + 2, 10].Value = song.FilePath;
                    worksheet.Cells[i + 2, 11].Value = song.Country;
                    worksheet.Cells[i + 2, 12].Value = song.MyScore;
                    worksheet.Cells[i + 2, 13].Value = song.Comment;
                    worksheet.Cells[i + 2, 14].Value = song.Danceability;
                    worksheet.Cells[i + 2, 15].Value = song.Loudness;
                    worksheet.Cells[i + 2, 16].Value = song.Speechiness;
                    worksheet.Cells[i + 2, 17].Value = song.Acousticness;
                    worksheet.Cells[i + 2, 18].Value = song.Instrumentalness;
                    worksheet.Cells[i + 2, 19].Value = song.Liveness;
                    worksheet.Cells[i + 2, 20].Value = song.Valence;
                    worksheet.Cells[i + 2, 21].Value = song.DurationMs;
                    worksheet.Cells[i + 2, 22].Value = song.Mode;
                    worksheet.Cells[i + 2, 23].Value = song.TimeSignature;
                    worksheet.Cells[i + 2, 24].Value = song.IsSearchedOnSpotify;
                    worksheet.Cells[i + 2, 25].Value = song.NoMatchOnSpotify;
                }

                // Guardar el archivo Excel
                package.SaveAs(new FileInfo(excelFilePath));
            }

            RichTextContent += $"Excel file created successfully at {excelFilePath}\n";
        }
        catch (Exception ex)
        {
            RichTextContent += $"Error creating Excel file: {ex.Message}\n";
        }

        IsScanning = false;
        _progressWindow.Close(); // Cerrar la ventana de progreso
    }

    private async Task UpdateFromExcelAsync()
    {
        IsScanning = true;
        Progress = 0;
        RichTextContent = "Updating songs from old Excel...\n";

        // Mostrar la ventana de progreso
        _progressWindow = new ProgressWindow
        {
            DataContext = this
        };
        _progressWindow.Show();

        Config config = Config.Load();
        string newExcelPath = config.ExcelPath;
        string oldExcelPath = config.ExcelPathOld;


        if (string.IsNullOrEmpty(newExcelPath) || string.IsNullOrEmpty(oldExcelPath) ||
            !System.IO.File.Exists(newExcelPath) || !System.IO.File.Exists(oldExcelPath))
        {
            System.Windows.MessageBox.Show("Invalid Excel paths in config.xml", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            IsScanning = false;
            _progressWindow.Close();
            return;
        }

        try
        {
            var newSongs = new Dictionary<string, Dictionary<string, object>>();
            var oldSongs = new Dictionary<string, Dictionary<string, object>>();
            ExcelPackage.License.SetNonCommercialPersonal("<LaPausa.org>");

            // Leer el nuevo archivo Excel
            await Task.Run(() =>
            {
                using (var package = new ExcelPackage(new FileInfo(newExcelPath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) throw new Exception("New Excel file is empty.");

                    int rows = worksheet.Dimension.Rows;
                    int cols = worksheet.Dimension.Columns;

                    // Encuentra dinámicamente la columna "Title"
                    int titleColumnIndex = -1;
                    for (int col = 1; col <= cols; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (header.Equals("Title", StringComparison.OrdinalIgnoreCase))
                        {
                            titleColumnIndex = col;
                            break;
                        }
                    }
                    if (titleColumnIndex == -1)
                        throw new Exception("No 'Title' column found in the new Excel file.");

                    for (int row = 2; row <= rows; row++) // Asumiendo que la fila 1 son encabezados
                    {
                        var title = worksheet.Cells[row, titleColumnIndex].Text; // Columna "Title"
                        if (string.IsNullOrEmpty(title)) continue;

                        var songData = new Dictionary<string, object>();
                        for (int col = 1; col <= cols; col++)
                        {
                            var header = worksheet.Cells[1, col].Text;
                            var value = worksheet.Cells[row, col].Value;
                            songData[header] = value;
                        }
                        newSongs[title] = songData;

                        // Actualizar el progreso
                        Progress = (row - 1) * 100 / rows / 3; // Progreso entre 0% y 33%
                    }
                }
            });

            // Leer el archivo Excel antiguo
            await Task.Run(() =>
            {
                using (var package = new ExcelPackage(new FileInfo(oldExcelPath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) throw new Exception("Old Excel file is empty.");

                    int rows = worksheet.Dimension.Rows;
                    int cols = worksheet.Dimension.Columns;

                    // Encuentra dinámicamente la columna "Title"
                    int titleColumnIndex = -1;
                    for (int col = 1; col <= cols; col++)
                    {
                        var header = worksheet.Cells[1, col].Text.Trim();
                        if (header.Equals("Title", StringComparison.OrdinalIgnoreCase))
                        {
                            titleColumnIndex = col;
                            break;
                        }
                    }
                    if (titleColumnIndex == -1)
                        throw new Exception("No 'Title' column found in the old Excel file.");

                    for (int row = 2; row <= rows; row++) 
                    {
                        var title = worksheet.Cells[row, titleColumnIndex].Text; // Columna "Title"
                        if (string.IsNullOrEmpty(title)) continue;

                        var songData = new Dictionary<string, object>();
                        for (int col = 1; col <= cols; col++)
                        {
                            var header = worksheet.Cells[1, col].Text;
                            var value = worksheet.Cells[row, col].Value;
                            songData[header] = value;
                        }
                        oldSongs[title] = songData;

                        // Actualizar el progreso
                        Progress = 33 + (row - 1) * 100 / rows / 3; // Progreso entre 33% y 66%
                    }
                }
            });

            // Actualizar el nuevo archivo Excel
            await Task.Run(() =>
            {
                using (var package = new ExcelPackage(new FileInfo(newExcelPath)))
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                    if (worksheet == null) throw new Exception("New Excel file is empty.");

                    int rows = worksheet.Dimension.Rows;
                    int cols = worksheet.Dimension.Columns;

                    // Obtener encabezados
                    var headers = new List<string>();
                    for (int col = 1; col <= cols; col++)
                    {
                        headers.Add(worksheet.Cells[1, col].Text);
                    }

                    int titleColumnIndex = headers.FindIndex(h => h.Equals("Title", StringComparison.OrdinalIgnoreCase)) + 1;
                    if (titleColumnIndex == 0)
                        throw new Exception("No 'Title' column found.");

                    for (int row = 2; row <= rows; row++) // Asumiendo que la fila 1 son encabezados
                    {
                        var title = worksheet.Cells[row, titleColumnIndex].Text;
                        if (string.IsNullOrEmpty(title) || !oldSongs.ContainsKey(title)) continue;

                        var oldSongData = oldSongs[title];

                        for (int col = 1; col <= cols; col++)
                        {
                            var header = worksheet.Cells[1, col].Text;

                            if (!oldSongData.ContainsKey(header)) continue;

                            var newValue = worksheet.Cells[row, col].Value;
                            var oldValue = oldSongData[header];

                            bool isNullOrZero = newValue == null ||
                                                (newValue is double d && d == 0) ||
                                                (newValue is string s && string.IsNullOrWhiteSpace(s));

                            if (isNullOrZero && oldValue != null)
                            {
                                worksheet.Cells[row, col].Value = oldValue;
                            }
                        }

                        // Actualizar el progreso
                        Progress = 66 + (row - 1) * 100 / rows / 3;
                    }

                    package.Save();
                }
            });

            RichTextContent += "Update completed successfully.\n";
        }
        catch (Exception ex)
        {
            RichTextContent += $"Error updating songs: {ex.Message}\n";
        }

        IsScanning = false;
        Progress = 100; // Asegurarse de que el progreso llegue al 100%
        _progressWindow.Close(); // Cerrar la ventana de progreso
    }




}
