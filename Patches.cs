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

        

        [HarmonyPatch(typeof(BaseAi), "Update")]
        internal class BaseAiPatches_Update
        {
#if DEV_BUILD_PROFILE
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
#endif
            public static bool Prefix(BaseAi __instance
#if DEV_BUILD
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
#endif
                    bool success = !Manager.TryUpdate(__instance);
#if DEV_BUILD_PROFILE
                    Manager.LogCustomCycleTime(__state.mId, DateTime.Now.Ticks - __state.mStartTime, __state.mName);
                    __state =  new StateData(__state.mId, DateTime.Now.Ticks, __state.mName);
#endif
                    return success;
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during BaseAi.Update.Prefix: {e}");
                    return false;
                }
            }

#if DEV_BUILD_PROFILE
            public static void Postfix(ref StateData __state)
            {
                Manager.LogBaseCycleTime(__state.mId, DateTime.Now.Ticks - __state.mStartTime, __state.mName);
            }
#endif
#endif
        }


        [HarmonyPatch(typeof(BaseAi), "DeserializeUsingBaseAiDataProxy", new Type[] {typeof(BaseAiDataProxy)})]
        internal class BaseAiPatches_DeserializeUsingBaseAiDataProxy
        {
            //This fucker likes to undo all the work we do at startup for custom wolves because it fires later
            public static void Prefix(BaseAi __instance, BaseAiDataProxy proxy)
            {
                if (Manager.AiAugments.ContainsKey(__instance?.GetHashCode() ?? 0))
                {
                    if (__instance.m_StartMode != AiMode.None)
                    {
                        proxy.m_StartMode = __instance.m_StartMode;
                    }
                    if (__instance.m_DefaultMode != AiMode.None)
                    {
                        proxy.m_DefaultMode = __instance.m_DefaultMode;
                    }
                    if (__instance.m_CurrentMode != AiMode.None)
                    {
                        proxy.m_CurrentMode = __instance.m_CurrentMode;
                    }
                    if ((__instance.m_Waypoints?.Count ?? 0) > 0)
                    {
                        proxy.m_Waypoints = __instance.m_Waypoints;
                    }
                }
            }
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
