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

        protected Action<string> mLogMessageAction;
        protected Settings mSettings;
        protected bool mInitialized = false;
        protected bool mEnabled = false;
        protected long mStartTime = System.DateTime.Now.Ticks;
        protected long mLastReadoutTime = System.DateTime.Now.Ticks;
        protected BaseAi mSelectedBaseAI;

        protected long TicksSinceStart { get { return System.DateTime.Now.Ticks - mStartTime; } }
        public BaseAi SelectedBaseAI { get { return mSelectedBaseAI; } set { mSelectedBaseAI = value; } }


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


        public void Update()
        {
            if (!mEnabled || !mInitialized)
            {
                return;
            }
        }


        public void Log(string message)
        {
            mLogMessageAction.Invoke($"[{TicksSinceStart}t/{TicksSinceStart * MillisecondsPerTick}ms/{TicksSinceStart * SecondsPerTick}s] {message}");
        }


        public void Enable()
        {
            if (mEnabled || !mInitialized)
            {
                return;
            }
            Log("Enabling LegendaryWolvesManager");
        }


        public void Disable()
        {
            if (!mEnabled || !mInitialized)
            {
                return;
            }
            Log("Disabling LegendaryWolvesManager");
            mEnabled = false;
        }
    }
}