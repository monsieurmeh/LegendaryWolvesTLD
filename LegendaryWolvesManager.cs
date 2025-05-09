//#define DEV_BUILD_SPAWNONE
#define DEV_BUILD_STATELABEL

using Harmony;
using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.Scenes;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;
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
        private Dictionary<string, List<(Vector3, Vector3)>> mHidingSpots = new Dictionary<string, List<(Vector3, Vector3)>>();
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
            InitializeHidingSpots();
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
            mTakenHidingSpots = 0UL;
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
            if (!AiAugments.TryGetValue(baseAi.GetHashCode(), out ICustomAi customAi))
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
                    Log($"Spawning BaseWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new BaseWolf(baseAi));
                    break; 
                case WolfTypes.HidingWolf:
                    Log($"Spawning HidingWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new HidingWolf(baseAi));
                    break;
                case WolfTypes.ScaredyWolf:
                    Log($"Spawning ScaredyWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new ScaredyWolf(baseAi));
                    break;
                case WolfTypes.Wanderer:
                    Log($"Spawning WanderingWolf at {baseAi.gameObject.transform.position}!");
                    mAiAugments.Add(baseAi.GetHashCode(), new WanderingWolf(baseAi));
                    break;
                case WolfTypes.BigWolf:
                    Log($"Spawning BigWolf at {baseAi.gameObject.transform.position}!");
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


        #region HidingWolf location management


        private void InitializeHidingSpots()
        {
            InitializeHidingSpotsTimberwolfMountain();
        }


        private void InitializeHidingSpotsTimberwolfMountain()
        {
            mHidingSpots.Add("CrashMountainRegion", new List<(Vector3, Vector3)>());
            mHidingSpots["CrashMountainRegion"].Add((new Vector3(1141.10f, 52.54f, 895.27f), new Vector3(32.51f, -2.40f, 0.0f)));
            mHidingSpots["CrashMountainRegion"].Add((new Vector3(1683.05f, 207.01f, 974.37f), new Vector3(215.20f, -26.32f, 0.0f)));
            mHidingSpots["CrashMountainRegion"].Add((new Vector3(1602.42f, 211.01f, 1745.20f), new Vector3(226.41f, -1.36f, 0.0f)));
            mHidingSpots["CrashMountainRegion"].Add((new Vector3(762.25f, 153.11f, 337.12f), new Vector3(259.54f, -3.10f, 0.0f)));
            mHidingSpots["CrashMountainRegion"].Add((new Vector3(636.35f, 275.35f, 1099.64f), new Vector3(279.70f, -9.98f, 0.0f)));
        }
            

        public int SpotsAvailable()
        {
            if (mHidingSpots.TryGetValue(GameManager.m_ActiveScene, out List<(Vector3, Vector3)> spots))
            {
                return spots.Count;
            }
            return 0;
        }


        public bool SpotAvailable(int index, out Vector3 position, out Vector3 orientation)
        {
            position = Vector3.zero;
            orientation = Vector3.zero;
            if (mHidingSpots.TryGetValue(GameManager.m_ActiveScene, out List <(Vector3, Vector3)> spots))
            {
                position = spots[index].Item1;
                orientation = spots[index].Item2;
            }
            return (mTakenHidingSpots & (1UL << index)) == 0U;
        }


        public void TakeSpot(int index)
        {
            mTakenHidingSpots |= (1UL << index);
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

        public static TEnum ToEnum<TEnum>(this uint uval) where TEnum : Enum { return UnsafeUtility.As<uint, TEnum>(ref uval); }
        public static TEnum ToEnumL<TEnum>(this ulong uval) where TEnum : Enum { return UnsafeUtility.As<ulong, TEnum>(ref uval); }

        public static uint ToUInt<TEnum>(this TEnum val) where TEnum : Enum {  return UnsafeUtility.As<TEnum, uint>(ref val); }
        public static ulong ToULong<TEnum>(this TEnum val) where TEnum : Enum { return UnsafeUtility.As<TEnum, ulong>(ref val); }
        public static bool Any<TEnum>(this TEnum val) where TEnum : Enum { return val.ToUInt() != 0; }
        public static bool AnyL<TEnum>(this TEnum val) where TEnum : Enum { return val.ToULong() != 0UL; }
        public static bool OnlyOne<TEnum>(this TEnum val) where TEnum : Enum { uint f = val.ToUInt(); return f != 0 && (f & (f - 1)) == 0; }
        public static bool OnlyOneL<TEnum>(this TEnum val) where TEnum : Enum { ulong f = val.ToULong(); return f != 0UL && (f & (f - 1UL)) == 0UL; }
        public static bool OnlyOneOrZero<TEnum>(this TEnum val) where TEnum : Enum { uint f = val.ToUInt(); return (f & (f - 1)) == 0; }
        public static bool OnlyOneOrZeroL<TEnum>(this TEnum val) where TEnum : Enum { ulong f = val.ToULong(); return (f & (f - 1UL)) == 0UL; }
        public static bool IsSet<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToUInt() & toCheck.ToUInt()) != 0; }
        public static bool IsSetL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToULong() & toCheck.ToULong()) != 0UL; }
        public static bool IsUnset<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToUInt() & toCheck.ToUInt()) == 0; }
        public static bool IsUnsetL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToULong() & toCheck.ToULong()) == 0UL; }
        public static bool AnyOf<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToUInt() & toCheck.ToUInt()) != 0; }
        public static bool AnyOfL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToULong() & toCheck.ToULong()) != 0UL; }
        public static bool AllOf<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { uint c = toCheck.ToUInt(); return (val.ToUInt() & c) == c; }
        public static bool AllOfL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { ulong c = toCheck.ToULong(); return (val.ToULong() & c) == c; }
        public static bool OnlyOneOf<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { uint v = (val.ToUInt() & toCheck.ToUInt()); return v != 0 && (v & (v - 1)) == 0; }
        public static bool OnlyOneOfL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { ulong v = (val.ToULong() & toCheck.ToULong()); return v != 0UL && (v & (v - 1UL)) == 0UL; }
        public static bool NoneOf<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToUInt() & toCheck.ToUInt()) == 0; }
        public static bool NoneOfL<TEnum>(this TEnum val, TEnum toCheck) where TEnum : Enum { return (val.ToULong() & toCheck.ToULong()) == 0UL; }
        public static bool OthersSet<TEnum>(this TEnum val, TEnum toIgnore) where TEnum : Enum { return (val.ToUInt() & ~toIgnore.ToUInt()) != 0; }
        public static bool OthersSetL<TEnum>(this TEnum val, TEnum toIgnore) where TEnum : Enum { return (val.ToULong() & ~toIgnore.ToULong()) != 0UL; }
        public static TEnum UnsetFlags<TEnum>(this TEnum val, TEnum flags) where TEnum : Enum { return (val.ToUInt() & ~flags.ToUInt()).ToEnum<TEnum>(); }
        public static TEnum UnsetFlagsL<TEnum>(this TEnum val, TEnum flags) where TEnum : Enum { return (val.ToULong() & ~flags.ToULong()).ToEnumL<TEnum>(); }
        public static TEnum SetFlags<TEnum>(this TEnum val, TEnum flags, bool shouldSet = true) where TEnum : Enum { return (shouldSet ? (val.ToUInt() | flags.ToUInt()) : (val.ToUInt() & ~flags.ToUInt())).ToEnum<TEnum>(); }
        public static TEnum SetFlagsL<TEnum>(this TEnum val, TEnum flags, bool shouldSet = true) where TEnum : Enum { return (shouldSet ? (val.ToULong() | flags.ToULong()) : (val.ToULong() & ~flags.ToULong())).ToEnumL<TEnum>(); }
    }
}