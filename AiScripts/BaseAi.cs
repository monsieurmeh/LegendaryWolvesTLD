//#define DEV_BUILD_STATELABEL
#define DEV_BUILD_TYPELABEL

using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Runtime;
using Il2CppTLD.AI;
using UnityEngine;
using static Il2Cpp.BaseAi;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class CustomAiBase : ICustomAi
    {
        private static Dictionary<AiMode, Action<CustomAiBase>> ExitAiModeDictionary = new Dictionary<AiMode, Action<CustomAiBase>>()
        {
            {AiMode.None,                   (ai) => { }                                     },
            {AiMode.Attack,                 (ai) => ai.ExitAttack()                         },
            {AiMode.Dead,                   (ai) => ai.ExitDead()                           },
            {AiMode.Feeding,                (ai) => ai.ExitFeeding()                        },
            {AiMode.Flee,                   (ai) => ai.ExitFlee()                           },
            {AiMode.FollowWaypoints,        (ai) => ai.ExitFollowWaypoints()                },
            {AiMode.HoldGround,             (ai) => ai.ExitHoldGround()                     },
            {AiMode.Idle,                   (ai) => ai.ExitIdle()                           },
            {AiMode.Investigate,            (ai) => ai.ExitInvestigate()                    },
            {AiMode.InvestigateFood,        (ai) => ai.ExitInvestigateFood()                },
            {AiMode.InvestigateSmell,       (ai) => ai.ExitInvestigateSmell()               },
            {AiMode.Rooted,                 (ai) => ai.ExitRooted()                         },
            {AiMode.Sleep,                  (ai) => ai.ExitSleep()                          },
            {AiMode.Stalking,               (ai) => ai.ExitStalking()                       },
            {AiMode.Struggle,               (ai) => ai.ExitStruggle()                       },
            {AiMode.Wander,                 (ai) => ai.ExitWander()                         },
            {AiMode.WanderPaused,           (ai) => ai.ExitWanderPaused()                   },
            {AiMode.GoToPoint,              (ai) => ai.ExitGoToPoint()                      },
            {AiMode.InteractWithProp,       (ai) => ai.ExitInteractWithProp()               },
            {AiMode.ScriptedSequence,       (ai) => ai.ExitScriptedSequence()               },
            {AiMode.Stunned,                (ai) => ai.ExitStunned()                        },
            {AiMode.ScratchingAntlers,      (ai) => ai.ExitScratchingAntlers()              },
            {AiMode.PatrolPointsOfInterest, (ai) => ai.ExitPatrolPointsOfInterest()         },
            {AiMode.HideAndSeek,            (ai) => ai.ExitHideAndSeek()                    },
            {AiMode.JoinPack,               (ai) => ai.ExitJoinPack()                       },
            {AiMode.PassingAttack,          (ai) => ai.ExitPassingAttack()                  },
            {AiMode.Howl,                   (ai) => ai.ExitHowl()                           },
            {AiMode.Disabled,               (ai) => { }                                     },
            {(AiMode)NewAiModes.Hiding,     (ai) => (ai as IHideBehaviorOwner)?.ExitHiding()},
            {(AiMode)NewAiModes.Returning,  (ai) => (ai as IReturningBehaviorOwner)?.ExitReturning()}
        };
        private static Dictionary<AiMode, Action<CustomAiBase>> EnterAiModeDictionary = new Dictionary<AiMode, Action<CustomAiBase>>()
        {
            {AiMode.None,                   (ai) => { }                                     },
            {AiMode.Attack,                 (ai) => ai.EnterAttack()                         },
            {AiMode.Dead,                   (ai) => ai.EnterDead()                           },
            {AiMode.Feeding,                (ai) => ai.EnterFeeding()                        },
            {AiMode.Flee,                   (ai) => ai.EnterFlee()                           },
            {AiMode.FollowWaypoints,        (ai) => ai.EnterFollowWaypoints()                },
            {AiMode.HoldGround,             (ai) => ai.EnterHoldGround()                     },
            {AiMode.Idle,                   (ai) => ai.EnterIdle()                           },
            {AiMode.Investigate,            (ai) => ai.EnterInvestigate()                    },
            {AiMode.InvestigateFood,        (ai) => ai.EnterInvestigateFood()                },
            {AiMode.InvestigateSmell,       (ai) => ai.EnterInvestigateSmell()               },
            {AiMode.Rooted,                 (ai) => ai.EnterRooted()                         },
            {AiMode.Sleep,                  (ai) => ai.EnterSleep()                          },
            {AiMode.Stalking,               (ai) => ai.EnterStalking()                       },
            {AiMode.Struggle,               (ai) => ai.EnterStruggle()                       },
            {AiMode.Wander,                 (ai) => ai.EnterWander()                         },
            {AiMode.WanderPaused,           (ai) => ai.EnterWanderPaused()                   },
            {AiMode.GoToPoint,              (ai) => ai.EnterGoToPoint()                      },
            {AiMode.InteractWithProp,       (ai) => ai.EnterInteractWithProp()               },
            {AiMode.ScriptedSequence,       (ai) => ai.EnterScriptedSequence()               },
            {AiMode.Stunned,                (ai) => ai.EnterStunned()                        },
            {AiMode.ScratchingAntlers,      (ai) => ai.EnterScratchingAntlers()              },
            {AiMode.PatrolPointsOfInterest, (ai) => ai.EnterPatrolPointsOfInterest()         },
            {AiMode.HideAndSeek,            (ai) => ai.EnterHideAndSeek()                    },
            {AiMode.JoinPack,               (ai) => ai.EnterJoinPack()                       },
            {AiMode.PassingAttack,          (ai) => ai.EnterPassingAttack()                  },
            {AiMode.Howl,                   (ai) => ai.EnterHowl()                           },
            {AiMode.Disabled,               (ai) => { }                                      },
            {(AiMode)NewAiModes.Hiding,     (ai) => (ai as IHideBehaviorOwner)?.EnterHiding()},
            {(AiMode)NewAiModes.Returning,  (ai) => (ai as IReturningBehaviorOwner)?.EnterReturning()}
        }; 
        private static Dictionary<AiMode, Action<CustomAiBase>> ProcessAiModeDictionary = new Dictionary<AiMode, Action<CustomAiBase>>()
        {
            {AiMode.None,                   (ai) => { }                                     },
            {AiMode.Attack,                 (ai) => ai.ProcessAttack()                         },
            {AiMode.Dead,                   (ai) => ai.ProcessDead()                           },
            {AiMode.Feeding,                (ai) => ai.ProcessFeeding()                        },
            {AiMode.Flee,                   (ai) => ai.ProcessFlee()                           },
            {AiMode.FollowWaypoints,        (ai) => ai.ProcessFollowWaypoints()                },
            {AiMode.HoldGround,             (ai) => ai.ProcessHoldGround()                     },
            {AiMode.Idle,                   (ai) => ai.ProcessIdle()                           },
            {AiMode.Investigate,            (ai) => ai.ProcessInvestigate()                    },
            {AiMode.InvestigateFood,        (ai) => ai.ProcessInvestigateFood()                },
            {AiMode.InvestigateSmell,       (ai) => ai.ProcessInvestigateSmell()               },
            {AiMode.Rooted,                 (ai) => ai.ProcessRooted()                         },
            {AiMode.Sleep,                  (ai) => ai.ProcessSleep()                          },
            {AiMode.Stalking,               (ai) => ai.ProcessStalking()                       },
            {AiMode.Struggle,               (ai) => ai.ProcessStruggle()                       },
            {AiMode.Wander,                 (ai) => ai.ProcessWander()                         },
            {AiMode.WanderPaused,           (ai) => ai.ProcessWanderPaused()                   },
            {AiMode.GoToPoint,              (ai) => ai.ProcessGoToPoint()                      },
            {AiMode.InteractWithProp,       (ai) => ai.ProcessInteractWithProp()               },
            {AiMode.ScriptedSequence,       (ai) => ai.ProcessScriptedSequence()               },
            {AiMode.Stunned,                (ai) => ai.ProcessStunned()                        },
            {AiMode.ScratchingAntlers,      (ai) => ai.ProcessScratchingAntlers()              },
            {AiMode.PatrolPointsOfInterest, (ai) => ai.ProcessPatrolPointsOfInterest()         },
            {AiMode.HideAndSeek,            (ai) => ai.ProcessHideAndSeek()                    },
            {AiMode.JoinPack,               (ai) => ai.ProcessJoinPack()                       },
            {AiMode.PassingAttack,          (ai) => ai.ProcessPassingAttack()                  },
            {AiMode.Howl,                   (ai) => ai.ProcessHowl()                           },
            {AiMode.Disabled,               (ai) => { }                                      },
            {(AiMode)NewAiModes.Hiding,     (ai) => (ai as IHideBehaviorOwner)?.ProcessHiding()},
            {(AiMode)NewAiModes.Returning,  (ai) => (ai as IReturningBehaviorOwner)?.ProcessReturning()}
        };
        private static Dictionary<AiMode, AiAnimationState> SetAiAnimationStateDictionary = new Dictionary<AiMode, AiAnimationState>()
        {
            {AiMode.None,                   AiAnimationState.Invalid            },
            {AiMode.Attack,                 AiAnimationState.Attack             },
            {AiMode.Dead,                   AiAnimationState.Paused             },
            {AiMode.Feeding,                AiAnimationState.Feeding            },
            {AiMode.Flee,                   AiAnimationState.Flee               },
            {AiMode.FollowWaypoints,        AiAnimationState.Wander             },
            {AiMode.HoldGround,             AiAnimationState.HoldGround         },
            {AiMode.Idle,                   AiAnimationState.Idle               },
            {AiMode.Investigate,            AiAnimationState.Investigate        },
            {AiMode.InvestigateFood,        AiAnimationState.Feeding            },
            {AiMode.InvestigateSmell,       AiAnimationState.InvestigateSmell   },
            {AiMode.Rooted,                 AiAnimationState.Paused             },
            {AiMode.Sleep,                  AiAnimationState.Sleep              },
            {AiMode.Stalking,               AiAnimationState.Stalking           },
            {AiMode.Struggle,               AiAnimationState.Paused             },
            {AiMode.Wander,                 AiAnimationState.Wander             },
            {AiMode.WanderPaused,           AiAnimationState.Paused             },
            {AiMode.GoToPoint,              AiAnimationState.GoToPoint          },
            {AiMode.InteractWithProp,       AiAnimationState.InteractWithProp   },
            {AiMode.ScriptedSequence,       AiAnimationState.ScriptedSequence   },
            {AiMode.Stunned,                AiAnimationState.Paused             },
            {AiMode.ScratchingAntlers,      AiAnimationState.Paused             },
            {AiMode.PatrolPointsOfInterest, AiAnimationState.Wander             },
            {AiMode.HideAndSeek,            AiAnimationState.Stalking           },
            {AiMode.JoinPack,               AiAnimationState.Paused             },
            {AiMode.PassingAttack,          AiAnimationState.Attack             },
            {AiMode.Howl,                   AiAnimationState.Paused             },
            {AiMode.Disabled,               AiAnimationState.Paused             },
            {(AiMode)NewAiModes.Hiding,     AiAnimationState.Paused             },
            {(AiMode)NewAiModes.Returning,  AiAnimationState.Wander          }
        };


        public CustomAiBase(BaseAi baseAi)
        {
            mBaseAi = baseAi;
            mTimeOfDay = GameManager.m_TimeOfDay;
        }

        protected float mTimeSinceCheckForTargetInPatrolWaypointsMode = 0.0f;


        #region ICustomAi

        protected BaseAi mBaseAi;
        protected TimeOfDay mTimeOfDay;
        protected bool mSetAiModeLock = false;
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
            mBaseAi.m_TimeInModeSeconds += Time.deltaTime;
            mBaseAi.m_TimeInModeTODHours = (24.0f / (mTimeOfDay.m_WeatherSystem.m_DayLengthScale * mTimeOfDay.m_WeatherSystem.m_DayLength)) * Time.deltaTime + mBaseAi.m_TimeInModeTODHours;

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
                mBaseAi.UpdateWounds(Time.deltaTime);
                mBaseAi.m_SuppressFootStepDetectionAndSmellSecondsRemaining -= Time.deltaTime;
                GameAudioManager.SetAudioSourceTransform(mBaseAi.m_EmitterProxy, mBaseAi.m_CachedTransform);
            }
        }


        protected void Process()
        {
            ProcessAiModeDictionary[CurrentMode].Invoke(this);
        }


        protected virtual void PostProcess()
        {
            if (CurrentMode == AiMode.Dead || CurrentMode == AiMode.ScriptedSequence)
            {
                return;
            }
            if (IsImposter())
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


        #region Helpers

        public static float SquaredDistance(Vector3 a, Vector3 b)
        {
            return ((a.x - b.x) * (a.x - b.x)) + ((a.y - b.y) * (a.y - b.y)) + ((a.z - b.z) * (a.z - b.z));
        }


        protected AiTarget CurrentTarget { get { return mBaseAi.m_CurrentTarget; } }
        protected AiMode CurrentMode { get { return mBaseAi.m_CurrentMode; } set { mBaseAi.m_CurrentMode = value; } }
        protected AiModeFlags CurrentModeFlag { get { return (AiModeFlags)(1U << (int)CurrentMode); } }
        protected AiMode PreviousMode { get { return mBaseAi.m_PreviousMode; } set { mBaseAi.m_PreviousMode = value; } }
        protected string Name { get { return mBaseAi.gameObject?.name ?? "NULL"; } }


        protected virtual void ClearTargetAndSetDefaultAiMode()
        {
            mBaseAi.ClearTarget();
            SetDefaultAiMode();
            return;
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


        #region Overrides

        #region Properties

        //In theory, these should still be affected by other mods which primarily tweak existing fields at awake
        // In the future if we decide to create new mod-specific default values, we'll need to cache the "official" hinterland version and do our own on start (not awake to ensure that other mods get to it first) check to see if props match HL defaults, and ignore them if not because someone else got to them

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


        #region SetAiMode

        //Eventually a lot of these can be moved into more appropriate subclasses, like AI for stags and AI for timberwolves specifically
        protected virtual AiMode PreprocessNewAiMode(AiMode mode)
        {
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


        public virtual void SetAiMode(AiMode mode)
        {
            mode = PreprocessNewAiMode(mode);
            if (mode == AiMode.None)
            {
                return;
            }
            ExitAiModeDictionary[CurrentMode].Invoke(this);
            EnterAiModeDictionary[mode].Invoke(this);
            PreviousMode = CurrentMode;
            CurrentMode = mode;
            mBaseAi.m_TimeInModeSeconds = 0.0f;
            mBaseAi.m_TimeInModeTODHours = 0.0f;
            GameAudioManager.SetAiStateSwitch(CurrentMode, GameAudioManager.GetSoundEmitterProxyFromGameObject(mBaseAi.gameObject));
        }


        #endregion


        #region Process

        protected virtual void ProcessAttack()
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

         
        protected virtual void ProcessDead()
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


        protected virtual void ProcessFeeding()
        {
            mBaseAi.ProcessFeeding();
        }


        protected virtual void ProcessFlee()
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


        //Using custom logic for my AI processes, HL seems to be favoring PatrolPointsOfInterest over this mode these days
        protected virtual void ProcessFollowWaypoints()
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


        protected virtual void ProcessHoldGround()
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


        protected virtual void ProcessIdle()
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


        protected virtual void ProcessWanderPaused()
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


        protected virtual void ProcessGoToPoint()
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


        #region MaybeHoldGround


        protected virtual void MaybeHoldGround()
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


        protected virtual bool MaybeHoldGroundForAttack(HoldGroundReason reason, Func<float, bool> shouldHoldGroundFunc)
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


        #region ExitState

        protected virtual void ExitAttack()
        {
            mBaseAi.ExitAttack();
        }

        protected virtual void ExitDead()
        {
            mBaseAi.ExitDead();
        }

        protected virtual void ExitFeeding()
        {
            mBaseAi.ExitFeeding();
        }

        protected virtual void ExitFlee()
        {
            mBaseAi.ExitFlee();
        }

        protected virtual void ExitFollowWaypoints()
        {
            mBaseAi.ExitFollowWaypoints();
        }

        protected virtual void ExitHoldGround()
        {
            mBaseAi.ExitHoldGround();
        }

        protected virtual void ExitIdle()
        {
            mBaseAi.ExitIdle();
        }

        protected virtual void ExitInvestigate()
        {
            mBaseAi.ExitInvestigate();
        }

        protected virtual void ExitInvestigateFood()
        {
            mBaseAi.ExitInvestigateFood();
        }

        protected virtual void ExitInvestigateSmell()
        {
            mBaseAi.ExitInvestigateSmell();
        }

        protected virtual void ExitRooted()
        {

        }

        protected virtual void ExitSleep()
        {
            mBaseAi.ExitSleep();
        }

        protected virtual void ExitStalking()
        {
            mBaseAi.ExitStalking();
        }

        protected virtual void ExitStruggle()
        {
            mBaseAi.ExitStruggle();
        }

        protected virtual void ExitWander()
        {
            mBaseAi.ExitWander();
        }

        protected virtual void ExitWanderPaused()
        {
            mBaseAi.ExitWanderPaused();
        }

        protected virtual void ExitGoToPoint()
        {
            mBaseAi.ExitGoToPoint();
        }

        protected virtual void ExitInteractWithProp()
        {
            mBaseAi.ExitInteractWithProp();
        }

        protected virtual void ExitScriptedSequence()
        {
            mBaseAi.ExitScriptedSequence();
        }

        protected virtual void ExitStunned()
        {
            mBaseAi.ExitStunned();
        }

        protected virtual void ExitScratchingAntlers()
        {
            mBaseAi.Moose?.ExitScratchingAntlers();
        }

        protected virtual void ExitPatrolPointsOfInterest()
        {
            mBaseAi.ExitPatrolPointsOfInterest();
        }

        protected virtual void ExitHideAndSeek()
        {
            mBaseAi.Timberwolf?.ExitHideAndSeek();
        }

        protected virtual void ExitJoinPack()
        {
            mBaseAi.Timberwolf?.ExitJoinPack();
        }

        protected virtual void ExitPassingAttack()
        {
            //Nothing? Not sure...
            //mBaseAi.Timberwolf?.Exit();
        }

        protected virtual void ExitHowl()
        {
            mBaseAi.BaseWolf?.ExitHowl();
        }

        #endregion


        #region EnterState

        protected virtual void EnterAttack()
        {
            mBaseAi.EnterAttack();
        }

        protected virtual void EnterDead()
        {
            mBaseAi.EnterDead();
        }

        protected virtual void EnterFeeding()
        {
            mBaseAi.EnterFeeding();
        }

        protected virtual void EnterFlee()
        {
            mBaseAi.EnterFlee();
        }

        protected virtual void EnterFollowWaypoints()
        {
            mBaseAi.EnterFollowWaypoints();
        }

        protected virtual void EnterHoldGround()
        {
            mBaseAi.EnterHoldGround();
        }

        protected virtual void EnterIdle()
        {
            mBaseAi.EnterIdle();
        }

        protected virtual void EnterInvestigate()
        {
            mBaseAi.EnterInvestigate();
        }

        protected virtual void EnterInvestigateFood()
        {
            mBaseAi.EnterInvestigateFood();
        }

        protected virtual void EnterInvestigateSmell()
        {
            mBaseAi.EnterInvestigateSmell();
        }

        protected virtual void EnterRooted()
        {
            mBaseAi.MoveAgentStop();
        }

        protected virtual void EnterSleep()
        {
            mBaseAi.EnterSleep();
        }

        protected virtual void EnterStalking()
        {
            mBaseAi.EnterStalking();
        }

        protected virtual void EnterStruggle()
        {
            mBaseAi.EnterStruggle();
        }

        protected virtual void EnterWander()
        {
            mBaseAi.EnterWander();
        }

        protected virtual void EnterWanderPaused()
        {
            mBaseAi.EnterWanderPaused();
        }

        protected virtual void EnterGoToPoint()
        {
            mBaseAi.EnterGoToPoint();
        }

        protected virtual void EnterInteractWithProp()
        {
            mBaseAi.EnterInteractWithProp();
        }

        protected virtual void EnterScriptedSequence()
        {
            mBaseAi.EnterScriptedSequence();
        }

        protected virtual void EnterStunned()
        {
            mBaseAi.EnterStunned();
        }

        protected virtual void EnterScratchingAntlers()
        {
            mBaseAi.Moose?.EnterScratchingAntlers();
        }

        protected virtual void EnterPatrolPointsOfInterest()
        {
            mBaseAi.EnterPatrolPointsOfInterest();
        }

        protected virtual void EnterHideAndSeek()
        {
            mBaseAi.Timberwolf?.EnterHideAndSeek();
        }

        protected virtual void EnterJoinPack()
        {
            mBaseAi.Timberwolf?.EnterJoinPack();
        }

        protected virtual void EnterPassingAttack()
        {
            mBaseAi.Timberwolf?.EnterPassingAttack();
        }

        protected virtual void EnterHowl()
        {
            mBaseAi.BaseWolf?.EnterHowl();
        }

        #endregion


        protected virtual void SetAnimationParameters()
        {
            float someFloat = mBaseAi.m_AiGoalSpeed;
            if (mBaseAi.BaseWolf?.m_Wounded ?? false)
            {
                someFloat = (someFloat - mBaseAi.m_WalkSpeed) * Mathf.Clamp01(1 - mBaseAi.GetWoundedAnimParameter()) + mBaseAi.m_WalkSpeed;
            }
            mBaseAi.m_MoveAgent.SetMoveSpeed(someFloat);
            mBaseAi.SetAnimStateForMoveAgent(CurrentModeFlag.AnyOf(AiModeFlags.MovementAllowed) ? MoveState.Moving : MoveState.Idle, (int)SetAiAnimationStateDictionary[CurrentMode]);
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





        protected virtual void MaybeImposter()
        {
            mBaseAi.MaybeImposter();
        }


        protected virtual bool IsImposter()
        {
            return mBaseAi.IsImposter();
        }


        protected virtual void FirstFrame()
        {
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


        //returns true if calling method should continue, false if early out
        protected virtual bool PickFleeDestinationAndTryStartPath()
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


        #endregion
    }
}
