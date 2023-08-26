using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    class Player
    {
        public readonly Game game;
        public string PlayerName { get; }
        public Gameboard Gameboard { get; }
        public int Points { get; private set; }

        public Player(Game game, string playerName, bool displayShipsOnGrid)
        {
            this.game = game;
            Gameboard = new Gameboard(this, game.SettingManager.boardSize.getValue(), displayShipsOnGrid);
            PlayerName = playerName;
            Points = 0;
        }

    }
}
