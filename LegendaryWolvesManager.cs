#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using UnityEngine;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class LegendaryWolvesManager
    {
        #region Consts & Enums

        const float MillisecondsPerTick = 0.0001f;
        const float SecondsPerTick = MillisecondsPerTick * 0.001f;
        const long TicksPerUpdate = 10000000;
        const string Null = "null";

        #endregion


        #region Lazy Singleton

        private class Nested
        {
            static Nested()
            {
            }

            internal static readonly LegendaryWolvesManager instance = new LegendaryWolvesManager();
        }

        private LegendaryWolvesManager() { }
        public static LegendaryWolvesManager Instance { get { return Nested.instance; } }

        #endregion


        private struct AugmentDetails
        {
            
        }


        private Action<string> mLogMessageAction;
        private Action<string> mLogErrorAction;
        private Settings mSettings;
        private bool mInitialized = false;
        private bool mEnabled = false;
        private long mStartTime = System.DateTime.Now.Ticks;
        private long mLastReadoutTime = System.DateTime.Now.Ticks;

        private Dictionary<int, BaseAi> mAugmentList;
        private Dictionary<int, AugmentDetails> mAugmentDetails;

        private long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }
        private long TicksSinceLastReadout { get { return System.DateTime.Now.Ticks - mLastReadoutTime; } }

        public Dictionary<int, BaseAi> AugmentList { get { return mAugmentList; } }

        public bool Initialize(Settings settings, Action<string> logMessageAction, Action<string> logErrorAction)
        {
            if (mInitialized)
            {
                return false;
            }
            mInitialized = true;
            mStartTime = System.DateTime.Now.Ticks;
            mSettings = settings;
            mAugmentList = new Dictionary<int, BaseAi>();
            mAugmentDetails = new Dictionary<int, AugmentDetails>();
            mLogMessageAction = logMessageAction;
            mLogErrorAction = logErrorAction;
            return true;
        }


        public bool Shutdown()
        {
            if (!mInitialized)
            {
                return false;
            }
            ClearAugments();
            mAugmentList = null;
            mAugmentDetails = null;
            mInitialized = false;
            mSettings = null;
            mLogMessageAction = null;
            mLogErrorAction = null;
            return true;
        }


        public void Update()
        {
            if (TicksSinceLastReadout >= TicksPerUpdate)
            {
                mLastReadoutTime = System.DateTime.Now.Ticks;
                foreach (BaseAi baseAi in mAugmentList.Values)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is in augment list");
#endif
                }
            }
        }


        public void ClearAugments()
        {
            foreach (BaseAi baseAi in mAugmentList.Values)
            {
                TryUnaugmentWolf(baseAi);
            }
            mAugmentList.Clear();
            mAugmentDetails.Clear();
        }


        public bool TryAugmentWolf(BaseAi baseAi, float augmentValue)
        {
#if DEV_BUILD
            try
            {
#endif
                if (baseAi == null)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log("null BaseAi, aborting TryAugmentWolf");
#endif
                    return false;
                }
                Vector3 newScale = new Vector3(1, 1, 1);
                if (baseAi.m_AiType != AiType.Predator)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is not a predator, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (baseAi.m_AiSubType != AiSubType.Wolf)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is not a wolf, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (mAugmentList.ContainsKey(baseAi.GetHashCode()))
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is already in list, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                AugmentWolf(baseAi, augmentValue);
                return true;
 #if DEV_BUILD           
            }
            catch (Exception e)
            {
                LogError($"Error while trying to augment wolf ai: {e}");
                return false;
            }
#endif
        }

        
        private void AugmentWolf(BaseAi baseAi, float augmentValue)
        {
            augmentValue = Mathf.Clamp(augmentValue, 1, 10);
#if DEV_BUILD_LOG_VERBOSE
            Log($"Watch out, augmenting {BaseAiInfo(baseAi)}!");// size/speed by factor of {augmentValue}!");
#endif
            mAugmentList.Add(baseAi.GetHashCode(), baseAi);
            //baseAi.m_RunSpeed *= augmentValue;
            //baseAi.m_StalkSpeed *= augmentValue;
            //baseAi.m_WalkSpeed *= augmentValue;
            //baseAi.m_turnSpeed *= augmentValue;
            //baseAi.m_TurnSpeedDegreesPerSecond *= augmentValue;
            //Vector3 newScale = new Vector3(augmentValue, augmentValue, augmentValue);
            //baseAi.gameObject.transform.set_localScale_Injected(ref newScale);
        }


        public bool TryUnaugmentWolf(BaseAi baseAI)
        {
#if DEV_BUILD
            try
            {
#endif
                if (baseAI == null)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log($"Null base Ai, aborting TryUnaugmentWolf");
#endif
                    return false;
                }
                if (!mAugmentList.ContainsKey(baseAI.GetHashCode()))
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAI, $" is not contained in list, aborting tryUnaugmentWolf");
#endif
                    return false;
                }
#if DEV_BUILD_LOG_VERBOSE
                Log($"Previously augmented AI found on {BaseAiInfo(baseAI)}, un-augmenting...");
#endif
                UnaugmentWolf(baseAI);
                return true;
#if DEV_BUILD
            }
            catch (Exception e)
            {
                LogError($"Error while trying to augment wolf ai: {e}");
                return false;
            }
#endif
        }



        private void UnaugmentWolf(BaseAi baseAI)
        {
            mAugmentList.Remove(baseAI.GetHashCode());
            //Vector3 newScale = new Vector3(1, 1, 1);
            //baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }

        
        //Only familiar with pre/post fixes right now, might not have to replicate all of this with another patch methodology?
        public void ProcessCurrentAiMode(BaseAi baseAi)
        {
#if DEV_BUILD
            try
            {
#endif
                baseAi.ProcessCommonPre();
#if DEV_BUILD_LOG_VERBOSE
                Log($"Processing current ai mode {baseAi.m_CurrentMode} of {BaseAiInfo(baseAi)}...");
#endif
                switch (baseAi?.m_CurrentMode ?? AiMode.None)
                {
                    case AiMode.Attack:
#if DEV_BUILD_LOG
                        Log("Flee, scaredy wolf!");
#endif
                        baseAi?.ProcessFlee(); //Scaredy wolf!
                                              //baseAi.ProcessAttack();
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
#if DEV_BUILD_LOG
                        Log("Flee, scaredy wolf!");
#endif
                        baseAi.ProcessFlee(); //Scaredy wolf!
                                              //baseAi.ProcessStruggle();
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
                baseAi.ProcessCommonPost();
#if DEV_BUILD
            }
            catch (Exception e)
            {
                LogError($"Error while trying to process wolf ai: {e}");
                return;
            }
#endif
        }


        public void Log(string message, bool error = false)
        {
            string logMessage = $"[{TicksSinceStart}t/{TicksSinceStart * MillisecondsPerTick}ms/{TicksSinceStart * SecondsPerTick}s] {message}";
            if (error)
            {
                mLogErrorAction.Invoke(logMessage);
            }
            else
            {
                mLogMessageAction.Invoke(logMessage);
            }
        }

        
        public void LogError(string message)
        {
            Log(message, true);
        }


        public void Log(BaseAi baseAi, string msg, bool error = false)
        {
            Log($"{BaseAiInfo(baseAi)} {msg}", error);
        }


        public void LogError(BaseAi baseAi, string msg)
        {
            Log(baseAi, msg, true);
        }


        public static string BaseAiInfo(BaseAi baseAi)
        {
            return $"{baseAi?.gameObject?.name ?? Null} ({baseAi?.GetType()}) [{baseAi?.GetHashCode()}] at {baseAi?.gameObject?.transform?.position ?? Vector3.zero}";
        }
    }
}