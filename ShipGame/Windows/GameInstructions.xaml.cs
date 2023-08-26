using Statki2.Controls;
using Statki2.Objects;
using Statki2.Settings;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Statki2.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy GameInstructions.xaml
    /// </summary>
    public partial class GameInstructions
    {
        public GameInstructions()
        {
            InitializeComponent();
            int pos = 0;
            foreach (BoardStateColor boardStateColor in SettingManager.BoardStateColors)
            {
                FieldColorExplanationControl field = new(boardStateColor);
                ColorsExplanationGrid.Children.Add(field);
                Grid.SetColumn(field, pos % 3);
                Grid.SetRow(field, pos / 3);
                pos++;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
