using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Settings
{
    internal class ShipLengthsSetting : ASingleSetting<Dictionary<int,int>>
    {

        public override string Name { get; } = "shipLengths";

        public ShipLengthsSetting() : base(new Dictionary<int, int>(){{9, 0}, {8, 0}, {7, 0}, {6, 0}, {5, 0}, {4, 1}, {3, 2}, {2, 3}, {1, 4}}) {}

        
        protected override bool propetryValidator(Dictionary<int, int> newValue)
        {
            for (int i = 0; i < 10; i++)
            {
                if (!newValue.ContainsKey(i)) return false;
                if (newValue[i] < 0 || newValue[i] > 10) return false;
            }
            return true;
        }

        public void SetShipLengthsCount(int shipLength, int count)
        {
            Dictionary<int, int> currentValue = getValue();
            currentValue[shipLength] = count;
            setValue(currentValue);
        }

        public override string serialize()
        {
            string serialized = "";
            foreach(var kvp in getValue())
                serialized += (serialized.Length != 0 ? ";" : "") + $"{kvp.Key},{kvp.Value}";
            return serialized;
        }

        protected override Dictionary<int, int> deserialize(string serialized)
        {
            Dictionary<int,int> deserializedVal = new Dictionary<int, int>(); 
            string[] kvps = serialized.Split(';');
            foreach(string kvpString in kvps)
            {
                string[] kvp = kvpString.Split(',');
                if (kvp.Length != 0) continue;
                if (!int.TryParse(kvpString[0].ToString(), out int kvpKey) || !int.TryParse(kvpString[1].ToString(), out int kvpValue)) continue;
                deserializedVal.Add(kvpKey, kvpValue);
            }
            return deserializedVal;
        }

        public override Dictionary<int, int> getValue()
        {
            return base.getValue().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
}
