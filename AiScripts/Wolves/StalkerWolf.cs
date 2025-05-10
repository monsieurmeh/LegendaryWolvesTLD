using HarmonyLib;
using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2CppSystem.Security.Util;
using UnityEngine;
using UnityEngine.AI;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class StalkingWolf : BaseWolf
    {
        protected float m_TimeSinceLastSmellCheck = 0.0f;
        public override WolfTypes WolfType { get { return WolfTypes.Stalker; } }


        public StalkingWolf(BaseAi target) : base(target) { }


        protected override bool ProcessCustom()
        {
            switch(CurrentMode)
            {
                case AiMode.InvestigateSmell: 
                    ProcessInvestigateSmellCustom(); 
                    return true;
                default: 
                    return false;
            }
        }


        protected override bool TestIsImposterCustom(out bool isImposter)
        {
            isImposter = false;
            return true;
        }


        protected void ProcessInvestigateSmellCustom()
        {
            if (!mBaseAi.CanPlayerBeReached(GameManager.m_PlayerManager.m_LastPlayerPosition))
            {
                //Log($"Cant reach player, swapping back to default mode...");
                SetDefaultAiMode();
                return; 
            }
            if (!mBaseAi.m_SmellTarget?.IsPlayer() ?? true)
            {
                mBaseAi.m_SmellTarget = GameManager.m_PlayerManager.m_AiTarget;
            }
            if (mBaseAi.m_SmellTarget == null)
            {
                //Log($"smell target is null, aborting!");
                return;
            }
            if (!mBaseAi.m_HasInvestigateSmellPath) 
            {
                if (!AiUtils.GetClosestNavmeshPos(out Vector3 navMeshPos, mBaseAi.m_SmellTarget.transform.position, mBaseAi.m_SmellTarget.transform.position))
                {
                    //Log($"Unable to get closest navmesh point, aborting!");
                    SetDefaultAiMode();
                    return;
                }
                mBaseAi.m_PathingToSmellTargetPos = navMeshPos;
                if (Vector3.Distance(mBaseAi.m_CachedTransform.position, mBaseAi.m_PathingToSmellTargetPos) < mBaseAi.m_MinSmellDistance)
                {
                    //Log($"[Pathfind Check] Distance from pos ({mBaseAi.m_CachedTransform.position}) to target ({mBaseAi.m_CachedTransform.position}) [{Vector3.Distance(mBaseAi.m_CachedTransform.position, mBaseAi.m_PathingToSmellTargetPos)}] is less than minSmellDistance ({mBaseAi.m_MinSmellDistance}), trying attack or returning");
                    if (mBaseAi.CanSeeTarget())
                    {
                        SetAiMode(AiMode.Attack);
                    }
                    return;
                }
                mBaseAi.StartPath(mBaseAi.m_PathingToSmellTargetPos, mBaseAi.m_StalkSpeed);
                mBaseAi.m_HasInvestigateSmellPath = true;
            }
            if (Vector3.Distance(mBaseAi.m_CachedTransform.position, mBaseAi.m_PathingToSmellTargetPos) < mBaseAi.m_MinSmellDistance)
            {
                //Log($"[Moving Check] Distance from pos ({mBaseAi.m_CachedTransform.position}) to target ({mBaseAi.m_CachedTransform.position}) [{Vector3.Distance(mBaseAi.m_CachedTransform.position, mBaseAi.m_PathingToSmellTargetPos)}] is less than minSmellDistance ({mBaseAi.m_MinSmellDistance}), stopping move agent and resetting calcs");
                mBaseAi.m_HasInvestigateSmellPath = false;
                mBaseAi.MoveAgentStop();
            }
            if (mBaseAi.CanSeeTarget())
            {
                SetAiMode(AiMode.Attack);
                return;
            }
        }


        protected override bool PostProcessCustom()
        {
            if (m_TimeSinceLastSmellCheck <= 2.0f)
            {
                m_TimeSinceLastSmellCheck += Time.deltaTime;
                return false;
            }
            else
            {
                m_TimeSinceLastSmellCheck = 0;
            }
            if (CurrentModeFlag.NoneOf(AiModeFlags.TypicalDontInterrupt))
            {
                if (mBaseAi.CanPathfindToPosition(GameManager.m_PlayerManager.m_LastPlayerPosition))
                {
                    mBaseAi.m_SmellTarget = GameManager.m_PlayerManager.m_AiTarget;
                    mBaseAi.m_AiTarget = mBaseAi.m_SmellTarget;
                    SetAiMode(AiMode.InvestigateSmell);
                }
                else if (CurrentMode != AiMode.InvestigateSmell)
                {
                    SetAiMode(AiMode.Wander);
                }
            }
            return false;
        }
    }
}
 