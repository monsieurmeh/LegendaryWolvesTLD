//#define DEV_BUILD_SPAWNONE
#define DEV_BUILD_STATELABEL

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


        #region Internal stuff

        private Action<string> mLogMessageAction;
        private Action<string> mLogErrorAction;
        private Settings mSettings;
        private bool mInitialized = false;
        private bool mEnabled = false;
        private long mStartTime = System.DateTime.Now.Ticks;
        private long mLastReadoutTime = System.DateTime.Now.Ticks;
        private bool mStartupReadoutDone = false;

#if DEV_BUILD_SPAWNONE
        private bool mSpawnedOne = false;
#endif

        private Dictionary<int, ICustomAi> mAiAugments;

        private long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }
        private long TicksSinceLastReadout { get { return System.DateTime.Now.Ticks - mLastReadoutTime; } }

        public Dictionary<int, ICustomAi> AiAugments { get { return mAiAugments; } }


        public bool Initialize(Settings settings, Action<string> logMessageAction, Action<string> logErrorAction)
        {
            if (mInitialized)
            {
                return false;
            }
            mInitialized = true;
            mStartTime = System.DateTime.Now.Ticks;
            mSettings = settings;
            mAiAugments = new Dictionary<int, ICustomAi>();
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



#endregion


        #region API

        public void ClearAugments()
        {
            foreach (ICustomAi customAi in mAiAugments.Values)
            {
                TryUnaugment(customAi.BaseAi);
            }
            mAiAugments.Clear();
        }
        

        //further expansion here later, we can branch off into different augmentors etc
        // for now, just wolves... name of the mod, after all.
        public bool TryAugment(BaseAi baseAi, float augmentValue)
        {
            if (baseAi == null) 
            {
                return false;
            }
            if (baseAi.m_AiSubType != AiSubType.Wolf)
            {
                return false;
            }
            if (mAiAugments.ContainsKey(baseAi.GetHashCode()))
            {
                return false;
            }


#if DEV_BUILD_SPAWNONE
            if (mSpawnedOne)
            {
                return false;
            }
#endif


            AugmentAi(baseAi, augmentValue);
            return true;
        }


        public bool TryUnaugment(BaseAi baseAI)
        {
            if (baseAI == null)
            {
                return false;
            }
            if (!mAiAugments.ContainsKey(baseAI.GetHashCode()))
            {
                return false;
            }
            UnaugmentAi(baseAI.GetHashCode());
            return true;
        }


        public bool TryUpdate(BaseAi baseAi)
        {
            if (!AiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomAi ai))
            {
                return false;
            }
            ai.Update();
            return true;
        }


        public bool TrySetAiMode(BaseAi baseAi, AiMode aiMode)
        {
            if (!AiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomAi customAi) || customAi.SetAiModeLock)
            {
                return false;
            }
            customAi.SetAiMode(aiMode);
            return true;
        }

        #endregion


        #region Internal Methods

        private void AugmentAi(BaseAi baseAi, float augmentValue)
        {
            AugmentWolfAi(baseAi);
        }


        private void AugmentWolfAi(BaseAi baseAi)
        {
            if (baseAi.Timberwolf != null)
            {
                // Don't want to override timberwolf behaviour just yet; I have different plans for them!
                return;
            }
            WolfTypes newType = (WolfTypes)new System.Random().Next(0, (int)WolfTypes.COUNT);
            switch (newType)
            {
                case WolfTypes.Default:
                    //Log($"Spawning BaseWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new BaseWolf(baseAi));
                    break;
                case WolfTypes.ScaredyWolf:
                    //Log($"Spawning ScaredyWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new ScaredyWolf(baseAi));
                    break;
                case WolfTypes.Wanderer:
                    //Log($"Spawning WanderingWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new WanderingWolf(baseAi));
                    break;
                case WolfTypes.BigWolf:
                    //Log($"Spawning BigWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new BigWolf(baseAi));
                    break;
                default:
                    return;
            }
            if (mAiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomAi customAi))
            {
                customAi.Augment();
#if DEV_BUILD_SPAWNONE
                mSpawnedOne = true;
#endif
            }
        }


        private void UnaugmentAi(int hashCode)
        {
            if (mAiAugments.TryGetValue(hashCode, out ICustomAi customAi))
            {
                Log($"Unaugmenting ai with hashcode {hashCode}");
                customAi.UnAugment();
                mAiAugments.Remove(hashCode);
            }
        }

        #endregion


            #region Debug

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

            #endregion
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