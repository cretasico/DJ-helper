using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DJ_helper.ViewModel
{
    // RelayCommand implementa ICommand para crear comandos reutilizables en MVVM
    public class RelayCommand : ICommand
    {
        // Delegado que representa la acción principal a ejecutar por el comando
        private readonly Action _execute;

        // Delegado que representa una función para determinar si el comando se puede ejecutar
        private readonly Func<bool> _canExecute;

        // Constructor del comando, recibe la acción y la función de habilitación opcional
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            _execute = execute;            // Guarda la acción a ejecutar en el comando
            _canExecute = canExecute;      // Guarda la función para verificar si el comando puede ejecutarse
        }

        // Método CanExecute: determina si el comando puede ejecutarse
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute();

        // Método Execute: ejecuta la acción asociada al comando
        public void Execute(object parameter) => _execute();

        // Evento CanExecuteChanged: notifica cambios en el estado de habilitación del comando
        public event EventHandler CanExecuteChanged;

        // Método RaiseCanExecuteChanged: invoca CanExecuteChanged para actualizar la UI
        public void RaiseCanExecuteChanged()
        {
            // Verifica si alguien está suscrito a CanExecuteChanged y dispara el evento si es así
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
