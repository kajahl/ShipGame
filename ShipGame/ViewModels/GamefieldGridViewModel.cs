using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Media3D;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using Statki2.Settings;
using Statki2.Objects;
using Statki2.Controls;

namespace Statki2.ViewModels
{
    class GamefieldGridViewModel
    {
        readonly Player player;
        readonly Grid grid;
        readonly RoutedEventHandler fieldClickHanlder;
        readonly Label boardName;
        readonly Grid gameboard;
        readonly List<ShipCountFieldControl> shipLenCountControls;

        public GamefieldGridViewModel(Player player, Grid grid, RoutedEventHandler fieldClickHanlder)
        {
            this.player = player;
            this.grid = grid;
            boardName = new Label();
            gameboard = new Grid();
            shipLenCountControls = new();
            this.fieldClickHanlder = fieldClickHanlder;
            CreateGrid();
        }

        // Tworzenie siatki gracza
        private void CreateGrid()
        {
            //Główna siatka
            Grid designGrid = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            //Wiersze głownej siatki
            RowDefinition nameRowDefinition = new()
            {
                Name = $"{player.PlayerName}BoardName",
                Height = new GridLength(50.0, GridUnitType.Pixel)
            };

            RowDefinition boardRowDefinition = new()
            {
                Name = $"{player.PlayerName}BoardGamefield",
                Height = new GridLength(1.0, GridUnitType.Star)
            };

            RowDefinition shipBoxRowDefinition = new()
            {
                Name = $"{player.PlayerName}ShipBox",
                Height = new GridLength(60.0, GridUnitType.Pixel)
            };

            designGrid.RowDefinitions.Add(nameRowDefinition);
            designGrid.RowDefinitions.Add(boardRowDefinition);
            designGrid.RowDefinitions.Add(shipBoxRowDefinition);

            //Zawartość pierwszego wiersza głównej siatki = Nazwa gracza
            designGrid.Children.Add(boardName);
            boardName.Content = player.PlayerName;
            boardName.HorizontalAlignment = HorizontalAlignment.Center;
            boardName.VerticalAlignment = VerticalAlignment.Center;
            boardName.FontWeight = FontWeights.Bold;
            boardName.FontSize = 20;
            Grid.SetRow(boardName, 0);

            //Zawartość drugiego wiersza głównej siatki = Siatka gry
            for (int i = 0; i < player.Gameboard.BoardSideSize; i++)
            {
                RowDefinition singleRowDefinition = new();
                ColumnDefinition singleColumnDefinition = new();

                gameboard.RowDefinitions.Add(singleRowDefinition);
                gameboard.ColumnDefinitions.Add(singleColumnDefinition);
            }

            gameboard.Width = SettingManager.FullWindowWidth / 2 - 20 - 20;
            gameboard.Height = SettingManager.FullWindowHeight - 250;

            designGrid.Children.Add(gameboard);
            Grid.SetRow(gameboard, 1);

            //Zawartość trzeciego wiersza głownej siatki = Lista statków
            Grid shipBox = new();
            IOrderedEnumerable<KeyValuePair<int, int>> shipLengths = from entry in this.player.game.SettingManager.shipLengths.getValue() orderby entry.Key ascending select entry;
            int shipBoxCount = 0;
            foreach(var shipKvp in shipLengths)
            {
                if (shipKvp.Value == 0) continue;

                ColumnDefinition singleColumnDefinition = new();
                ShipCountFieldControl shipCountFieldControl = new(shipKvp, this.player.Gameboard);
                shipLenCountControls.Add(shipCountFieldControl);
                shipBox.ColumnDefinitions.Add(singleColumnDefinition);
                shipBox.Children.Add(shipCountFieldControl);
                Grid.SetColumn(shipCountFieldControl, shipBoxCount++);
            }
            designGrid.Children.Add(shipBox);
            Grid.SetRow(shipBox, 2);

            grid.Children.Add(designGrid);
            CreateGamefield();
            UpdateGrid();
        }

        // Tworzenie pól dla statków
        private void CreateGamefield()
        {
            //Pola siatki gry
            for (int col = 0; col < player.Gameboard.BoardSideSize; col++)
            {
                for (int row = 0; row < player.Gameboard.BoardSideSize; row++)
                {
                    Button field = new()
                    {
                        Name = $"{player.PlayerName}{row}{col}",
                        Tag = $"{row}x{col}",
                        Content = player.Gameboard.GetFieldAt(row, col).GetFieldName()
                    };
                    field.Click += fieldClickHanlder;
                    field.Background = SettingManager.EmptyField.background;
                    field.Foreground = SettingManager.EmptyField.foreground;
                    //todo: change focus bg+fg color
                    Grid.SetColumn(field, col);
                    Grid.SetRow(field, row);

                    gameboard.Children.Add(field);
                }
            }
        }

        // Aktualizowanie siatki gracza
        public void UpdateGrid()
        {
            Button[] boardFields = gameboard.Children.Cast<Button>().ToArray();
            foreach (Button button in boardFields)
            {
                (int, int) pos = Utils.GetPositionFromButtonTag(button);

                Field field = player.Gameboard.GetFieldAt(pos.Item1, pos.Item2);

                // Podstawowe oznaczenia
                if (field.IsShip && player.Gameboard.DisplayShipsOnGrid)
                {
                    button.Background = SettingManager.ShipField.background;
                    button.Foreground = SettingManager.ShipField.foreground;
                }
                else
                {
                    button.Background = SettingManager.EmptyField.background;
                    button.Foreground = SettingManager.EmptyField.foreground;
                }

                // Oznaczenia bomb
                if (field.IsShip && field.IsBombed)
                {
                    if (player.Gameboard.IsShipFullBombedAt(field.GetPos()))
                    {
                        button.Background = SettingManager.FullShipBombed.background;
                        button.Foreground = SettingManager.FullShipBombed.foreground;
                    }
                    else
                    {
                        button.Background = SettingManager.SingleFieldShipBombed.background;
                        button.Foreground = SettingManager.SingleFieldShipBombed.foreground;
                    }
                }
                if (!field.IsShip && field.IsBombed)
                {
                    button.Background = SettingManager.EmptyFieldBombed.background;
                    button.Foreground = SettingManager.EmptyFieldBombed.foreground;
                }
            }
            foreach (var shipCountFieldControl in this.shipLenCountControls)
                shipCountFieldControl.UpdateField();
        }
    }
}
