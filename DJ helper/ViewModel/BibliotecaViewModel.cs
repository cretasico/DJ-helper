using DJ_helper.Model;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using System;

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
            // Inicializa ConfiguracionManager con una ruta más segura
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Model/config.xml");
            _configManager = new ConfiguracionManager(configPath);

            // Obtén y asigna los valores de configuración iniciales
            RutaExcel = _configManager.ObtenerRutaExcel();
            SpotifyAPIKey = _configManager.ObtenerSpotifyAPIKey();
            NombreBiblioteca = _configManager.ObtenerNombreBiblioteca();

            // Inicializa la biblioteca de canciones y sus eventos
            _bibliotecaCanciones = new BibliotecaCanciones();
            _bibliotecaCanciones.ProgresoActualizado += (progreso, archivo) =>
            {
                Progreso = progreso;
                ArchivoActual = archivo;
            };

            Canciones = new BindingList<Cancion>();

            // Inicializa comandos
            ExportarCommand = new AsyncRelayCommand(ExportarCanciones);
            CargarCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(CargarCanciones);
            ActualizarRutaExcelCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ActualizarRutaExcel);

        }

        // Propiedades de configuración
        public string RutaExcel { get; set; }
        public string SpotifyAPIKey { get; set; }
        public string NombreBiblioteca { get; set; }

        // Lista de canciones para la UI
        public BindingList<Cancion> Canciones { get; private set; }


        // Comandos para la UI
        public IAsyncRelayCommand ExportarCommand { get; }
        public IRelayCommand CargarCommand { get; }
        public IRelayCommand ActualizarRutaExcelCommand { get; }

        #region Propiedades de Progreso y Archivo Actual
        public int Progreso
        {
            get => _progreso;
            set
            {
                if (_progreso != value)
                {
                    _progreso = value;
                    OnPropertyChanged(nameof(Progreso));
                }
            }
        }

        public string ArchivoActual
        {
            get => _archivoActual;
            set
            {
                if (_archivoActual != value)
                {
                    _archivoActual = value;
                    OnPropertyChanged(nameof(ArchivoActual));
                }
            }
        }
        #endregion

        #region Métodos para Exportación y Carga
        private async Task ExportarCanciones()
        {
            try
            {
                await _bibliotecaCanciones.ExportarCancionesAExcel(
                    _configManager.ObtenerSourcePath(),
                    RutaExcel,
                    NombreBiblioteca
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al exportar canciones: {ex.Message}");
            }
        }

        private void CargarCanciones()
        {
            try
            {
                _bibliotecaCanciones.CargarDesdeExcel(RutaExcel);
                Canciones.Clear(); // Limpia la lista actual sin reemplazar la instancia
                foreach (var cancion in _bibliotecaCanciones.Canciones)
                {
                    Canciones.Add(cancion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar canciones: {ex.Message}");
            }
        }


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
