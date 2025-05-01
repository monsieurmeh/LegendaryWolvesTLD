#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.LegendaryWolvesManager;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        private static void Log(string msg, bool error = false) => Instance.Log(msg, error);
        private static void Log(BaseAi baseAi, string msg, bool error = false) => Instance.Log(baseAi, msg, error);
        private static void LogError(string msg) => Log(msg, true);
        private static void LogError(BaseAi baseAi, string msg) => Log(baseAi, msg, true);


        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(BaseAi __result)
            {
#if DEV_BUILD
                try
                {
#endif
                    Instance.TryAugmentWolf(__result, new System.Random().Next(100, 500) * 0.01f);
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
                    Instance.TryUnaugmentWolf(__instance);
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
            public static bool Prefix(BaseAi __instance)
            {
#if DEV_BUILD
                try
                {
#endif
                    if (__instance == null)
                    {
#if DEV_BUILD_LOG_VERBOSE
                        Log(__instance, " is null, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (__instance.m_AiType != AiType.Predator)
                    {
#if DEV_BUILD_LOG_VERBOSE
                        Log(__instance, " is not a predator, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (__instance.m_AiSubType != AiSubType.Wolf)
                    {
#if DEV_BUILD_LOG_VERBOSE
                        Log(__instance, " is not a wolf, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (!Instance.AugmentList.ContainsKey(__instance.GetHashCode()))
                    {
#if DEV_BUILD_LOG_VERBOSE
                        Log(__instance, $" is not contained in augmented ai list with count {Instance.AugmentList.Count}, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
#if DEV_BUILD_LOG_VERBOSE
                    Log(__instance, " looks good, running custom ai logic and aborting existing call");
#endif
                    Instance.ProcessCurrentAiMode(__instance);
                    return false;
#if DEV_BUILD
                }
                catch (Exception e)
                {
                    LogError($"Error during BaseAi.ProcessCurrentAiMode.Prefix: {e}");
                    return false;
                }
#endif
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
                    Instance.ClearAugments();
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
