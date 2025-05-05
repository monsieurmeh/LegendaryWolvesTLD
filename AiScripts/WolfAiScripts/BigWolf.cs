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
        protected float mAugmentFactor;
        public override WolfTypes WolfType { get { return WolfTypes.BigWolf; } }

        public BigWolf(BaseAi target, float augmentFactor = 0f) : base(target) { mAugmentFactor = augmentFactor; }


        public override void Augment()
        {
            if (mBaseAi.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.sharedMaterial.color = Color.red;
            }
            if (mAugmentFactor > 10f || mAugmentFactor < 5f)
            {
                mAugmentFactor = new System.Random().Next(500, 1000) * 0.01f;
            }
            mBaseAi.m_RunSpeed *= (mAugmentFactor * mAugmentFactor);
            mBaseAi.m_StalkSpeed *= (mAugmentFactor * mAugmentFactor);
            mBaseAi.m_WalkSpeed *= (mAugmentFactor * mAugmentFactor);
            mBaseAi.m_turnSpeed *= (mAugmentFactor * (mAugmentFactor * mAugmentFactor));
            mBaseAi.m_TurnSpeedDegreesPerSecond *= (mAugmentFactor * mAugmentFactor);
            Vector3 newScale = new Vector3(mAugmentFactor, mAugmentFactor, mAugmentFactor);
            mBaseAi.gameObject.transform.set_localScale_Injected(ref newScale);
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
