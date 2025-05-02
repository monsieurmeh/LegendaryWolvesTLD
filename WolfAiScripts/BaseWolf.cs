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
using Il2CppSuperSplines;
using ModSettings;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class BaseWolf : ICustomWolfAI
    {
        #region Internals

        protected BaseAi mBaseAi;
        public BaseAi BaseAi { get { return mBaseAi; } }

        public virtual WolfTypes WolfType { get { return WolfTypes.Default; } }

        public BaseWolf(BaseAi baseAi)
        {
            mBaseAi = baseAi;
        }

        #endregion


        #region Protected accessors

        protected AiTarget CurrentTarget { get { return mBaseAi.m_CurrentTarget; } }

        #endregion


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
                    ProcessAttack();
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
            if (CurrentTarget.IsDead())
            {
                ProcessTargetDead();
                return true;
            }
            return false;
        }


        protected virtual bool TryGetTargetPosition(out Vector3 targetPosition)
        {
            if (CurrentTarget.transform == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }
            targetPosition = CurrentTarget.transform.position;
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

            if (allowSlowdown && shouldHoldGroundFunc.Invoke(outerRadius))
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
            if (CurrentTarget == null)
            {
                Fail(failContext, "Null target!");
                SetDefaultAiMode();
                return true;
            }
            return false;
        }


        protected virtual bool CheckCurrentTargetGameObjectNull(string failContext)
        {
            if (CurrentTarget.gameObject == null)
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


		protected virtual bool TryGetPlayerSafeHaven(string failContext, out AuroraField playerSaveHaven)
		{
			playerSaveHaven = mBaseAi.m_PlayerSafeHaven;
            if (playerSaveHaven == null)
			{
				Fail(failContext, "Null player safe haven!");
				SetDefaultAiMode();
				return false;
			}
			return true;
		}


        protected virtual bool TryGetContainingAuroraField(string failContext, out AuroraField containingAuroraField)
        {
            containingAuroraField = mBaseAi.m_ContainingAuroraField;
            if (containingAuroraField == null)
            {
                Fail(failContext, "Null containing Aurora Field!");
                SetDefaultAiMode();
                return false;
            }
            return true;
        }

        #endregion


        #region ProcessXYZ overrides


        protected virtual void ProcessAttack()
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
            if (CurrentTarget.IsPlayer())
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
                    float targetDistance = CurrentTarget.Distance(mBaseAi.transform.position);
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
                        ProcessAttack2();
                        return;
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
							Vector3 newTargetPosition;
                            if (mBaseAi.m_HoldGroundReason != HoldGroundReason.None)
                            {
                                switch (mBaseAi.m_HoldGroundReason)
                                {
                                    case HoldGroundReason.SafeHaven:
                                        if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven))
                                            return;
                                        newTargetPosition = playerSafeHaven.transform.position;
                                        break;
                                    case HoldGroundReason.NearbyAuroraField:
                                        if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField))
                                            return;
                                        newTargetPosition = containingAuroraField.transform.position;
                                        break;
                                    case HoldGroundReason.InsideAuroraField:
                                        if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField2))
                                            return;
                                        try
                                        {
                                            SplineNode splineNode = ((object)containingAuroraField2) as SplineNode;
                                            newTargetPosition = splineNode.Transform.position;
                                        }
                                        catch
                                        {

                                            newTargetPosition = containingAuroraField2.transform.position;
                                        }
                                        break;
                                    case HoldGroundReason.RedFlare:
                                    case HoldGroundReason.Torch:
                                    case HoldGroundReason.Spear:
                                    case HoldGroundReason.Fire:
                                    case HoldGroundReason.BlueFlare:
                                    case HoldGroundReason.BlueFlareOnGround:
                                    case HoldGroundReason.RedFlareOnGround:
                                    case HoldGroundReason.TorchOnGround:
                                        newTargetPosition = CurrentTarget.GetEyePos();
                                        break;
                                    default:
                                        newTargetPosition = Vector3.zero;
                                        break;
                                }
                                mBaseAi.m_CurrentRadius = Vector3.Distance(mBaseAi.transform.position, newTargetPosition);

                                //outer radius calc
                                float temp = 0f;
                                switch (mBaseAi.m_HoldGroundReason)
                                {
                                    case HoldGroundReason.NearbyAuroraField:
                                        AuroraField nearbyAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (nearbyAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp = playerSafeHaven2.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        }
                                        else
                                        {
                                            temp = nearbyAuroraField.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField;
                                        }
                                        break;
                                    case HoldGroundReason.InsideAuroraField:
                                        AuroraField containingAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (containingAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp = playerSafeHaven2.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        }
                                        else
                                        {
                                            temp = containingAuroraField.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField + mBaseAi.m_ExtraMarginForStopInField;
                                        }
                                        break;
                                    case HoldGroundReason.Torch:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromTorch;
                                        break;
                                    case HoldGroundReason.TorchOnGround:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromTorchOnGround;
                                        break;
                                    case HoldGroundReason.RedFlare:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromFlare;
                                        break;
                                    case HoldGroundReason.RedFlareOnGround:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromFlareOnGround;
                                        break;
                                    case HoldGroundReason.Spear:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromSpear;
                                        break;
                                    case HoldGroundReason.Fire:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromFire;
                                        break;
                                    case HoldGroundReason.BlueFlare:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromBlueFlare;
                                        break;
                                    case HoldGroundReason.BlueFlareOnGround:
                                        temp = mBaseAi.m_HoldGroundOuterDistanceFromBlueFlareOnGround;
                                        break;
                                    default:
                                    case HoldGroundReason.SafeHaven:
                                        if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven))
                                            return;
                                        //	LAB_180526fa5:
                                        temp = playerSafeHaven.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        break;
                                }

                                //inner radius calc
                                float temp2 = 0f;
                                switch (mBaseAi.m_HoldGroundReason)
                                {
                                    case HoldGroundReason.NearbyAuroraField:
                                        AuroraField nearbyAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (nearbyAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp2 = playerSafeHaven2.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        }
                                        else
                                        {
                                            temp2 = nearbyAuroraField.m_ExtentRadius - mBaseAi.m_ExtraMarginForStopInField;
                                        }
                                        break;
                                    case HoldGroundReason.InsideAuroraField:
                                        AuroraField containingAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (containingAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp2 = playerSafeHaven2.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        }
                                        else
                                        {
                                            temp2 = containingAuroraField.m_ExtentRadius - mBaseAi.m_OuterDistanceFromField;
                                        }
                                        break;
                                    case HoldGroundReason.Torch:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromTorch;
                                        break;
                                    case HoldGroundReason.TorchOnGround:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromTorchOnGround;
                                        break;
                                    case HoldGroundReason.RedFlare:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromFlare;
                                        break;
                                    case HoldGroundReason.RedFlareOnGround:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromFlareOnGround;
                                        break;
                                    case HoldGroundReason.Spear:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromSpear;
                                        break;
                                    case HoldGroundReason.Fire:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromFire;
                                        break;
                                    case HoldGroundReason.BlueFlare:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromBlueFlare;
                                        break;
                                    case HoldGroundReason.BlueFlareOnGround:
                                        temp2 = mBaseAi.m_HoldGroundDistanceFromBlueFlareOnGround;
                                        break;
                                    default:
                                    case HoldGroundReason.SafeHaven:
                                        if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven))
                                            return;
                                        //LAB_180527141:
                                        temp2 = playerSafeHaven.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven;
                                        break;
                                }

                                //slowdown calc
                                float slowdownRatio = (mBaseAi.m_CurrentRadius - temp2) / (temp - temp2);
                                slowdownRatio = Mathf.Clamp01(slowdownRatio);
                                slowdownRatio = Mathf.Sqrt(slowdownRatio);
                                mBaseAi.m_SpeedWhileStopping = slowdownRatio * mBaseAi.m_SpeedLimitAtOuterRadius;


                                //SOME OTHER radius calc
                                float temp3 = 0f;
                                switch (mBaseAi.m_HoldGroundReason)
                                {
                                    case HoldGroundReason.NearbyAuroraField:
                                        AuroraField nearbyAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (nearbyAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp3 = mBaseAi.m_CurrentRadius - (playerSafeHaven2.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven);
                                        }
                                        else
                                        {
                                            temp3 = mBaseAi.m_CurrentRadius - (nearbyAuroraField.m_ExtentRadius - mBaseAi.m_ExtraMarginForStopInField);
                                        }
                                        break;
                                    case HoldGroundReason.InsideAuroraField:
                                        AuroraField containingAuroraField = mBaseAi.m_ContainingAuroraField;
                                        if (containingAuroraField == null)
                                        {
                                            if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven2))
                                            {
                                                return;
                                            }
                                            temp3 = mBaseAi.m_CurrentRadius - (playerSafeHaven2.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven);
                                        }
                                        else
                                        {
                                            temp3 = containingAuroraField.m_ExtentRadius + mBaseAi.m_OuterDistanceFromField + mBaseAi.m_ExtraMarginForStopInField - mBaseAi.m_CurrentRadius;
                                        }
                                        break;
                                    case HoldGroundReason.Torch:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromTorch;
                                        break;
                                    case HoldGroundReason.TorchOnGround:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromTorchOnGround;
                                        break;
                                    case HoldGroundReason.RedFlare:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromFlare;
                                        break;
                                    case HoldGroundReason.RedFlareOnGround:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromFlareOnGround;
                                        break;
                                    case HoldGroundReason.Spear:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromSpear;
                                        break;
                                    case HoldGroundReason.Fire:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromFire;
                                        break;
                                    case HoldGroundReason.BlueFlare:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromBlueFlare;
                                        break;
                                    case HoldGroundReason.BlueFlareOnGround:
                                        temp3 = mBaseAi.m_CurrentRadius - mBaseAi.m_HoldGroundDistanceFromBlueFlareOnGround;
                                        break;
                                    default:
                                    case HoldGroundReason.SafeHaven:
                                        if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven))
                                            return;
                                        //LAB_1805272f6:
                                        temp3 = mBaseAi.m_CurrentRadius - (playerSafeHaven.m_ExtentRadius + mBaseAi.m_MinDistanceToKeepWithSafeHaven);
                                        break;
                                }

                                if (temp3 <= mBaseAi.m_MinDistanceToHoldFromInnerRadius)
                                {
                                    if (mBaseAi.m_AiGoalSpeed >= 0.0001f)
                                    {
                                        mBaseAi.StartPath(mBaseAi.m_AdjustedTargetPosition, 0.0f, null);
                                        return;
                                    }
                                    SetAiMode(AiMode.HoldGround);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            //if we've gotten this far, lets move on to next method
            ProcessAttack2();
        }

        #endregion


        #region Overridable Sub-Processes

        protected virtual void ProcessStartAttackHowl()
        {
            if (CurrentTarget.IsPlayer() && mBaseAi.m_AttackingLoopAudioID == 0 && mBaseAi.m_TimeInModeSeconds > 1.5)
            {
                mBaseAi.m_AttackingLoopAudioID = GameAudioManager.Play3DSound(mBaseAi.m_ChasingAudio, mBaseAi.gameObject);
            }
        }


        protected virtual void ProcessTargetDead()
        {
            ClearTargetAndSetDefaultAiMode();
        }


        protected virtual void ProcessAttack2()
        {
            Vector3 targetPos;
            Transform targetTransform;

            try
            {
                SplineNode node = ((object)CurrentTarget) as SplineNode;
                targetTransform = node.Transform;
            }
            catch
            {
                targetTransform = CurrentTarget.transform;
            }

            targetPos = targetTransform.position;


            if (BaseAi.Moose != null && BaseAi.Moose.MaybeFleeOnSlope())
            {
                return;
            }

            if (CurrentTarget.IsNpcSurvivor())
            {
                Vector3 headOffset = mBaseAi.m_HeadOffset;
                Quaternion rotation = targetTransform.rotation;
                Vector3 adjustedOffset = rotation * headOffset;

                Vector3 delta = targetPos - adjustedOffset;
            }

            if (CurrentTarget.IsPlayer())
            {
                Vector3 delta = targetPos;

                if (!mBaseAi.CanPlayerBeReached(delta))
                {
                    if (mBaseAi.m_DefaultMode != (AiMode)0x16)
                    {
                        mBaseAi.CantReachTarget();
                        mBaseAi.m_LastKnownAttackTargetPosition = Vector3.zero;
                        return;
                    }

                    Vector3 lastKnown = mBaseAi.m_LastKnownAttackTargetPosition; 
                    Vector3 selfPos = Vector3.zero;
                    Transform selfTransform = null;

                    try
                    {
                        SplineNode node = ((object)mBaseAi) as SplineNode;
                        selfTransform = node.Transform;
                    }
                    catch
                    {
                        selfTransform = mBaseAi.transform;
                    }

                    selfPos = selfTransform.position;
                    float flatDistance = Vector2.Distance(new Vector2(selfPos.x, selfPos.z),
                                                          new Vector2(lastKnown.x, lastKnown.z));

                    if (lastKnown.sqrMagnitude < 0.1f)
                    {
                        flatDistance = 0f;
                    }

                    if (flatDistance < 7.0f)
                    {
                        PointOfInterest pointOfInterest = new PointOfInterest();
                        pointOfInterest.m_Location = lastKnown;

                        mBaseAi.m_ActivePointsOfInterest?.Insert(mBaseAi.m_TargetPointOfInterestIndex, pointOfInterest);

                        mBaseAi.SetAiMode((AiMode)0x16);
                        mBaseAi.AnimSetTrigger(mBaseAi.m_AnimParameter_Roar_Trigger);
                        mBaseAi.MoveAgentStop();
                        mBaseAi.m_PlayingAttackStartAnimation = true;
                        return;
                    }
                }
            }

            mBaseAi.m_LastKnownAttackTargetPosition = targetPos;
            float runSpeed = CurrentTarget.IsPlayer() ? mBaseAi.m_ChasePlayerSpeed : mBaseAi.m_RunSpeed;

            if (mBaseAi.m_HoldGroundReason != 0)
            {
                Vector3 adjustedPos = mBaseAi.m_AdjustedTargetPosition;
                targetPos = adjustedPos;
                runSpeed = mBaseAi.m_SpeedWhileStopping;
            }
            bool pathStarted = mBaseAi.StartPath(targetPos, runSpeed, null);
            if (!pathStarted)
            {
                mBaseAi.CantReachTarget();
                return;
            }

            if (mBaseAi.m_HoldGroundReason == 0)
                mBaseAi.MaybeApplyAttack();

            if (!CurrentTarget.IsDead())
                return;

            /* Keeping for later in case I decide to do more with cougars. For augmented wolves this is not needed
            // Cougar fallback logic
            var cougar = BaseAi.get_Cougar(this);
            if (cougar == null)
            {
                Transform cachedTransform = this.m_CachedTransform;
                if (cachedTransform == null || CurrentTarget == null) return;

                Vector3 position = cachedTransform.position;
                float distance = CurrentTarget.Distance(position);

                if (distance < this.m_RangeMeleeAttack + 1.0f)
                {
                    int mode = 3;
                    // Possibly set AI mode to attack
                }
            }
            */
        }


        #endregion
    }
}
