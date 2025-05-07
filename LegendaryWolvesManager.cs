#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_PROFILE
//#define DEV_BUILD_SPAWNONE
//#define DEV_BUILD_LOG_VERBOSE
#define DEV_BUILD_STATELABEL

using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using Il2CppNodeCanvas.Tasks.Conditions;
using Il2CppTLD.AI;
using Il2CppTMPro;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
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


        #region debug profiling code

#if DEV_BUILD_PROFILE
        public Dictionary<int, string> AiCycleTimeNameDict = new Dictionary<int, string>();
        public Dictionary<int, List<long>> BaseAiCycleTimes = new Dictionary<int, List<long>>();
        public Dictionary<int, List<long>> CustomAiCycleTimes = new Dictionary<int, List<long>>();

        public void LogCustomCycleTime(int id, long time, string name)
        {
            if (!CustomAiCycleTimes.TryGetValue(id, out List<long> times))
            {
                times = new List<long>();
                CustomAiCycleTimes.Add(id, times);
            }
            times.Add(time);
            if (!AiCycleTimeNameDict.TryGetValue(id, out _))
            {
                AiCycleTimeNameDict.Add(id, name);
            }
        }


        public void LogBaseCycleTime(int id, long time, string name)
        {
            if (!BaseAiCycleTimes.TryGetValue(id, out List<long> times))
            {
                times = new List<long>();
                BaseAiCycleTimes.Add(id, times);
            }
            times.Add(time);
            if (!AiCycleTimeNameDict.TryGetValue(id, out _))
            {
                AiCycleTimeNameDict.Add(id, name);
            }
        }

        protected void ReadoutCycleTimes()
        {
            Log($"CustomAiCycleTimes: {CustomAiCycleTimes.Count}; BaseAiCycletimes: {BaseAiCycleTimes.Count}");
            foreach (int key in CustomAiCycleTimes.Keys)
            {
                long totalCustom = 0l;
                int countCustom = 0;

                foreach (long time in CustomAiCycleTimes[key])
                {
                    totalCustom += time;
                    countCustom++;
                }
                long totalBase = 0l;
                int countBase = 0;
                if (BaseAiCycleTimes.ContainsKey(key))
                {

                    foreach (long time in BaseAiCycleTimes[key])
                    {
                        totalBase += time;
                        countBase++;
                    }
                }
                Log($"{AiCycleTimeNameDict[key]} with hashcode {key} suffered through average time of {(float)(totalCustom / (countCustom > 0 ? countCustom : 1))} custom process ticks vs {(float)(totalBase / (countBase > 0 ? countBase : 1))} base process ticks");
            }
            BaseAiCycleTimes.Clear();
            CustomAiCycleTimes.Clear();
            AiCycleTimeNameDict.Clear();
        }

#endif


        #endregion


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
#if DEV_BUILD_PROFILE
            ReadoutCycleTimes();
#endif
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

        #endregion


        #region Internal Methods

        private void AugmentAi(BaseAi baseAi, float augmentValue)
        {
            AugmentWolfAi(baseAi);
            if (!mStartupReadoutDone)
            {
                mStartupReadoutDone = true;
                Il2CppSystem.Collections.Generic.List<AuroraField> auroras = AuroraManager.m_AuroraFieldsSceneManager.m_RegisteredAuroraFields;
                for (int i = 0, iMax = auroras.Count; i < iMax; i++)
                {
                    for (int j = 0, jMax = auroras[i].m_InfluencedObjects?.Count ?? 0; j < jMax; j++)
                    {
                        Log($"Found {auroras[i].m_InfluencedObjects[j].name} in an aurorafield influenced object list!");
                    }
                }
            }
        }


        private void AugmentWolfAi(BaseAi baseAi)
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


        public static string GetStackTrace()
        {
            return UnityEngine.StackTraceUtility.ExtractStackTrace();
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