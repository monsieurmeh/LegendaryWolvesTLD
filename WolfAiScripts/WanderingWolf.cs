#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE


using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class WanderingWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.Wanderer; } }

        public WanderingWolf(BaseAi target) : base(target) { }

        public override void Augment()
        {
            BaseAi.PointOfInterest poi = new BaseAi.PointOfInterest();
            Transform playerTransform = GameManager.GetPlayerTransform();
            poi.m_Location = new Vector3(playerTransform.position.x, playerTransform.position.y, playerTransform.position.z);
            poi.m_Location = new Vector3(playerTransform.position.x + 10, playerTransform.position.y, playerTransform.position.z - 10);
            poi.m_Location = new Vector3(playerTransform.position.x + 10, playerTransform.position.y, playerTransform.position.z + 10); 
            poi.m_Location = new Vector3(playerTransform.position.x - 10, playerTransform.position.y, playerTransform.position.z - 10);
            poi.m_Location = new Vector3(playerTransform.position.x - 10, playerTransform.position.y, playerTransform.position.z + 10);
            mBaseAi.m_WalkSpeed *= 10;
            mBaseAi.m_ActivePointsOfInterest.Add(poi); 
            if (mBaseAi.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.material.color = Color.cyan;
            }
        }
    }
}
