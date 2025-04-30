using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using System.Runtime.InteropServices;
using UnityEngine;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(GameObject spawnablePrefab)
            {
                LegendaryWolvesManager.Instance.TryAugmentWolf(spawnablePrefab, new System.Random().Next(100, 500) * 0.01f);
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
    }
}
