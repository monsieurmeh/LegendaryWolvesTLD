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
    public interface IReturningBehaviorOwner : ICustomAi
    {
        void ProcessReturning();
        void EnterReturning();
        void ExitReturning();
    }
}
