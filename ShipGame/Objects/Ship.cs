using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Objects
{
    class Ship
    {
        public static int AUTO_ID = 0;
        public int id;
        public int shipLength;
        public List<Field> fields;

        public Ship(int shipLength, List<Field> fields)
        {
            id = AUTO_ID++;
            this.shipLength = shipLength;
            this.fields = fields;
        }

        public Ship(List<Field> fields)
        {
            id = AUTO_ID++;
            this.shipLength = fields.Count;
            this.fields = fields;
        }

        public bool isShipFullBombed() => fields.All(field => field.IsBombed);
        public bool isShipPartBombed() => fields.Any(field => field.IsBombed);
        public bool isShipPartBombed(int fields) => fields > shipLength ? false : this.fields.Count(field => field.IsBombed) >= fields;

        public static Ship operator +(Ship first, Ship second)
        {
            List<Field> fields = new(first.fields);
            fields.AddRange(second.fields);
            return new Ship(fields);
        }
    }
}
