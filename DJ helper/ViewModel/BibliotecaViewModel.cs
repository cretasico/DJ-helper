using DJ_helper.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;


namespace DJ_helper.ViewModel
{
    public class BibliotecaViewModel : INotifyPropertyChanged
    {
        private BibliotecaCanciones bibliotecaCanciones;

        public BibliotecaViewModel()
        {
            bibliotecaCanciones = new BibliotecaCanciones();
            Canciones = new BindingList<Cancion>(); // Lista enlazable
            CargarCommand = new RelayCommand(CargarCanciones); // Comando para cargar canciones
        }

        // Lista de canciones que se enlazará a la vista
        public BindingList<Cancion> Canciones { get; private set; }

        // Comando para cargar canciones
        public ICommand CargarCommand { get; }

        // Método para cargar las canciones usando el modelo
        private void CargarCanciones()
        {
            bibliotecaCanciones.CargarDesdeExcel(@"E:\Musica-E\Organizacion\Biblioteca.xlsx");
            Canciones.Clear();

            foreach (var cancion in bibliotecaCanciones.Canciones)
            {
                Canciones.Add(cancion);
            }

            OnPropertyChanged(nameof(Canciones));
        }

        // Implementación de INotifyPropertyChanged para actualizaciones de la vista
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
