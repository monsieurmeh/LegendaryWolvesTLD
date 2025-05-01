using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public interface ICustomWolfAI
    {
        WolfTypes WolfType { get; }
        BaseAi Target { get; }
        void ProcessCurrentAiMode(BaseAi baseAi);
        void Augment();
        void UnAugment();
    }
}
