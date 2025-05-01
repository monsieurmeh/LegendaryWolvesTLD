using Il2Cpp;
using UnityEngine;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class LegendaryWolvesManager
    {
        #region Consts & Enums

        const float MillisecondsPerTick = 1 / 10000;
        const float SecondsPerTick = MillisecondsPerTick / 1000;

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
            mLogMessageAction = logMessageAction;
            mAugmentedAiList = new HashSet<BaseAi>();
            mAugmentDetails = new Dictionary<BaseAi, AugmentDetails>();
            return true;
        }


        public bool Shutdown()
        {
            if (!mInitialized)
            {
                return false;
            }
            UnaugmentAll();
            mAugmentedAiList = null;
            mAugmentDetails = null;
            mInitialized = false;
            mSettings = null;
            mLogMessageAction = null;
            return true;
        }


        public void UnaugmentAll()
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
                Vector3 newScale = new Vector3(1, 1, 1);
                if (baseAi.m_AiType != AiType.Predator)
                {
                    Log(baseAi, " is not a predator, aborting TryAugmentWolf");
                    baseAi.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (baseAi.m_AiSubType != AiSubType.Wolf)
                {
                    Log(baseAi, " is not a wolf, aborting TryAugmentWolf");
                    baseAi.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (mAugmentedAiList.Contains(baseAi))
                {
                    Log(baseAi, " is already in list, aborting TryAugmentWolf");
                    baseAi.gameObject.transform.set_localScale_Injected(ref newScale);
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

        
        protected void AugmentWolf(BaseAi baseAI, float augmentValue)
        {
            augmentValue = Mathf.Clamp(augmentValue, 1, 10);
            Log($"Watch out, augmenting {BaseAiInfo(baseAI)}!");// size/speed by factor of {augmentValue}!");
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
                    Log($"Null base Ai, aborting TryUnaugmentWolf");
                    return false;
                }
                if (!mAugmentedAiList.Contains(baseAI))
                {
                    Log(baseAI, $" is not contained in list, aborting tryUnaugmentWolf");
                    return false;
                }
                Log($"Previously augmented AI found on {BaseAiInfo(baseAI)}, un-augmenting...");
                UnaugmentWolf(baseAI);
                return true;
            }
            catch (Exception e)
            {
                Log($"Error while trying to augment wolf ai: {e}");
                return false;
            }
        }
    


        protected void UnaugmentWolf(BaseAi baseAI)
        {
            mAugmentedAiList.Remove(baseAI);
            //Vector3 newScale = new Vector3(1, 1, 1);
            //baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }

        
        //Only familiar with pre/post fixes right now, might not have to replicate all of this with another patch methodology?
        public void ProcessCurrentAiMode(BaseAi baseAi)
        {
            baseAi.ProcessCommonPre();
            Log($"Processing current ai mode {baseAi.m_CurrentMode} of {BaseAiInfo(baseAi)}...");
            switch (baseAi.m_CurrentMode)
            {
                case AiMode.Attack:
                    Log("Flee, scaredy wolf!");
                    baseAi.ProcessFlee(); //Scaredy wolf!
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
                    Log("Flee, scaredy wolf!");
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


        public string GameObjectInfo(GameObject go)
        {
            return $"{go.name} [{go.GetHashCode()}]";
        }


        public string BaseAiInfo(BaseAi baseAi)
        {
            return $"{baseAi.gameObject.name} ({baseAi.GetType()}) [{baseAi.GetHashCode()}]";
        }
    }
}