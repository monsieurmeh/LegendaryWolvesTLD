//#define DEV_BUILD_STATELABEL
#define DEV_BUILD_TYPELABEL

using Il2Cpp;
using UnityEngine;
using static Il2Cpp.BaseAi;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public partial class CustomAiBase : ICustomAi
    {
        public CustomAiBase(BaseAi baseAi)
        {
            mBaseAi = baseAi;
            mTimeOfDay = GameManager.m_TimeOfDay;
        }

        protected BaseAi mBaseAi;
        protected TimeOfDay mTimeOfDay;
        protected bool mSetAiModeLock = false;
        protected float mTimeSinceCheckForTargetInPatrolWaypointsMode = 0.0f;

#if DEV_BUILD_STATELABEL || DEV_BUILD_TYPELABEL
        protected AiMode mCachedMode = AiMode.None;
        protected bool mReadout = false;
        public Transform mMarkerTransform;
        public Renderer mMarkerRenderer;
#endif

        public BaseAi BaseAi { get { return mBaseAi; } }
        public virtual WolfTypes WolfType { get { return WolfTypes.Default; } }
        public bool SetAiModeLock { get { return mSetAiModeLock; } }


        public virtual void Augment()
        {
#if DEV_BUILD_STATELABEL || DEV_BUILD_TYPELABEL
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.localScale = new Vector3(1.0f, 250.0F, 1.0f);
            marker.GetComponent<Collider>().enabled = false;
            GameObject.Destroy(marker.GetComponent<Collider>());
            mMarkerTransform = marker.transform;
            mMarkerTransform.SetParent(mBaseAi.transform);
            mMarkerTransform.position = mBaseAi.transform.position;
            mMarkerRenderer = marker.GetComponent<Renderer>();
#endif
#if DEV_BUILD_TYPELABEL
            mMarkerRenderer.material.color = GetMarkerColorByType();
#endif

        }

#if DEV_BUILD_STATELABEL || DEV_BUILD_TYPELABEL
        public void SetMarkerColor()
        {
            //mMarkerRenderer.material.color = GetMarkerColor();
            mMarkerRenderer.material.color = GetMarkerColorByType();
        }


        public Color GetMarkerColor()
        {
            if (mCachedMode != CurrentMode)
            {
                mCachedMode = CurrentMode;
                mMarkerRenderer.gameObject.active = true;
            }
            switch (CurrentMode)
            {
                case AiMode.Wander:
                case AiMode.PatrolPointsOfInterest:
                case AiMode.FollowWaypoints:
                case AiMode.WanderPaused:
                case (AiMode)NewAiModes.Returning:
                    return Color.grey;
                case AiMode.Attack:
                case AiMode.PassingAttack:
                    return Color.red;
                case AiMode.HoldGround:
                    return Color.blue;
                case AiMode.Flee:
                    return Color.green;
                case AiMode.Investigate:
                case AiMode.InvestigateSmell:
                    return new Color(255, 0, 255);
                case (AiMode)NewAiModes.Hiding:
                    return new Color(255, 255, 0);
                default:
                    mMarkerRenderer.gameObject.active = false;
                    return Color.clear;
            }
        }


        public Color GetMarkerColorByType()
        {
            switch (WolfType)
            {
                case WolfTypes.Default:
                    return Color.grey;
                case WolfTypes.BigWolf:
                    return Color.red;
                case WolfTypes.HidingWolf:
                    return Color.yellow;
                case WolfTypes.Stalker:
                    return new Color(255, 0, 255);
                case WolfTypes.ScaredyWolf:
                    return Color.green;
                case WolfTypes.Wanderer:
                    return Color.blue;
                default:
                    return Color.black;
            }
        }
#endif

        public virtual void UnAugment() { }


        public virtual void Update()
        {
#if DEV_BUILD_STATELABEL
            mMarkerRenderer.material.color = GetMarkerColor();
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
            if ((!mBaseAi.IsMoveAgent() || !mBaseAi.m_MoveAgent.enabled && !mBaseAi.m_NavMeshAgent) && (!mBaseAi.m_FirstFrame && CurrentMode == AiMode.Dead))
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
                    mBaseAi.AlignTransformWithNormal(hitInfo.point, hitInfo.normal, CurrentMode != AiMode.Dead, true);
                }
                mBaseAi.m_ForceToCorpse = false;
                GameAudioManager.StopAllSoundsFromGameObject(mBaseAi.gameObject);
            }
            if (mBaseAi.m_FirstFrame)
            {
                FirstFrame();
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
        }


        protected void ProcessCurrentAiMode()
        {
            PreProcess();
            Process();
            PostProcess();
        }


        #region Helpers/Internal Accessors

        public static float SquaredDistance(Vector3 a, Vector3 b)
        {
            return ((a.x - b.x) * (a.x - b.x)) + ((a.y - b.y) * (a.y - b.y)) + ((a.z - b.z) * (a.z - b.z));
        }


        protected AiTarget CurrentTarget { get { return mBaseAi.m_CurrentTarget; } }
        protected AiMode CurrentMode { get { return mBaseAi.m_CurrentMode; } set { mBaseAi.m_CurrentMode = value; } }
        protected AiModeFlags CurrentModeFlag { get { return (AiModeFlags)(1U << (int)CurrentMode); } }
        protected AiMode PreviousMode { get { return mBaseAi.m_PreviousMode; } set { mBaseAi.m_PreviousMode = value; } }
        protected string Name { get { return mBaseAi.gameObject?.name ?? "NULL"; } }


        protected void ClearTargetAndSetDefaultAiMode()
        {
            mBaseAi.ClearTarget();
            SetDefaultAiMode();
            return;
        }


        protected void SetDefaultAiMode()
        {
            SetAiMode(mBaseAi.m_DefaultMode);
        }


        protected bool CheckSceneTransitionStarted(PlayerManager playerManager)
        {
            if (playerManager.m_SceneTransitionStarted)
            {
                SetDefaultAiMode();
                return true;
            }
            return false;
        }


        protected bool CheckTargetDead()
        {
            if (CurrentTarget.IsDead())
            {
                ProcessTargetDead();
                return true;
            }
            return false;
        }


        protected bool TryGetTargetPosition(out Vector3 targetPosition)
        {
            if (CurrentTarget.transform == null)
            {
                targetPosition = Vector3.zero;
                return false;
            }
            targetPosition = CurrentTarget.transform.position;
            return true;
        }


        protected void RefreshTargetPosition()
        {
            if (TryGetTargetPosition(out Vector3 targetPosition))
            {
                mBaseAi.MaybeAdjustTargetPosition(targetPosition);
            }
        }


        protected bool CanReachTarget(Vector3 targetPosition)
        {
            return mBaseAi.CanPlayerBeReached(targetPosition);
        }


        protected bool TryGetInnerRadiusForHoldGroundCause(HoldGroundReason reason, out float innerRadius)
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


        protected bool TryGetOuterRadiusForHoldGroundCause(HoldGroundReason reason, out float outerRadius)
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


        protected bool TryGetHoldGroundReasonPosition(HoldGroundReason reason, out Vector3 newTargetPosition)
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


        protected bool ShouldHoldGround()
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
                if (MaybeHoldGroundForAttack(HoldGroundReason.Spear, MaybeHoldGroundForSpear))
                {
                    //Log($"[ProcessAttack] Holding ground due spear threat, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttack(HoldGroundReason.Torch, (radius) => mBaseAi.MaybeHoldGroundForTorch(radius)))
                {
                    //Log($"[ProcessAttack] Holding ground due torch, aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttack(HoldGroundReason.TorchOnGround, (radius) => mBaseAi.MaybeHoldGroundForTorchOnGround(radius)))
                {
                    //Log($"[ProcessAttack] Holding ground due to torch on ground, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttack(HoldGroundReason.RedFlare, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to red flare, aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttack(HoldGroundReason.RedFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForRedFlareOnGround(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to red flare on ground, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttack(HoldGroundReason.Fire, (radius) => mBaseAi.MaybeHoldGroundForRedFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to fire, aborting...");
                    return true;
                }
            }
            if (mBaseAi.m_HoldGroundCooldownSeconds < Time.time - mBaseAi.m_LastTimeWasHoldingGround)
            {
                if (MaybeHoldGroundForAttack(HoldGroundReason.BlueFlare, (radius) => mBaseAi.MaybeHoldGroundForBlueFlare(radius)))
                {
                    //Log($"[ProcessAttack] Holding due to blue flare , aborting...");
                    return true;
                }
                if (MaybeHoldGroundForAttack(HoldGroundReason.BlueFlareOnGround, (radius) => mBaseAi.MaybeHoldGroundForBlueFlareOnGround(radius)))
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
    }
}