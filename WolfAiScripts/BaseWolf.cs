#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public abstract class BaseWolf : ICustomWolfAI
    {
        protected BaseAi mTarget;

        public BaseAi Target { get { return mTarget; } }
        public abstract WolfTypes WolfType { get; }


        public BaseWolf(BaseAi target)
        {
            mTarget = target;
        }


        public virtual void Augment() { }


        public virtual void UnAugment() { }


        protected virtual void PreProcess()
        {
            mTarget.ProcessCommonPre();
        }


        protected virtual void Process()
        {
            switch (mTarget?.m_CurrentMode ?? AiMode.None)
            {
                case AiMode.Attack:
                    mTarget?.ProcessAttack(); 
                    break;
                case AiMode.Dead:
                    mTarget.ProcessDead();
                    break;
                case AiMode.Feeding:
                    mTarget.ProcessFeeding();
                    break;
                case AiMode.Flee:
                    mTarget.ProcessFlee();
                    break;
                case AiMode.FollowWaypoints:
                    mTarget.ProcessFollowWaypoints();
                    break;
                case AiMode.HoldGround:
                    mTarget.ProcessHoldGround();
                    break;
                case AiMode.Idle:
                    mTarget.ProcessIdle();
                    break;
                case AiMode.Investigate:
                    mTarget.ProcessInvestigate();
                    break;
                case AiMode.InvestigateFood:
                    mTarget.ProcessInvestigateFood();
                    break;
                case AiMode.InvestigateSmell:
                    mTarget.ProcessInvestigateSmell();
                    break;
                case AiMode.Rooted:
                    mTarget.ProcessRooted();
                    break;
                case AiMode.Sleep:
                    mTarget.ProcessSleep();
                    break;
                case AiMode.Stalking:
                    mTarget.ProcessStalking();
                    break;
                case AiMode.Struggle:
                    mTarget.ProcessStruggle();
                    break;
                case AiMode.Wander:
                    mTarget.ProcessWander();
                    break;
                case AiMode.WanderPaused:
                    mTarget.ProcessWanderPaused();
                    break;
                case AiMode.GoToPoint:
                    mTarget.ProcessGoToPoint();
                    break;
                case AiMode.InteractWithProp:
                    mTarget.ProcessInteractWithProp();
                    break;
                case AiMode.ScriptedSequence:
                    mTarget.ProcessScriptedSequence();
                    break;
                case AiMode.Stunned:
                    mTarget.ProcessStunned();
                    break;
                case AiMode.ScratchingAntlers:
                    mTarget.Moose?.ProcessScratchingAntlers();
                    break;
                case AiMode.PatrolPointsOfInterest:
                    mTarget.ProcessPatrolPointsOfInterest();
                    break;
                case AiMode.HideAndSeek:
                    mTarget.Timberwolf?.ProcessHideAndSeek();
                    break;
                case AiMode.JoinPack:
                    mTarget.Timberwolf?.ProcessJoinPack();
                    break;
                case AiMode.PassingAttack:
                    mTarget.ProcessPassingAttack();
                    break;
                case AiMode.Howl:
                    mTarget.BaseWolf?.ProcessHowl();
                    break;
                case AiMode.Disabled:
                case AiMode.None:
                default:
                    break;
            }
        }


        protected virtual void PostProcess()
        {
            mTarget.ProcessCommonPost();
        }


        public void ProcessCurrentAiMode()
        {
#if DEV_BUILD
            try
            {
#endif
            PreProcess();
            Process();
            PostProcess();
#if DEV_BUILD
            }
            catch (Exception e)
            {
                LogError($"Error while trying to process wolf ai: {e}");
                return;
            }
#endif
        }
    }
}
