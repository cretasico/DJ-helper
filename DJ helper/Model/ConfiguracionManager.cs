using System.Linq;
using System.Xml.Linq;

namespace DJ_helper.Model
{
    public class ConfiguracionManager
    {
        private readonly string _configFilePath;

        public ConfiguracionManager(string configFilePath)
        {
            _configFilePath = configFilePath;
        }

        // Métodos para obtener los valores
        public string ObtenerRutaExcel()
        {
            XDocument doc = XDocument.Load(_configFilePath);
            return doc.Root.Element("ExcelPath")?.Value;
        }
<<<<<<< HEAD
        public string ObtenerSourcePath()
        {
            XDocument doc = XDocument.Load(_configFilePath);
            return doc.Root.Element("Source")?.Value;
        }

=======
>>>>>>> DJ-helper/DJ-helper

        public string ObtenerSpotifyAPIKey()
        {
            XDocument doc = XDocument.Load(_configFilePath);
            return doc.Root.Element("SpotifyAPIKey")?.Value;
        }

        public string ObtenerNombreBiblioteca()
        {
            XDocument doc = XDocument.Load(_configFilePath);
            return doc.Root.Element("LibraryName")?.Value;
        }

        // Métodos para obtener etiquetas de archivos MP3
        public string ObtenerFileTag(string tagName)
        {
            XDocument doc = XDocument.Load(_configFilePath);
            var tagElement = doc.Root.Element("FileTags")?.Elements("Tag")
                .FirstOrDefault(x => x.Attribute("name")?.Value == tagName);
            return tagElement?.Value;
        }

        // Métodos para obtener etiquetas de Spotify API
        public string ObtenerSpotifyTag(string tagName)
        {
            XDocument doc = XDocument.Load(_configFilePath);
            var tagElement = doc.Root.Element("SpotifyTags")?.Elements("Tag")
                .FirstOrDefault(x => x.Attribute("name")?.Value == tagName);
            return tagElement?.Value;
        }

        // Métodos para actualizar los valores
        public void ActualizarRutaExcel(string nuevaRuta)
        {
            XDocument doc = XDocument.Load(_configFilePath);
            XElement rutaExcelElement = doc.Root.Element("ExcelPath");
            if (rutaExcelElement != null)
            {
                rutaExcelElement.Value = nuevaRuta;
                doc.Save(_configFilePath); // Guarda los cambios en el archivo XML
            }
        }

        public void ActualizarSpotifyAPIKey(string nuevaAPIKey)
        {
            XDocument doc = XDocument.Load(_configFilePath);
            XElement apiKeyElement = doc.Root.Element("SpotifyAPIKey");
            if (apiKeyElement != null)
            {
                apiKeyElement.Value = nuevaAPIKey;
                doc.Save(_configFilePath); // Guarda los cambios en el archivo XML
            }
        }

        public void ActualizarNombreBiblioteca(string nuevoNombre)
        {
            XDocument doc = XDocument.Load(_configFilePath);
            XElement nombreBibliotecaElement = doc.Root.Element("LibraryName");
            if (nombreBibliotecaElement != null)
            {
                nombreBibliotecaElement.Value = nuevoNombre;
                doc.Save(_configFilePath); // Guarda los cambios en el archivo XML
            }
        }
    }
}
