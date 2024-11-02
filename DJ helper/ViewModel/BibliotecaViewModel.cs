using DJ_helper.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace DJ_helper.ViewModel
{
    public class BibliotecaViewModel : INotifyPropertyChanged
    {
        private readonly ConfiguracionManager _configManager;
        private BibliotecaCanciones bibliotecaCanciones;

        public BibliotecaViewModel()
        {
            // Inicializa ConfiguracionManager con la ruta al archivo XML
            _configManager = new ConfiguracionManager("Model/config.xml");

            // Obtén y asigna los valores de configuración iniciales
            RutaExcel = _configManager.ObtenerRutaExcel();
            SpotifyAPIKey = _configManager.ObtenerSpotifyAPIKey();
            NombreBiblioteca = _configManager.ObtenerNombreBiblioteca();

            // Inicializa la biblioteca de canciones y comandos
            bibliotecaCanciones = new BibliotecaCanciones();
            Canciones = new BindingList<Cancion>();
            CargarCommand = new RelayCommand(CargarCanciones);
            ActualizarRutaExcelCommand = new RelayCommand(ActualizarRutaExcel);
        }

        // Propiedades para las configuraciones
        public string RutaExcel { get; set; }
        public string SpotifyAPIKey { get; set; }
        public string NombreBiblioteca { get; set; }

        // Lista de canciones para enlazar a la vista
        public BindingList<Cancion> Canciones { get; private set; }

        // Comandos para la UI
        public ICommand CargarCommand { get; }
        public ICommand ActualizarRutaExcelCommand { get; } // Comando para actualizar la ruta de Excel

        // Método para cargar canciones desde la ruta de Excel
        private void CargarCanciones()
        {
            bibliotecaCanciones.CargarDesdeExcel(RutaExcel);
            Canciones.Clear();

            foreach (var cancion in bibliotecaCanciones.Canciones)
            {
                Canciones.Add(cancion);
            }

            OnPropertyChanged(nameof(Canciones));
        }

        // Método para actualizar la ruta de Excel en el archivo de configuración
        private void ActualizarRutaExcel()
        {
            _configManager.ActualizarRutaExcel(RutaExcel);
            OnPropertyChanged(nameof(RutaExcel));
        }

        // Implementación de INotifyPropertyChanged para actualizar la vista
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
