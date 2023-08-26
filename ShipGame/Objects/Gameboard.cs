using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    class Gameboard
    {
        readonly Player player;
        public int BoardSideSize { get; }
        public List<Ship> Ships { get; private set; }
        public Field[][] Fields { get; private set; }
        public bool DisplayShipsOnGrid { get; private set; }

        public Gameboard(Player player, int boardSideSize, bool displayShipsOnGrid)
        {
            this.player = player;
            BoardSideSize = boardSideSize;
            DisplayShipsOnGrid = displayShipsOnGrid;
            Ships = new List<Ship>();
            Fields =
                Enumerable.Range(0, boardSideSize).Select(
                    (row) => Enumerable.Range(0, boardSideSize).Select(
                        (col) => new Field(row, col)).ToArray()).ToArray();
        }

        // Ustawienie widoczności statków
        public void SetDisplayShipsOnGrid(bool displayShipsOnGrid) => this.DisplayShipsOnGrid = displayShipsOnGrid;

        // Wyczyszczenie wszystkich pól
        public void ResetGameboard()
        {
            GetFieldsAsList().ForEach(field =>
            {
                field.SetShip(false);
                field.SetBombed(false);
            });
            Ships.Clear();
        }

        // Gettery pól
        public Field GetFieldAt((int, int) pos) => GetFieldAt(pos.Item1, pos.Item2);
        public Field GetFieldAt(int row, int col) => Fields[row][col];

        // Ustawianie statków
        public void PlaceShipAt((int, int) pos)
        {
            if (player.game.State != GameState.WaitingForUnitPlacement) return;
            GetFieldAt(pos).SetShip(true);
        }
        public void RemoveShipFrom((int, int) pos)
        {
            if (player.game.State != GameState.WaitingForUnitPlacement) return;
            GetFieldAt(pos).SetShip(false);
        }

        // Bombardowanie pól
        //(ruch mozliwy, trafiony, zatopiona ostatnia czesc statku)
        public (bool, bool, bool) BombFieldAt((int, int) pos)
        {
            if (player.game.State != GameState.GameInProgress) return (false, false, false);

            bool avalibleMove = !GetFieldAt(pos).IsBombed;
            if (!avalibleMove) return (false, false, false);

            GetFieldAt(pos).SetBombed(true);

            bool isShipAtPos = GetFieldAt(pos).IsShip;
            if (!isShipAtPos) return (avalibleMove, isShipAtPos, false);

            bool isFullShipBombed = isShipAtPos && IsShipFullBombedAt(pos);
            return (avalibleMove, isShipAtPos, isFullShipBombed);
        }
        public void RemoveBombFrom((int, int) pos)
        {
            if (player.game.State != GameState.GameInProgress) return;
            GetFieldAt(pos).SetBombed(false);
        }

        // Sprawdzenie itegralności pól - poprawnie ustawione statki + wykorzystane wszystkie statki
        public (GameSubstate, string) CheckGameboardIntegrity()
        {
            if (!IsAllShipsCorrectlyPlaced()) return (GameSubstate.UnitsPlacedIncorrect, "Jeden lub więcej twoich statków jest rozmieszczony niezgodnie z zasadami");
            DetectShips();
            if (!IsAllShipsUsed()) return (GameSubstate.UnitsPlacedIncorrect, "Nie wykorzystałeś wszystkich statków lub rozłożyłeś ich za dużo");
            return (GameSubstate.UnitsPlacedCorrect, "Poprawnie rozłożyłeś swoje jednostki. Możesz przejść do bitwy!");
        }

        // Zwrócenie tablicy 2d pól jako listy
        public List<Field> GetFieldsAsList()
        {
            List<Field> fields = new();
            foreach (Field[] field1d in Fields)
                fields.AddRange(field1d);
            return fields;
        }

        // Wykryj statki na polach
        public void DetectShips()
        {
            Ships = new List<Ship>();
            int[][] intArray = Utils.ConvertFieldsToBitArray(Fields, player.game.SettingManager.boardSize.getValue());
            List<List<int>> verticalLineHorizontalShips = new();
            List<List<int>> verticalLinesVerticalShips = new();
            List<List<int>> horizontalLinesHorizontalShips = new();

            // Generowanie linii góra-dół (statki pionowe)
            for (int col = 0; col < player.game.SettingManager.boardSize.getValue(); col++)
            {
                List<int> line = new();
                for (int row = 0; row < player.game.SettingManager.boardSize.getValue(); row++)
                {
                    int v = intArray[row][col];
                    // Pominięcie pól ze statkiem który tylko przechodzi przez linie, a nie jest na niej ustawiony
                    //      OOOOZO
                    //      OXXOZO <- Sprawdzana i zapisywana linia, X = Statek na linii, Z = Statek przechodzący przez linie
                    //      OOOOZO
                    if (col != 0 && intArray[row][col - 1] == 1) v = 0;
                    if (col != player.game.SettingManager.boardSize.getValue() - 1 && intArray[row][col + 1] == 1) v = 0;
                    line.Add(v);
                }
                verticalLinesVerticalShips.Add(line);
            }

            // Generowanie linii góra-dół (statki poziome)
            for (int col = 0; col < player.game.SettingManager.boardSize.getValue(); col++)
            {
                List<int> line = new();
                for (int row = 0; row < player.game.SettingManager.boardSize.getValue(); row++)
                {
                    int v = intArray[row][col];
                    // Pominięcie pól ze statkiem który tylko przechodzi przez linie, a nie jest na niej ustawiony
                    //      OOOOZO
                    //      OXXOZO <- Sprawdzana i zapisywana linia, Z = Statek na linii, X = Statek przechodzący przez linie
                    //      OOOOZO
                    if (row != 0 && intArray[row - 1][col] == 1) v = 0;
                    if (row != player.game.SettingManager.boardSize.getValue() - 1 && intArray[row + 1][col] == 1) v = 0;
                    line.Add(v);
                }
                verticalLineHorizontalShips.Add(line);
            }

            // Zliczenie "jedynek" i wyrzucenie ich z planszy
            for (int y = 0; y < player.game.SettingManager.boardSize.getValue(); y++)
            {
                for (int i = 0; i < player.game.SettingManager.boardSize.getValue(); i++)
                {
                    if (verticalLineHorizontalShips[i][y] == verticalLinesVerticalShips[i][y] && verticalLineHorizontalShips[i][y] != 0)
                    {
                        verticalLineHorizontalShips[i][y] = 0;
                        verticalLinesVerticalShips[i][y] = 0;
                        intArray[y][i] = 0;
                        List<Field> list = new();
                        Field field = GetFieldAt(y, i);
                        list.Add(field);
                        Ship ship = new(1, list);
                        //Ships.Add(ship);
                        _ = this + ship;
                    }
                }
            }

            // Generowanie linii prawo-lewo (statki poziome)
            for (int y = 0; y < player.game.SettingManager.boardSize.getValue(); y++)
            {
                List<int> line = new();
                for (int i = 0; i < player.game.SettingManager.boardSize.getValue(); i++)
                {
                    int v = intArray[y][i];
                    // Pominięcie pól ze statkiem który tylko przechodzi przez linie, a nie jest na niej ustawiony
                    //      OOOOZO
                    //      OXXOZO <- Sprawdzana i zapisywana linia, Z = Statek na linii, X = Statek przechodzący przez linie
                    //      OOOOZO
                    if (y != 0 && intArray[y - 1][i] == 1) v = 0;
                    if (y != player.game.SettingManager.boardSize.getValue() - 1 && intArray[y + 1][i] == 1) v = 0;
                    line.Add(v);
                }
                horizontalLinesHorizontalShips.Add(line);
            }

            // Wykrycie statków Len>1
            for (int lineNo = 0; lineNo < horizontalLinesHorizontalShips.Count; lineNo++)
            {
                DetectShipsAtLine(horizontalLinesHorizontalShips[lineNo], lineNo, true);
                DetectShipsAtLine(verticalLinesVerticalShips[lineNo], lineNo, false);
            }

            return;
        }

        // Prywatne metody pomocnicze do wykrywania statków
        private void DetectShipsAtLine(List<int> line, int lineNumber, bool horizontal)
        {
            int streak = 0;
            List<Field> fields = new();
            int row = horizontal ? lineNumber : 0; //Jeżeli horiz (prawo-lewo check) to row (wiersz) jest stałe
            int col = horizontal ? 0 : lineNumber;

            foreach (int item in line)
            {
                if (item == 1)
                {
                    streak++;
                    fields.Add(GetFieldAt(row, col));
                }
                if (streak == 1 && item == 0)
                {
                    fields = new List<Field>();
                }
                if (streak > 1 && item == 0)
                {
                    Ship ship = new(streak, fields);
                    //Ships.Add(ship);
                    _ = this + ship;
                    fields = new List<Field>();
                    streak = 0;
                }

                if (horizontal) col++;
                else row++;
            }
            if (streak > 1)
            {
                Ship ship = new(streak, fields);
                //Ships.Add(ship);
                _ = this + ship;
            }
        }

        // Czy wykorzystano wszystkie statki
        public bool IsAllShipsUsed() => player.game.SettingManager.shipLengths.getValue().All(kvp => Ships.Count(ship => ship.shipLength == kvp.Key) == kvp.Value);

        // Czy wykorzystano wszystkie statki - wersja z outputem
        //public bool isAllShipsUsed() {
        //    Debug.WriteLine($"{this.player.playerName}");
        //    return this.player.game.settingManager.ShipLengthCount.All(kvp =>
        //    {
        //        Debug.WriteLine($"L:{kvp.Key} Oczekiwana ilość:{kvp.Value} Otrzymana ilość: {this.ships.Count(ship => ship.shipLength == kvp.Key)}");
        //        return this.ships.Count(ship => ship.shipLength == kvp.Key) == kvp.Value;
        //    });
        //}

        // Czy wszystkie statki zostały prawidłowo rozłożone
        public bool IsAllShipsCorrectlyPlaced() => GetFieldsAsList().All(field => CheckSquareNeighboorhood(field.Row, field.Col)); //Safe - sprawdzenie wszystkich pól, nawet jak statek nie został wykryty to zostanie sprawdzone
                                                                                                                                   //public bool isAllShipsCorrectlyPlaced() => this.ships.All(ship => ship.fields.All(field => this.checkSquareNeighboorhood(field.row, field.col))); //Sprawdzenie tylko pól wokół znalezionych statków

        // Czy statek na zadanej pozycji jest w pełni zbombardowany
        public bool IsShipFullBombedAt((int row, int col) pos)
        {
            Ship ship = Ships.First(s => s.fields.Any(field => field.Row == pos.row && field.Col == pos.col));
            return ship.isShipFullBombed();
        }

        // Gettery - zliczanie pól
        public int GetCountAllFields() => Fields.Length;
        public int GetCountAllShipFields() => GetFieldsAsList().Count(field => field.IsShip);
        public int GetCountLeftFieldsToBomb() => GetCountAllFields() - GetFieldsAsList().Count(field => field.IsBombed);
        public int GetCountLeftShipsToBomb() => GetCountAllShipFields() - GetFieldsAsList().Count(field => field.IsShip && field.IsBombed); //do zmiany na sprawdzanie całych statków -> GetCountLeftShips
        public int GetCountLeftShips() => Ships.FindAll(s => !s.isShipFullBombed()).Count;
        public int GetCountLeftShips(int shipLength) => Ships.FindAll(s => s.shipLength == shipLength && !s.isShipFullBombed()).Count;

        // Prywatne metody pomocnicze do sprawdzania czy wszystkie statki stoją zgodnie z zasadami
        // Sprawdzanie okolicy statku (pole statku, pole wyżej, pole po prawej, pole niżej, pole po lewej)
        private List<int> GetSquareNeighboorhood(int row, int col)
        {
            //Square = 0, Up = 1, Right = 2, Down = 3, Left = 4
            List<int> list = new();
            if (Fields[row][col].IsShip) list.Add(0);
            if (row != 0 && Fields[row - 1][col].IsShip) list.Add(1);
            if (row != player.game.SettingManager.boardSize.getValue() - 1 && Fields[row + 1][col].IsShip) list.Add(3);
            if (col != 0 && Fields[row][col - 1].IsShip) list.Add(4);
            if (col != player.game.SettingManager.boardSize.getValue() - 1 && Fields[row][col + 1].IsShip) list.Add(2);
            return list;
        }
        // Sprawdzenie czy okolica jest prawidłowo rozstawiona - nie ma statku LUB jest statek tylko w poziomie LUB jest statek tylko w pionie
        private bool CheckSquareNeighboorhood(int row, int col)
        {
            List<int> list = GetSquareNeighboorhood(row, col);
            if (!list.Contains(0)) return true;
            list.Remove(0);
            if (list.All(x => x % 2 == 0)) return true; //Jeżeli wszystkie są podzielne przez 2 <=> Istnieje statek w poziomie, Bądź jest to koniec statku
            if (list.All(x => x % 2 == 1)) return true; //Jeżeli wszystkie są niepodzielne przez 2 <=> Istnieje statek w pionie, Bądź jest to koniec statku
            return false;
        }

        //Nadpisanie +
        public static Gameboard operator +(Gameboard g, Ship a)
        {
            g.Ships.Add(a);
            return g;
        }
    }
}
