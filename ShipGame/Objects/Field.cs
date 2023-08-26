using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    class Field
    {
        public int Row { get; }
        public int Col { get; }
        public bool IsBombed { get; private set; }
        public bool IsShip { get; private set; }

        public Field(int row, int col)
        {
            this.Row = row;
            this.Col = col;
            IsBombed = false;
            IsShip = false;
        }

        public string GetFieldName()
        {
            return $"{(char)(Row + 65)}{Col}";
        }

        public (int, int) GetPos()
        {
            return (Row, Col);
        }

        public void SetBombed(bool bombed)
        {
            IsBombed = bombed;
        }

        public void SetShip(bool ship)
        {
            IsShip = ship;
        }

        public static bool operator ==(Field? left, Field? right)
        {
            if (left is null && right is null) return true;
            if (right is null) return false;
            if (left is null) return false;
            return left.Col == right.Col && left.Row == right.Row;
        }

        public static bool operator !=(Field left, Field? right)
        {
            return right is null || left.Col != right.Col || left.Row != right.Row;
        }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static string operator <<(Field field, int shift)
        {
            string s = "";
            for (int i = 0; i < shift; i++) s += "0";
            return $"{field.GetFieldName()} ({s})";
        }

        public static implicit operator Field((int row,int col) pos)
        {
            return new Field(pos.row, pos.col);
        }

        public override string ToString()
        {
            return $"Pole {this.GetFieldName()} : Statek[{(this.IsShip ? 'X' : '_')}] Bomba[{(this.IsBombed ? 'X' : '_')}]";
        }
    }
}
