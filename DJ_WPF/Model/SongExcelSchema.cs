using System;
using System.Collections.Generic;

namespace DJ_WPF.Model
{
    public record ExcelColumn(string Header, Func<Song, object?> Getter, Action<Song, string>? Setter = null);

    public static class SongExcelSchema
    {
        public const string SheetName = "Songs";

        public static readonly IReadOnlyList<ExcelColumn> Columns = new List<ExcelColumn>
        {
            new("Title", s => s.Title, (s,v) => s.Title = v),
            new("Artist", s => s.Artist, (s,v) => s.Artist = v),
            new("BPM", s => s.BPM, (s,v) => s.BPM = int.TryParse(v, out var x) ? x : 0),
            new("Genre", s => s.Genre, (s,v) => s.Genre = v),
            new("Year", s => s.Year, (s,v) => s.Year = int.TryParse(v, out var x) ? x : 0),
            new("Energy", s => s.Energy, (s,v) => s.Energy = double.TryParse(v, out var x) ? x : 0),
            new("Key", s => s.Key, (s,v) => s.Key = v),
            new("Popularity", s => s.Popularity, (s,v) => s.Popularity = int.TryParse(v, out var x) ? x : 0),

            new("FileName", s => s.FileName, (s,v) => s.FileName = v),
            new("FilePath", s => s.FilePath, (s,v) => s.FilePath = v),
            new("Country", s => s.Country, (s,v) => s.Country = v),
            new("MyScore", s => s.MyScore, (s,v) => s.MyScore = v),
            new("Comment", s => s.Comment, (s,v) => s.Comment = v),

            
        };
    }
}
