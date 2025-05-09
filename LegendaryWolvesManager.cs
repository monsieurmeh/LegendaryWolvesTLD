//#define DEV_BUILD_SPAWNONE
#define DEV_BUILD_STATELABEL

using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Scenes;
using UnityEngine;
using static Il2Cpp.Panel_Debug;
using static Il2CppParadoxNotion.Services.Logger;
using static Il2CppSystem.Linq.Expressions.Interpreter.CastInstruction.CastInstructionNoT;
using static UnityEngine.GraphicsBuffer;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class LegendaryWolvesManager
    {
        #region CameraFollow stuff

        public class CameraFollow
        { 
            protected Transform mTarget;
            protected Transform mCamera;
            protected float mDistance = 5.0f;
            protected float mMinDistance = 3.0f;
            protected float mMaxDistance = 10.0f;
            protected float mXSpeed = 120.0f;
            protected float mYSpeed = 120.0f;
            protected float mZSpeed = 1.0f;
            protected float mYMinLimit = 20f;
            protected float mYMaxLimit = 80f;
            protected float mX = 0.0f;
            protected float mY = 0.0f;


            public void SetTarget(Transform target, Transform camera)
            {
                mTarget = target;
                mCamera = camera;
                mX = camera.eulerAngles.x;
                mY = camera.eulerAngles.y;
            }


            public void Update()
            {
                if (mCamera == null || mTarget == null)
                {
                    return;
                }
                //this is not working ;/
                mCamera.position = mTarget.position + new Vector3(0.0f, 25f, 10.0f);
                /*
                mX += InputManager.GetAxisMouseX(GameManager.m_PlayerManager) * mXSpeed * Time.deltaTime;
                mY -= InputManager.GetAxisMouseY(GameManager.m_PlayerManager) * mYSpeed * Time.deltaTime;
                mDistance += InputManager.GetAxisScrollWheel(GameManager.m_PlayerManager) * mZSpeed * Time.deltaTime;
                mY = Mathf.Clamp(mY, mYMinLimit, mYMaxLimit);
                mDistance = Mathf.Clamp(mDistance, mMinDistance, mMaxDistance);
                Quaternion rotation = Quaternion.Euler(mX, mY, 0);
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -mDistance);
                Vector3 position = Quaternion.Euler(mY, mX, 0) * negDistance + mTarget.position;
                mCamera.rotation = rotation;
                mCamera.position = position;
                */
            }
        }

        #endregion


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
        private bool mFollowingWanderingWolf = false;
        private WanderingWolf mWanderingWolfToFollow;
        private CameraFollow mCameraFollow = new CameraFollow();
        private Dictionary<string, List<Vector3>> mHidingSpots = new Dictionary<string, List<Vector3>>();
        private ulong mTakenHidingSpots = 0UL;
        
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
            mHidingSpots.Add("RuralArea", new List<Vector3>());
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
            if (mFollowingWanderingWolf)
            {
                if (CameraDebugMode.s_Mode != CameraDebugMode.Mode.Fly)
                {
                    mFollowingWanderingWolf = false;
                }
                else
                {
                    mCameraFollow.Update();
                }
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
            WolfTypes newType = WolfTypes.HidingWolf;// (WolfTypes)new System.Random().Next(0, (int)WolfTypes.COUNT);
            switch (newType)
            {
                case WolfTypes.Default:
                    //Log($"Spawning BaseWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new BaseWolf(baseAi));
                    break; 
                case WolfTypes.HidingWolf:
                    Log($"Spawning HidingWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new HidingWolf(baseAi, new Vector3(740, 148, 454), baseAi.transform.rotation.eulerAngles));
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
                case WolfTypes.Stalker:
                    Log($"Spawning StalkingWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new StalkingWolf(baseAi));
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

        public bool FollowingWanderingWolf { get { return mFollowingWanderingWolf; } set { mFollowingWanderingWolf = value; } }


        public static void TryFollowWanderingWolf()
        {
            if (CameraDebugMode.s_Mode == CameraDebugMode.Mode.Lock)
            {
                return;
            }
            if (Instance != null)
            {
                if (Instance.FollowingWanderingWolf && CameraDebugMode.s_Mode == CameraDebugMode.Mode.Fly)
                {
                    Instance.StopFollowingWanderingWolf();
                }
                else
                {
                    foreach (ICustomAi customAi in Instance.AiAugments.Values)
                    {
                        if (customAi is WanderingWolf wanderingWolf)
                        {
                            Instance.StartFollowingWanderingWolf(wanderingWolf);
                        }
                    }
                }
            }
        }


        public void StopFollowingWanderingWolf()
        {
            mFollowingWanderingWolf = false;
            HUDMessage.AddMessage("Following wandering wolf", false, false);
            mCameraFollow.SetTarget(null, null);
            FlyMode.Enter();
        }


        public void StartFollowingWanderingWolf(WanderingWolf wanderingWolf)
        {
            mFollowingWanderingWolf = true;
            mWanderingWolfToFollow = wanderingWolf;
            InputManager.ResetControllerState();
            HUDMessage.AddMessage("Following wandering wolf", false, false);
            mCameraFollow.SetTarget(wanderingWolf.BaseAi.transform, FlyMode.m_Camera.transform);
            FlyMode.Enter();

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