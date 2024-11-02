using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml; // Espacio de nombres para EPPlus

namespace DJ_helper.Model
{
    internal class BibliotecaCanciones
    {
        // Lista de canciones cargadas desde el archivo Excel
        public List<Cancion> Canciones { get; private set; }

        // Constructor
        public BibliotecaCanciones()
        {
            Canciones = new List<Cancion>();
        }

        // Método para cargar las canciones desde un archivo Excel
        public void CargarDesdeExcel(string rutaArchivo)
        {
            // Verifica que el archivo existe antes de intentar abrirlo
            if (!File.Exists(rutaArchivo))
            {
                throw new FileNotFoundException("El archivo de canciones no se encontró.", rutaArchivo);
            }

            // Limpia la lista antes de cargar nuevas canciones
            Canciones.Clear();

            // Configuración de EPPlus para trabajar con el archivo Excel
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage(new FileInfo(rutaArchivo)))
            {
                var hoja = package.Workbook.Worksheets[0]; // Accede a la primera hoja
                int filas = hoja.Dimension.Rows;

                // Itera por cada fila, comenzando en la segunda para omitir los encabezados
                for (int i = 2; i <= filas; i++)
                {
                    var cancion = new Cancion
                    {
                        Nombre = hoja.Cells[i, 1].Text,
                        Titulo = hoja.Cells[i, 2].Text,
                        Artista = hoja.Cells[i, 3].Text,
                        Genero = hoja.Cells[i, 4].Text,
                        Ubicacion = hoja.Cells[i, 5].Text,
                        BPM = int.TryParse(hoja.Cells[i, 6].Text, out int bpm) ? bpm : 0,
                        Clave = hoja.Cells[i, 7].Text,
                        Pais = hoja.Cells[i, 8].Text,
                        Valoracion = int.TryParse(hoja.Cells[i, 9].Text, out int valoracion) ? valoracion : 0,
                        Comentario = hoja.Cells[i, 10].Text
                    };

                    Canciones.Add(cancion);
                }
            }
        }
    }
}
