using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Settings
{
    internal class ShowEnemyShipsSetting : ASingleSetting<bool>
    {
        public ShowEnemyShipsSetting() : base(false) {}

        public override string Name { get; } = "showEnemyShips";

        public override string serialize()
        {
            return $"{(getValue() ? 1 : 0)}";
        }

        protected override bool deserialize(string serialized)
        {
            if (serialized == "1") return true;
            return this.defaultValue;
        }

        protected override bool propetryValidator(bool newValue)
        {
            return true;
        }
    }
}
