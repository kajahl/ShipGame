using Statki2.Objects;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Statki2.Controls
{
    /// <summary>
    /// Logika interakcji dla klasy FieldColorExplanationControl.xaml
    /// </summary>
    public partial class FieldColorExplanationControl : UserControl
    {
        internal FieldColorExplanationControl(BoardStateColor boardStateColor)
        {
            InitializeComponent();
            mainLabel.Content = boardStateColor.description;
            mainLabel.Background = boardStateColor.background;
            mainLabel.Foreground = boardStateColor.foreground;
        }
    }
}
