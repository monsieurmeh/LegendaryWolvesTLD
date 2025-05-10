using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2CppSystem.Security.Util;
using UnityEngine;
using UnityEngine.AI;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class WanderingWolf : BaseWolf
    {
        public override WolfTypes WolfType { get { return WolfTypes.Wanderer; } }

        public WanderingWolf(BaseAi target) : base(target) { }

        protected override float m_MinWaypointDistance { get { return 100.0f; } }
        protected override float m_MaxWaypointDistance { get { return 1000.0f; } }
        //protected override float m_FollowWaypointsSpeed { get { return 100.0f; } }

        public override void Augment()
        {            
            int newNumWaypoints = UnityEngine.Random.Range(4, 8);
            mBaseAi.m_Waypoints = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Vector3>(newNumWaypoints);
            int failCount = 0;
            for (int i = 0, iMax = newNumWaypoints; i < iMax;)
            {
                if (AiUtils.GetRandomPointOnNavmesh(out Vector3 validPos, mBaseAi.transform.position, Mathf.Max(m_MinWaypointDistance - failCount, 10.0f), Mathf.Max(m_MaxWaypointDistance - failCount, 10.0f), -1, false, 0.2f) && mBaseAi.CanPathfindToPosition(validPos, MoveAgent.PathRequirement.FullPath))
                {
                    mBaseAi.m_Waypoints[i] = validPos;
                    GameObject waypointMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    GameObject.Destroy(waypointMarker.GetComponent<Collider>());
                    waypointMarker.transform.localScale = new Vector3(0.5f, 25f, 0.5f);
                    waypointMarker.transform.position = validPos;
                    waypointMarker.GetComponent<Renderer>().material.color = Color.green;
                    waypointMarker.name = $"WanderingWolf Waypoint Marker #{i}";
                    if (i > 0)
                    {
                        GameObject waypointConnector = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        GameObject.Destroy(waypointConnector.GetComponent<Collider>());
                        Vector3 direction = validPos - mBaseAi.m_Waypoints[i - 1];
                        float distance = direction.magnitude;
                        waypointConnector.transform.position = ((mBaseAi.m_Waypoints[i - 1] + mBaseAi.m_Waypoints[i]) / 2.0f) + new Vector3(0f, 25.0f, 0f);
                        waypointConnector.transform.localScale = new Vector3(0.5f, distance / 2.0f, 0.5f);
                        waypointConnector.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
                        waypointConnector.GetComponent<Renderer>().material.color = Color.green;
                        waypointConnector.name = $"WanderingWolf Waypoint Marker Connector #{i - 1} -> #{i}";
                    }
                    i++;
                }
                else
                {
                    failCount++;
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


        protected override bool TestIsImposterCustom(out bool isImposter)
        {
            isImposter = false;
            return false;
        }
    }
}
