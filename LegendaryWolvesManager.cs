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

        protected List<BaseAi> mAugmentedAIList = new List<BaseAi>();

        protected long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }


        public void Initialize(Settings settings, Action<string> logMessageAction)
        {
            if (mInitialized)
            {
                return;
            }
            mInitialized = true;
            mStartTime = System.DateTime.Now.Ticks;
            mSettings = settings;
            mLogMessageAction = logMessageAction;
        }


        public bool TryAugmentWolf(GameObject spawnablePrefab, float augmentValue)
        {
            if (!spawnablePrefab.TryGetComponent(out BaseAi baseAI))
            {
                return false;
            }
            if (baseAI.m_AiType != AiType.Predator)
            {
                return false;
            }
            if (baseAI.m_AiSubType != AiSubType.Wolf)
            {
                return false;
            }
            if (mAugmentedAIList.Contains(baseAI))
            {
                return false;
            }
            Log($"Watch out, augmenting {spawnablePrefab.name} size/speed by factor of {augmentValue}!");
            mAugmentedAIList.Add(baseAI);
            baseAI.m_RunSpeed *= augmentValue;
            baseAI.m_StalkSpeed *= augmentValue;
            baseAI.m_WalkSpeed *= augmentValue;
            //baseAI.m_StalkSlowlySpeed *= augmentValue;
            baseAI.m_turnSpeed *= augmentValue;
            baseAI.m_TurnSpeedDegreesPerSecond *= augmentValue;
            //baseAI.m_StalkCatchUpSpeed *= augmentValue;
            Vector3 newScale = new Vector3(augmentValue, augmentValue, augmentValue);
            baseAI.gameObject.transform.set_localScale_Injected(ref newScale);

            return false;
        }


        public void Log(string message)
        {
            mLogMessageAction.Invoke($"[{TicksSinceStart}t/{TicksSinceStart * MillisecondsPerTick}ms/{TicksSinceStart * SecondsPerTick}s] {message}");
        }
    }
}