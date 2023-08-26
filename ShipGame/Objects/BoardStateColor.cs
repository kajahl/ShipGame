using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Statki2.Objects
{
    class BoardStateColor
    {
        public Brush foreground { get; }
        public Brush background { get; }
        public string description { get; }
        public BoardStateColor(Brush foreground, Brush background, string description)
        {
            this.foreground = foreground;
            this.background = background;
            this.description = description;
        }
    }
}
