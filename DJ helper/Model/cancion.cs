using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJ_helper.Model
{
    public class Cancion
    {
        public string Nombre { get; set; }        // Nombre del archivo de la canción
        public string Titulo { get; set; }        // Título de la canción
        public string Artista { get; set; }       // Artista o grupo
        public string Genero { get; set; }        // Género musical
        public string Ubicacion { get; set; }     // Ruta de archivo o ubicación
        public int BPM { get; set; }              // Beats per minute
        public string Clave { get; set; }         // Clave musical o Key
        public string Pais { get; set; }          // País de origen
        public int Valoracion { get; set; }       // Valoración o puntaje
        public string Comentario { get; set; }    // Comentarios adicionales
    }
}
