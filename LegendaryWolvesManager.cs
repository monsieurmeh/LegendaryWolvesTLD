#define DEV_BUILD

using Il2Cpp;
using UnityEngine;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class LegendaryWolvesManager
    {
        #region Consts & Enums

        const float MillisecondsPerTick = 1 / 10000;
        const float SecondsPerTick = MillisecondsPerTick / 1000;
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
        private Settings mSettings;
        private bool mInitialized = false;
        private bool mEnabled = false;
        private long mStartTime = System.DateTime.Now.Ticks;
        private long mLastReadoutTime = System.DateTime.Now.Ticks;

        private HashSet<BaseAi> mAugmentedAiList;
        private Dictionary<BaseAi, AugmentDetails> mAugmentDetails;

        private long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }

        public HashSet<BaseAi> AugmentedAiList { get { return mAugmentedAiList; } }

        public bool Initialize(Settings settings, Action<string> logMessageAction)
        {
            if (mInitialized)
            {
                return false;
            }
            mInitialized = true;
            mStartTime = System.DateTime.Now.Ticks;
            mSettings = settings;
            mAugmentedAiList = new HashSet<BaseAi>();
            mAugmentDetails = new Dictionary<BaseAi, AugmentDetails>();
            return true;
            mLogMessageAction = logMessageAction;
        }


        public bool Shutdown()
        {
            if (!mInitialized)
            {
                return false;
            }
            ClearAugments();
            mAugmentedAiList = null;
            mAugmentDetails = null;
            mInitialized = false;
            mSettings = null;
            mLogMessageAction = null;
            return true;
        }


        public void ClearAugments()
        {
            foreach (BaseAi baseAi in mAugmentedAiList)
            {
                TryUnaugmentWolf(baseAi);
            }
            mAugmentedAiList.Clear();
            mAugmentDetails.Clear();
        }


        public bool TryAugmentWolf(BaseAi baseAi, float augmentValue)
        {
            try
            {
                if (baseAi == null)
                {
#if DEV_BUILD
                    Log("null BaseAi, aborting TryAugmentWolf");
#endif
                    return false;
                }
                Vector3 newScale = new Vector3(1, 1, 1);
                if (baseAi.m_AiType != AiType.Predator)
                {
#if DEV_BUILD
                    Log(baseAi, " is not a predator, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (baseAi.m_AiSubType != AiSubType.Wolf)
                {
#if DEV_BUILD
                    Log(baseAi, " is not a wolf, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (mAugmentedAiList.Contains(baseAi))
                {
#if DEV_BUILD
                    Log(baseAi, " is already in list, aborting TryAugmentWolf");
#endif
                    baseAi.gameObject?.transform?.set_localScale_Injected(ref newScale);
                    return false;
                }
                AugmentWolf(baseAi, augmentValue);
                return true;
            }
            catch (Exception e)
            {
                Log($"Error while trying to augment wolf ai: {e}");
                return false;
            }
        }

        
        private void AugmentWolf(BaseAi baseAI, float augmentValue)
        {
            augmentValue = Mathf.Clamp(augmentValue, 1, 10);
#if DEV_BUILD
            Log($"Watch out, augmenting {BaseAiInfo(baseAI)}!");// size/speed by factor of {augmentValue}!");
#endif
            mAugmentedAiList.Add(baseAI);
            //baseAI.m_RunSpeed *= augmentValue;
            //baseAI.m_StalkSpeed *= augmentValue;
            //baseAI.m_WalkSpeed *= augmentValue;
            //baseAI.m_turnSpeed *= augmentValue;
            //baseAI.m_TurnSpeedDegreesPerSecond *= augmentValue;
            //Vector3 newScale = new Vector3(augmentValue, augmentValue, augmentValue);
            //baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }


        public bool TryUnaugmentWolf(BaseAi baseAI)
        {
            try
            {
                if (baseAI == null)
                {
#if DEV_BUILD
                    Log($"Null base Ai, aborting TryUnaugmentWolf");
#endif
                    return false;
                }
                if (!mAugmentedAiList.Contains(baseAI))
                {
#if DEV_BUILD
                    Log(baseAI, $" is not contained in list, aborting tryUnaugmentWolf");
#endif
                    return false;
                }
#if DEV_BUILD
                Log($"Previously augmented AI found on {BaseAiInfo(baseAI)}, un-augmenting...");
#endif
                UnaugmentWolf(baseAI);
                return true;
            }
            catch (Exception e)
            {
                Log($"Error while trying to augment wolf ai: {e}");
                return false;
            }
        }
    


        private void UnaugmentWolf(BaseAi baseAI)
        {
            mAugmentedAiList.Remove(baseAI);
            //Vector3 newScale = new Vector3(1, 1, 1);
            //baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }

        
        //Only familiar with pre/post fixes right now, might not have to replicate all of this with another patch methodology?
        public void ProcessCurrentAiMode(BaseAi baseAi)
        {
            try
            {
                baseAi.ProcessCommonPre();
#if DEV_BUILD
                Log($"Processing current ai mode {baseAi.m_CurrentMode} of {BaseAiInfo(baseAi)}...");
#endif
                switch (baseAi?.m_CurrentMode ?? AiMode.None)
                {
                    case AiMode.Attack:
#if DEV_BUILD
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
#if DEV_BUILD
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
            }
            catch (Exception e)
            {
                Log($"Error while trying to process wolf ai: {e}");
                return;
            }
        }


        public void Log(string message)
        {
            mLogMessageAction.Invoke($"[{TicksSinceStart}t/{TicksSinceStart * MillisecondsPerTick}ms/{TicksSinceStart * SecondsPerTick}s] {message}");
        }


        public void Log(GameObject go, string msg)
        {
            Log($"{GameObjectInfo(go)} {msg}");
        }


        public void Log(BaseAi baseAi, string msg)
        {
            Log($"{BaseAiInfo(baseAi)} {msg}");
        }


        public static string GameObjectInfo(GameObject go)
        {
            return $"{go?.name ?? Null} [{go?.GetHashCode()}]";
        }


        public static string BaseAiInfo(BaseAi baseAi)
        {
            return $"{baseAi?.gameObject?.name ?? Null} ({baseAi?.GetType()}) [{baseAi?.GetHashCode()}]";
        }
    }
}