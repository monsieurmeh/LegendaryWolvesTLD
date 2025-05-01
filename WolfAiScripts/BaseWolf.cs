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


        protected virtual void PreProcess(BaseAi baseAi)
        {
            baseAi.ProcessCommonPre();
        }


        protected virtual void Process(BaseAi baseAi)
        {
            switch (baseAi?.m_CurrentMode ?? AiMode.None)
            {
                case AiMode.Attack:
                    baseAi?.ProcessAttack(); 
                    break;
                case AiMode.Dead:
                    baseAi.ProcessDead();
                    break;
                case AiMode.Feeding:
                    baseAi.ProcessFeeding();
                    break;
                case AiMode.Flee:
                    baseAi.ProcessFlee();
                    break;
                case AiMode.FollowWaypoints:
                    baseAi.ProcessFollowWaypoints();
                    break;
                case AiMode.HoldGround:
                    baseAi.ProcessHoldGround();
                    break;
                case AiMode.Idle:
                    baseAi.ProcessIdle();
                    break;
                case AiMode.Investigate:
                    baseAi.ProcessInvestigate();
                    break;
                case AiMode.InvestigateFood:
                    baseAi.ProcessInvestigateFood();
                    break;
                case AiMode.InvestigateSmell:
                    baseAi.ProcessInvestigateSmell();
                    break;
                case AiMode.Rooted:
                    baseAi.ProcessRooted();
                    break;
                case AiMode.Sleep:
                    baseAi.ProcessSleep();
                    break;
                case AiMode.Stalking:
                    baseAi.ProcessStalking();
                    break;
                case AiMode.Struggle:
                    baseAi.ProcessStruggle();
                    break;
                case AiMode.Wander:
                    baseAi.ProcessWander();
                    break;
                case AiMode.WanderPaused:
                    baseAi.ProcessWanderPaused();
                    break;
                case AiMode.GoToPoint:
                    baseAi.ProcessGoToPoint();
                    break;
                case AiMode.InteractWithProp:
                    baseAi.ProcessInteractWithProp();
                    break;
                case AiMode.ScriptedSequence:
                    baseAi.ProcessScriptedSequence();
                    break;
                case AiMode.Stunned:
                    baseAi.ProcessStunned();
                    break;
                case AiMode.ScratchingAntlers:
                    baseAi.Moose?.ProcessScratchingAntlers();
                    break;
                case AiMode.PatrolPointsOfInterest:
                    baseAi.ProcessPatrolPointsOfInterest();
                    break;
                case AiMode.HideAndSeek:
                    baseAi.Timberwolf?.ProcessHideAndSeek();
                    break;
                case AiMode.JoinPack:
                    baseAi.Timberwolf?.ProcessJoinPack();
                    break;
                case AiMode.PassingAttack:
                    baseAi.ProcessPassingAttack();
                    break;
                case AiMode.Howl:
                    baseAi.BaseWolf?.ProcessHowl();
                    break;
                case AiMode.Disabled:
                case AiMode.None:
                default:
                    break;
            }
        }


        protected virtual void PostProcess(BaseAi baseAi)
        {
            baseAi.ProcessCommonPost();
        }


        public void ProcessCurrentAiMode(BaseAi baseAi)
        {
#if DEV_BUILD
            try
            {
#endif
            PreProcess(baseAi);
            Process(baseAi);
            PostProcess(baseAi);
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
