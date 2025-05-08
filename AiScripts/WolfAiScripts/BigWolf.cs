#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE


using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class BigWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.BigWolf; } }

        public BigWolf(BaseAi target) : base(target) { }


        public override void Augment()
        {
            if (mBaseAi.gameObject?.TryGetComponent(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.sharedMaterial.color = Color.red;
            }
            mBaseAi.m_RunSpeed *= 8;
            mBaseAi.m_StalkSpeed *= 8;
            mBaseAi.m_WalkSpeed *= 8;
            mBaseAi.m_turnSpeed *= 8;
            mBaseAi.m_TurnSpeedDegreesPerSecond *= 8;
            Vector3 newScale = new Vector3(2, 2, 2);
            mBaseAi.gameObject.transform.set_localScale_Injected(ref newScale);
            base.Augment();
        }


        public override void UnAugment()
        {
            Vector3 one = Vector3.one;
            mBaseAi?.gameObject?.transform?.set_localScale_Injected(ref one);
        }


        protected override float m_HoldGroundDistanceFromSpear { get { return 0f; } }
        protected override float m_HoldGroundOuterDistanceFromSpear { get { return 0f; } }
    }
}
