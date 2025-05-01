using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using System.Buffers.Text;
using System.Runtime.InteropServices;
using UnityEngine;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        private static void Log(string msg) => LegendaryWolvesManager.Instance.Log(msg);
        private static void Log(GameObject go, string msg) => LegendaryWolvesManager.Instance.Log(go, msg);
        private static void Log(BaseAi baseAi, string msg) => LegendaryWolvesManager.Instance.Log(baseAi, msg);


        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(BaseAi __result)
            {
                LegendaryWolvesManager.Instance.TryAugmentWolf(__result, new System.Random().Next(100, 500) * 0.01f);
            }
        }


        [HarmonyPatch(typeof(BaseAi), "Despawn")]
        internal class BaseAiPatches_Despawn
        {
            public static void Prefix(BaseAi __instance)
            {
                LegendaryWolvesManager.Instance.TryUnaugmentWolf(__instance);
            }
        }


        [HarmonyPatch(typeof(BaseAi), "ProcessCurrentAiMode")]
        internal class BaseAiPatches_ProcessCurrentAiMode
        {
            public static bool Prefix(BaseAi __instance)
            {
                if (__instance.m_AiType != AiType.Predator)
                {
                    Log(__instance, " is not a predator, aborting BaseAi.ProcessCurrentAiMode.Prefix");
                    return true;
                }
                if (__instance.m_AiSubType != AiSubType.Wolf)
                {
                    Log(__instance, " is not a wolf, aborting BaseAi.ProcessCurrentAiMode.Prefix");
                    return true;
                }
                if (!LegendaryWolvesManager.Instance.AugmentedAiList.Contains(__instance))
                {
                    Log(__instance, " is not contained in augmented ai list, aborting BaseAi.ProcessCurrentAiMode.Prefix");
                    return true;
                }
                Log(__instance, " looks good, running custom ai logic and aborting existing call");
                LegendaryWolvesManager.Instance.ProcessCurrentAiMode(__instance);
                return false;
            }
        }
    }
}
