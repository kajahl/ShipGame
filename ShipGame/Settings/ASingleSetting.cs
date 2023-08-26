using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.Settings
{
    internal abstract class ASingleSetting<T>
    {
        public abstract string Name { get; }
        private T property;
        public readonly T defaultValue;
        protected ASingleSetting(T defaultValue)
        {
            this.defaultValue = defaultValue;
            property = defaultValue;
        }

        public virtual T getValue()
        {
            return property;
        }

        public virtual void setValue(T property)
        {
            if (property == null) return;
            if (!propetryValidator(property)) return;
            this.property = property;
        }

        protected abstract bool propetryValidator(T newValue);

        public abstract string serialize(); // Object -> string
        protected abstract T deserialize(string serialized); // String -> object
        public void deserializeObject(string serialized) => setValue(deserialize(serialized));

    }
}
