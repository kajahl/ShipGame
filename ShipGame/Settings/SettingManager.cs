using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Statki2.Objects;

namespace Statki2.Settings
{

    class SettingManager
    {
        // Pola na podstawie których określana jest szerokość i wysokość pojedynczego "pola" na polu gry
        public static double FullWindowWidth = 1200.0;
        public static double FullWindowHeight = 675.0;

        // Kolory pól w zależności od stanu
        public static BoardStateColor EmptyField = new(Brushes.Black, Brushes.Cyan, "Puste pole");
        public static BoardStateColor ShipField = new(Brushes.White, Brushes.Black, "Statek");
        public static BoardStateColor SingleFieldShipBombed = new(Brushes.White, Brushes.DarkRed, "Trafiona część statku");
        public static BoardStateColor FullShipBombed = new(Brushes.White, Brushes.DarkViolet, "Zatopiony statek");
        public static BoardStateColor EmptyFieldBombed = new(Brushes.Black, Brushes.LimeGreen, "Trafione puste pole");

        public static List<BoardStateColor> BoardStateColors = new()
        {
            EmptyField,
            ShipField,
            SingleFieldShipBombed,
            FullShipBombed,
            EmptyFieldBombed
        };

        // Rozmiar pola gry
        public BoardSizeSetting boardSize = new();

        // Typ i ilość dostępnych statków
        public ShipLengthsSetting shipLengths = new();

        // Widoczność statków przeciwnika
        public ShowEnemyShipsSetting showEnemyShips = new();

        // Todo: Save & Load
        public void SaveSettings()
        {

        }

        public void LoadSettings()
        {

        }
    }
}
