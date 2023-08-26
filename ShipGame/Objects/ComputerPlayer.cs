using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    class ComputerPlayer : Player
    {

        public ComputerPlayer(Game game, string playerName, bool displayShipsOnGrid) : base(game, playerName, displayShipsOnGrid) { }


        // Automatyczne generowanie statkow
        public void AutoplaceShips()
        {
            Random random = new();
            bool vertical = (int)random.NextInt64(0, 1) == 1;

            // Wybranie odpowiedniej dlugosci
            foreach (var shipLen in game.SettingManager.shipLengths.getValue())
            {
                // Wybranie długości odpowiednią ilośc razy
                for (int shipCount = 0; shipCount < shipLen.Value; shipCount++)
                {
                    // Wylosowanie pozycji
                    int pos = (int)random.NextInt64(10, 100);
                    vertical = !vertical;
                    (int row, int col) = GetNextAvaliablePosAfterNPos(pos, shipLen.Key, vertical);
                    PlaceShipAt(row, col, vertical, shipLen.Key);
                }
            }
        }

        // Ustawienie statku o zadanej długości na podanym miejscu
        private void PlaceShipAt(int row, int col, bool vertical, int shipLen)
        {
            List<Field> fields = new();
            do
            {
                Gameboard.PlaceShipAt((row, col));
                fields.Add(Gameboard.GetFieldAt(row, col));
                if (vertical) col++;
                else row++;
                shipLen--;
            } while (shipLen > 0);
            Gameboard.Ships.Add(new Ship(fields));
        }

        // Sprawdzenie czy można ustawić pole statku zgodnie z zasadami (czy na sąsiadujących polach znajduje sie statek)
        private bool CanPlaceShipFieldAt(Gameboard gameboard, int row, int col)
        {
            if (row != 0 && gameboard.GetFieldAt(row - 1, col).IsShip) return false;
            if (row != game.SettingManager.boardSize.getValue() - 1 && gameboard.GetFieldAt(row + 1, col).IsShip) return false;
            if (col != 0 && gameboard.GetFieldAt(row, col - 1).IsShip) return false;
            if (col != game.SettingManager.boardSize.getValue() - 1 && gameboard.GetFieldAt(row, col + 1).IsShip) return false;
            return true;
        }

        // Sprawdzenie czy można ustawić pełny statek na zadanym miejscu
        private bool CanPlaceFullShipAt(int row, int col, int shipLen, bool vertical)
        {
            int checkCol = col;
            int checkRow = row;

            if (vertical && checkCol + shipLen >= game.SettingManager.boardSize.getValue()) return false;
            if (!vertical && checkRow + shipLen >= game.SettingManager.boardSize.getValue()) return false;

            int fieldsToCheck = shipLen;
            do
            {
                if (!CanPlaceShipFieldAt(Gameboard, checkRow, checkCol)) return false;
                if (vertical) checkCol++;
                else checkRow++;
                fieldsToCheck--;
            } while (fieldsToCheck > 0);
            return true;
        }

        // Znajduje dostępną pozycję dla statku o zadanej długości po dodaniu N pól od pozycji (0,0)
        private (int, int) GetNextAvaliablePosAfterNPos(int npos, int shipLen, bool vertical)
        {
            int row = npos / game.SettingManager.boardSize.getValue(); // /10 = ile pełnych dziesiątek
            int col = npos % game.SettingManager.boardSize.getValue(); // %10 = 0-9

            // Jeżeli skrajne wiersze/kolumny - przesuń o 1 do środka
            if (vertical && row == 0) row++;
            if (vertical && row == game.SettingManager.boardSize.getValue() - 1) row--;
            if (!vertical && col == 0) col++;
            if (!vertical && col == game.SettingManager.boardSize.getValue() - 1) col--;

            // Ograniczenie - aby pętla była skończona, może sprawdzić tylko N^2 pól, gdzie N to bok pola gry
            // Tj. Sprawdza wszystkie pola gry - jeżeli nie znajdzie = zwraca niemożliwą pozycje (todo: errorHandler)
            int triesLeft = game.SettingManager.boardSize.getValue() * game.SettingManager.boardSize.getValue();
            bool canPlace;
            do
            {
                col++;
                if (col == game.SettingManager.boardSize.getValue())
                {
                    col = 0;
                    row++;
                    if (row == game.SettingManager.boardSize.getValue())
                    {
                        row = 0;
                    }
                }
                canPlace = CanPlaceFullShipAt(row, col, shipLen, vertical);
                triesLeft--;
            } while (!canPlace && triesLeft > 0);


            if (triesLeft <= 0) return (-1, -1);
            else return (row, col);
        }

        // Wykonuje automatyczny ruch po sztucznym opóźnieniu
        public async void MakeAutoMove()
        {
            await Task.Delay(1000);
            (int, int) posToBomb = GetNextPosToBomb();
            game.BombPlayerField(posToBomb);
        }

        // Zwraca losową dostępną pozycję do bombardowania przez komputer
        private (int, int) GetRandomPos()
        {
            List<Field> leftFieldsToBomb = game.Player.Gameboard.GetFieldsAsList().FindAll(p => !p.IsBombed);
            int randomNumber = (int)Random.Shared.NextInt64(1, leftFieldsToBomb.Count - 1);
            return leftFieldsToBomb.ElementAt(randomNumber).GetPos();
        }

        // Zwraca następną pozycję do bombardowania przez komputer na podstawie poprzednich ruchów
        private (int, int) GetNextPosToBomb()
        {
            // [1] Sprawdza czy któryś statek ma 2 zbombardowane pola ale nie jest w pełni zatopiony
            Ship? hittedShip = game.Player.Gameboard.Ships.Find(ship => ship.isShipPartBombed(2) && !ship.isShipFullBombed());
            if (hittedShip == null)
            {
                // Jeżeli statek z [1] nie istnieje to sprawdź czy istnieje jakikolwiek statek niezatopiony z przynajmniej jednym zbombardowanym polem
                hittedShip = game.Player.Gameboard.Ships.Find(ship => ship.isShipPartBombed(1) && !ship.isShipFullBombed());
                if (hittedShip == null) return GetRandomPos();

                // Znalezienie jedynego zbombardowanego pola
                Field? hittedField = hittedShip.fields.Find(f => f.IsBombed);
                if (hittedField == null) return GetRandomPos();

                int row = hittedField.Row;
                int col = hittedField.Col;

                // Bombardowanie pól dookoła w celu znalezienia drugiego pola ze statkiem (w celu spełnienia warunku [1])
                if (row != 0 && !game.Player.Gameboard.GetFieldAt(row - 1, col).IsBombed) return (row - 1, col);
                else if (row != game.SettingManager.boardSize.getValue() - 1 && !game.Player.Gameboard.GetFieldAt(row + 1, col).IsBombed) return (row + 1, col);
                else if (col != 0 && !game.Player.Gameboard.GetFieldAt(row, col - 1).IsBombed) return (row, col - 1);
                else if (col != game.SettingManager.boardSize.getValue() - 1 && !game.Player.Gameboard.GetFieldAt(row, col + 1).IsBombed) return (row, col + 1);
                else return GetRandomPos();
            }

            // Czy statek jest ułożony poziomo (sprawdzenie na podstawie poprzednich ruchów)
            Field firstFieldOfShip = hittedShip.fields.First();
            bool horizontalShip = hittedShip.fields.FindAll(f => f.IsBombed).All(f => f.Row == firstFieldOfShip.Row);

            // Horizontal   => Stały wiersz,    row=const   szukam MinMax(col)
            // Vertical     => Stała kolumna,   col=const   szukam MinMax(row)

            // Pobranie dwóch krańcowych zbombardowanych punktów statku
            int staticCoord = horizontalShip ? firstFieldOfShip.Row : firstFieldOfShip.Col;
            int minIndex = hittedShip.fields.FindAll(f => f.IsBombed).Min(f => horizontalShip ? f.Col : f.Row);
            int maxIndex = hittedShip.fields.FindAll(f => f.IsBombed).Max(f => horizontalShip ? f.Col : f.Row);

            // Sprawdzenie czy da się zbombardować o jedno pole wcześniej od minimalnego zbombardowanego pola
            int minOrStaticA = horizontalShip ? staticCoord : minIndex - 1;
            int minOrStaticB = horizontalShip ? minIndex - 1 : staticCoord;

            if (minIndex != 0 && !game.Player.Gameboard.GetFieldAt(minOrStaticA, minOrStaticB).IsBombed) return (minOrStaticA, minOrStaticB);

            // Sprawdzenie czy da się zbombardować o jedno pole później niż maksymalne zbombardowane pole
            int maxOrStaticA = horizontalShip ? staticCoord : maxIndex + 1;
            int maxOrStaticB = horizontalShip ? maxIndex + 1 : staticCoord;

            if (maxIndex != game.SettingManager.boardSize.getValue() - 1 && !game.Player.Gameboard.GetFieldAt(maxOrStaticA, maxOrStaticB).IsBombed) return (maxOrStaticA, maxOrStaticB);

            // Jeżeli powyższe metody nie zadziałały - bombardowane jest pole między polami min i max
            // (przypadek gdy skrypt jakoś zbombardował pola z pominięciem jednego lub więcej pola - tj. XOOXX, gdzie X to trafiony statek)
            for (int i = minIndex; i < maxIndex; i++)
            {
                if (
                    i != 0 &&
                    i != game.SettingManager.boardSize.getValue() - 1 &&
                    !game.Player.Gameboard.GetFieldAt(horizontalShip ? staticCoord : i, horizontalShip ? i : staticCoord).IsBombed
                ) return (horizontalShip ? staticCoord : i, horizontalShip ? i : staticCoord);
            }

            // Jeżeli wszystkie pozostałe warunki sie nie sprawdziły (statek prawdopodobnie już jest zatopiony)
            return GetRandomPos();
        }

    }
}
