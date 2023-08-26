using Statki2.Objects;
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

namespace Statki2.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy ShipCountFieldControl.xaml
    /// </summary>
    public partial class ShipCountFieldControl : UserControl
    {
        readonly Gameboard gameboard;
        readonly KeyValuePair<int, int> shipKvp;
        internal ShipCountFieldControl(KeyValuePair<int,int> shipKvp, Gameboard gameboard)
        {
            InitializeComponent();
            this.gameboard = gameboard;
            this.shipKvp = shipKvp;
            shipLength.Content = shipKvp.Key;
            UpdateField();
        }

        public void UpdateField()
        {
            int currentShipCount = gameboard.GetCountLeftShips(shipKvp.Key);
            if (currentShipCount == shipKvp.Value) SetColor(Brushes.LimeGreen);
            else if (currentShipCount != 0) SetColor(Brushes.GreenYellow);
            else SetColor(Brushes.Gray);
            shipCount.Content = $"{currentShipCount}/{shipKvp.Value}";
        }

        private void SetColor(SolidColorBrush color)
        {
            shipLength.Background = color;
        }
    }
}
