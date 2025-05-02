#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
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


        private Action<string> mLogMessageAction;
        private Action<string> mLogErrorAction;
        private Settings mSettings;
        private bool mInitialized = false;
        private bool mEnabled = false;
        private long mStartTime = System.DateTime.Now.Ticks;
        private long mLastReadoutTime = System.DateTime.Now.Ticks;

        private Dictionary<int, ICustomWolfAI> mAiAugments;

        private long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }
        private long TicksSinceLastReadout { get { return System.DateTime.Now.Ticks - mLastReadoutTime; } }

        public Dictionary<int, ICustomWolfAI> AiAugments { get { return mAiAugments; } }

        public bool Initialize(Settings settings, Action<string> logMessageAction, Action<string> logErrorAction)
        {
            if (mInitialized)
            {
                return false;
            }
            mInitialized = true;
            mStartTime = System.DateTime.Now.Ticks;
            mSettings = settings;
            mAiAugments = new Dictionary<int, ICustomWolfAI>();
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
            mAiAugments = null;
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
            }
        }


        public void ClearAugments()
        {
            foreach (ICustomWolfAI customWolfAi in mAiAugments.Values)
            {
                TryUnaugmentWolf(customWolfAi.BaseAi);
            }
            mAiAugments.Clear();
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
                if (baseAi.m_AiType != AiType.Predator)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is not a predator, aborting TryAugmentWolf");
#endif
                    return false;
                }
                if (baseAi.m_AiSubType != AiSubType.Wolf)
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is not a wolf, aborting TryAugmentWolf");
#endif
                    return false;
                }
                if (mAiAugments.ContainsKey(baseAi.GetHashCode()))
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAi, " is already in list, aborting TryAugmentWolf");
#endif
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
            WolfTypes newType = WolfTypes.Default;//(WolfTypes)new System.Random().Next(0, (int)WolfTypes.COUNT);
            switch (newType)
            {
                case WolfTypes.Default:
#if DEV_BUILD_LOG
                    Log($"Spawning BaseWolf at {baseAi.gameObject.transform.position}!");
    #endif
                    mAiAugments.Add(baseAi.GetHashCode(), new BaseWolf(baseAi));
                    break;
                //case WolfTypes.ScaredyWolf:
#if DEV_BUILD_LOG
                // Log($"Spawning ScaredyWolf at {baseAi.gameObject.transform.position}!");
#endif
                //mAiAugments.Add(baseAi.GetHashCode(), new ScaredyWolf(baseAi));
                //break;
                // case WolfTypes.Wanderer:
#if DEV_BUILD_LOG
                // Log($"Spawning WanderingWolf at {baseAi.gameObject.transform.position}!");
#endif
                // mAiAugments.Add(baseAi.GetHashCode(), new WanderingWolf(baseAi));
                //break;
                //case WolfTypes.BigWolf:
                //#if DEV_BUILD_LOG
                //Log($"Spawning BigWolf at {baseAi.gameObject.transform.position}!");
                //#endif
                //mAiAugments.Add(baseAi.GetHashCode(), new BigWolf(baseAi));
                //break;
                default:
#if DEV_BUILD_LOG
                    Log($"No handler for {newType}, aborting AugmentWolf...");
#endif
                    return;
            }
            if (mAiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomWolfAI customWolfAi))
            {
                customWolfAi.Augment();
            }
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
                if (!mAiAugments.ContainsKey(baseAI.GetHashCode()))
                {
#if DEV_BUILD_LOG_VERBOSE
                    Log(baseAI, $" is not contained in list, aborting tryUnaugmentWolf");
#endif
                    return false;
                }
#if DEV_BUILD_LOG_VERBOSE
                Log($"Previously augmented AI found on {BaseAiInfo(baseAI)}, un-augmenting...");
#endif
                UnaugmentWolf(baseAI.GetHashCode());
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



        private void UnaugmentWolf(int hashCode)
        {
            if (mAiAugments.TryGetValue(hashCode, out ICustomWolfAI wolfAi))
            {
                wolfAi.UnAugment();
                mAiAugments.Remove(hashCode);
            }
        }

        

        public void ProcessCurrentAiMode(BaseAi baseAi)
        {
#if DEV_BUILD
            try
            {
#endif
                if (mAiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomWolfAI customWolfAi))
                {
                    customWolfAi.ProcessCurrentAiMode();
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


    public static class Helpers
    {
        public static LegendaryWolvesManager Manager { get { return LegendaryWolvesManager.Instance; } }
        public static void Log(string msg, bool error = false) => LegendaryWolvesManager.Instance.Log(msg, error);
        public static void Log(BaseAi baseAi, string msg, bool error = false) => LegendaryWolvesManager.Instance.Log(baseAi, msg, error);
        public static void LogError(string msg) => Log(msg, true);
        public static void LogError(BaseAi baseAi, string msg) => Log(baseAi, msg, true);

    }
}