#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using static Il2Cpp.BaseAi;
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


        #region Process Helpers

        protected virtual void ClearTargetAndSetDefaultAiMode()
        {
            mTarget.ClearTarget();
            SetDefaultAiMode();
            return;
        }

        protected virtual void SetAiMode(AiMode mode)
        {
            mTarget.SetAiMode(mode);
        }


        protected virtual void SetDefaultAiMode()
        {
            SetAiMode(mTarget.m_DefaultMode);
        }


        protected virtual bool CheckSceneTransitionStarted(PlayerManager playerManager)
        {
            if (playerManager.m_SceneTransitionStarted)
            {
                SetDefaultAiMode();
                return false;
            }
            return true;
        }


        protected virtual bool CheckTargetDead()
        {
            if (mTarget.m_CurrentTarget.IsDead())
            {
                ProcessTargetDead();
                return true;
            }
            return false;
        }

        #endregion


        #region Process Error-Catch Helpers

        protected virtual void Fail(string context, string message)
        {
            LogError($"[BaseWolf.{context}.Fail]: {message}");
        }


        protected virtual bool CheckCurrentTargetInvalid(string failContext)
        {
            return CheckCurrentTargetNull(failContext) || CheckCurrentTargetGameObjectNull(failContext);
        }


        protected virtual bool CheckCurrentTargetNull(string failContext)
        {
            if (mTarget.m_CurrentTarget == null)
            {
                Fail(failContext, "Null target!");
                SetDefaultAiMode();
                return true;
            }
            return false;
        }


        protected virtual bool CheckCurrentTargetGameObjectNull(string failContext)
        {
            if (mTarget.m_CurrentTarget.gameObject == null)
            {
                Fail(failContext, "Null target gameobject!");
                SetDefaultAiMode();
                return true;
            }
            return false;
        }


        protected virtual bool TryGetPlayerManager(string failContext, out PlayerManager playerManager)
        {
            playerManager = GameManager.m_PlayerManager;

            if (playerManager == null)
            {
                Fail(failContext, "Null PlayerManager!");
                SetDefaultAiMode();
                return false;
            }
            return true;
        }

        #endregion


        #region General Helpers





        #endregion





        #region ProcessXYZ overrides


        void ProcessAttack()
        {
            if (CheckCurrentTargetInvalid("ProcessAttack")) return;
            if (!TryGetPlayerManager("ProcessAttack", out PlayerManager playerManager)) return;
            if (CheckSceneTransitionStarted(playerManager)) { return; }
            ProcessStartAttackHowl();
            if (CheckTargetDead()) return;
            //check for active struggle
            if (mTarget.m_CurrentTarget.IsPlayer() && !playerManager.PlayerIsInvisibleToAi() && !GameManager.m_PlayerStruggle.m_Active)
            {
                SetDefaultAiMode();
                return;
            }

            if (mTarget.m_CurrentTarget.IsPlayer())
            {
                if (mTarget.m_CurrentTarget
            }

            /*
            if (mTarget.m_CurrentTarget.IsPlayer())
	        {
	
		        if ((m_CurrentTarget == (AiTarget_o *)0x0) ||
		           (pUVar12 = SuperSplines.SplineNode$$get_Transform((SuperSplines_SplineNode_o *)m_CurrentTarget,(MethodInfo *)0x0), pUVar12 == (UnityEngine_Transform_o *)0x0)) goto LAB_1805279f2;
		        local_88.x = 0.0;
		        local_88.y = 0.0;
		        local_88._8_8_ = local_88._8_8_ & 0xffffffff00000000;
		        pcVar10 = DAT_1843f8f20;
		        if ((DAT_1843f8f20 == (code *)0x0) &&
		           (pcVar10 = (code *)FUN_1802f1f40(
										           "UnityEngine.Transform::get_position_Injected(UnityEngine .Vector3&)"
										           ), pcVar10 == (code *)0x0)) {
		          uVar18 = FUN_1802f1ba0(
								        "UnityEngine.Transform::get_position_Injected(UnityEngine.Vector3&)"
								        );
		          FUN_1802efda0(uVar18,0);
		          pcVar10 = (code *)swi(3);
		          (*pcVar10)();
		          return;
		        }
		        DAT_1843f8f20 = pcVar10;
		        (*DAT_1843f8f20)(pUVar12,&local_88);
		        local_68.x = local_88.x;
		        local_68.y = local_88.y;
		        local_68.z = local_88.z;
		        bvar = BaseAi$$CanPlayerBeReached
						         (__this,(UnityEngine_Vector3_o *)&local_68,1,(MethodInfo *)0x0);
		        if (bvar) {
		          m_CurrentTarget = (__this->fields).m_CurrentTarget;
		          if ((m_CurrentTarget == (AiTarget_o *)0x0) ||
			         (pUVar12 = SuperSplines.SplineNode$$get_Transform
								          ((SuperSplines_SplineNode_o *)m_CurrentTarget,(MethodInfo *)0x0 ),
			         pUVar12 == (UnityEngine_Transform_o *)0x0)) goto LAB_1805279f2;
		          local_88.x = 0.0;
		          local_88.y = 0.0;
		          local_88._8_8_ = local_88._8_8_ & 0xffffffff00000000;
		          pcVar10 = DAT_1843f8f20;
		          if ((DAT_1843f8f20 == (code *)0x0) &&
			         (pcVar10 = (code *)FUN_1802f1f40(
											         "UnityEngine.Transform::get_position_Injected(UnityEngi ne.Vector3&)"
											         ), pcVar10 == (code *)0x0)) {
			        uVar18 = FUN_1802f1ba0(
								          "UnityEngine.Transform::get_position_Injected(UnityEngine.Vector3& )"
								          );
			        FUN_1802efda0(uVar18,0);
			        pcVar10 = (code *)swi(3);
			        (*pcVar10)();
			        return;
		          }
		          DAT_1843f8f20 = pcVar10;
		          (*DAT_1843f8f20)(pUVar12);
		          (__this->fields).m_LastKnownAttackTargetPosition.fields.x = local_88.x;
		          (__this->fields).m_LastKnownAttackTargetPosition.fields.y = local_88.y;
		          (__this->fields).m_LastKnownAttackTargetPosition.fields.z = local_88.z;
		        }
	        }
             */


        }

        #endregion



        #region Overridable Sub-Processes

        protected virtual void ProcessStartAttackHowl()
        {
            if (mTarget.m_CurrentTarget.IsPlayer() && mTarget.m_AttackingLoopAudioID == 0 && mTarget.m_TimeInModeSeconds > 1.5)
            {
                mTarget.m_AttackingLoopAudioID = GameAudioManager.Play3DSound(mTarget.m_ChasingAudio, mTarget.gameObject);
            }
        }


        protected virtual void ProcessTargetDead()
        {
            ClearTargetAndSetDefaultAiMode();
        }

        #endregion
    }
}
