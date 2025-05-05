#define DEV_BUILD
//#define DEV_BUILD_PROFILE

using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {

        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(BaseAi __result)
            {
#if DEV_BUILD
                try
                {
#endif
                    Manager.TryAugment(__result, new System.Random().Next(100, 500) * 0.01f);
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during SpawnRegion.InstantiateSpawnInternal.Prefix: {e}");
                    return;
                };
#endif
            }
        }


        [HarmonyPatch(typeof(BaseAi), "Despawn")]
        internal class BaseAiPatches_Despawn
        {
            public static void Prefix(BaseAi __instance)
            {
#if DEV_BUILD
                try
                {
#endif
                    Manager.TryUnaugment(__instance);
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during BaseAi.Despawn.Prefix: {e}");
                    return;
                };
#endif
            }
        }


        [HarmonyPatch(typeof(BaseAi), "ProcessCurrentAiMode")]
        internal class BaseAiPatches_ProcessCurrentAiMode
        {
            internal struct StateData
            {
                public int mId;
                public long mStartTime;
                public string mName;

                public StateData(int id, long startTime, string name)
                {
                    mId = id;
                    mStartTime = startTime;
                    mName = name;
                }
            }

            public static bool Prefix(BaseAi __instance
#if DEV_BUILD_PROFILE

                , ref StateData __state

#endif
                )
            {
#if DEV_BUILD
                try
                {
#endif

#if DEV_BUILD_PROFILE
                    __state = new StateData(__instance.GetHashCode(), DateTime.Now.Ticks, __instance.gameObject?.name ?? "null");

#endif
                    bool success = !Manager.TryProcessCurrentAiMode(__instance);

#if DEV_BUILD_PROFILE
                    Manager.LogCustomCycleTime(__state.mId, DateTime.Now.Ticks - __state.mStartTime, __state.mName);
                    __state =  new StateData(__state.mId, DateTime.Now.Ticks, __state.mName);
#endif
                    return success;
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during BaseAi.ProcessCurrentAiMode.Prefix: {e}");
                    return false;
                }
#endif
            }

#if DEV_BUILD_PROFILE
            public static void Postfix(ref StateData __state)
            {
                Manager.LogBaseCycleTime(__state.mId, DateTime.Now.Ticks - __state.mStartTime, __state.mName);
            }
#endif
        }


        [HarmonyPatch(typeof(GameManager), "DoExitToMainMenu")]
        internal class GameManagerPatches_DoExitToMainMenu
        {
            public static void Prefix()
            {
#if DEV_BUILD
                try
                {
#endif
                    Manager.ClearAugments();
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during GameManager.DoExitToMainMenu.Prefix: {e}");
                    return;
                }
#endif
            }
        }
    }
}
