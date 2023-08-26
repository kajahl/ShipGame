using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Statki2.ViewModels;

namespace Statki2.Objects
{
    static class Utils
    {
        // Konwertuje string postaci 123x123 na identyfikator pola
        public static (int, int) GetPositionFromString(string tag)
        {
            string[] ids = tag.Split("x");
            int row = int.Parse(ids[0]);
            int col = int.Parse(ids[1]);
            return (row, col);
        }

        // Pobiera z przycisku dane, jakiego pola dotyczy
        public static (int, int) GetPositionFromButtonTag(Button button) => GetPositionFromString((string)button.Tag);

        // Zwraca przycisk odpowiadający konkretnemu polu
        public static Field GetGameboardFieldFromButton(Gameboard gameboard, Button button) => gameboard.GetFieldAt(GetPositionFromString((string)button.Tag));

        // Zwraca tekst widoczny na głównym widoku gry w zależności od etapu gry
        public static ViewModelData GetGamestateStrings(Game game)
        {
            if (game.State == GameState.NotInitialized) return new ViewModelData("Oczekiwanie na start gry", $"Rozpocznij grę, klikając w przycisk poniżej pól gry.{GetSubMessageIfExists(game)}", "Rozpocznij grę", "");
            if (game.State == GameState.WaitingForUnitPlacement)
            {
                if (game.Substate == GameSubstate.UnitsPlaceInProgress) return new ViewModelData("Oczekiwanie na rozmieszczenie jednostek", $"Umieść wszystkie jednostki na swojej planszy. Ilość jednostek  do rozmieszczenia znajduje się pod polem gry. Jednostki nie mogą się dotykać bokami.{GetSubMessageIfExists(game)}", "Potwierdź rozstawienie jednostek", "");
                if (game.Substate == GameSubstate.UnitsPlacedIncorrect) return new ViewModelData("Oczekiwanie na rozmieszczenie jednostek", $"Niepoprawnie rozłożono jednostki. Do dyspozycji masz Ilość jednostek  do rozmieszczenia znajduje się pod polem gry. Jednostki nie mogą się dotykać bokami.{GetSubMessageIfExists(game)}", "Potwierdź rozstawienie jednostek", "");
                if (game.Substate == GameSubstate.UnitsPlacedCorrect) return new ViewModelData("Oczekiwanie na rozmieszczenie jednostek", $"Poprawnie rozmieszczono jednostki - Oczekiwanie na rozpoczęcie bitwy.{GetSubMessageIfExists(game)}", "Rozpocznij bitwę", "");
            }
            if (game.State == GameState.GameInProgress)
            {
                if (game.Substate == GameSubstate.PlayerMove) return new ViewModelData("Gra w toku", $"Twoja tura! Postaraj się zestrzelić statek wroga!{GetSubMessageIfExists(game)}", "Zakończ grę", "Zakończenie gry przed zestrzeleniem wszystkich jednostek wroga skutkuje przegraną");
                if (game.Substate == GameSubstate.ComputerMove) return new ViewModelData("Gra w toku", $"Tura przeciwnika, oczekuj na swój ruch...{GetSubMessageIfExists(game)}", "Zakończ grę", "Zakończenie gry przed zestrzeleniem wszystkich jednostek wroga skutkuje przegraną");
            }
            if (game.State == GameState.GameEnd)
            {
                if (game.Substate == GameSubstate.PlayerWin) return new ViewModelData("Koniec gry!", $"Brawo, Wygrałeś!{GetSubMessageIfExists(game)}", "Rozpocznij nową grę", "");
                if (game.Substate == GameSubstate.ComputerWin) return new ViewModelData("Koniec gry!", $"Niestety przegrałeś, spróbuj ponownie w następnej grze!{GetSubMessageIfExists(game)}", "Rozpocznij nową grę", "");
            }
            return new ViewModelData("", "", "", "");
        }

        // Zwraca "podwiadomość" do głównego widoku gry
        private static string GetSubMessageIfExists(Game game)
        {
            string s = game.GetSubMessage();
            if (s.Length == 0) return "";
            else return $"\n{s}";
        }

        // Konwersja pola gry na bitmapę
        public static int[][] ConvertFieldsToBitArray(Field[][] fields, int sideSize)
        {
            return Enumerable.Range(0, sideSize).Select(
                (row) => Enumerable.Range(0, sideSize).Select(
                    (col) => fields[row][col].IsShip ? 1 : 0).ToArray()).ToArray();
        }

        // Zwraca w postaci tekstu typ i ilość dostępnych jednostek wykorzystania
        //private static string GetAvalibleUnitsAsString(Dictionary<int, int> ships)
        //{
        //    string s = "";
        //    foreach (var ship in ships)
        //    {
        //        if (ship.Value == 0) continue;
        //        s += (s.Length != 0 ? ", " : "") + $"[{ship.Key}]x{ship.Value}";
        //    }
        //    return s;
        //}
    }
}
