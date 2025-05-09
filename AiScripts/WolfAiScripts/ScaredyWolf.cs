using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class ScaredyWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.ScaredyWolf; } }

        public ScaredyWolf(BaseAi target) : base(target) { }


        public override void SetAiMode(AiMode mode)
        {
            if (((AiModeFlags)(1U << (int)mode)).AnyOf(AiModeFlags.ScaredyWolfIgnoreModes))
            {
                Log($"Scaredy wolves don't like to {mode}!");
                mode = AiMode.Flee;
            }
            if (CurrentMode != mode)
            {
                base.SetAiMode(mode);
            }
        }


        public override void Augment()
        {
            if (mBaseAi.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.material.color = Color.yellow;
            }
            base.Augment();
        }
    }
}
