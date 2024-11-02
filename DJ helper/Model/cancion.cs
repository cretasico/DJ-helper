using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DJ_helper.Model
{
    public class Cancion
    {
        // Propiedades fijas con nombres y tipos específicos
        public string Title { get; set; }           // Título de la canción
        public string Artist { get; set; }          // Artista o grupo
        public int BPM { get; set; }                // Beats per minute
        public string Genre { get; set; }           // Género musical
        public int Date { get; set; }               // Año de lanzamiento
        public double Energy { get; set; }          // Energía (de Spotify API)
        public string Key { get; set; }             // Clave musical
        public int Popularity { get; set; }         // Popularidad (de Spotify API)

        // Propiedades adicionales específicas del proyecto
        public string FileName { get; set; }        // Nombre del archivo
        public string Path { get; set; }            // Ruta de archivo
        public string Country { get; set; }         // País de origen
        public int MyScore { get; set; }            // Valoración personal
        public string Comment { get; set; }         // Comentarios adicionales

        // Constructor para inicializar las propiedades
        public Cancion(string title, string artist, int bpm, string genre, int date, double energy, string key, int popularity,
                       string fileName, string path, string country, int myScore, string comment)
        {
            Title = title;
            Artist = artist;
            BPM = bpm;
            Genre = genre;
            Date = date;            // Solo el año
            Energy = energy;
            Key = key;
            Popularity = popularity;
            FileName = fileName;
            Path = path;
            Country = country;
            MyScore = myScore;
            Comment = comment;
        }
    }
}
