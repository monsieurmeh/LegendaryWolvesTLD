using Il2Cpp;
using MonsieurMeh.Mods.TLD.LegendaryWolves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public interface IHideBehaviorOwner : ICustomAi
    {
        void ProcessHiding();
        void EnterHiding();
        void ExitHiding();
    }
}
