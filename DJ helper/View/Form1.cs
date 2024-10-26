using DJ_helper.ViewModel;
using System;
using System.Drawing;
using System.Windows.Forms;
using Telerik.WinControls.UI;  // Importa el espacio de nombres de Telerik

namespace DJ_helper.View
{
    public partial class Form1 : Form
    {
        private BibliotecaViewModel viewModel;
        private Button btnCargar;
        private DataGridView dataGridBiblioteca;

        public Form1()
        {
            InitializeComponent();

            // Inicializar ViewModel
            viewModel = new BibliotecaViewModel();

            // Configura el formulario
            this.Text = "Biblioteca de Canciones";
            this.Size = new Size(800, 600);

            // Configurar botón "Cargar"
            btnCargar = new Button
            {
                Text = "Cargar",
                Size = new Size(100, 40),
                Location = new Point(20, 20)
            };
            btnCargar.Click += (sender, e) => viewModel.CargarCommand.Execute(null);

            // Configurar DataGridView
            dataGridBiblioteca = new DataGridView
            {
                Size = new Size(750, 500),
                Location = new Point(20, 80),
                AutoGenerateColumns = true,  // Generar columnas automáticamente
                DataSource = viewModel.Canciones // Enlazar lista de canciones
            };

            // Agregar controles al formulario
            this.Controls.Add(btnCargar);
            this.Controls.Add(dataGridBiblioteca);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}