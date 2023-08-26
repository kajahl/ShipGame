using Statki2.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    enum GameState
    {
        NotInitialized,
        WaitingForUnitPlacement,
        GameInProgress,
        GameEnd
    }

    enum GameSubstate
    {
        UnitsPlaceInProgress,
        UnitsPlacedCorrect,
        UnitsPlacedIncorrect,
        PlayerMove,
        ComputerMove,
        PlayerWin,
        ComputerWin
    }

    class Game
    {
        public MainWindow MainWindow { get; }
        public SettingManager SettingManager { get; }

        public Player Player { get; }
        public ComputerPlayer ComputerPlayer { get; }

        public GameState State { get; private set; }
        public GameSubstate Substate { get; private set; }
        private string subMessage;

        public Game(MainWindow mainWindow)
        {
            MainWindow = mainWindow; ;
            SettingManager = new SettingManager();
            SettingManager.LoadSettings();

            Player = new Player(this, "Gracz", true);
            ComputerPlayer = new ComputerPlayer(this, "Komputer", SettingManager.showEnemyShips.getValue());

            State = GameState.NotInitialized;
            Substate = GameSubstate.PlayerMove;
            subMessage = "";
        }

        // Startowanie gry
        private void StartGame()
        {
            if (State != GameState.NotInitialized && State != GameState.GameEnd) return;
            ResetGame();

            State = GameState.WaitingForUnitPlacement;
            Substate = GameSubstate.UnitsPlaceInProgress;

            ComputerPlayer.AutoplaceShips();

            //updateView
            MainWindow.UpdateView();
        }

        // Sprawdzanie poprawności rozłożenia statków
        private void CheckReadyForBattle()
        {
            if (State != GameState.WaitingForUnitPlacement) return;

            //Sprawdzenie poprawności statków gracza
            (GameSubstate, string) checkIntegrity = Player.Gameboard.CheckGameboardIntegrity();
            Substate = checkIntegrity.Item1;
            subMessage = checkIntegrity.Item2;

            //updateView
            MainWindow.UpdateView();
        }

        // Rozpoczęcie bitwy
        private void StartBattle()
        {
            if (State != GameState.WaitingForUnitPlacement) return;

            State = GameState.GameInProgress;
            Substate = GameSubstate.PlayerMove;

            //updateView
            MainWindow.UpdateView();
        }

        // Pobieranie podwiadomości do widoku
        public string GetSubMessage()
        {
            string message = subMessage;
            subMessage = "";
            return message;
        }

        // Bombardowanie pola przeciwnika
        public void BombEnemnyField((int, int) pos)
        {
            if (State != GameState.GameInProgress) return;
            if (Substate != GameSubstate.PlayerMove) return;

            //(ruch mozliwy, trafiony, zatopiona ostatnia czesc statku)
            (bool, bool, bool) bombResult = ComputerPlayer.Gameboard.BombFieldAt(pos);
            //1. jeżeli trafił - nadal może strzelać, 2. jeżeli nie trafił - tura przeciwnika

            // Nielegalny ruch
            if (!bombResult.Item1) return;

            // Pudło
            if (!bombResult.Item2)
            {
                Substate = GameSubstate.ComputerMove;
                subMessage = "Pudło! Teraz tura przeciwnika...";
                ComputerPlayer.MakeAutoMove();
            }
            // Trafienie
            else if (bombResult.Item2 && !bombResult.Item3)
            {
                Substate = GameSubstate.PlayerMove;
                subMessage = "Trafienie! Możesz strzelać dalej.";
            }
            // Trafienie, Zatopienie
            else
            {
                Substate = GameSubstate.ComputerMove;
                subMessage = "Trafienie! Zatopienie! Teraz tura przeciwnika...";
                ComputerPlayer.MakeAutoMove();
            }

            CheckIfSomeoneWins();
            //updateView
            MainWindow.UpdateView();
        }

        // Bombardowanie pola gracza
        public void BombPlayerField((int, int) pos)
        {
            if (State != GameState.GameInProgress) return;
            if (Substate != GameSubstate.ComputerMove) return;

            //(ruch mozliwy, trafiony, zatopiona ostatnia czesc statku)
            (bool, bool, bool) bombResult = Player.Gameboard.BombFieldAt(pos);

            // Nielegalny ruch
            if (!bombResult.Item1) return;


            // Pudło
            if (!bombResult.Item2)
            {
                Substate = GameSubstate.PlayerMove;
                subMessage = "Przeciwnik nie trafił! Teraz pora na twój ruch...";
            }
            // Trafienie
            else if (bombResult.Item2 && !bombResult.Item3)
            {
                Substate = GameSubstate.ComputerMove;
                subMessage = "Przeciwnik trafił statek! Jednaj jeszcze sie trzyma.. Nadal może strzelać.";
                ComputerPlayer.MakeAutoMove();
            }
            // Trafienie, Zatopienie
            else
            {
                Substate = GameSubstate.PlayerMove;
                subMessage = "Przeciwnik trafił i zatopił! Teraz pora na twój ruch...";
            }

            CheckIfSomeoneWins();
            //updateView
            MainWindow.UpdateView();
        }

        // Sprawdzenie czy ktoś już wygrał
        private void CheckIfSomeoneWins()
        {
            if (Player.Gameboard.GetCountLeftShipsToBomb() <= 0) Substate = GameSubstate.ComputerWin;
            if (ComputerPlayer.Gameboard.GetCountLeftShipsToBomb() <= 0) Substate = GameSubstate.PlayerWin;

            if (Substate != GameSubstate.PlayerWin && Substate != GameSubstate.ComputerWin) return;
            subMessage = "";
            State = GameState.GameEnd;
            this.ComputerPlayer.Gameboard.SetDisplayShipsOnGrid(true);
        }

        // Wymuszenie końca gry (przycisk)
        private void ForceEndGame()
        {
            ResetGame();
            Substate = GameSubstate.ComputerWin;
            subMessage = "Poddałeś gre.. Możesz zagrać ponownie!";
            State = GameState.GameEnd;
            MainWindow.UpdateView();
        }

        // Resetowanie gry
        private void ResetGame()
        {
            State = GameState.NotInitialized;
            Substate = GameSubstate.PlayerMove;

            Player.Gameboard.ResetGameboard();
            ComputerPlayer.Gameboard.ResetGameboard();
            ComputerPlayer.Gameboard.SetDisplayShipsOnGrid(this.SettingManager.showEnemyShips.getValue());
        }

        // Handler dla głównego przycisku w widoku
        public void HandleButtonClick()
        {
            if (State == GameState.NotInitialized) StartGame();
            else if (State == GameState.WaitingForUnitPlacement && Substate != GameSubstate.UnitsPlacedCorrect) CheckReadyForBattle();
            else if (State == GameState.WaitingForUnitPlacement && Substate == GameSubstate.UnitsPlacedCorrect) StartBattle();
            else if (State == GameState.GameInProgress) ForceEndGame();
            else if (State == GameState.GameEnd) StartGame();
        }

    }
}
