using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2CppSystem.Security.Util;
using UnityEngine;
using UnityEngine.AI;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class HidingWolf : BaseWolf, IHideBehaviorOwner, IReturningBehaviorOwner
    {
        protected Vector3 mHidingPosition;
        protected Vector3 mHidingOrientation;
        protected float m_TimeSinceLastTargetCheck;

        public override WolfTypes WolfType { get { return WolfTypes.HidingWolf; } }

        public HidingWolf(BaseAi target) : base(target)
        {
            bool spotFound = false;
            for (int i = 0, iMax = LegendaryWolvesManager.Instance.SpotsAvailable(); i < iMax; i++)
            {
                if (!LegendaryWolvesManager.Instance.SpotAvailable(i, out Vector3 hidingSpot, out Vector3 hidingOrientation))
                {
                    Log($"Spot {i} at {hidingSpot} is taken!");
                    continue;
                }
                if (!AiUtils.GetClosestNavmeshPos(out Vector3 finalHidingSpot, hidingSpot, hidingSpot))
                {
                    Log($"Cant get closest nav mesh point to spot {i} at {hidingSpot}!");
                    continue;
                }
                if (!mBaseAi.m_MoveAgent.CanFindPath(finalHidingSpot, MoveAgent.PathRequirement.FullPath))
                {
                    Log($"Cant pathfind to position {i} at {finalHidingSpot}!");
                    continue;
                }
                mHidingPosition = finalHidingSpot;
                mHidingOrientation = hidingOrientation;
                LegendaryWolvesManager.Instance.TakeSpot(i);
                Log($"Taking spot {i} at {finalHidingSpot}...");
                spotFound = true;
                break;
            }
            while (!spotFound)
            {
                if (AiUtils.GetRandomPointOnNavmesh(out Vector3 validPos, mBaseAi.transform.position, 250.0f, 25.0f, -1, false, 0.2f) && mBaseAi.m_MoveAgent.CanFindPath(validPos, MoveAgent.PathRequirement.FullPath))
                {
                    mHidingPosition = validPos;
                    mHidingOrientation = new Vector3(UnityEngine.Random.Range(0f, 360f), 0f, 0f);
                    spotFound = true;
                }
            }
            GameObject waypointMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            GameObject.Destroy(waypointMarker.GetComponent<Collider>());
            waypointMarker.transform.localScale = new Vector3(.5f, 100f, .5f);
            waypointMarker.transform.position = mHidingPosition;
            waypointMarker.GetComponent<Renderer>().material.color = Color.green;
            waypointMarker.name = $"hidingwolf point";
        }


        public override void Augment()
        {
            mBaseAi.m_DefaultMode = (AiMode)NewAiModes.Hiding;
            mBaseAi.m_StartMode = (AiMode)NewAiModes.Hiding;
            base.Augment();
        }


        protected override bool IsImposter()
        {
            return false;
        }


        public virtual void ProcessHiding()
        {
            if (Vector3.Distance(mBaseAi.transform.position, mHidingPosition) >= 2.0f)
            {
                SetAiMode((AiMode)NewAiModes.Returning);
                return;
            }
            if (m_TimeSinceLastTargetCheck <= 2.0f)
            {
                m_TimeSinceLastTargetCheck += Time.deltaTime;
            }
            else
            {
                m_TimeSinceLastTargetCheck = 0.0f; 
                mBaseAi.ScanForNewTarget();
            }
        }



        public virtual void EnterHiding()
        {
            mBaseAi.MoveAgentStop();
            mBaseAi.ClearTarget();
            mBaseAi.m_MoveAgent.PointTowardsDirection(mHidingPosition);
        }



        public virtual void ExitHiding()
        {

        }


        public virtual void ProcessReturning()
        {
            mBaseAi.ScanForNewTarget();
            if (Vector3.Distance(mBaseAi.transform.position, mHidingPosition) >= 2.0f)
            {
                return;
            }
            SetAiMode((AiMode)NewAiModes.Hiding);
        }



        public virtual void EnterReturning()
        {
            mBaseAi.MoveAgentStop();
            mBaseAi.ClearTarget();
            mBaseAi.m_AiGoalSpeed = mBaseAi.m_RunSpeed;
            mBaseAi.StartPath(mHidingPosition, mBaseAi.m_AiGoalSpeed);
        }



        public virtual void ExitReturning()
        {

        }
    }
}
