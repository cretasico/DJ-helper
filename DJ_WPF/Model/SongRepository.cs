using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DJ_WPF.Model;
using System.Reflection.Emit;

namespace DJ_WPF.Model
{
    public class SongRepository
    {
        private readonly string _excelPath;

        public SongRepository(Config config)
        {
            _excelPath = config.ExcelPath;
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage.License.SetNonCommercialPersonal("<LaPausa.org>"); 

        }

        public List<Song> LoadSongs()
        {
            var songs = new List<Song>();

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("Excel file not found.", _excelPath);

            using (var package = new ExcelPackage(new FileInfo(_excelPath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return songs;

                int rows = worksheet.Dimension.Rows;
                int cols = worksheet.Dimension.Columns;
                var headers = new List<string>();

                for (int col = 1; col <= cols; col++)
                    headers.Add(worksheet.Cells[1, col].Text);

                for (int row = 2; row <= rows; row++)
                {
                    var song = new Song(
                        title: worksheet.Cells[row, headers.IndexOf("Title") + 1].Text,
                        artist: worksheet.Cells[row, headers.IndexOf("Artist") + 1].Text,
                        bpm: int.TryParse(worksheet.Cells[row, headers.IndexOf("BPM") + 1].Text, out int bpm) ? bpm : 0,
                        genre: worksheet.Cells[row, headers.IndexOf("Genre") + 1].Text,
                        year: int.TryParse(worksheet.Cells[row, headers.IndexOf("Year") + 1].Text, out int year) ? year : 0,
                        energy: double.TryParse(worksheet.Cells[row, headers.IndexOf("Energy") + 1].Text, out double energy) ? energy : 0,
                        key: worksheet.Cells[row, headers.IndexOf("Key") + 1].Text,
                        popularity: int.TryParse(worksheet.Cells[row, headers.IndexOf("Popularity") + 1].Text, out int pop) ? pop : 0,
                        fileName: worksheet.Cells[row, headers.IndexOf("FileName") + 1].Text,
                        filePath: worksheet.Cells[row, headers.IndexOf("FilePath") + 1].Text,
                        country: worksheet.Cells[row, headers.IndexOf("Country") + 1].Text,
                        myScore: worksheet.Cells[row, headers.IndexOf("MyScore") + 1].Text,
                        comment: worksheet.Cells[row, headers.IndexOf("Comment") + 1].Text
                    );

                    songs.Add(song);
                }
            }
            return songs;
        }

        public void SaveSongs(List<Song> songs)
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Songs");

                var headers = new[] { "Title", "Artist", "BPM", "Genre", "Year", "Energy", "Key", "Popularity", "FileName", "FilePath", "Country", "MyScore", "Comment" };
                for (int col = 0; col < headers.Length; col++)
                    worksheet.Cells[1, col + 1].Value = headers[col];

                for (int row = 0; row < songs.Count; row++)
                {
                    var s = songs[row];
                    worksheet.Cells[row + 2, 1].Value = s.Title;
                    worksheet.Cells[row + 2, 2].Value = s.Artist;
                    worksheet.Cells[row + 2, 3].Value = s.BPM;
                    worksheet.Cells[row + 2, 4].Value = s.Genre;
                    worksheet.Cells[row + 2, 5].Value = s.Year;
                    worksheet.Cells[row + 2, 6].Value = s.Energy;
                    worksheet.Cells[row + 2, 7].Value = s.Key;
                    worksheet.Cells[row + 2, 8].Value = s.Popularity;
                    worksheet.Cells[row + 2, 9].Value = s.FileName;
                    worksheet.Cells[row + 2, 10].Value = s.FilePath;
                    worksheet.Cells[row + 2, 11].Value = s.Country;
                    worksheet.Cells[row + 2, 12].Value = s.MyScore;
                    worksheet.Cells[row + 2, 13].Value = s.Comment;
                }

                package.SaveAs(new FileInfo(_excelPath));
            }
        }

        public Song FindByTitle(string title)
        {
            return LoadSongs().FirstOrDefault(s => s.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdateSong(Song updatedSong)
        {
            var songs = LoadSongs();
            var existing = songs.FirstOrDefault(s => s.Title.Equals(updatedSong.Title, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                songs[songs.IndexOf(existing)] = updatedSong;
                SaveSongs(songs);
            }
        }

        public void BackupExcel()
        {
            var backupPath = Path.Combine(Path.GetDirectoryName(_excelPath),
                                          Path.GetFileNameWithoutExtension(_excelPath) + "_backup_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx");
            File.Copy(_excelPath, backupPath);
        }

        public List<Song> FindSongsByColumnValue(string columnName, string value)
        {
            var matchingSongs = new List<Song>();

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("Excel file not found.", _excelPath);

            using (var package = new ExcelPackage(new FileInfo(_excelPath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return matchingSongs;

                int rows = worksheet.Dimension.Rows;
                int cols = worksheet.Dimension.Columns;
                var headers = new List<string>();

                for (int col = 1; col <= cols; col++)
                    headers.Add(worksheet.Cells[1, col].Text);

                int targetColumnIndex = headers.IndexOf(columnName);
                if (targetColumnIndex == -1)
                    throw new ArgumentException($"Column '{columnName}' not found.");

                for (int row = 2; row <= rows; row++)
                {
                    var cellValue = worksheet.Cells[row, targetColumnIndex + 1].Text;
                    if (cellValue.Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        var song = new Song(
                            title: worksheet.Cells[row, headers.IndexOf("Title") + 1].Text,
                            artist: worksheet.Cells[row, headers.IndexOf("Artist") + 1].Text,
                            bpm: int.TryParse(worksheet.Cells[row, headers.IndexOf("BPM") + 1].Text, out int bpm) ? bpm : 0,
                            genre: worksheet.Cells[row, headers.IndexOf("Genre") + 1].Text,
                            year: int.TryParse(worksheet.Cells[row, headers.IndexOf("Year") + 1].Text, out int year) ? year : 0,
                            energy: double.TryParse(worksheet.Cells[row, headers.IndexOf("Energy") + 1].Text, out double energy) ? energy : 0,
                            key: worksheet.Cells[row, headers.IndexOf("Key") + 1].Text,
                            popularity: int.TryParse(worksheet.Cells[row, headers.IndexOf("Popularity") + 1].Text, out int pop) ? pop : 0,
                            fileName: worksheet.Cells[row, headers.IndexOf("FileName") + 1].Text,
                            filePath: worksheet.Cells[row, headers.IndexOf("FilePath") + 1].Text,
                            country: worksheet.Cells[row, headers.IndexOf("Country") + 1].Text,
                            myScore: worksheet.Cells[row, headers.IndexOf("MyScore") + 1].Text,
                            comment: worksheet.Cells[row, headers.IndexOf("Comment") + 1].Text
                        );

                        matchingSongs.Add(song);
                    }
                }
            }

            return matchingSongs;
        }
        public List<Song> FindSongsByTwoColumns(string columnName1, string value1, string columnName2, string value2)
        {
            var matchingSongs = new List<Song>();

            if (!File.Exists(_excelPath))
                throw new FileNotFoundException("Excel file not found.", _excelPath);

            using (var package = new ExcelPackage(new FileInfo(_excelPath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet == null) return matchingSongs;

                int rows = worksheet.Dimension.Rows;
                int cols = worksheet.Dimension.Columns;
                var headers = new List<string>();

                for (int col = 1; col <= cols; col++)
                    headers.Add(worksheet.Cells[1, col].Text);

                int index1 = headers.IndexOf(columnName1);
                int index2 = headers.IndexOf(columnName2);

                if (index1 == -1 || index2 == -1)
                    throw new ArgumentException("One or both column names were not found in the headers.");

                for (int row = 2; row <= rows; row++)
                {
                    var cellValue1 = worksheet.Cells[row, index1 + 1].Text;
                    var cellValue2 = worksheet.Cells[row, index2 + 1].Text;

                    if (cellValue1.Equals(value1, StringComparison.OrdinalIgnoreCase) &&
                        cellValue2.Equals(value2, StringComparison.OrdinalIgnoreCase))
                    {
                        var song = new Song(
                            title: worksheet.Cells[row, headers.IndexOf("Title") + 1].Text,
                            artist: worksheet.Cells[row, headers.IndexOf("Artist") + 1].Text,
                            bpm: int.TryParse(worksheet.Cells[row, headers.IndexOf("BPM") + 1].Text, out int bpm) ? bpm : 0,
                            genre: worksheet.Cells[row, headers.IndexOf("Genre") + 1].Text,
                            year: int.TryParse(worksheet.Cells[row, headers.IndexOf("Year") + 1].Text, out int year) ? year : 0,
                            energy: double.TryParse(worksheet.Cells[row, headers.IndexOf("Energy") + 1].Text, out double energy) ? energy : 0,
                            key: worksheet.Cells[row, headers.IndexOf("Key") + 1].Text,
                            popularity: int.TryParse(worksheet.Cells[row, headers.IndexOf("Popularity") + 1].Text, out int pop) ? pop : 0,
                            fileName: worksheet.Cells[row, headers.IndexOf("FileName") + 1].Text,
                            filePath: worksheet.Cells[row, headers.IndexOf("FilePath") + 1].Text,
                            country: worksheet.Cells[row, headers.IndexOf("Country") + 1].Text,
                            myScore: worksheet.Cells[row, headers.IndexOf("MyScore") + 1].Text,
                            comment: worksheet.Cells[row, headers.IndexOf("Comment") + 1].Text
                        );

                        matchingSongs.Add(song);
                    }
                }
            }

            return matchingSongs;
        }


    }
}
