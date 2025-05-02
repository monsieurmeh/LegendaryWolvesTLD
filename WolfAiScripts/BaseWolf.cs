#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using static Il2Cpp.BaseAi;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;
using static UnityEngine.GraphicsBuffer;
using System.Reflection;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public abstract class BaseWolf : ICustomWolfAI
    {
        protected BaseAi mBaseAi;
        public BaseAi BaseAi { get { return mBaseAi; } }

        public abstract WolfTypes WolfType { get; }

        public BaseWolf(BaseAi baseAi)
        {
            mBaseAi = baseAi;
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
            mBaseAi.ProcessCommonPre();
        }


        protected virtual void Process()
        {
            switch (mBaseAi?.m_CurrentMode ?? AiMode.None)
            {
                case AiMode.Attack:
                    mBaseAi?.ProcessAttack();
                    break;
                case AiMode.Dead:
                    mBaseAi.ProcessDead();
                    break;
                case AiMode.Feeding:
                    mBaseAi.ProcessFeeding();
                    break;
                case AiMode.Flee:
                    mBaseAi.ProcessFlee();
                    break;
                case AiMode.FollowWaypoints:
                    mBaseAi.ProcessFollowWaypoints();
                    break;
                case AiMode.HoldGround:
                    mBaseAi.ProcessHoldGround();
                    break;
                case AiMode.Idle:
                    mBaseAi.ProcessIdle();
                    break;
                case AiMode.Investigate:
                    mBaseAi.ProcessInvestigate();
                    break;
                case AiMode.InvestigateFood:
                    mBaseAi.ProcessInvestigateFood();
                    break;
                case AiMode.InvestigateSmell:
                    mBaseAi.ProcessInvestigateSmell();
                    break;
                case AiMode.Rooted:
                    mBaseAi.ProcessRooted();
                    break;
                case AiMode.Sleep:
                    mBaseAi.ProcessSleep();
                    break;
                case AiMode.Stalking:
                    mBaseAi.ProcessStalking();
                    break;
                case AiMode.Struggle:
                    mBaseAi.ProcessStruggle();
                    break;
                case AiMode.Wander:
                    mBaseAi.ProcessWander();
                    break;
                case AiMode.WanderPaused:
                    mBaseAi.ProcessWanderPaused();
                    break;
                case AiMode.GoToPoint:
                    mBaseAi.ProcessGoToPoint();
                    break;
                case AiMode.InteractWithProp:
                    mBaseAi.ProcessInteractWithProp();
                    break;
                case AiMode.ScriptedSequence:
                    mBaseAi.ProcessScriptedSequence();
                    break;
                case AiMode.Stunned:
                    mBaseAi.ProcessStunned();
                    break;
                case AiMode.ScratchingAntlers:
                    mBaseAi.Moose?.ProcessScratchingAntlers();
                    break;
                case AiMode.PatrolPointsOfInterest:
                    mBaseAi.ProcessPatrolPointsOfInterest();
                    break;
                case AiMode.HideAndSeek:
                    mBaseAi.Timberwolf?.ProcessHideAndSeek();
                    break;
                case AiMode.JoinPack:
                    mBaseAi.Timberwolf?.ProcessJoinPack();
                    break;
                case AiMode.PassingAttack:
                    mBaseAi.ProcessPassingAttack();
                    break;
                case AiMode.Howl:
                    mBaseAi.BaseWolf?.ProcessHowl();
                    break;
                case AiMode.Disabled:
                case AiMode.None:
                default:
                    break;
            }
        }


        protected virtual void PostProcess()
        {
            mBaseAi.ProcessCommonPost();
        }


        #region Process Helpers

        protected virtual void ClearTargetAndSetDefaultAiMode()
        {
            mBaseAi.ClearTarget();
            SetDefaultAiMode();
            return;
        }

        protected virtual void SetAiMode(AiMode mode)
        {
            mBaseAi.SetAiMode(mode);
        }


        protected virtual void SetDefaultAiMode()
        {
            SetAiMode(mBaseAi.m_DefaultMode);
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
            if (mBaseAi.m_CurrentTarget.IsDead())
            {
                ProcessTargetDead();
                return true;
            }
            return false;
        }


        protected virtual bool TryGetTargetPosition(out Vector3 targetPosition)
        {
            if (mBaseAi.m_CurrentTarget.transform == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }
            targetPosition = mBaseAi.m_CurrentTarget.transform.position;
            return true;
        }


        protected virtual void RefreshTargetPosition()
        {
            if (TryGetTargetPosition(out Vector3 targetPosition))
            {
                mBaseAi.MaybeAdjustTargetPosition(targetPosition);
            }
        }


        protected virtual bool CanReachTarget(Vector3 targetPosition)
        {
            return mBaseAi.CanPlayerBeReached(targetPosition);
        }


        protected virtual bool MaybeHoldGroundForAttackCustom(HoldGroundReason reason, Func<float, bool> shouldHoldGroundFunc)
        {
            if (mBaseAi == null || mBaseAi.m_WildlifeMode == WildlifeMode.Aurora)
                return false;

            float innerRadius = mBaseAi.GetInnerRadiusForHoldGroundCause(reason);
            float outerRadius = mBaseAi.GetOuterRadiusForHoldGroundCause(reason);

            bool allowSlowdown = BaseAi.m_AllowSlowdownForHold;

            if (shouldHoldGroundFunc.Invoke(innerRadius))
            {
                SetAiMode(AiMode.HoldGround);
                return true;
            }

            if (allowSlowdown && shouldHoldGroundFunc.Invoke(innerRadius))
            {
                mBaseAi.m_HoldGroundReason = reason;
                RefreshTargetPosition();
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
            if (mBaseAi.m_CurrentTarget == null)
            {
                Fail(failContext, "Null target!");
                SetDefaultAiMode();
                return true;
            }
            return false;
        }


        protected virtual bool CheckCurrentTargetGameObjectNull(string failContext)
        {
            if (mBaseAi.m_CurrentTarget.gameObject == null)
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


        #region ProcessXYZ overrides


        void ProcessAttack()
        {
            if (CheckCurrentTargetInvalid("ProcessAttack"))
                return;
            if (!TryGetPlayerManager("ProcessAttack", out PlayerManager playerManager))
                return;
            if (CheckSceneTransitionStarted(playerManager))
                return;
            if (CheckTargetDead())
                return;

            ProcessStartAttackHowl();
            //check for active struggle
            if (mBaseAi.m_CurrentTarget.IsPlayer())
            {
                if (!playerManager.PlayerIsInvisibleToAi() && !GameManager.m_PlayerStruggle.m_Active)
                {
                    SetDefaultAiMode();
                    return;
                }
                if (!TryGetTargetPosition(out Vector3 targetPosition))
                {
                    SetDefaultAiMode();
                    return;
                }
                if (CanReachTarget(targetPosition))
                {
                    mBaseAi.m_LastKnownAttackTargetPosition = targetPosition;
                }
            }

            if (mBaseAi.m_PlayingAttackStartAnimation)
            {
                AiUtils.TurnTowardsTarget(mBaseAi);
                mBaseAi.MaybeApplyAttack();
                if (mBaseAi.m_TimeInModeSeconds <= mBaseAi.m_AnimationTime)
                    return;
                mBaseAi.m_PlayingAttackStartAnimation = false;
            }

            mBaseAi.m_HoldGroundReason = HoldGroundReason.None;
            if (mBaseAi.CanHoldGround())
            {
                if (mBaseAi.IsInFlashLight())
                {
                    if (mBaseAi.Timberwolf != null)
                    {
                        SetAiMode(AiMode.Flee);
                        return;
                    }
                }
                else
                {
                    float targetDistance = mBaseAi.m_CurrentTarget.Distance(mBaseAi.transform.position);
                    if (targetDistance <= mBaseAi.m_FightOrFlightRange)
                    {
                        if (mBaseAi.StarvingWolf != null)
                        {
                            if (GameManager.m_ChemicalPoisoning != null)
                            {
                                //Something to do with the chemical poisoning as well, but I can't decode this shit. lol
                                /*
					            if (*(char *)&GameManager.m_ChemicalPoisoning[7].monitor != '\0') 
					            {
						            mode = 4;
						            goto LAB_1805279b4;
					            }
                                 */
                            }
                        }
                        //goto LAB_180527487; basically, exit "CanHoldGround" mode and do further processing, im assuming stalking up on shit
                    }
                }
                if (mBaseAi.MaybeHoldGroundDueToSafeHaven())
                    return;
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Spear, (radius) => mBaseAi.MaybeHoldGroundForSpear(radius)))
                    return;
                if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
                {
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Torch, (radius) => mBaseAi.MaybeHoldGroundForTorch(radius)))
                        return;
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.TorchOnGround, (radius) => mBaseAi.MaybeHoldGroundForTorchOnGround(radius)))
                        return;
                }
                if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
                {
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.RedFlare, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                        return;
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.RedFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForRedFlareOnGround(radius)))
                        return;
                }
                if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
                {
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Fire, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                        return;
                }
                if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
                {
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.BlueFlare, (radius) => mBaseAi.MaybeHoldGroundForBlueFlare(radius)))
                        return;
                    if (MaybeHoldGroundForAttackCustom(HoldGroundReason.BlueFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForBlueFlareOnGround(radius)))
                        return;
                }

                if (mBaseAi.m_HoldGroundReason != HoldGroundReason.None)
                {
                    if (!mBaseAi.m_UseSlowdownForHold)
                    {
                        if (BaseAi.m_AllowSlowdownForHold && !mBaseAi.IsInFlashLight())
                        {
                            RefreshTargetPosition();
                            switch (mBaseAi.m_HoldGroundReason)
                            {
                              

                            }
                        }
                    }
                }
            }
        }

        #endregion


        #region Overridable Sub-Processes

        protected virtual void ProcessStartAttackHowl()
        {
            if (mBaseAi.m_CurrentTarget.IsPlayer() && mBaseAi.m_AttackingLoopAudioID == 0 && mBaseAi.m_TimeInModeSeconds > 1.5)
            {
                mBaseAi.m_AttackingLoopAudioID = GameAudioManager.Play3DSound(mBaseAi.m_ChasingAudio, mBaseAi.gameObject);
            }
        }


        protected virtual void ProcessTargetDead()
        {
            ClearTargetAndSetDefaultAiMode();
        }


        #endregion
    }
}
