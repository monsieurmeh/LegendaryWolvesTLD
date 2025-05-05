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
using UnityEngine.Playables;
using HarmonyLib;
using UnityEngine.Rendering.PostProcessing;
using static UnityEngine.SendMouseEvents;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class CustomAiBase : ICustomAi
    {
        public CustomAiBase(BaseAi baseAi)
        {
            mBaseAi = baseAi;
        }

        #region ICustomAi

        protected BaseAi mBaseAi;

        public BaseAi BaseAi { get { return mBaseAi; } }
        public virtual WolfTypes WolfType { get { return WolfTypes.Default; } }


        public virtual void Augment() { }


        public virtual void UnAugment() { }


        public virtual void Update()
        {
#if DEV_BUILD
            try
            {
#endif
                if (GameManager.m_IsPaused)
                {
                    return;
                }
                if (GameManager.s_IsGameplaySuspended)
                {
                    return;
                }
                if (GameManager.s_IsAISuspended)
                {
                    return;
                }   
                if ((!mBaseAi.IsMoveAgent() || !mBaseAi.m_MoveAgent.enabled && !mBaseAi.m_NavMeshAgent) && (!mBaseAi.m_FirstFrame && mBaseAi.m_CurrentMode == AiMode.Dead))
                {
                    ProcessDead();
                    return;

                }
                if (mBaseAi.m_ForceToCorpse)
                {
                    mBaseAi.m_CurrentHP = 0.0f;
                    mBaseAi.StickPivotToGround();
                    SetAiMode(AiMode.Dead);
                    if (mBaseAi.GetHitInfoUnderPivot(out RaycastHit hitInfo))
                    {
                        mBaseAi.AlignTransformWithNormal(hitInfo.point, hitInfo.normal, mBaseAi.m_CurrentMode != AiMode.Dead, true);
                    }
                    mBaseAi.m_ForceToCorpse = false;
                    GameAudioManager.StopAllSoundsFromGameObject(mBaseAi.gameObject);
                }
                if (mBaseAi.m_FirstFrame)
                {
                    if (mBaseAi.m_CurrentMode != AiMode.Dead)
                    {
                        mBaseAi.StickCharacterControllerToGround();
                        if (mBaseAi.GetHitInfoUnderCharacterController(out RaycastHit hitInfo, FindGroundType.FirstTime))
                        {
                            mBaseAi.AlignTransformWithNormal(hitInfo.point, hitInfo.normal, mBaseAi.m_CurrentMode != AiMode.Dead, true);
                        }
                    }
                    mBaseAi.DoCustomModeModifiers();
                    mBaseAi.m_FirstFrame = false;
                }
                if (!mBaseAi.IsImposter() && mBaseAi.m_ImposterAnimatorDisabled)
                {
                    mBaseAi.m_ImposterAnimatorDisabled = false;
                    mBaseAi.m_Animator.cullingMode = mBaseAi.m_ImposterCullingMode;
                }
                else if (!mBaseAi.m_ImposterAnimatorDisabled)
                {
                    mBaseAi.m_ImposterCullingMode = mBaseAi.m_Animator.cullingMode;
                    mBaseAi.m_Animator.cullingMode = AnimatorCullingMode.CullCompletely;
                    mBaseAi.m_ImposterAnimatorDisabled = true;
                }
                mBaseAi.Timberwolf?.MaybeForceHideAndSeek();
                ProcessCurrentAiMode();
                mBaseAi.UpdateAnim();
                if (CurrentTarget != null)
                {
                    CurrentTarget.m_BaseAiTargetingMe = mBaseAi;
                }
#if DEV_BUILD
            }
            catch (Exception e)
            {
                LogError($"Error while trying to process wolf ai: {e}");
                return;
            }
#endif
        }

        #endregion


        #region Core methods


        protected void ProcessCurrentAiMode()
        {
            PreProcess();
            Process();
            PostProcess();
        }


        protected virtual void PreProcess()
        {
            if (mBaseAi.m_CachedTransform == null)
            {
                LogError(mBaseAi, "Null cached transform!");
                return;
            }
            mBaseAi.m_TimeInModeSeconds += Time.deltaTime;
            TimeOfDay timeOfDay = GameManager.m_TimeOfDay;
            if (timeOfDay == null)
            {
                LogError(mBaseAi, "Null TimeOfDay!");
                return;
            }
            UniStormWeatherSystem weatherSystem = timeOfDay.m_WeatherSystem;
            if (timeOfDay == null)
            {
                LogError(mBaseAi, "Null UniStormWeatherSystem!");
                return;
            }
            mBaseAi.m_TimeInModeTODHours = (24.0f / (weatherSystem.m_DayLengthScale * weatherSystem.m_DayLength)) * Time.deltaTime + mBaseAi.m_TimeInModeTODHours;


            if (mBaseAi.m_CurrentHP <= 0.0001f)
            {
                if (mBaseAi.m_CurrentMode == AiMode.Dead)
                {
                    return;
                }
                SetAiMode(AiMode.Dead);
            }

            if (mBaseAi.m_CurrentMode != AiMode.Dead)
            {
                mBaseAi.MaybeRestoreTargetAfterSpear();
                mBaseAi.MaybeHoldGround();
                mBaseAi.MaybeAttemptDodge();
                mBaseAi.UpdateWounds(Time.deltaTime);
                mBaseAi.m_SuppressFootStepDetectionAndSmellSecondsRemaining -= Time.deltaTime;
                GameAudioManager.SetAudioSourceTransform(mBaseAi.m_EmitterProxy, mBaseAi.m_CachedTransform);
            }
        }


        protected virtual void Process()
        {
            switch (mBaseAi?.m_CurrentMode ?? AiMode.None)
            {
                case AiMode.Idle:
                    ProcessIdle();
                    break;
                case AiMode.Sleep:
                    ProcessSleep();
                    break;
                case AiMode.Stalking:
                    ProcessStalking();
                    break;
                case AiMode.WanderPaused:
                    ProcessWanderPaused();
                    break;
                case AiMode.GoToPoint:
                    ProcessGoToPoint();
                    break;
                case AiMode.Stunned:
                    ProcessStunned();
                    break;
                case AiMode.ScratchingAntlers:
                    ProcessScratchingAntlers();
                    break;
                case AiMode.HideAndSeek:
                    ProcessHideAndSeek();
                    break;
                case AiMode.JoinPack:
                    ProcessJoinPack();
                    break;
                case AiMode.Howl:
                    ProcessHowl();
                    break;
                case AiMode.Attack:
                    ProcessAttack();
                    break;
                case AiMode.Dead:
                    ProcessDead();
                    break;
                case AiMode.Feeding:
                    ProcessFeeding();
                    break;
                case AiMode.Flee:
                    ProcessFlee();
                    break;
                case AiMode.FollowWaypoints:
                    ProcessFollowWaypoints();
                    break;
                case AiMode.HoldGround:
                    ProcessHoldGround();
                    break;
                case AiMode.Investigate:
                    ProcessInvestigate();
                    break;
                case AiMode.InvestigateFood:
                    ProcessInvestigateFood();
                    break;
                case AiMode.InvestigateSmell:
                    ProcessInvestigateSmell();
                    break;
                case AiMode.Rooted:
                    ProcessRooted();
                    break;
                case AiMode.Struggle:
                    ProcessStruggle();
                    break;
                case AiMode.Wander:
                    ProcessWander();
                    break;
                case AiMode.InteractWithProp:
                    ProcessInteractWithProp();
                    break;
                case AiMode.ScriptedSequence:
                    ProcessScriptedSequence();
                    break;
                case AiMode.PatrolPointsOfInterest:
                    ProcessPatrolPointsOfInterest();
                    break;
                case AiMode.PassingAttack:
                    ProcessPassingAttack();
                    break;
                case AiMode.Disabled:
                case AiMode.None:
                default:
                    break;
            }
            return;
        }


        protected virtual void PostProcess()
        {
            if (mBaseAi.m_CurrentMode == AiMode.Dead && mBaseAi.m_CurrentMode == AiMode.ScriptedSequence)
            {
                return;
            }
            if (mBaseAi.IsImposter())
            {
                return;
            }
            mBaseAi.SetAnimationParameters();
            if (mBaseAi.m_SpeedForPathfindingOverride)
            {
                return;
            }

            if (mBaseAi.m_MoveAgent == null)
            {
                return;
            }
            mBaseAi.m_MoveAgent.m_MaxSpeed = mBaseAi.m_SpeedFromMecanimBone != null ? mBaseAi.GetSpeedFromMecanimBone() : mBaseAi.m_AiGoalSpeed;
            mBaseAi.m_MoveAgent.m_RotationSpeed =
                (mBaseAi.m_CurrentMode != AiMode.Wander
                && mBaseAi.m_WanderTurnTargets != null
                && mBaseAi.m_WanderTurnTargets.Count > 1
                && mBaseAi.m_WanderCurrentTarget > mBaseAi.m_WanderTurnTargets.Count)
                ? mBaseAi.m_WanderTurnSpeedDegreesPerSecond
                : mBaseAi.m_TurnSpeedDegreesPerSecond;
            mBaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_ActualSpeed, mBaseAi.m_MoveAgent.m_MaxSpeed);
        }

        #endregion


        #region Helpers

        protected AiTarget CurrentTarget { get { return mBaseAi.m_CurrentTarget; } }

        protected virtual void ClearTargetAndSetDefaultAiMode()
        {
            mBaseAi.ClearTarget();
            SetDefaultAiMode();
            return;
        }

        protected virtual void SetAiMode(AiMode mode)
        {
            Log($"Setting aimode to {mode}");
            mBaseAi.SetAiMode(mode);
        }


        protected virtual void SetDefaultAiMode()
        {
            Log($"RESetting aimode to {mBaseAi.m_DefaultMode}");
            SetAiMode(mBaseAi.m_DefaultMode);
        }


        protected virtual bool CheckSceneTransitionStarted(PlayerManager playerManager)
        {
            if (playerManager.m_SceneTransitionStarted)
            {
                SetDefaultAiMode();
                return true;
            }
            return false;
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

            if (!TryGetInnerRadiusForHoldGroundCause(reason, out float innerRadius))
                return false;

            if (!TryGetOuterRadiusForHoldGroundCause(reason, out float outerRadius))
                return false;

            bool allowSlowdown = BaseAi.m_AllowSlowdownForHold;

            if (shouldHoldGroundFunc.Invoke(innerRadius))
            {
                mBaseAi.m_HoldGroundReason = reason;
                SetAiMode(AiMode.HoldGround);
                return true;
            }

            if (allowSlowdown && shouldHoldGroundFunc.Invoke(outerRadius))
            {
                mBaseAi.m_HoldGroundReason = reason;
                RefreshTargetPosition();
            }
            return false;
        }


        protected virtual bool TryGetInnerRadiusForHoldGroundCause(HoldGroundReason reason, out float innerRadius)
        {
            innerRadius = 0.0f;
            switch (reason)
            {
                case HoldGroundReason.RedFlare:
                    innerRadius = m_HoldGroundDistanceFromFlare;
                    break;
                case HoldGroundReason.Torch:
                    innerRadius = m_HoldGroundDistanceFromTorch;
                    break;
                case HoldGroundReason.SafeHaven:
                    innerRadius = m_MinDistanceToKeepWithSafeHaven + (TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven) ? playerSafeHaven.m_ExtentRadius : 0.0f);
                    break;
                case HoldGroundReason.NearbyAuroraField:
                    innerRadius = MathF.Max(0.0f, (TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField) ? containingAuroraField.m_ExtentRadius : 0.0f) - m_ExtraMarginForStopInField);
                    break;
                case HoldGroundReason.InsideAuroraField:
                    innerRadius = MathF.Max(0.0f, (TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField2) ? containingAuroraField2.m_ExtentRadius : 0.0f) - mBaseAi.m_OuterDistanceFromField);
                    break;
                case HoldGroundReason.Spear:
                    innerRadius = m_HoldGroundDistanceFromSpear;
                    break;
                case HoldGroundReason.Fire:
                    innerRadius = m_HoldGroundDistanceFromFire;
                    break;
                case HoldGroundReason.BlueFlare:
                    innerRadius = m_HoldGroundDistanceFromBlueFlare;
                    break;
                case HoldGroundReason.BlueFlareOnGround:
                    innerRadius = m_HoldGroundDistanceFromBlueFlareOnGround;
                    break;
                case HoldGroundReason.RedFlareOnGround:
                    innerRadius = m_HoldGroundDistanceFromFlareOnGround;
                    break;
                case HoldGroundReason.TorchOnGround:
                    innerRadius = m_HoldGroundDistanceFromTorchOnGround;
                    break;
            }
            return true;
        }


        protected virtual bool TryGetOuterRadiusForHoldGroundCause(HoldGroundReason reason, out float outerRadius)
        {
            outerRadius = 0.0f;
            switch (reason)
            {
                case HoldGroundReason.RedFlare:
                    outerRadius = m_HoldGroundOuterDistanceFromFlare;
                    break;
                case HoldGroundReason.Torch:
                    outerRadius = m_HoldGroundOuterDistanceFromTorch;
                    break;
                case HoldGroundReason.SafeHaven:
                    outerRadius = mBaseAi.m_OuterDistanceFromField + m_MinDistanceToKeepWithSafeHaven + (TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven) ? playerSafeHaven.m_ExtentRadius : 0.0f);
                    break;
                case HoldGroundReason.NearbyAuroraField:
                    if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField))
                        return false;
                    outerRadius = mBaseAi.m_OuterDistanceFromField + containingAuroraField.m_ExtentRadius;
                    break;
                case HoldGroundReason.InsideAuroraField:
                    if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField2))
                        return false;
                    outerRadius = mBaseAi.m_OuterDistanceFromField + containingAuroraField2.m_ExtentRadius + m_ExtraMarginForStopInField;
                    break;
                case HoldGroundReason.Spear:
                    outerRadius = m_HoldGroundOuterDistanceFromSpear;
                    break;
                case HoldGroundReason.Fire:
                    outerRadius = m_HoldGroundOuterDistanceFromFire;
                    break;
                case HoldGroundReason.BlueFlare:
                    outerRadius = m_HoldGroundOuterDistanceFromBlueFlare;
                    break;
                case HoldGroundReason.BlueFlareOnGround:
                    outerRadius = m_HoldGroundOuterDistanceFromBlueFlareOnGround;
                    break;
                case HoldGroundReason.RedFlareOnGround:
                    outerRadius = m_HoldGroundOuterDistanceFromFlareOnGround;
                    break;
                case HoldGroundReason.TorchOnGround:
                    outerRadius = m_HoldGroundOuterDistanceFromTorchOnGround;
                    break;
            }
            return true;
        }


        protected virtual bool TryGetHoldGroundReasonPosition(HoldGroundReason reason, out Vector3 newTargetPosition)
        {
            newTargetPosition = Vector3.zero;
            switch (mBaseAi.m_HoldGroundReason)
            {
                case HoldGroundReason.SafeHaven:
                    if (!TryGetPlayerSafeHaven("ProcessAttack", out AuroraField playerSafeHaven))
                        return false;
                    newTargetPosition = playerSafeHaven.transform.position;
                    break;
                case HoldGroundReason.NearbyAuroraField:
                    if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField))
                        return false;
                    newTargetPosition = containingAuroraField.transform.position;
                    break;
                case HoldGroundReason.InsideAuroraField:
                    if (!TryGetContainingAuroraField("ProcessAttack", out AuroraField containingAuroraField2))
                        return false;
                    newTargetPosition = containingAuroraField2.transform.position;
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
            }
            return true;
        }


        protected virtual bool ShouldHoldGround()
        {
            if (!mBaseAi.CanHoldGround())
            {
                return false;
            }
            if (mBaseAi.IsInFlashLight())
            {
                if (mBaseAi.Timberwolf != null)
                {
                    //Log($"[ProcessAttack] Flee, timberwolf!");
                    SetAiMode(AiMode.Flee);
                    return true; //stops next attack action
                }
            }
            if (mBaseAi.MaybeHoldGroundDueToSafeHaven())
            {
                //Log($"[ProcessAttack] Holding ground due to safe haven, aborting...");
                return true;
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Spear, MaybeHoldGroundForSpear))
                {
                    //Log($"[ProcessAttack] Holding ground due spear threat, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Torch, (radius) => mBaseAi.MaybeHoldGroundForTorch(radius)))
                {
                    //Log($"[ProcessAttack] Holding ground due torch, aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.TorchOnGround, (radius) => mBaseAi.MaybeHoldGroundForTorchOnGround(radius)))
                {
                    //Log($"[ProcessAttack] Holding ground due to torch on ground, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.RedFlare, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to red flare, aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.RedFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForRedFlareOnGround(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to red flare on ground, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.Fire, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to fire, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.BlueFlare, (radius) => mBaseAi.MaybeHoldGroundForBlueFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to blue flare , aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttackCustom(HoldGroundReason.BlueFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForBlueFlareOnGround(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to blue flare on ground, aborting...");
                    return true;
                }
            }

            if (mBaseAi.m_HoldGroundReason != HoldGroundReason.None)
            {
                if (!mBaseAi.m_UseSlowdownForHold)
                {
                    if (BaseAi.m_AllowSlowdownForHold && !mBaseAi.IsInFlashLight())
                    {
                        RefreshTargetPosition();
                        if (mBaseAi.m_HoldGroundReason != HoldGroundReason.None)
                        {
                            if (TryGetHoldGroundReasonPosition(mBaseAi.m_HoldGroundReason, out Vector3 newTargetPosition))
                                return false;
                            mBaseAi.m_CurrentRadius = Vector3.Distance(mBaseAi.transform.position, newTargetPosition);
                            if (!TryGetOuterRadiusForHoldGroundCause(mBaseAi.m_HoldGroundReason, out float outerRadius))
                                return false;
                            if (!TryGetInnerRadiusForHoldGroundCause(mBaseAi.m_HoldGroundReason, out float innerRadius))
                                return false;
                            float slowdownRatio = (mBaseAi.m_CurrentRadius - innerRadius) / (outerRadius - innerRadius);
                            slowdownRatio = Mathf.Clamp01(slowdownRatio);
                            slowdownRatio = Mathf.Sqrt(slowdownRatio);
                            mBaseAi.m_SpeedWhileStopping = slowdownRatio * mBaseAi.m_SpeedLimitAtOuterRadius;
                            if (mBaseAi.m_CurrentRadius - innerRadius <= mBaseAi.m_MinDistanceToHoldFromInnerRadius)
                            {
                                if (mBaseAi.m_AiGoalSpeed >= 0.0001f)
                                {
                                    //Log($"[ProcessAttack] player is too close for hold ground, approaching...");
                                    mBaseAi.StartPath(mBaseAi.m_AdjustedTargetPosition, 0.0f, null);
                                    return false;
                                }
                                SetAiMode(AiMode.HoldGround);
                                return true;
                            }
                        }
                    }
                }
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


        #region Hinterland Process Overrides

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

            if (CurrentTarget.IsPlayer())
            {
                if (playerManager.PlayerIsInvisibleToAi() && !GameManager.m_PlayerStruggle.m_Active)
                {
                    //Log($"[ProcessAttack] Target is player, player is invisible, no struggle, setting default AI mode and aborting...");
                    SetDefaultAiMode();
                    return;
                }
                if (!TryGetTargetPosition(out Vector3 targetPosition))
                {
                    //Log($"[ProcessAttack] Can't get target position, setting default AI mode and aborting...");
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
                //Log($"[ProcessAttack] playing start attack animation, trying to apply attack...");
                AiUtils.TurnTowardsTarget(mBaseAi);
                mBaseAi.MaybeApplyAttack();
                if (mBaseAi.m_TimeInModeSeconds <= mBaseAi.m_AnimationTime)
                {
                    //Log($"[ProcessAttack] playing start attack animation, trying to apply attack...");
                    return;
                }
                mBaseAi.m_PlayingAttackStartAnimation = false;
            }

            mBaseAi.m_HoldGroundReason = HoldGroundReason.None;

            if (ShouldHoldGround())
            {
                return;
            }
            Vector3 targetPos;
            Transform targetTransform;
            targetTransform = CurrentTarget.transform;
            targetPos = targetTransform.position;


            if (BaseAi.Moose != null && BaseAi.Moose.MaybeFleeOnSlope())
            {
                //Log($"[ProcessCharge] is moose and on slope, run away!");
                return;
            }

            if (CurrentTarget.IsPlayer())
            {
                Vector3 delta = targetPos;

                if (!mBaseAi.CanPlayerBeReached(delta))
                {
                    //Log($"[ProcessCharge] Cant reach player!");
                    if (mBaseAi.m_DefaultMode != (AiMode)0x16)
                    {
                        //Log($"[ProcessCharge] Default mode is not {(AiMode)0x16}, triggering CantReachTarget and aborting");
                        mBaseAi.CantReachTarget();
                        mBaseAi.m_LastKnownAttackTargetPosition = Vector3.zero;
                        return;
                    }

                    Vector3 lastKnown = mBaseAi.m_LastKnownAttackTargetPosition;
                    Vector3 selfPos = Vector3.zero;
                    Transform selfTransform = null;
                    selfTransform = mBaseAi.transform;


                    selfPos = selfTransform.position;
                    float flatDistance = Vector2.Distance(new Vector2(selfPos.x, selfPos.z),
                                                          new Vector2(lastKnown.x, lastKnown.z));

                    if (lastKnown.sqrMagnitude < 0.1f)
                    {
                        flatDistance = 0f;
                    }

                    if (flatDistance < 7.0f)
                    {
                        //Log($"[ProcessCharge] Last known location within arbitrary distance of 7, creating new point of interest, setting ai mode to {(AiMode)0x16}, engaging roar trigger and stopping move agent");
                        PointOfInterest pointOfInterest = new PointOfInterest();
                        pointOfInterest.m_Location = lastKnown;

                        mBaseAi.m_ActivePointsOfInterest?.Insert(mBaseAi.m_TargetPointOfInterestIndex, pointOfInterest);

                        SetAiMode((AiMode)0x16);
                        mBaseAi.AnimSetTrigger(mBaseAi.m_AnimParameter_Roar_Trigger);
                        mBaseAi.MoveAgentStop();
                        mBaseAi.m_PlayingAttackStartAnimation = true;
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
                //Log($"[ProcessCharge] Path starteed but returned false with runspeed {runSpeed}, triggering CantReachTarget and aborting");
                mBaseAi.CantReachTarget();
                return;
            }

            if (mBaseAi.m_HoldGroundReason == 0)
            {
                //Log($"[ProcessCharge] Got to end of attack phase, trying to apply attack action!");
                mBaseAi.MaybeApplyAttack();
            }
        }


        protected virtual void ProcessDead()
        {
            mBaseAi.ProcessDead();
        }


        protected virtual void ProcessFeeding()
        {
            mBaseAi.ProcessFeeding();
        }


        protected virtual void ProcessFlee()
        {
            mBaseAi.ProcessFlee();
        }


        protected virtual void ProcessFollowWaypoints()
        {
            mBaseAi.ProcessFollowWaypoints();
        }


        protected virtual void ProcessHoldGround()
        {
            mBaseAi.ProcessHoldGround();
        }


        protected virtual void ProcessIdle()
        {
            mBaseAi.ClearTarget();
            mBaseAi.ScanForNewTarget();
            mBaseAi.ScanForSmells();
            if (mBaseAi.m_CurrentMode != AiMode.Idle || mBaseAi.m_StartMode == AiMode.Idle)
            {
                return;
            }
            if (mBaseAi.m_TimeInModeSeconds > 10.0f)
            {
                SetAiMode(mBaseAi.m_DefaultMode);
            }
        }


        protected virtual void ProcessInvestigate()
        {
            mBaseAi.ProcessInvestigate();
        }


        protected virtual void ProcessInvestigateFood()
        {
            mBaseAi.ProcessInvestigateFood();
        }


        protected virtual void ProcessInvestigateSmell()
        {
            mBaseAi.ProcessInvestigateSmell();
        }


        protected virtual void ProcessRooted()
        {
            mBaseAi.ProcessRooted();
        }


        protected virtual void ProcessSleep()
        {
            if (!mBaseAi.m_Awake)
            {
                if (mBaseAi.m_SleepTimeHours < mBaseAi.m_TimeInModeTODHours)
                {
                    if (GameManager.m_Weather.IsBlizzard())
                    {
                        mBaseAi.m_TimeInModeTODHours = Mathf.Max(0.0f, mBaseAi.m_SleepTimeHours - 1.0f);
                    }
                    else
                    {
                        mBaseAi.m_Animator.SetBool(mBaseAi.m_AnimParameter_Sleep, false);
                        mBaseAi.m_Awake = true;
                        mBaseAi.m_ExitSleepModeTime = Time.time + 4.0f;
                    }
                }
            }
            else if (Time.time > mBaseAi.m_ExitSleepModeTime)
            {
                SetAiMode(mBaseAi.m_DefaultMode);
            }
        }


        protected virtual void ProcessStalking()
        {
            mBaseAi.ProcessStalking();
        }


        protected virtual void ProcessStruggle()
        {
            mBaseAi.ProcessStruggle();
        }


        protected virtual void ProcessWander()
        {
            mBaseAi.ProcessWander();
        }


        protected virtual void ProcessWanderPaused()
        {
            if (mBaseAi.IsImposter())
            {
                SetAiMode(mBaseAi.m_DefaultMode);
                return;
            }

            if (mBaseAi.m_FailsafeExitTime > 0.0f)
            {
                mBaseAi.m_FailsafeExitTime -= Time.deltaTime;
                if (mBaseAi.m_FailsafeExitTime <= 0.0)
                {
                    GameManager.m_AuroraManager.AuroraIsActive();
                    SetAiMode(mBaseAi.m_DefaultMode);
                    return;
                }
            }
            mBaseAi.ScanForNewTarget();
            mBaseAi.MaybeHoldGroundAuroraField();
        }


        protected virtual void ProcessGoToPoint()
        {
            mBaseAi.StartPath(mBaseAi.m_GoToPoint, mBaseAi.m_GotoPointMovementSpeed);
            if (mBaseAi.m_State == State.Pathfinding && mBaseAi.m_MoveAgent.m_DestinationReached)
            {
                mBaseAi.m_State = State.Blending;
            }
            if (mBaseAi.m_State == State.Blending)
            {
                mBaseAi.m_State = State.Finished;
                SetAiMode(mBaseAi.m_TargetMode);
            }
        }


        protected virtual void ProcessInteractWithProp()
        {
            mBaseAi.ProcessInteractWithProp();
        }


        protected virtual void ProcessScriptedSequence()
        {
            mBaseAi.ProcessScriptedSequence();
        }


        protected virtual void ProcessStunned()
        {
            mBaseAi.ProcessStunned();
        }


        protected virtual void ProcessScratchingAntlers()
        {
            mBaseAi.Moose?.ProcessScratchingAntlers();
        }


        protected virtual void ProcessPatrolPointsOfInterest()
        {
            mBaseAi.ProcessPatrolPointsOfInterest();
        }


        protected virtual void ProcessHideAndSeek()
        {
            mBaseAi.Timberwolf?.ProcessHideAndSeek();
        }


        protected virtual void ProcessJoinPack()
        {
            mBaseAi.Timberwolf?.ProcessJoinPack();
        }


        protected virtual void ProcessPassingAttack()
        {
            mBaseAi.ProcessPassingAttack();
        }


        protected virtual void ProcessHowl()
        {
            mBaseAi.BaseWolf?.ProcessHowl();
        }


        #endregion


        #region MaybeHoldGround overrides

        protected virtual bool MaybeHoldGroundForSpear(float radius)
        {
            if (mBaseAi.m_WildlifeMode == WildlifeMode.Aurora)
            {
                //Log($"[MaybeHoldGroundForSpear] Aurora wildlife, dont stop for spear");
                return false;
            }

            if (Math.Abs(radius) <= 0.0001f)
            {
                //Log($"[MaybeHoldGroundForSpear] small check radius, dont stop for spear");
                return false;
            }   

            PlayerManager player = GameManager.m_PlayerManager;

            if (player == null)
            {
                //Log($"[MaybeHoldGroundForSpear] null playermanager, dont stop for spear");
                return false;
            }

            if (player.m_ItemInHandsInternal == null)
            {
                //Log($"[MaybeHoldGroundForSpear] no item in hands, dont stop for spear");
                return false;
            }


            if (player.m_ItemInHandsInternal.m_BearSpearItem == null)
            {
                //Log($"[MaybeHoldGroundForSpearCustom] item in hand is not bear spear, dont stop for spear");
                return false;
            }

            /* not sure if I actually want this mechanic, considering the additonal facing check - timberwolf packs will be hard enough to fend off with this without the raise requrement!
            if (player.m_ItemInHandsInternal.m_BearSpearItem.m_CurrentSpearState != BearSpearItem.SpearState.Raised)
            {
                Log($"[MaybeHoldGroundForSpearCustom] bear spear is not raised, dont stop for spear");
                return false;
            }
            */

            Vector3 aiForward = mBaseAi.m_CachedTransform.forward;
            Vector3 playerForward = GameManager.GetPlayerTransform().forward;

            float angle = Vector3.Angle(aiForward, -playerForward);

            if (angle > 45.0f)
            {
                //Log($"[MaybeHoldGroundForSpearCustom, radius {radius}] Not looking at each other, dont stop for spear");
                return false;
            }

            if (!AiUtils.PositionVisible(mBaseAi.GetEyePos(), mBaseAi.transform.forward, CurrentTarget.GetEyePos(), radius, mBaseAi.m_DetectionFOV, 0.0f, 0x100c1b41))
            {
                //Log($"[MaybeHoldGroundForSpearCustom, radius {radius}] cant see player eyes, dont stop for spear");
                return false;
            }

            //Log($"[MaybeHoldGroundForSpearCustom, radius {radius}] all checks passed, stopping or slowing for spear");
            return true;
        }

        #endregion


        #region Sub-Processes
        

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

        #endregion


        #region AI Property Overrides

        protected virtual float m_HoldGroundDistanceFromSpear { get { return mBaseAi.m_HoldGroundDistanceFromSpear; } }
        protected virtual float m_HoldGroundOuterDistanceFromSpear { get { return mBaseAi.m_HoldGroundOuterDistanceFromSpear; } }
        protected virtual float m_HoldGroundDistanceFromBlueFlare { get { return mBaseAi.m_HoldGroundDistanceFromBlueFlare; } }
        protected virtual float m_HoldGroundOuterDistanceFromBlueFlare { get { return mBaseAi.m_HoldGroundOuterDistanceFromBlueFlare; } }
        protected virtual float m_HoldGroundDistanceFromBlueFlareOnGround { get { return mBaseAi.m_HoldGroundDistanceFromBlueFlareOnGround; } }
        protected virtual float m_HoldGroundOuterDistanceFromBlueFlareOnGround { get { return mBaseAi.m_HoldGroundOuterDistanceFromBlueFlareOnGround; } }
        protected virtual float m_HoldGroundDistanceFromFire { get { return mBaseAi.m_HoldGroundDistanceFromFire; } }
        protected virtual float m_HoldGroundOuterDistanceFromFire { get { return mBaseAi.m_HoldGroundOuterDistanceFromFire; } }
        protected virtual float m_HoldGroundDistanceFromFlare { get { return mBaseAi.m_HoldGroundDistanceFromFlare; } }
        protected virtual float m_HoldGroundOuterDistanceFromFlare { get { return mBaseAi.m_HoldGroundOuterDistanceFromFlare; } }
        protected virtual float m_HoldGroundDistanceFromFlareOnGround { get { return mBaseAi.m_HoldGroundDistanceFromFlareOnGround; } }
        protected virtual float m_HoldGroundOuterDistanceFromFlareOnGround { get { return mBaseAi.m_HoldGroundOuterDistanceFromFlareOnGround; } }
        protected virtual float m_HoldGroundDistanceFromTorch { get { return mBaseAi.m_HoldGroundDistanceFromTorch; } }
        protected virtual float m_HoldGroundOuterDistanceFromTorch { get { return mBaseAi.m_HoldGroundOuterDistanceFromTorch; } }
        protected virtual float m_HoldGroundDistanceFromTorchOnGround { get { return mBaseAi.m_HoldGroundDistanceFromTorchOnGround; } }
        protected virtual float m_HoldGroundOuterDistanceFromTorchOnGround { get { return mBaseAi.m_HoldGroundOuterDistanceFromTorchOnGround; } }
        protected virtual float m_MinDistanceToKeepWithSafeHaven { get { return mBaseAi.m_MinDistanceToKeepWithSafeHaven; } }
        protected virtual float m_ExtraMarginForStopInField { get { return mBaseAi.m_ExtraMarginForStopInField; } }
        protected virtual float m_MinDistanceToHoldFromInnerRadius { get { return mBaseAi.m_MinDistanceToHoldFromInnerRadius; } }


        #endregion
    }
}
