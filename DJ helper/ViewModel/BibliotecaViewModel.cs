using DJ_helper.Model;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DJ_helper.ViewModel
{
    public class BibliotecaViewModel : INotifyPropertyChanged
    {
        private readonly ConfiguracionManager _configManager;
        private readonly BibliotecaCanciones _bibliotecaCanciones;
        private int _progreso;
        private string _archivoActual;

        public BibliotecaViewModel()
        {
            // Inicializa ConfiguracionManager con la ruta al archivo XML
            _configManager = new ConfiguracionManager("Model/config.xml");

            // Obtén y asigna los valores de configuración iniciales
            RutaExcel = _configManager.ObtenerRutaExcel();
            SpotifyAPIKey = _configManager.ObtenerSpotifyAPIKey();
            NombreBiblioteca = _configManager.ObtenerNombreBiblioteca();

            // Inicializa la biblioteca de canciones y comandos
            _bibliotecaCanciones = new BibliotecaCanciones();
            _bibliotecaCanciones.ProgresoActualizado += (progreso, archivo) =>
            {
                Progreso = progreso;
                ArchivoActual = archivo;
            };

            Canciones = new BindingList<Cancion>();
            ExportarCommand = new RelayCommand(async _ => await ExportarCanciones());
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
        public ICommand ActualizarRutaExcelCommand { get; }
        public ICommand ExportarCommand { get; }

        #region Propiedades de Progreso y Archivo Actual
        // Propiedad para mostrar el progreso de exportación
        public int Progreso
        {
            get => _progreso;
            set { _progreso = value; OnPropertyChanged(nameof(Progreso)); }
        }

        // Propiedad para mostrar el archivo actual en proceso
        public string ArchivoActual
        {
            get => _archivoActual;
            set { _archivoActual = value; OnPropertyChanged(nameof(ArchivoActual)); }
        }
        #endregion

        #region Creación de archivo Excel y Exportación
        // Método para exportar canciones a un archivo Excel
        private async Task ExportarCanciones()
        {
            // Llama al método para exportar canciones, usando los datos de configuración
            await _bibliotecaCanciones.ExportarCancionesAExcel(
                _configManager.ObtenerSourcePath(),
                _configManager.ObtenerRutaExcel(),
                _configManager.ObtenerNombreBiblioteca()
            );
        }
        #endregion

        #region Carga de datos desde Excel
        // Método para cargar canciones desde la ruta de Excel
        private void CargarCanciones()
        {
            _bibliotecaCanciones.CargarDesdeExcel(RutaExcel);
            Canciones.Clear();

            // Agregar las canciones cargadas a la lista enlazada
            foreach (var cancion in _bibliotecaCanciones.Canciones)
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
        #endregion

        #region Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
