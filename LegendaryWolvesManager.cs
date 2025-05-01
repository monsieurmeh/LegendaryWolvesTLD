using Il2Cpp;
using Il2CppNodeCanvas.Tasks.Actions;
using Il2CppTLD.AI;
using System.Buffers.Text;
using UnityEngine;
using static Il2Cpp.UIRoot;
using static MelonLoader.bHaptics;

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
            foreach (BaseAi baseAI in mAugmentedAiList)
            {
                TryUnaugmentWolf(baseAI);
            }
            mAugmentedAiList.Clear();
            mAugmentedAiList = null;
            mAugmentDetails.Clear();
            mAugmentDetails = null;
            mInitialized = false;
            mSettings = null;
            mLogMessageAction = null;
            return true;
        }


        public bool TryAugmentWolf(GameObject spawnablePrefab, float augmentValue)
        {
            try
            {
                if (!spawnablePrefab.TryGetComponent(out BaseAi baseAI))
                {
                    Log($"no base AI, aborting TryAugmentWolf");
                    return false;
                }
                Vector3 newScale = new Vector3(1, 1, 1);
                if (baseAI.m_AiType != AiType.Predator)
                {
                    Log($"Non-predator AI, aborting TryAugmentWolf");
                    baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (baseAI.m_AiSubType != AiSubType.Wolf)
                {
                    Log($"Non-wolf AI, aborting TryAugmentWolf");
                    baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (mAugmentedAiList.Contains(baseAI))
                {
                    Log($"Ai already in list, aborting TryAugmentWolf");
                    baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                AugmentWolf(baseAI, augmentValue);
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
            Log($"Watch out, augmenting {baseAI.gameObject.name} ai!");// size/speed by factor of {augmentValue}!");
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
                    Log($"base AI not contained in list, aborting tryUnaugmentWolf");
                    return false;
                }
                Log($"Previously augmented AI found on {baseAI.gameObject.name}, un-augmenting...");
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
            switch (baseAi.m_CurrentMode)
            {
                case AiMode.Attack:
                    baseAi.ProcessAttack();
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
                    baseAi.ProcessStruggle();
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
    }
}