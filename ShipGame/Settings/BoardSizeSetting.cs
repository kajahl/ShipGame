using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Settings
{
    internal class BoardSizeSetting : ASingleSetting<int>
    {
        public BoardSizeSetting() : base(10) { }

        public override string Name { get; } = "BoardSizeSetting";

        public override string serialize()
        {
            return $"{getValue()}";
        }

        protected override int deserialize(string serialized)
        {
            if (!int.TryParse(serialized, out int deserializedVal)) return this.defaultValue;
            return deserializedVal;
        }

        protected override bool propetryValidator(int newValue)
        {
            if (newValue < 10) return false;
            if (newValue > 20) return false;
            return true;
        }
    }
}
