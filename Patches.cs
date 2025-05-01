#define DEV_BUILD

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
        private static void Log(string msg) => Instance.Log(msg);
        private static void Log(GameObject go, string msg) => Instance.Log(go, msg);
        private static void Log(BaseAi baseAi, string msg) => Instance.Log(baseAi, msg);


        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(BaseAi __result)
            {
                try
                {
                    Instance.TryAugmentWolf(__result, new System.Random().Next(100, 500) * 0.01f);
                }
                catch (Exception e)
                {
                    Log($"Error during SpawnRegion.InstantiateSpawnInternal.Prefix: {e}");
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(BaseAi), "Despawn")]
        internal class BaseAiPatches_Despawn
        {
            public static void Prefix(BaseAi __instance)
            {
                try
                {
                    Instance.TryUnaugmentWolf(__instance);
                }
                catch (Exception e)
                {
                    Log($"Error during BaseAi.Despawn.Prefix: {e}");
                    return;
                }
            }
        }


        [HarmonyPatch(typeof(BaseAi), "ProcessCurrentAiMode")]
        internal class BaseAiPatches_ProcessCurrentAiMode
        {
            public static bool Prefix(BaseAi __instance)
            {
                try
                {
                    if (__instance == null)
                    {
#if DEV_BUILD
                        Log(__instance, " is null, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (__instance.m_AiType != AiType.Predator)
                    {
#if DEV_BUILD
                        Log(__instance, " is not a predator, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (__instance.m_AiSubType != AiSubType.Wolf)
                    {
#if DEV_BUILD
                        Log(__instance, " is not a wolf, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
                    if (!Instance.AugmentedAiList.Contains(__instance))
                    {
#if DEV_BUILD
                        Log(__instance, " is not contained in augmented ai list, aborting BaseAi.ProcessCurrentAiMode.Prefix");
#endif
                        return true;
                    }
#if DEV_BUILD
                    Log(__instance, " looks good, running custom ai logic and aborting existing call");
#endif
                    Instance.ProcessCurrentAiMode(__instance);
                    return false;
                }
                catch (Exception e)
                {
                    Log($"Error during BaseAi.ProcessCurrentAiMode.Prefix: {e}");
                    return false;
                }
            }
        }


        [HarmonyPatch(typeof(GameManager), "DoExitToMainMenu")]
        internal class GameManagerPatches_DoExitToMainMenu
        {
            public static void Prefix()
            {
                try
                { 
                    Instance.ClearAugments();
                }
                catch (Exception e)
                {
                    Log($"Error during GameManager.DoExitToMainMenu.Prefix: {e}");
                    return;
                }
            }
        }
    }
}
