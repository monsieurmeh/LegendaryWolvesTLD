#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE


using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class ScaredyWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.ScaredyWolf; } }

        public ScaredyWolf(BaseAi target) : base(target) { }

        protected override void Process()
        {

            switch (mTarget?.m_CurrentMode ?? AiMode.None)
            {
                case AiMode.Attack:
                case AiMode.Flee:
                case AiMode.Stalking:
                case AiMode.Struggle:
#if DEV_BUILD_LOG
                        Log("Flee, scaredy wolf!");
#endif
                    mTarget.ProcessFlee();
                    break;
                default:
                    base.Process();
                    break;
            }
        }


        public override void Augment()
        {
            if (mTarget.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.material.color = Color.yellow;
            }
        }
    }
}
