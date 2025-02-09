using System;
using System.Collections.Generic;
using System.IO;
<<<<<<< HEAD
using System.Threading.Tasks;
using OfficeOpenXml; // Espacio de nombres para EPPlus
using TagLib; // Biblioteca para leer metadatos de archivos de audio
=======
using OfficeOpenXml; // Espacio de nombres para EPPlus
>>>>>>> DJ-helper/DJ-helper

namespace DJ_helper.Model
{
    public class BibliotecaCanciones
    {
        // Lista de canciones cargadas
        public List<Cancion> Canciones { get; private set; }

        // Evento para notificar el progreso de exportación
        public event Action<int, string> ProgresoActualizado;

        // Constructor
        public BibliotecaCanciones()
        {
            Canciones = new List<Cancion>();
        }

        #region Creating the Excel file at the first time
        // Método para exportar canciones de archivos MP3 en el directorio de origen a un archivo Excel
        public async Task ExportarCancionesAExcel(string sourcePath, string excelPath, string libraryName)
        {
            // Verificar la existencia del directorio fuente
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException($"El directorio {sourcePath} no existe.");

            // Obtener todos los archivos MP3 en el directorio
            var mp3Files = Directory.GetFiles(sourcePath, "*.mp3");
            if (mp3Files.Length == 0)
                throw new FileNotFoundException("No se encontraron archivos MP3 en el directorio especificado.");

            // Inicializa la lista de canciones
            List<Cancion> canciones = new List<Cancion>();

            // Procesar cada archivo y extraer metadatos
            for (int i = 0; i < mp3Files.Length; i++)
            {
                string filePath = mp3Files[i];
                var archivo = TagLib.File.Create(filePath); // Usar TagLib para leer metadatos

                // Crear instancia de Cancion con metadatos
                var cancion = new Cancion(
                    title: archivo.Tag.Title ?? string.Empty,
                    artist: archivo.Tag.FirstAlbumArtist ?? string.Empty,
                    bpm: (int)archivo.Tag.BeatsPerMinute, // Conversión explícita de uint a int
                    genre: archivo.Tag.FirstGenre ?? string.Empty,
                    date: (int)(archivo.Tag.Year != 0 ? (int)archivo.Tag.Year : DateTime.Now.Year), // Conversión explícita de uint a int
                    energy: 0, // Valores predeterminados si no están en los metadatos
                    key: string.Empty,
                    popularity: 0,
                    fileName: Path.GetFileName(filePath),
                    path: filePath,
                    country: string.Empty,
                    myScore: 0,
                    comment: archivo.Tag.Comment ?? string.Empty
                );


                canciones.Add(cancion);

                // Calcular y notificar el progreso
                int progreso = (int)((i + 1) * 100 / mp3Files.Length);
                ProgresoActualizado?.Invoke(progreso, cancion.FileName);
            }

            // Llamar al método para guardar la lista de canciones en Excel
            await GuardarEnExcel(canciones, excelPath, libraryName);
        }

        // Método para guardar la lista de canciones en un archivo Excel
        private async Task GuardarEnExcel(List<Cancion> canciones, string excelPath, string libraryName)
        {
            // Configurar licencia de EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            string fullPath = Path.Combine(excelPath, libraryName);

            using (var package = new ExcelPackage(new FileInfo(fullPath)))
            {
                var hoja = package.Workbook.Worksheets.Add("Canciones");

                // Definir encabezados
                hoja.Cells[1, 1].Value = "Title";
                hoja.Cells[1, 2].Value = "Artist";
                hoja.Cells[1, 3].Value = "BPM";
                hoja.Cells[1, 4].Value = "Genre";
                hoja.Cells[1, 5].Value = "Date";
                hoja.Cells[1, 6].Value = "Energy";
                hoja.Cells[1, 7].Value = "Key";
                hoja.Cells[1, 8].Value = "Popularity";
                hoja.Cells[1, 9].Value = "FileName";
                hoja.Cells[1, 10].Value = "Path";
                hoja.Cells[1, 11].Value = "Country";
                hoja.Cells[1, 12].Value = "MyScore";
                hoja.Cells[1, 13].Value = "Comment";

                // Llenar datos de canciones
                for (int i = 0; i < canciones.Count; i++)
                {
                    var cancion = canciones[i];
                    hoja.Cells[i + 2, 1].Value = cancion.Title;
                    hoja.Cells[i + 2, 2].Value = cancion.Artist;
                    hoja.Cells[i + 2, 3].Value = cancion.BPM;
                    hoja.Cells[i + 2, 4].Value = cancion.Genre;
                    hoja.Cells[i + 2, 5].Value = cancion.Date;
                    hoja.Cells[i + 2, 6].Value = cancion.Energy;
                    hoja.Cells[i + 2, 7].Value = cancion.Key;
                    hoja.Cells[i + 2, 8].Value = cancion.Popularity;
                    hoja.Cells[i + 2, 9].Value = cancion.FileName;
                    hoja.Cells[i + 2, 10].Value = cancion.Path;
                    hoja.Cells[i + 2, 11].Value = cancion.Country;
                    hoja.Cells[i + 2, 12].Value = cancion.MyScore;
                    hoja.Cells[i + 2, 13].Value = cancion.Comment;
                }

                // Guardar el archivo Excel
                await package.SaveAsync();
            }
        }

        #endregion

        #region Loading from Excel
        // Método para cargar las canciones desde un archivo Excel
        public void CargarDesdeExcel(string rutaArchivo)
        {
            // Verificar la existencia del archivo
            if (!System.IO.File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException("El archivo de canciones no se encontró.", rutaArchivo);
            }

<<<<<<< HEAD

            // Limpiar la lista antes de cargar nuevas canciones
=======
            // Limpia la lista antes de cargar nuevas canciones
>>>>>>> DJ-helper/DJ-helper
            Canciones.Clear();

            // Configuración de EPPlus para trabajar con el archivo Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(rutaArchivo)))
            {
                var hoja = package.Workbook.Worksheets[0]; // Accede a la primera hoja
                int filas = hoja.Dimension.Rows;

                // Iterar sobre cada fila, comenzando en la segunda para omitir los encabezados
                for (int i = 2; i <= filas; i++)
                {
                    var cancion = new Cancion(
                        title: hoja.Cells[i, 1].Text,
                        artist: hoja.Cells[i, 2].Text,
                        bpm: int.TryParse(hoja.Cells[i, 3].Text, out int bpm) ? bpm : 0,
                        genre: hoja.Cells[i, 4].Text,
                        date: int.TryParse(hoja.Cells[i, 5].Text, out int date) ? date : 0,
                        energy: double.TryParse(hoja.Cells[i, 6].Text, out double energy) ? energy : 0.0,
                        key: hoja.Cells[i, 7].Text,
                        popularity: int.TryParse(hoja.Cells[i, 8].Text, out int popularity) ? popularity : 0,
                        fileName: hoja.Cells[i, 9].Text,
                        path: hoja.Cells[i, 10].Text,
                        country: hoja.Cells[i, 11].Text,
                        myScore: int.TryParse(hoja.Cells[i, 12].Text, out int myScore) ? myScore : 0,
                        comment: hoja.Cells[i, 13].Text
                    );

                    Canciones.Add(cancion);
                }
            }
        }
        #endregion
    }
}
