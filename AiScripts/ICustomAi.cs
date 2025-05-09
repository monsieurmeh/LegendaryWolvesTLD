using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public interface ICustomAi
    {
        BaseAi BaseAi { get; }
        bool SetAiModeLock { get; }
        void SetAiMode(AiMode mode);
        void Update();
        void Augment();
        void UnAugment();
    }
}
