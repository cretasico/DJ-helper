using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DJ_WPF.Model
{
    public class Song
    {

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private string _myScore;
        public string MyScore
        {
            get => _myScore;
            set
            {
                if (_myScore != value)
                {
                    _myScore = value;
                    IsModified = true;
                    OnPropertyChanged(nameof(MyScore));
                }
            }
        }

        public bool IsModified { get; set; } = false;
        // Fixed properties with specific names and types
        public string Title { get; set; }          // Song title
        public string Artist { get; set; }         // Artist or band
        public int BPM { get; set; }               // Beats per minute
        public string Genre { get; set; }          // Music genre
        public int Year { get; set; }              // Release year
        public double Energy { get; set; }         // Energy (from Spotify API)
        public string Key { get; set; }            // Musical key
        public int Popularity { get; set; }        // Popularity (from Spotify API)

        // Additional properties specific to the project
        public string FileName { get; set; }       // File name
        public string FilePath { get; set; }       // File path
        public string Country { get; set; }        // Country of origin
        //public string MyScore { get; set; }           // Personal rating
        public string Comment { get; set; }        // Additional comments

        // New properties
        public float Danceability { get; set; }
        public float Loudness { get; set; }
        public float Speechiness { get; set; }
        public float Acousticness { get; set; }
        public float Instrumentalness { get; set; }
        public float Liveness { get; set; }
        public float Valence { get; set; }
        public int DurationMs { get; set; }
        public int Mode { get; set; } // 1 = mayor, 0 = menor
        public int TimeSignature { get; set; }
        public bool IsSearchedOnSpotify { get; set; } // Indicates if searched on Spotify
        public bool NoMatchOnSpotify { get; set; }    // Indicates if no match was found on Spotify


        // Constructor to initialize properties
        public Song(string title, string artist, int bpm, string genre, int year, double energy, string key, int popularity,
                    string fileName, string filePath, string country, string myScore, string comment,
                    float danceability = 0, float loudness = 0, float speechiness = 0, float acousticness = 0,
                    float instrumentalness = 0, float liveness = 0, float valence = 0, int durationMs = 0,
                    int mode = 1, int timeSignature = 4, bool isSearchedOnSpotify = false, bool noMatchOnSpotify = false)
        {
            Title = title;
            Artist = artist;
            BPM = bpm;
            Genre = genre;
            Year = year;         // Only the year
            Energy = energy;
            Key = key;
            Popularity = popularity;
            FileName = fileName;
            FilePath = filePath;
            Country = country;
            MyScore = myScore;
            Comment = comment;

            // Initialize new properties
            Danceability = danceability;
            Loudness = loudness;
            Speechiness = speechiness;
            Acousticness = acousticness;
            Instrumentalness = instrumentalness;
            Liveness = liveness;
            Valence = valence;
            DurationMs = durationMs;
            Mode = mode;
            TimeSignature = timeSignature;
            IsSearchedOnSpotify = isSearchedOnSpotify;
            NoMatchOnSpotify = noMatchOnSpotify;
        }


    }

}
