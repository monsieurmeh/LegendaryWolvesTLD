using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2CppSystem.Security.Util;
using UnityEngine;
using UnityEngine.AI;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class HidingWolf : BaseWolf, IHideBehaviorOwner
    {
        protected Vector3 mHidingPosition;
        protected Vector3 mHidingOrientation;
        protected bool m_HasReachedHidingLocation;
        protected float m_TimeSinceLastTargetCheck;

        public override WolfTypes WolfType { get { return WolfTypes.HidingWolf; } }

        public HidingWolf(BaseAi target, Vector3 hidingPosition, Vector3 hidingOrientation) : base(target)
        {
            mHidingOrientation = hidingOrientation;
            AiUtils.GetClosestNavmeshPos(out mHidingPosition, hidingPosition, hidingPosition);
        }


        public override void Augment()
        {
            mBaseAi.m_DefaultMode = (AiMode)NewAiModes.Hiding;
            mBaseAi.m_StartMode = (AiMode)NewAiModes.Hiding;
            base.Augment();
        }


        public override void SetAiMode(AiMode mode)
        {
            if (mode == (AiMode)NewAiModes.Hiding && CurrentMode != mode)
            {
                EnterHiding();
            }
            else if (mode != (AiMode)NewAiModes.Hiding && CurrentMode == (AiMode)NewAiModes.Hiding)
            {
                ExitHiding();
            }
            base.SetAiMode(mode);
        }


        protected override bool IsImposter()
        {
            return false;
        }


        public void ProcessHiding()
        {
            if (!m_HasReachedHidingLocation)
            {
                if (Vector3.Distance(mBaseAi.transform.position, mHidingPosition) >= 2.0f)
                {
                    Log($"Distance is {Vector3.Distance(mBaseAi.transform.position, mHidingPosition)}");
                    return; 
                }
                Log("Stopping move agent...");
                mBaseAi.MoveAgentStop();
                m_HasReachedHidingLocation = true;
                mBaseAi.m_MoveAgent.PointTowardsDirection(mHidingPosition);
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



        public void EnterHiding()
        {
            mBaseAi.ClearTarget();
            m_HasReachedHidingLocation = false;
        }



        public void ExitHiding()
        {
            //m_HasReachedHidingLocation = false;   
        }
    }
}
