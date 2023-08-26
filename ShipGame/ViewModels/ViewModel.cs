using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Statki2.Objects;

namespace Statki2.ViewModels
{
    class ViewModel
    {
        readonly Game game;

        private GamefieldGridViewModel playerGamefieldGrid;
        private GamefieldGridViewModel computerGamefieldGrid;

        private Label gameStateName;
        private TextBlock gameStateDescription;
        private Button mainButton;
        private Label mainButtonTooltip;

        public ViewModel(MainWindow mainWindow, Game game)
        {
            this.game = game;

            playerGamefieldGrid = new GamefieldGridViewModel(game.Player, mainWindow.PlayerGrid, HandlePlayerFieldClicked);
            computerGamefieldGrid = new GamefieldGridViewModel(game.ComputerPlayer, mainWindow.ComputerGrid, HandleComputerPlayerFieldClicked);

            gameStateName = new Label();
            gameStateName.HorizontalAlignment = HorizontalAlignment.Center;
            gameStateName.VerticalAlignment = VerticalAlignment.Center;
            gameStateName.HorizontalContentAlignment = HorizontalAlignment.Center;
            gameStateName.VerticalContentAlignment = VerticalAlignment.Center;
            gameStateName.FontWeight = FontWeights.Bold;
            gameStateName.FontSize = 16;

            gameStateDescription = new TextBlock();
            gameStateDescription.HorizontalAlignment = HorizontalAlignment.Center;
            gameStateDescription.VerticalAlignment = VerticalAlignment.Center;
            gameStateDescription.TextAlignment = TextAlignment.Center;

            mainButton = new Button();
            mainButton.HorizontalAlignment = HorizontalAlignment.Center;
            mainButton.VerticalAlignment = VerticalAlignment.Center;
            mainButton.Padding = new Thickness(6, 6, 6, 6);
            mainButton.Margin = new Thickness(3, 3, 3, 3);
            mainButton.FontWeight = FontWeights.Bold;
            mainButton.Click += HandleButtonClicked;

            mainButtonTooltip = new Label();
            mainButtonTooltip.HorizontalAlignment = HorizontalAlignment.Center;
            mainButtonTooltip.VerticalAlignment = VerticalAlignment.Center;
            mainButtonTooltip.HorizontalContentAlignment = HorizontalAlignment.Center;
            mainButtonTooltip.VerticalContentAlignment = VerticalAlignment.Center;

            mainWindow.Header.Children.Add(gameStateName);
            mainWindow.Header.Children.Add(gameStateDescription);
            Grid.SetRow(gameStateName, 1);
            Grid.SetRow(gameStateDescription, 2);

            mainWindow.Footer.Children.Add(mainButton);
            mainWindow.Footer.Children.Add(mainButtonTooltip);
            Grid.SetRow(mainButton, 0);
            Grid.SetRow(mainButtonTooltip, 1);

            //playerGamefieldGrid.CreateGrid();
            //computerGamefieldGrid.CreateGrid();

            UpdateFields();
        }

        // Aktualizowanie pól graczy
        public void UpdateGrids()
        {
            playerGamefieldGrid.UpdateGrid();
            computerGamefieldGrid.UpdateGrid();
        }

        // Aktualizowanie pól głównego widoku
        public void UpdateFields()
        {
            ViewModelData data = Utils.GetGamestateStrings(game);
            gameStateName.Content = data.GameStateName;
            gameStateDescription.Text = data.GameStateDescription;
            mainButton.Content = data.MainButton;
            mainButtonTooltip.Content = data.MainButtonTooltip;
        }

        // Handler dla kliknięcia w główny przycisk
        private void HandleButtonClicked(object sender, RoutedEventArgs e)
        {
            game.HandleButtonClick();
        }

        // Handler dla kliknięcia w pole gracza
        private void HandlePlayerFieldClicked(object sender, RoutedEventArgs e)
        {
            if (game.State != GameState.WaitingForUnitPlacement) return;
            Button clicked_btn = (Button)sender;
            Field field = Utils.GetGameboardFieldFromButton(game.Player.Gameboard, clicked_btn);
            field.SetShip(!field.IsShip);
            playerGamefieldGrid.UpdateGrid();
        }

        // Handler dla kliknięcia w pole komputera
        private void HandleComputerPlayerFieldClicked(object sender, RoutedEventArgs e)
        {
            if (game.State != GameState.GameInProgress && game.State != GameState.WaitingForUnitPlacement) return;
            Button clicked_btn = (Button)sender;
            Field field = Utils.GetGameboardFieldFromButton(game.ComputerPlayer.Gameboard, clicked_btn);
            if (game.State == GameState.WaitingForUnitPlacement)
            {
                Debug.WriteLine(field);
                return;
            }
            game.BombEnemnyField(field.GetPos());
            computerGamefieldGrid.UpdateGrid();
        }

    }
}
