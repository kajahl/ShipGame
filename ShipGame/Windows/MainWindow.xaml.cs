using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Statki2.Objects;
using Statki2.ViewModels;
using Statki2.Windows;

namespace Statki2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Game game;
        private readonly ViewModel model;
        public MainWindow()
        {
            InitializeComponent();

            this.game = new Game(this);
            this.model = new ViewModel(this, game);
        }

        // Całościowe aktualizowanie głównego okna gry
        public void UpdateView()
        {
            this.model.UpdateGrids();
            this.model.UpdateFields();
        }

        private void Handle_MenuClick_AboutWindow(object sender, RoutedEventArgs e)
        {
            AboutWindow aboutWindow = new();
            aboutWindow.Show();
        }

        private void Handle_MenuClick_GameInstructions(object sender, RoutedEventArgs e)
        {
            GameInstructions gameInstructions = new();
            gameInstructions.Show();
        }

        private void Handle_MenuClick_RestartGame(object sender, RoutedEventArgs e)
        {
            string? process = Process.GetCurrentProcess().MainModule?.FileName;
            if (process == null) return;
            Process.Start(process);
            Application.Current.Shutdown();
        }
    }
}
