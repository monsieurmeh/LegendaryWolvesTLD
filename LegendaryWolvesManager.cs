using Il2Cpp;
using Il2CppTLD.AI;
using System.Buffers.Text;
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

        protected Action<string> mLogMessageAction;
        protected Settings mSettings;
        protected bool mInitialized = false;
        protected bool mEnabled = false;
        protected long mStartTime = System.DateTime.Now.Ticks;
        protected long mLastReadoutTime = System.DateTime.Now.Ticks;

        protected List<BaseAi> mAugmentedAIList;

        protected long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }


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
            mAugmentedAIList = new List<BaseAi>();
            return true;
        }


        public bool Shutdown()
        {
            if (!mInitialized)
            {
                return false;
            }
            for (int i = 0, iMax = mAugmentedAIList.Count; i < iMax; i++)
            {
                TryUnaugmentWolf(mAugmentedAIList[i]);
            }
            mAugmentedAIList.Clear();
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
                    return false;
                }
                Vector3 newScale = new Vector3(1, 1, 1);
                if (baseAI.m_AiType != AiType.Predator)
                {
                    baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (baseAI.m_AiSubType != AiSubType.Wolf)
                {
                    baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
                    return false;
                }
                if (mAugmentedAIList.Contains(baseAI))
                {
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
            Log($"Watch out, augmenting {baseAI.gameObject.name} size/speed by factor of {augmentValue}!");
            mAugmentedAIList.Add(baseAI);
            baseAI.m_RunSpeed *= augmentValue;
            baseAI.m_StalkSpeed *= augmentValue;
            baseAI.m_WalkSpeed *= augmentValue;
            baseAI.m_turnSpeed *= augmentValue;
            baseAI.m_TurnSpeedDegreesPerSecond *= augmentValue;
            Vector3 newScale = new Vector3(augmentValue, augmentValue, augmentValue);
            baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }


        public bool TryUnaugmentWolf(BaseAi baseAI)
        {
            try
            {
                if (baseAI == null)
                {
                    return false;
                }
                if (!mAugmentedAIList.Contains(baseAI))
                {
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
            mAugmentedAIList.Remove(baseAI);
            Vector3 newScale = new Vector3(1, 1, 1);
            baseAI.gameObject.transform.set_localScale_Injected(ref newScale);
        }


        public void Log(string message)
        {
            mLogMessageAction.Invoke($"[{TicksSinceStart}t/{TicksSinceStart * MillisecondsPerTick}ms/{TicksSinceStart * SecondsPerTick}s] {message}");
        }
    }
}