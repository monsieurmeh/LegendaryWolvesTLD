using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using UnityEngine;
using UnityEngine.AI;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class WanderingWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.Wanderer; } }

        public WanderingWolf(BaseAi target) : base(target) { }

        protected override float m_MaxWaypointDistance { get { return 500.0f; } }

        public override void Augment()
        {            
            int newNumWaypoints = UnityEngine.Random.Range(4, 8);
            mBaseAi.m_Waypoints = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(newNumWaypoints);
            for (int i = 0, iMax = newNumWaypoints; i < iMax;)
            {
                if (AiUtils.GetRandomPointOnNavmesh(out Vector3 validPos, mBaseAi.transform.position, 25.0f, m_MaxWaypointDistance * 0.75f, -1, false, 0.2f) && mBaseAi.CanPathfindToPosition(validPos, MoveAgent.PathRequirement.FullPath))
                {
                    mBaseAi.m_Waypoints[i] = validPos;
                    i++;
                }
            }
            mBaseAi.m_DefaultMode = AiMode.FollowWaypoints;
            mBaseAi.m_StartMode = AiMode.FollowWaypoints;
            mBaseAi.m_CurrentMode = AiMode.FollowWaypoints;
            mBaseAi.m_WaypointCompletionBehaviour = BaseAi.WaypointCompletionBehaviouir.Restart;
            mBaseAi.m_TargetWaypointIndex = 0;
            if (mBaseAi.gameObject?.TryGetComponent<SkinnedMeshRenderer>(out SkinnedMeshRenderer renderer) ?? false)
            {
                renderer.material.color = Color.cyan; 
            }
            base.Augment();
        }


        protected override bool IsImposter()
        {
            return false;
        }
    }
}
