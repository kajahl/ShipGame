using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Statki2.ViewModels
{
    class ViewModelData
    {
        public string GameStateName { get; }
        public string GameStateDescription { get; }
        public string MainButton { get; }
        public string MainButtonTooltip { get; }

        public ViewModelData(string gameStateName, string gameStateDescription, string mainButton, string mainButtonTooltip)
        {
            this.GameStateName = gameStateName;
            this.GameStateDescription = gameStateDescription;
            this.MainButton = mainButton;
            this.MainButtonTooltip = mainButtonTooltip;
        }
    }
}
