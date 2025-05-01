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
            if (mTarget.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.sharedMaterial.color = Color.red;
            }
            if (mAugmentFactor > 10f || mAugmentFactor < 5f)
            {
                mAugmentFactor = new System.Random().Next(500, 1000) * 0.01f;
            }
            mTarget.m_RunSpeed *= mAugmentFactor;
            mTarget.m_StalkSpeed *= mAugmentFactor;
            mTarget.m_WalkSpeed *= mAugmentFactor;
            mTarget.m_turnSpeed *= mAugmentFactor;
            mTarget.m_TurnSpeedDegreesPerSecond *= mAugmentFactor;
            Vector3 newScale = new Vector3(mAugmentFactor, mAugmentFactor, mAugmentFactor);
            mTarget.gameObject.transform.set_localScale_Injected(ref newScale);
        }


        public override void UnAugment()
        {
            Vector3 one = Vector3.one;
            mTarget?.gameObject?.transform?.set_localScale_Injected(ref one);
        }
    }
}
