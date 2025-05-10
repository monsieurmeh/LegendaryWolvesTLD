//#define DEV_BUILD_STATELABEL
#define DEV_BUILD_TYPELABEL

using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Runtime;
using Il2CppParadoxNotion.Serialization;
using Il2CppTLD.AI;
using UnityEngine;
using static Il2Cpp.BaseAi;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public partial class CustomAiBase : ICustomAi
    {
        #region ProcessCurrentAiMode

        protected void PreProcess()
        {
            if (PreProcessCustom())
            {
                return;
            }
            float deltaTime = Time.deltaTime;
            mBaseAi.m_TimeInModeSeconds += deltaTime;
            mBaseAi.m_TimeInModeTODHours = (24.0f / (mTimeOfDay.m_WeatherSystem.m_DayLengthScale * mTimeOfDay.m_WeatherSystem.m_DayLength)) * deltaTime + mBaseAi.m_TimeInModeTODHours;

            if (mBaseAi.m_CurrentHP <= 0.0001f)
            {
                if (CurrentMode == AiMode.Dead)
                {
                    return;
                }
                SetAiMode(AiMode.Dead);
            }

            if (CurrentMode != AiMode.Dead)
            {
                mBaseAi.MaybeRestoreTargetAfterSpear();
                MaybeHoldGround();
                mBaseAi.MaybeAttemptDodge();
                ProcessWounds(deltaTime);
                ProcessBleeding(deltaTime);
                mBaseAi.m_SuppressFootStepDetectionAndSmellSecondsRemaining -= deltaTime;
                GameAudioManager.SetAudioSourceTransform(mBaseAi.m_EmitterProxy, mBaseAi.m_CachedTransform);
            }
        }


        #region Process

        protected void Process()
        {
            if (ProcessCustom())
            {
                return;
            }
            switch (CurrentMode)
            {
                case AiMode.Attack: ProcessAttack(); break;
                case AiMode.Dead: ProcessDead(); break;
                case AiMode.Feeding: ProcessFeeding(); break;
                case AiMode.Flee: ProcessFlee(); break;
                case AiMode.FollowWaypoints: ProcessFollowWaypoints(); break;
                case AiMode.HoldGround: ProcessHoldGround(); break;
                case AiMode.Idle: ProcessIdle(); break;
                case AiMode.Investigate: ProcessInvestigate(); break;
                case AiMode.InvestigateFood: ProcessInvestigateFood(); break;
                case AiMode.InvestigateSmell: ProcessInvestigateSmell(); break;
                case AiMode.Rooted: ProcessRooted(); break;
                case AiMode.Sleep: ProcessSleep(); break;
                case AiMode.Stalking: ProcessStalking(); break;
                case AiMode.Struggle: ProcessStruggle(); break;
                case AiMode.Wander: ProcessWander(); break;
                case AiMode.WanderPaused: ProcessWanderPaused(); break;
                case AiMode.GoToPoint: ProcessGoToPoint(); break;
                case AiMode.InteractWithProp: ProcessInteractWithProp(); break;
                case AiMode.ScriptedSequence: ProcessScriptedSequence(); break;
                case AiMode.Stunned: ProcessStunned(); break;
                case AiMode.ScratchingAntlers: ProcessScratchingAntlers(); break;
                case AiMode.PatrolPointsOfInterest: ProcessPatrolPointsOfInterest(); break;
                case AiMode.HideAndSeek: ProcessHideAndSeek(); break;
                case AiMode.JoinPack: ProcessJoinPack(); break;
                case AiMode.PassingAttack: ProcessPassingAttack(); break;
                case AiMode.Howl: ProcessHowl(); break;
            }
        }


        protected void ProcessAttack()
        {
            mBaseAi.ProcessAttack();
            /*

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
            */
        }


        protected void ProcessDead()
        {
            mBaseAi.ProcessDead();
            /*
            mBaseAi.MaybeSpawnCarcassSiteIfFarEnough();
            mBaseAi.m_TimeInDeadMode += Time.deltaTime;
            if (mBaseAi.m_EnableColliderOnDeath && mBaseAi.m_TimeInDeadMode > 5.0f)
            {
                mBaseAi.m_EnableColliderOnDeath = false;
                MatchTransform.EnableCollidersForAllActive(false);
            }
            if (mBaseAi.m_WildlifeMode == WildlifeMode.Aurora)
            {
                AuroraManager auroraManager = GameManager.m_AuroraManager;
                if (auroraManager.m_NormalizedActive <= auroraManager.m_FullyActiveValue)
                {
                    mBaseAi.m_AuroraObjectMaterials.SwitchToNormalMaterials();
                }
            }
            */
        }


        protected void ProcessFeeding()
        {
            mBaseAi.ProcessFeeding();
        }


        protected void ProcessFlee()
        {
            mBaseAi.ProcessFlee();
            /*
            //Considering putting a timer on this one, it seems expensive to check all this each frame for what, a despawn check? could absolutely be done on a timed frequency without affecting gameplay substantially
            Vector3 position = mBaseAi.m_CachedTransform.position;
            if (Utils.PositionIsOnscreen(position, 0.02f))
            {
                if (Utils.DistanceToMainCamera(position) > GameManager.m_SpawnRegionManager.m_AllowDespawnOnscreenDistance)
                {
                    if (!Utils.PositionIsInLOSOfPlayer(position))
                    {
                        mBaseAi.Despawn();
                        return;
                    }
                }
            }

            if (mBaseAi.m_GroupFleeLeader != null)
            {
                if (mBaseAi.m_GroupFleeLeader.m_CurrentMode != AiMode.Flee && !mBaseAi.m_ExitGroupFleeTimerStarted)
                {
                    mBaseAi.m_ExitGroupFleeTimerStarted = true;
                    mBaseAi.m_ExitGroupFleeTimerSeconds = UnityEngine.Random.Range(0.5f, 1.5f);
                }
            }

            if (mBaseAi.m_ExitGroupFleeTimerStarted)
            {
                mBaseAi.m_ExitGroupFleeTimerSeconds -= Time.deltaTime;
                if (mBaseAi.m_ExitGroupFleeTimerSeconds <= 0.0)
                {
                    //Log("Exiting group flee, resetting to default mode");
                    SetDefaultAiMode();
                    return;
                }
            }

            if (GameManager.m_Weather.IsIndoorEnvironment() && SquaredDistance(mBaseAi.m_CurrentTarget?.transform.position ?? mBaseAi.m_FleeFromPos, mBaseAi.gameObject.transform.position) > 900.0f)
            {
                //Log("Indoor environment and sqrdist(ai, target) > 900.0f, resetting to default mode");
                SetDefaultAiMode();
                return;
            }

            if (!mBaseAi.KeepFleeingFromTarget())
            {
                //Log("Should no longer flee, resetting to default mode...");
                SetDefaultAiMode();
                return;
            }

            if (mBaseAi.MaybeHandleTimeoutFleeing())
            {
                //Log("Flee time out, returning without resetting mode");
                return;
            }

            if (!mBaseAi.m_PickedFleeDestination && !PickFleeDestinationAndTryStartPath())
            {
                return;
            }

            if ((!mBaseAi.m_HasPickedForcedFleePos) || (mBaseAi.m_FleeReason != AiFleeReason.PackMorale))
            {
                if (SquaredDistance(mBaseAi.m_FleeToPos, mBaseAi.m_CachedTransform.position) > 25.0f && !mBaseAi.m_PickedFleeDestination && !PickFleeDestinationAndTryStartPath())
                {
                    return;
                }
                if (mBaseAi.m_MoveAgent.HasPath() && Vector3.Dot(Vector3.Normalize(mBaseAi.m_FleeToPos - mBaseAi.m_CachedTransform.position), mBaseAi.m_CachedTransform.forward) <= 0.0f && !mBaseAi.m_PickedFleeDestination && !PickFleeDestinationAndTryStartPath())
                {
                    return;
                }
            }
            else
            {
                if (!mBaseAi.m_PickedFleeDestination && !PickFleeDestinationAndTryStartPath())
                {
                    return;
                }
            }

            if (mBaseAi.m_MoveAgent.m_DestinationReached)
            {

                mBaseAi.m_PickedFleeDestination = false;
            }

            if (mBaseAi.m_AiType == AiType.Predator)
            {
                mBaseAi.MaybeAttackPlayerWhenTryingToFlee();
            }

            //should we be using a cached position here instead of the actual current position for BaseAi?
            if (mBaseAi.m_UseRetreatSpeedInFlee && Vector3.Distance(mBaseAi.transform.position, mBaseAi.m_CurrentTarget?.transform.position ?? mBaseAi.m_FleeFromPos) < 10.0f)
            {
                mBaseAi.m_UseRetreatSpeedInFlee = false;
                mBaseAi.m_AiGoalSpeed = mBaseAi.GetFleeSpeed();
            }

            mBaseAi.m_FleeingForSeconds += Time.deltaTime;
            mBaseAi.m_FleeingForSecondsSinceLastFleeToSpawnPos += Time.deltaTime;

            if (mBaseAi.m_GroupFleeLeader != null)
            {
                mBaseAi.m_WarnOthersTimer -= Time.deltaTime;
                if (mBaseAi.m_WarnOthersTimer < 0.0f)
                {
                    mBaseAi.WarnOthersNearby();
                    mBaseAi.m_WarnOthersTimer = mBaseAi.m_GroupFleeRepeatDetectSeconds;
                }
            }
            */
        }


        protected void ProcessFollowWaypoints()
        {
            if (!mBaseAi.m_MoveAgent.m_DestinationReached)
            {
                return;
            }
            mTimeSinceCheckForTargetInPatrolWaypointsMode += Time.deltaTime;
            if (mTimeSinceCheckForTargetInPatrolWaypointsMode >= 10.0f)
            {
                mTimeSinceCheckForTargetInPatrolWaypointsMode = 0.0f;
                mBaseAi.ScanForNewTarget();
                mBaseAi.ScanForSmells();
                mBaseAi.MaybeEnterWanderPause();
                return;
            }
            if (!mBaseAi.m_HasEnteredFollowWaypoints)
            {
                mBaseAi.DoEnterFollowWaypoints();
                mBaseAi.m_HasEnteredFollowWaypoints = true;
            }
            if (mBaseAi.m_TargetWaypointIndex == -1 ||
                mBaseAi.m_TargetWaypointIndex >= (mBaseAi.m_Waypoints?.Count ?? 0))
            {
                mBaseAi.ScanForNewTarget();
                mBaseAi.ScanForSmells();
                mBaseAi.MaybeEnterWanderPause();
                return;
            }
            mBaseAi.MaybeWander();
            mBaseAi.m_TargetWaypointIndex++;
            if (mBaseAi.m_TargetWaypointIndex >= mBaseAi.m_Waypoints.Length)
            {
                mBaseAi.HandleLastWaypoint();
            }
            //mBaseAi.m_AiGoalSpeed = m_FollowWaypointsSpeed;
            mBaseAi.PathfindToWaypoint(mBaseAi.m_TargetWaypointIndex);
        }


        protected void ProcessHoldGround()
        {
            mBaseAi.ProcessHoldGround();
            /*
            AiUtils.TurnTowardsTarget(mBaseAi);
            mBaseAi.HoldGroundFightOrFlight();
            if (CurrentMode != AiMode.HoldGround)
            {
                return;
            }

            bool b1 = false;
            bool b2 = false;

            if (mBaseAi.m_DelayStopHoldGroundTimers && !mTimeOfDay.IsTimeLapseActive())
            {
                mBaseAi.SetStopHoldGroundTimers();
                mBaseAi.m_DelayStopHoldGroundTimers = false;
            }

            if (mBaseAi.MaybeHoldGroundForRedFlare(m_HoldGroundDistanceFromFlare) || mBaseAi.MaybeHoldGroundForRedFlareOnGround(m_HoldGroundDistanceFromFlareOnGround))
            {
                mBaseAi.HoldGroundCommon(mBaseAi.m_TimeToStopHoldingGroundDueToFlare, mBaseAi.m_ChanceAttackOnFlareTimeout);
                b1 = true;
            }

            if (mBaseAi.m_WildlifeMode == WildlifeMode.Aurora)
            {
                if (mBaseAi.m_ContainingAuroraField?.m_IsActive ?? false)
                {
                    mBaseAi.m_HoldGroundReason = HoldGroundReason.InsideAuroraField;
                    mBaseAi.HoldGroundInsideAuroraField();
                    b2 = true;
                }
                else if (mBaseAi.m_PlayerSafeHaven != null)
                {
                    mBaseAi.HoldGroundSafeHaven();
                    mBaseAi.m_HoldGroundReason = HoldGroundReason.SafeHaven;
                    b2 = true;
                }
                else
                {
                    b2 = false;
                }
            }
            else
            {
                b2 = false;
            }

            if (mBaseAi.m_WildlifeMode == WildlifeMode.Aurora && b2 != mBaseAi.m_WasHoldingForField)
            {
                mBaseAi.m_WasHoldingForField = b2;
                if (b2 == false)
                {
                    if (CurrentTarget != null && mBaseAi.m_TimeInModeSeconds > mBaseAi.m_HoldForFieldMinimumDelaySeconds)
                    {
                        //possible request for super spline but damned if i can get it to work dude
                        if (mBaseAi.EnterAttackModeIfPossible(CurrentTarget.transform.position, true))
                        {
                            return;
                        }
                        SetAiMode(AiMode.Flee);
                        return;
                    }
                }
                else
                {
                    mBaseAi.InitializeHoldForFieldTimers();
                }
            }
            bool maybeAttackOrFleeForChemicalHazard = mBaseAi.MaybeAttackOrFleeForChemicalHazard();
            if (!b1 && !maybeAttackOrFleeForChemicalHazard)
            {
                if (mBaseAi.MaybeHoldGroundForTorch(m_HoldGroundDistanceFromTorch) || mBaseAi.MaybeHoldGroundForTorchOnGround(m_HoldGroundDistanceFromTorchOnGround))
                {
                    mBaseAi.HoldGroundCommon(mBaseAi.m_TimeToStopHoldingGroundDueToTorch, mBaseAi.m_ChanceAttackOnTorchTimeout);
                    b1 = true;
                }
                else if (mBaseAi.MaybeHoldGroundForFire(m_HoldGroundDistanceFromFire))
                {
                    mBaseAi.HoldGroundCommon(mBaseAi.m_TimeToStopHoldingGroundDueToFire, mBaseAi.m_ChanceAttackOnFireTimeout);
                    b1 = true;
                }
                else if (!b1 && mBaseAi.MaybeHoldGroundForSpear(mBaseAi.m_HoldGroundDistanceFromSpear))
                {
                    mBaseAi.HoldGroundCommon(mBaseAi.m_TimeToStopHoldingGroundDueToSpear, mBaseAi.m_ChanceAttackOnSpearTimeout);
                    b1 = true;
                }
                else if (!b1 && (mBaseAi.MaybeHoldGroundForBlueFlare(m_HoldGroundDistanceFromBlueFlare) || mBaseAi.MaybeHoldGroundForBlueFlareOnGround(m_HoldGroundDistanceFromBlueFlareOnGround)))
                {
                    mBaseAi.HoldGroundCommon(mBaseAi.m_TimeToStopHoldingGroundDueToBlueFlare, mBaseAi.m_ChanceAttackOnBlueFlareTimeout);
                    b1 = true;
                }
                else if (!b1 && mBaseAi.MaybeHoldGroundDueToStruggle())
                {
                    PlayerStruggle playerStruggle = GameManager.m_PlayerStruggle;
                    UniStormWeatherSystem weatherSystem = mTimeOfDay.m_WeatherSystem;

                    float struggleGracePeriod = Mathf.Clamp(playerStruggle.m_GracePeriodAfterStruggleInSeconds - playerStruggle.m_SecondsSinceLastStruggle, 0, float.MaxValue);
                    mBaseAi.HoldGroundCommon(24.0f / weatherSystem.m_DayLength * struggleGracePeriod + weatherSystem.m_ElapsedHoursAccumulator + weatherSystem.m_ElapsedHours, 100.0f);
                    return;
                }
            }

            //todo: this is awful, clean it up
            if (maybeAttackOrFleeForChemicalHazard || b2 || mBaseAi.IsTargetGoneOrOutOfRange())
            {
                mBaseAi.MaybeFleeFromHoldGround();
                if (CurrentMode != AiMode.HoldGround)
                {
                    return;
                }
                if (mBaseAi.Timberwolf?.CanEnterHideAndSeek() ?? false)
                {
                    SetAiMode(AiMode.HideAndSeek);
                    return;
                }
                if (GameManager.m_PackManager.IsPackCombatRestricted(mBaseAi.m_PackAnimal) || maybeAttackOrFleeForChemicalHazard || b2 || !b1)
                {
                    return;
                }
                if (mBaseAi.m_TimeInModeSeconds <= mBaseAi.m_HoldGroundMinimumDelaySeconds)
                {
                    return;
                }
                if (mBaseAi.m_PreviousMode == AiMode.Feeding)
                {
                    return;
                }
                if (mBaseAi.m_PreviousMode == AiMode.Stalking && !mBaseAi.CanEnterStalking())
                {
                    return;
                }
                if (mBaseAi.CanEnterStalking())
                {
                    SetAiMode(AiMode.Stalking);
                    return;
                }
            }
            else if (mBaseAi.m_PreviousMode == AiMode.Feeding)
            {
                mBaseAi.ClearTarget();
                SetAiMode(AiMode.Feeding);
                return;
            }
            SetDefaultAiMode();
            */
        }


        protected void ProcessIdle()
        {
            mBaseAi.ClearTarget();
            mBaseAi.ScanForNewTarget();
            mBaseAi.ScanForSmells();
            if (CurrentMode != AiMode.Idle || mBaseAi.m_StartMode == AiMode.Idle)
                return;
            if (mBaseAi.m_TimeInModeSeconds > 10.0f)
            {
                SetAiMode(mBaseAi.m_DefaultMode);
            }
        }


        protected void ProcessInvestigate()
        {
            mBaseAi.ProcessInvestigate();
        }


        protected void ProcessInvestigateFood()
        {
            mBaseAi.ProcessInvestigateFood();
        }


        protected void ProcessInvestigateSmell()
        {
            mBaseAi.ProcessInvestigateSmell();
        }


        protected void ProcessRooted()
        {
            mBaseAi.ProcessRooted();
        }


        protected void ProcessSleep()
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


        protected void ProcessStalking()
        {
            mBaseAi.ProcessStalking();
        }


        protected void ProcessStruggle()
        {
            mBaseAi.ProcessStruggle();
        }


        protected void ProcessWander()
        {
            mBaseAi.ProcessWander();
            /*
            bool hasNewWanderPos = false;
            Vector3 wanderPos = Vector3.zero;
            mBaseAi.ClearTarget();
            MaybeImposter();
            if (IsImposter())
            {
                mBaseAi.m_AiGoalSpeed = 0.0f;
                return;
            }
            mBaseAi.m_AiGoalSpeed = mBaseAi.m_WalkSpeed;
            if (mBaseAi.m_TimeInModeSeconds > 1.0f)
            {
                mBaseAi.ScanForNewTarget();
            }
            if ((mBaseAi.m_WanderingAroundPos == false) &&
               (mBaseAi.m_PickedWanderDestination != false))
            {
                mBaseAi.ScanForSmells();
            }
            if (mBaseAi.m_NextCheckMovedDistanceTime < Time.time)
            {
                if (SquaredDistance(mBaseAi.m_CachedTransform.position, mBaseAi.m_PositionAtLastMoveCheck) < 0.04f)
                {
                    mBaseAi.m_PickedWanderDestination = false;
                }
                mBaseAi.m_NextCheckMovedDistanceTime = Time.time + 1.0f;
                mBaseAi.m_PositionAtLastMoveCheck = mBaseAi.m_CachedTransform.position;
            }
            //Section A
            if (mBaseAi.m_PickedWanderDestination == false)
            {
                if (mBaseAi.Moose != null && !mBaseAi.m_UseWanderAwayFromPos)
                {
                    hasNewWanderPos = mBaseAi.Moose?.MaybeSelectScratchingStump(out wanderPos) ?? false;
                    if (hasNewWanderPos)
                    {
                        mBaseAi.m_CurrentWanderPos = wanderPos;
                    }
                }

                if (!mBaseAi.m_UseWanderAwayFromPos)
                {
                    if (mBaseAi.m_UseWanderToPos)
                    {
                        mBaseAi.m_CurrentWanderPos = mBaseAi.transform.position;
                        hasNewWanderPos = AiUtils.GetClosestNavmeshPos(out wanderPos, mBaseAi.m_WanderToPos, mBaseAi.m_CachedTransform.position);
                        if (hasNewWanderPos)
                        {
                            mBaseAi.m_CurrentWanderPos = wanderPos;
                        }
                        mBaseAi.m_UseWanderToPos = false;
                    }
                }
                else
                {
                    hasNewWanderPos = mBaseAi.PickWanderDestinationAwayFromPoint(out wanderPos, mBaseAi.m_WanderAwayFromPos);
                    if (hasNewWanderPos)
                    {
                        mBaseAi.m_CurrentWanderPos = wanderPos;
                    }
                    mBaseAi.m_UseWanderAwayFromPos = false;
                }

                if ((hasNewWanderPos) || mBaseAi.PickWanderDestination(out wanderPos))
                {
                    if (!hasNewWanderPos)
                    {
                        hasNewWanderPos = true;
                        mBaseAi.m_CurrentWanderPos = wanderPos;
                    }
                    if (mBaseAi.m_WildlifeMode == WildlifeMode.Aurora)
                    {
                        hasNewWanderPos = mBaseAi.MaybeMoveWanderPosOutsideOfField(out wanderPos, mBaseAi.m_CurrentWanderPos);

                    }
                }

                if (!hasNewWanderPos)
                {
                    mBaseAi.m_CurrentWanderPos = mBaseAi.transform.position;
                    hasNewWanderPos = AiUtils.GetClosestNavmeshPos(out wanderPos, mBaseAi.m_CachedTransform.position, mBaseAi.m_CachedTransform.position);
                    if (!hasNewWanderPos)
                    {
                        mBaseAi.MoveAgentStop();
                        SetDefaultAiMode();
                        return;
                    }
                    mBaseAi.m_CurrentWanderPos = wanderPos;
                }

                if (!mBaseAi.m_WanderUseTurnRadius)
                {
                    hasNewWanderPos = mBaseAi.StartPath(mBaseAi.m_CurrentWanderPos, mBaseAi.m_WalkSpeed);
                }
                else
                {
                    mBaseAi.m_WanderTurnTargets = AiUtils.GetPointsForGradualTurn(mBaseAi.transform, mBaseAi.m_CurrentWanderPos, mBaseAi.m_WanderTurnRadius, mBaseAi.m_WanderTurnSegmentAngle);
                    mBaseAi.m_WanderCurrentTarget = 0;
                    if (mBaseAi.m_WanderTurnTargets.Count == 0)
                    {
                        return;
                    }
                    hasNewWanderPos = mBaseAi.StartPath(mBaseAi.m_WanderTurnTargets[0], mBaseAi.m_WalkSpeed);
                }

                if (!hasNewWanderPos)
                {
                    SetDefaultAiMode();
                    return;
                }

                mBaseAi.m_PickedWanderDestination = true;
            }

            //todo: handle weird-ass low level redundancy for getting aurora ai's out of active aurora fields

            if (mBaseAi.m_MoveAgent.m_DestinationReached)
            {
                bool pathStarted = false;
                if (mBaseAi.m_WanderUseTurnRadius)
                {
                    mBaseAi.m_WanderCurrentTarget += 1;
                    if (mBaseAi.m_WanderCurrentTarget < BaseAi.m_WanderTurnTargets.Length)
                    {
                        mBaseAi.StartPath(mBaseAi.m_WanderTurnTargets[mBaseAi.m_WanderCurrentTarget], mBaseAi.m_WalkSpeed);
                        pathStarted = true;
                    }
                }
                if (!pathStarted)
                {
                    mBaseAi.m_PickedWanderDestination = false;
                }
            }

            if (mBaseAi.m_WanderDurationHours > 0.0001f && mBaseAi.m_WanderDurationHours < mBaseAi.m_ElapsedWanderHours)
            {
                mBaseAi.m_ElapsedWanderHours = 0.0f;
                mBaseAi.m_WanderDurationHours = 0.0f;
                mBaseAi.m_WanderingAroundPos = false;
                SetDefaultAiMode();
                return;
            }

            mBaseAi.MaybeHoldGroundAuroraField();
            mBaseAi.MaybeEnterWanderPause();
            if (mBaseAi.Bear?.ShouldAlwaysStalkPlayer() ?? false)
            {
                mBaseAi.MaybeForceStalkPlayer();
            }

            UniStormWeatherSystem uniStormWeatherSystem = mTimeOfDay.m_WeatherSystem;
            mBaseAi.m_ElapsedWanderHours += (24.0f / (uniStormWeatherSystem.m_DayLengthScale * uniStormWeatherSystem.m_DayLength)) * Time.deltaTime;
            */
        }


        protected void ProcessWanderPaused()
        {
            mBaseAi.ProcessWanderPaused();
            /*
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
            */
        }


        protected void ProcessGoToPoint()
        {
            mBaseAi.ProcessGoToPoint();
            /*
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
            */
        }


        protected void ProcessInteractWithProp()
        {
            mBaseAi.ProcessInteractWithProp();
        }


        protected void ProcessScriptedSequence()
        {
            mBaseAi.ProcessScriptedSequence();
        }


        protected void ProcessStunned()
        {
            mBaseAi.ProcessStunned();
        }


        protected void ProcessScratchingAntlers()
        {
            mBaseAi.Moose?.ProcessScratchingAntlers();
        }


        protected void ProcessPatrolPointsOfInterest()
        {
            mBaseAi.ProcessPatrolPointsOfInterest();
            /*

            // Thought this was needed but the behaviour I was looking for is "FollowWaypoints" not follow points of interest, which reset and are temporary. Waypoints appear to be permanent.
            if (mBaseAi.m_PlayingAttackStartAnimation)
            {
                AiUtils.TurnTowardsTarget(mBaseAi);
                mBaseAi.MaybeApplyAttack();
                if (5.0 < mBaseAi.m_TimeInModeSeconds)
                {
                    mBaseAi.m_PlayingAttackStartAnimation = false;
                    MaybeHoldGround();
                }
                return;
            }
            if (!mBaseAi.m_HasEnteredPatrolPointsOfInterest)
            {
                //Log("mBaseAi.m_HasEnteredPatrolPointsOfInteres is false, picking poi index and starting pathfinding");
                mBaseAi.ClearTarget();
                if (!mBaseAi.m_RandomizePointsOfInterest)
                {
                    mBaseAi.m_TargetPointOfInterestIndex = 0;
                }
                else
                {
                    mBaseAi.m_TargetPointOfInterestIndex = UnityEngine.Random.Range(0, mBaseAi.m_ActivePointsOfInterest.Count - 1);
                }
                //Log($"Picked POI index {mBaseAi.m_TargetPointOfInterestIndex}");
                mBaseAi.m_AiGoalSpeed = mBaseAi.m_WalkSpeed;
                mBaseAi.m_DefaultMode = (AiMode)0x16;
                mBaseAi.PathfindToPointOfInterest(mBaseAi.m_TargetPointOfInterestIndex);
                mBaseAi.m_HasEnteredPatrolPointsOfInterest = true;
            }
            if (!mBaseAi.m_IsAnimatingAtPointOfInterest)
            {
                if (mBaseAi.ReachedTargetPointOfInterest())
                {
                    //Log($"Target has reached POI and is not animating, triggering DoReachedTargetOfPointOfInterestBehavior");
                    mBaseAi.DoReachedTargetPointOfInterestBehavior();
                }
            }
            else
            {
                //Log($"Not animating at POI, incrementing time in mode");
                mBaseAi.m_ElapsedTimeAtPointOfInterestSeconds += (86400.0f / (mTimeOfDay.m_WeatherSystem.m_DayLengthScale * mTimeOfDay.m_WeatherSystem.m_DayLength)) * Time.deltaTime;
                if (BaseAi.m_DurationAtPointOfInterestSeconds < mBaseAi.m_ElapsedTimeAtPointOfInterestSeconds)
                {
                    //Log($"BaseAi.m_DurationAtPointOfInterestSeconds ({BaseAi.m_DurationAtPointOfInterestSeconds}) < mBaseAi.m_ElapsedTimeAtPointOfInterestSeconds ({mBaseAi.m_ElapsedTimeAtPointOfInterestSeconds}), triggering next POI");
                    BaseAi.PathfindToNextPointOfInterest();
                }
            }
            mBaseAi.ScanForNewTarget();
            mBaseAi.ScanForSmells();
            */
        }


        protected void ProcessHideAndSeek()
        {
            mBaseAi.Timberwolf?.ProcessHideAndSeek();
        }


        protected void ProcessJoinPack()
        {
            mBaseAi.Timberwolf?.ProcessJoinPack();
        }


        protected void ProcessPassingAttack()
        {
            mBaseAi.ProcessPassingAttack();
        }


        protected void ProcessHowl()
        {
            mBaseAi.BaseWolf?.ProcessHowl();
        }

        #endregion


        protected void PostProcess()
        {
            if (PostProcessCustom())
            {
                return;
            }
            if (CurrentMode == AiMode.Dead || CurrentMode == AiMode.ScriptedSequence)
            {
                return;
            }
            if (mBaseAi.IsImposter())
            {
                return;
            }
            SetAnimationParameters();
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
                (CurrentMode != AiMode.Wander
                && mBaseAi.m_WanderTurnTargets != null
                && mBaseAi.m_WanderTurnTargets.Count > 1
                && mBaseAi.m_WanderCurrentTarget > mBaseAi.m_WanderTurnTargets.Count)
                ? mBaseAi.m_WanderTurnSpeedDegreesPerSecond
                : mBaseAi.m_TurnSpeedDegreesPerSecond;
            mBaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_ActualSpeed, mBaseAi.m_MoveAgent.m_MaxSpeed);
        }


        #endregion


        #region SetAiMode

        protected AiMode PreprocessNewAiMode(AiMode mode)
        {
            if (PreprocesSetAiModeCustom(mode, out AiMode newMode))
            {
                return newMode;
            }
            if (mode == AiMode.Flee)
            {
                if (CurrentMode == AiMode.Flee && mBaseAi.m_FleeReason == AiFleeReason.AfterPassingAttack)
                {
                    return AiMode.None;
                }
            }
            else if (mode == AiMode.Attack)
            {
                if (mBaseAi.IsTooScaredToAttack())
                {
                    return AiMode.None;
                }
                bool skip = false;
                if (mBaseAi.Timberwolf != null)
                {
                    if (mBaseAi.m_CurrentTarget.IsPlayer())
                    {
                        if (CurrentMode == AiMode.Attack)
                        {
                            return AiMode.None;
                        }
                        if (PackManager.InPack(mBaseAi.m_PackAnimal))
                        {
                            if (!GameManager.m_PackManager.CanAttack(mBaseAi.m_PackAnimal, false))
                            {
                                mode = AiMode.HoldGround;
                            }
                        }
                        else
                        {
                            mode = AiMode.Flee;
                        }
                        skip = true;
                    }
                }
                if (!skip)
                {
                    if (ShouldHoldGround())
                    {
                        SetAiMode(AiMode.HoldGround);
                        return AiMode.None;
                    }
                    if (!mBaseAi.CanPathfindToPosition(mBaseAi.m_CurrentTarget.transform.position, MoveAgent.PathRequirement.FullPath))
                    {
                        mBaseAi.CantReachTarget();
                        return AiMode.None;
                    }
                }
            }
            else if (mode == AiMode.Wander && mBaseAi.Timberwolf != null && PackManager.InPack(mBaseAi.m_PackAnimal) && GameManager.m_PackManager.IsPackCombatRestricted(mBaseAi.m_PackAnimal))
            {
                mode = AiMode.HoldGround;
            }
            if (mBaseAi is AiStagWhite && CurrentMode == AiMode.Flee)
            {
                return AiMode.None;
            }
            if (mode == AiMode.Wander || mode == AiMode.Flee)
            {
                if (mBaseAi.m_DefaultMode == AiMode.PatrolPointsOfInterest)
                {
                    mode = AiMode.PatrolPointsOfInterest;
                }
            }
            else if (mode == AiMode.None)
            {
                mode = AiMode.Idle;
            }
            else if (mode == AiMode.Howl)
            {
                if (mBaseAi.BaseWolf != null)
                {
                    return AiMode.None;
                }
            }
            if (mode == CurrentMode)
            {
                if (CurrentMode != AiMode.Flee)
                {
                    return AiMode.None;
                }
                if (mBaseAi.m_UseRetreatSpeedInFlee == false)
                {
                    return AiMode.None;
                }
                mBaseAi.m_UseRetreatSpeedInFlee = false;
                mBaseAi.m_AiGoalSpeed = mBaseAi.GetFleeSpeed();
                return AiMode.None;
            }
            if (CurrentMode == AiMode.Stunned && mBaseAi.IsStunTimerActive() && mode != AiMode.Dead && mode != AiMode.ScriptedSequence)
            { 
                return AiMode.None;
            }
            return mode;
        }





        public void SetAiMode(AiMode mode)
        {
            mode = PreprocessNewAiMode(mode);
            if (mode == AiMode.None)
            {
                return;
            }
            ExitAiMode(CurrentMode);
            EnterAiMode(mode);
            PreviousMode = CurrentMode;
            CurrentMode = mode;
            mBaseAi.m_TimeInModeSeconds = 0.0f;
            mBaseAi.m_TimeInModeTODHours = 0.0f;
            GameAudioManager.SetAiStateSwitch(CurrentMode, GameAudioManager.GetSoundEmitterProxyFromGameObject(mBaseAi.gameObject));
        }


        #region EnterAiMode

        protected void EnterAiMode(AiMode mode)
        {
            if (EnterAiModeCustom(mode))
            {
                return;
            }
            switch (mode)
            {
                case AiMode.Attack: EnterAttack(); break;
                case AiMode.Dead: EnterDead(); break;
                case AiMode.Feeding: EnterFeeding(); break;
                case AiMode.Flee: EnterFlee(); break;
                case AiMode.FollowWaypoints: EnterFollowWaypoints(); break;
                case AiMode.HoldGround: EnterHoldGround(); break;
                case AiMode.Idle: EnterIdle(); break;
                case AiMode.Investigate: EnterInvestigate(); break;
                case AiMode.InvestigateFood: EnterInvestigateFood(); break;
                case AiMode.InvestigateSmell: EnterInvestigateSmell(); break;
                case AiMode.Rooted: EnterRooted(); break;
                case AiMode.Sleep: EnterSleep(); break;
                case AiMode.Stalking: EnterStalking(); break;
                case AiMode.Struggle: EnterStruggle(); break;
                case AiMode.Wander: EnterWander(); break;
                case AiMode.WanderPaused: EnterWanderPaused(); break;
                case AiMode.GoToPoint: EnterGoToPoint(); break;
                case AiMode.InteractWithProp: EnterInteractWithProp(); break;
                case AiMode.ScriptedSequence: EnterScriptedSequence(); break;
                case AiMode.Stunned: EnterStunned(); break;
                case AiMode.ScratchingAntlers: EnterScratchingAntlers(); break;
                case AiMode.PatrolPointsOfInterest: EnterPatrolPointsOfInterest(); break;
                case AiMode.HideAndSeek: EnterHideAndSeek(); break;
                case AiMode.JoinPack: EnterJoinPack(); break;
                case AiMode.PassingAttack: EnterPassingAttack(); break;
                case AiMode.Howl: EnterHowl(); break;
            }
        }


        protected void EnterAttack()
        {
            mBaseAi.EnterAttack();
        }


        protected void EnterDead()
        {
            mBaseAi.EnterDead();
        }


        protected void EnterFeeding()
        {
            mBaseAi.EnterFeeding();
        }


        protected void EnterFlee()
        {
            mBaseAi.EnterFlee();
        }


        protected void EnterFollowWaypoints()
        {
            mBaseAi.EnterFollowWaypoints();
        }


        protected void EnterHoldGround()
        {
            mBaseAi.EnterHoldGround();
        }


        protected void EnterIdle()
        {
            mBaseAi.EnterIdle();
        }


        protected void EnterInvestigate()
        {
            mBaseAi.EnterInvestigate();
        }


        protected void EnterInvestigateFood()
        {
            mBaseAi.EnterInvestigateFood();
        }


        protected void EnterInvestigateSmell()
        {
            mBaseAi.EnterInvestigateSmell();
        }


        protected void EnterRooted()
        {
            mBaseAi.MoveAgentStop();
        }


        protected void EnterSleep()
        {
            mBaseAi.EnterSleep();
        }


        protected void EnterStalking()
        {
            mBaseAi.EnterStalking();
        }


        protected void EnterStruggle()
        {
            mBaseAi.EnterStruggle();
        }


        protected void EnterWander()
        {
            mBaseAi.EnterWander();
        }


        protected void EnterWanderPaused()
        {
            mBaseAi.EnterWanderPaused();
        }


        protected void EnterGoToPoint()
        {
            mBaseAi.EnterGoToPoint();
        }


        protected void EnterInteractWithProp()
        {
            mBaseAi.EnterInteractWithProp();
        }


        protected void EnterScriptedSequence()
        {
            mBaseAi.EnterScriptedSequence();
        }


        protected void EnterStunned()
        {
            mBaseAi.EnterStunned();
        }


        protected void EnterScratchingAntlers()
        {
            mBaseAi.Moose?.EnterScratchingAntlers();
        }


        protected void EnterPatrolPointsOfInterest()
        {
            mBaseAi.EnterPatrolPointsOfInterest();
        }


        protected void EnterHideAndSeek()
        {
            mBaseAi.Timberwolf?.EnterHideAndSeek();
        }


        protected void EnterJoinPack()
        {
            mBaseAi.Timberwolf?.EnterJoinPack();
        }


        protected void EnterPassingAttack()
        {
            mBaseAi.Timberwolf?.EnterPassingAttack();
        }


        protected void EnterHowl()
        {
            mBaseAi.BaseWolf?.EnterHowl();
        }

        #endregion


        #region ExitAiMode

        protected void ExitAiMode(AiMode mode)
        {
            if (ExitAiModeCustom(mode))
            {
                return;
            }
            switch (mode)
            {
                case AiMode.Attack:
                    ExitAttack();
                    break;
                case AiMode.Dead:
                    ExitDead();
                    break;
                case AiMode.Feeding:
                    ExitFeeding();
                    break;
                case AiMode.Flee:
                    ExitFlee();
                    break;
                case AiMode.FollowWaypoints:
                    ExitFollowWaypoints();
                    break;
                case AiMode.HoldGround:
                    ExitHoldGround();
                    break;
                case AiMode.Idle:
                    ExitIdle();
                    break;
                case AiMode.Investigate:
                    ExitInvestigate();
                    break;
                case AiMode.InvestigateFood:
                    ExitInvestigateFood();
                    break;
                case AiMode.InvestigateSmell:
                    ExitInvestigateSmell();
                    break;
                case AiMode.Rooted:
                    ExitRooted();
                    break;
                case AiMode.Sleep:
                    ExitSleep();
                    break;
                case AiMode.Stalking:
                    ExitStalking();
                    break;
                case AiMode.Struggle:
                    ExitStruggle();
                    break;
                case AiMode.Wander:
                    ExitWander();
                    break;
                case AiMode.WanderPaused:
                    ExitWanderPaused();
                    break;
                case AiMode.GoToPoint:
                    ExitGoToPoint();
                    break;
                case AiMode.InteractWithProp:
                    ExitInteractWithProp();
                    break;
                case AiMode.ScriptedSequence:
                    ExitScriptedSequence();
                    break;
                case AiMode.Stunned:
                    ExitStunned();
                    break;
                case AiMode.ScratchingAntlers:
                    ExitScratchingAntlers();
                    break;
                case AiMode.PatrolPointsOfInterest:
                    ExitPatrolPointsOfInterest();
                    break;
                case AiMode.HideAndSeek:
                    ExitHideAndSeek();
                    break;
                case AiMode.JoinPack:
                    ExitJoinPack();
                    break;
                case AiMode.PassingAttack:
                    ExitPassingAttack();
                    break;
                case AiMode.Howl:
                    ExitHowl();
                    break;
            }
        }


        protected void ExitAttack()
        {
            mBaseAi.ExitAttack();
        }


        protected void ExitDead()
        {
            mBaseAi.ExitDead();
        }


        protected void ExitFeeding()
        {
            mBaseAi.ExitFeeding();
        }


        protected void ExitFlee()
        {
            mBaseAi.ExitFlee();
        }


        protected void ExitFollowWaypoints()
        {
            mBaseAi.ExitFollowWaypoints();
        }


        protected void ExitHoldGround()
        {
            mBaseAi.ExitHoldGround();
        }


        protected void ExitIdle()
        {
            mBaseAi.ExitIdle();
        }


        protected void ExitInvestigate()
        {
            mBaseAi.ExitInvestigate();
        }

        protected void ExitInvestigateFood()
        {
            mBaseAi.ExitInvestigateFood();
        }

        protected void ExitInvestigateSmell()
        {
            mBaseAi.ExitInvestigateSmell();
        }

        protected void ExitRooted()
        {

        }

        protected void ExitSleep()
        {
            mBaseAi.ExitSleep();
        }

        protected void ExitStalking()
        {
            mBaseAi.ExitStalking();
        }

        protected void ExitStruggle()
        {
            mBaseAi.ExitStruggle();
        }

        protected void ExitWander()
        {
            mBaseAi.ExitWander();
        }

        protected void ExitWanderPaused()
        {
            mBaseAi.ExitWanderPaused();
        }

        protected void ExitGoToPoint()
        {
            mBaseAi.ExitGoToPoint();
        }

        protected void ExitInteractWithProp()
        {
            mBaseAi.ExitInteractWithProp();
        }

        protected void ExitScriptedSequence()
        {
            mBaseAi.ExitScriptedSequence();
        }

        protected void ExitStunned()
        {
            mBaseAi.ExitStunned();
        }

        protected void ExitScratchingAntlers()
        {
            mBaseAi.Moose?.ExitScratchingAntlers();
        }

        protected void ExitPatrolPointsOfInterest()
        {
            mBaseAi.ExitPatrolPointsOfInterest();
        }

        protected void ExitHideAndSeek()
        {
            mBaseAi.Timberwolf?.ExitHideAndSeek();
        }

        protected void ExitJoinPack()
        {
            mBaseAi.Timberwolf?.ExitJoinPack();
        }

        protected void ExitPassingAttack()
        {
            //Nothing? Not sure...
            //mBaseAi.Timberwolf?.Exit();
        }

        protected void ExitHowl()
        {
            mBaseAi.BaseWolf?.ExitHowl();
        }

        #endregion

        #endregion


        #region MaybeHoldGround

        protected void MaybeHoldGround()
        {
            mBaseAi.MaybeHoldGround();
            /*
            if (mBaseAi.m_AiType != AiType.Predator)
            {
                return;
            }

            if (!mBaseAi.CanHoldGround())
            {
                return;
            }
            if (((1U << (int)CurrentMode) & (uint)AiModeFlags.EarlyOutMaybeHoldGround) != 0U)
            {
                return;
            }
            else if (CurrentMode == AiMode.Attack && mBaseAi.m_IgnoreFlaresAndFireWhenAttacking)
            {
                return;
            }

            bool holdingGround = mBaseAi.MaybeHoldGroundForTorch(m_HoldGroundDistanceFromTorch);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForTorchOnGround(m_HoldGroundDistanceFromTorchOnGround);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForFire(m_HoldGroundDistanceFromFire);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForRedFlare(m_HoldGroundDistanceFromFlare);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForRedFlareOnGround(m_HoldGroundDistanceFromFlareOnGround);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForBlueFlare(m_HoldGroundDistanceFromBlueFlare);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForBlueFlareOnGround(m_HoldGroundDistanceFromBlueFlareOnGround);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundForSpear(m_HoldGroundDistanceFromSpear);
            holdingGround = holdingGround || mBaseAi.MaybeHoldGroundDueToStruggle();
            if (holdingGround)
            {
                SetAiMode(AiMode.HoldGround);
            }
            */
        }


        protected bool MaybeHoldGroundForAttack(HoldGroundReason reason, Func<float, bool> shouldHoldGroundFunc)
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


        protected bool MaybeHoldGroundForSpear(float radius)
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


        #region Imposter Settings


        protected void MaybeImposter()
        {
            mBaseAi.m_Imposter = TestIsImposter();
            ApplyImposterStatus();
        }


        protected bool TestIsImposter()
        {
            if (TestIsImposterCustom(out bool isImposter))
            {
                return isImposter;
            }
            return Utils.DistanceToMainCamera(mBaseAi.m_CachedTransform.position) > (Utils.PositionIsOnscreen(mBaseAi.m_CachedTransform.position) ? mBaseAi.m_ImposterDistanceOnScreen : mBaseAi.m_ImposterDistanceOffScreen);
        }


        protected void ApplyImposterStatus()
        {
            if (ApplyImposterStatusCustom())
            {
                return;
            }
            mBaseAi.m_CharacterController.enabled = !mBaseAi.IsImposter();
        }


        #endregion


        #region Bleeding & Wound Management

        public bool CanBleedOut()
        {
            return CanBleedOutCustom(out bool canBleedOut) ? canBleedOut : mBaseAi.CanBleedOut();
        }


        protected virtual void ProcessWounds(float deltaTime)
        {
            if (ProcessWoundsCustom(deltaTime))
            {
                return;
            }
            if (!mBaseAi.m_Wounded)
            {
                return;
            }
            mBaseAi.m_ElapsedWoundedMinutes += (1440.0f / (mTimeOfDay.m_WeatherSystem.m_DayLength * mTimeOfDay.m_WeatherSystem.m_DayLengthScale)) * deltaTime;
        }


        protected void ProcessBleeding(float deltaTime)
        {
            if (ProcessBleedingOutCustom(deltaTime))
            {
                return;
            }
            if (!mBaseAi.m_BleedingOut)
            {
                return;
            }

            mBaseAi.m_ElapsedBleedingOutMinutes += (1440.0f / (mTimeOfDay.m_WeatherSystem.m_DayLength * mTimeOfDay.m_WeatherSystem.m_DayLengthScale)) * deltaTime;
            if (CurrentMode == AiMode.Struggle)
            {
                if (mBaseAi.m_DeathAfterBleeingOutMinutes - mBaseAi.m_ElapsedBleedingOutMinutes < mBaseAi.m_StruggleBleedOutCapTimeMinutes)
                {
                    mBaseAi.m_ElapsedBleedingOutMinutes = mBaseAi.m_DeathAfterBleeingOutMinutes - mBaseAi.m_StruggleBleedOutCapTimeMinutes;
                    return;
                }
            }
            if (mBaseAi.m_ElapsedBleedingOutMinutes >= mBaseAi.m_DeathAfterBleeingOutMinutes)
            {
                mBaseAi.m_ElapsedBleedingOutMinutes = mBaseAi.m_DeathAfterBleeingOutMinutes;
                if (!mBaseAi.Bear?.CanDieFromBleedingOut() ?? false)
                {
                    return;
                }
                mBaseAi.SetDamageImpactParameter(mBaseAi.m_LastDamageSide, mBaseAi.m_LastDamageBodyPart, SetupDamageParamsOptions.None);
                SetAiMode(AiMode.Dead);
            }
        }

        #endregion


        #region Damage Application

        public void ApplyDamage(float damage, float bleedoutMinutes, DamageSource damageSource)
        {
            if (CurrentMode == AiMode.Dead || mBaseAi.m_Invulnerable)
            {
                return;
            }

            if (mBaseAi.m_AiType == AiType.Predator && mBaseAi.m_DamageSource == DamageSource.Player)
            {
                damage *= GameManager.m_AuroraManager.AuroraIsActive() ? GameManager.m_AuroraManager.m_DamageToPredatorsScale : 1.0f;
            }

            mBaseAi.m_CurrentHP -= damage;
            mBaseAi.m_Wounded = true;
            mBaseAi.m_ElapsedWoundedMinutes = 0.0f;

            if ((((mBaseAi.m_CurrentMode == AiMode.Flee) && (mBaseAi.m_FleeReason == AiFleeReason.FleeTriggerVolume)) && (mBaseAi.m_CurrentHP <= 0.0)) && (mBaseAi.CanBleedOut()))
            {
                mBaseAi.m_DeathAfterBleeingOutMinutes = mBaseAi.m_BleedOutTimeMinutesForFleeFromTriggerVolume;
                mBaseAi.m_CurrentHP = 1.0f;
                mBaseAi.m_BleedingOut = true;
                mBaseAi.m_ElapsedBleedingOutMinutes = 0.0f;
                return;
            }
            if (mBaseAi.m_CurrentHP <= 0.0001f)
            {
                SetAiMode(AiMode.Dead);
                if (damageSource != DamageSource.Player)
                {
                    return;
                }
                GameManager.m_AchievementManager.m_HasKilledSomething = true;
            }
            if (PackManager.InPack(mBaseAi.m_PackAnimal) && CurrentMode != AiMode.Struggle)
            {
                GameManager.m_PackManager.ModifyGroupMoraleOnDamage(mBaseAi.m_PackAnimal);
            }
            if (bleedoutMinutes > 0.0f && mBaseAi.CanBleedOut())
            {
                if (!mBaseAi.m_BleedingOut)
                {
                    mBaseAi.m_BleedingOut = true;
                    mBaseAi.m_ElapsedBleedingOutMinutes = 0.0f;
                    mBaseAi.m_DeathAfterBleeingOutMinutes = bleedoutMinutes;
                }
                else if(mBaseAi.m_DeathAfterBleeingOutMinutes - mBaseAi.m_ElapsedBleedingOutMinutes > bleedoutMinutes)
                {
                    mBaseAi.m_DeathAfterBleeingOutMinutes = bleedoutMinutes;
                }

            }
        }

        #endregion


        #region Properties

        //In theory, these should still be affected by other mods which primarily tweak existing fields at awake
        // In the future if we decide to create new mod-specific default values, we'll need to cache the "official" hinterland version and do our own on start (not awake to ensure that other mods get to it first) check to see if props match HL defaults, and ignore them if not because someone else got to them
        // finger crossed .Net is a little smarter with these than with method polymorphism

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
        protected virtual float m_MaxWaypointDistance { get { return 1.0f; } }
        protected virtual float m_MinWaypointDistance { get { return 0.0f; } }

        #endregion


        //This method is finnicky >:(
        protected void SetAnimationParameters()
        {
            float someFloat = mBaseAi.m_AiGoalSpeed;
            if (mBaseAi.m_Wounded) //Original code seemed to limit this to just wolves, which is weird...
            {
                someFloat = (someFloat - mBaseAi.m_WalkSpeed) * Mathf.Clamp01(1 - mBaseAi.GetWoundedAnimParameter()) + mBaseAi.m_WalkSpeed;
            }
            mBaseAi.m_MoveAgent.SetMoveSpeed(someFloat);
            mBaseAi.SetAnimStateForMoveAgent(IsMoveState(CurrentMode) ? MoveState.Moving : MoveState.Idle, (int)GetAiAnimationState(CurrentMode));
            if (mBaseAi.BaseWolf != null)
            {
                if (!Utils.Approximately(mBaseAi.m_AnimParameter_Wounded_LastSent, mBaseAi.GetWoundedAnimParameter(), 0.0001f))
                {
                    mBaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_Wounded, mBaseAi.GetWoundedAnimParameter());
                    mBaseAi.m_AnimParameter_Wounded_LastSent = mBaseAi.GetWoundedAnimParameter();
                }
            }
            mBaseAi.SetTargetHeadingParameter();
            if (mBaseAi.m_CanPlayPitchRoll)
            {
                Vector3 forward = mBaseAi.m_CachedTransform.forward;
                forward.y = 0f;
                forward.Normalize();
                float pitchValue = Vector3.Dot(forward, mBaseAi.m_CachedTransform.forward);
                if (mBaseAi.m_CachedTransform.forward.y < 0f)
                {
                    pitchValue = -pitchValue;
                }
                mBaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_Pitch, pitchValue);
                Vector3 right = mBaseAi.m_CachedTransform.right;
                right.y = 0f;
                right.Normalize();;
                float rollValue = Vector3.Dot(right, mBaseAi.m_CachedTransform.right);
                if (mBaseAi.m_CachedTransform.right.y < 0f)
                {
                    rollValue = -rollValue;
                }
                BaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_Roll, rollValue);
            }
            if (mBaseAi.m_CanPlayTurn != false)
            {
                if (Time.deltaTime > 0.0001f)
                {
                    Vector3 currentForward = mBaseAi.m_CachedTransform.forward;
                    mBaseAi.m_PreviousForward = new Vector3(currentForward.x, 0f, currentForward.z).normalized;
                    mBaseAi.m_turnSpeed = Utils.GetAngleDegrees(new Vector3(currentForward.x, 0f, currentForward.z).normalized, mBaseAi.m_PreviousForward) / Time.deltaTime;
                }
                BaseAi.AnimSetFloat(mBaseAi.m_AnimParameter_TurnSpeed, mBaseAi.m_turnSpeed);
                SetTurnAngleParameters(mBaseAi.m_Animator, mBaseAi.m_TotalTurnAngle, mBaseAi.m_turnSpeed, mBaseAi.m_TurnHeading, mBaseAi.m_CachedTransform.forward, mBaseAi.m_AnimParameter_TurnAngle);
            }
            if (mBaseAi.Moose == null)
            {
                mBaseAi.Moose?.m_Animator.SetBoolID(mBaseAi.m_AnimParameter_IsInjured, mBaseAi.m_Wounded);
            }
            return;
        }


        protected AiAnimationState GetAiAnimationState(AiMode mode)
        {
            if (GetAiAnimationStateCustom(mode, out AiAnimationState overrideState))
            {
                return overrideState;
            }
            switch (mode)
            {
                case AiMode.None:
                case AiMode.Rooted:
                case AiMode.Struggle:
                case AiMode.WanderPaused:
                case AiMode.Stunned:
                case AiMode.ScratchingAntlers:
                case AiMode.JoinPack:
                case AiMode.Howl:
                case AiMode.Disabled:
                case AiMode.Dead:
                    return AiAnimationState.Paused;
                case AiMode.Attack:
                case AiMode.PassingAttack:
                    return AiAnimationState.Attack;
                case AiMode.Feeding:
                case AiMode.InvestigateFood:
                    return AiAnimationState.Feeding;
                case AiMode.Flee:
                    return AiAnimationState.Flee;
                case AiMode.Wander:
                case AiMode.FollowWaypoints:
                case AiMode.PatrolPointsOfInterest:
                    return AiAnimationState.Wander;
                case AiMode.HoldGround:
                    return AiAnimationState.HoldGround;
                case AiMode.Idle:
                    return AiAnimationState.Idle;
                case AiMode.Investigate:
                    return AiAnimationState.Investigate;
                case AiMode.InvestigateSmell:
                    return AiAnimationState.InvestigateSmell;
                case AiMode.Sleep:
                    return AiAnimationState.Sleep;
                case AiMode.Stalking:
                case AiMode.HideAndSeek:
                    return AiAnimationState.Stalking;
                case AiMode.GoToPoint:
                    return AiAnimationState.GoToPoint;
                case AiMode.InteractWithProp:
                    return AiAnimationState.InteractWithProp;
                case AiMode.ScriptedSequence:
                    return AiAnimationState.ScriptedSequence;
            }
            return AiAnimationState.Invalid;
        }


        protected bool IsMoveState(AiMode mode)
        {
            return IsMoveStateCustom(mode, out bool isMoveState) ? isMoveState : ((AiModeFlags)(1UL << (int)mode)).AnyOf(AiModeFlags.MovementAllowed);
        }


        protected void FirstFrame()
        {
            if (FirstFrameCustom())
            {
                return;
            }
            if (CurrentMode != AiMode.Dead)
            {
                mBaseAi.StickCharacterControllerToGround();
                if (mBaseAi.GetHitInfoUnderCharacterController(out RaycastHit hitInfo, FindGroundType.FirstTime))
                {
                    mBaseAi.AlignTransformWithNormal(hitInfo.point, hitInfo.normal, CurrentMode != AiMode.Dead, true);
                }
            }
            mBaseAi.DoCustomModeModifiers();
            mBaseAi.MoveAgentStop();
            mBaseAi.m_MoveAgent.m_DestinationReached = true;
            mBaseAi.m_FirstFrame = false;
        }


        protected void ProcessStartAttackHowl()
        {
            if (CurrentTarget.IsPlayer() && mBaseAi.m_AttackingLoopAudioID == 0 && mBaseAi.m_TimeInModeSeconds > 1.5)
            {
                mBaseAi.m_AttackingLoopAudioID = GameAudioManager.Play3DSound(mBaseAi.m_ChasingAudio, mBaseAi.gameObject);
            }
        }


        protected void ProcessTargetDead()
        {
            ClearTargetAndSetDefaultAiMode();
        }


        //returns true if calling method should continue, false if early out
        protected bool PickFleeDestinationAndTryStartPath()
        {
            if (mBaseAi.PickFleeDesination(out Vector3 fleePos))
            {
                if (!mBaseAi.StartPath(fleePos, mBaseAi.GetFleeSpeed()))
                {
                    SetDefaultAiMode();
                    return false;
                }
                //Log($"Picked new flee position {fleePos} which is {Vector3.Distance(fleePos, mBaseAi.m_CachedTransform.position)} away");
                mBaseAi.m_FleeToPos = fleePos;
                mBaseAi.m_PickedFleeDestination = true;
            }
            return true;
        }
    }
}
