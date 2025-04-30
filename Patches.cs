using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AI;
using System.Runtime.InteropServices;
using UnityEngine;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeType), typeof(Vector3), typeof(Quaternion) })]
        internal class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(GameObject spawnablePrefab, WildlifeMode wildlifetype, Vector3 pos, Quaternion rotation)
            {
                int augmentRoll = new System.Random().Next(0, 100);
                LegendaryWolvesManager.Instance?.Log($"Spawned a {spawnablePrefab} at {pos}! Augmenting roll of {augmentRoll}, augmenting: {augmentRoll >= 0}");
                if (augmentRoll >= 0)
                {
                    if (!spawnablePrefab.TryGetComponent(out BaseAi baseAI))
                    {
                        LegendaryWolvesManager.Instance?.Log($"Can't augment {spawnablePrefab}, no base AI found!");
                        return;
                    }
                    if (baseAI.m_AiType != AiType.Predator)
                    {
                        LegendaryWolvesManager.Instance?.Log($"Can't augment {spawnablePrefab}, non-predator behavior!");
                        return;
                    }
                    if (baseAI.m_AiSubType != AiSubType.Wolf)
                    {
                        LegendaryWolvesManager.Instance?.Log($"Can't augment {spawnablePrefab}, non-wolf behavior!");
                        return;
                    }
                    if (baseAI is not AiBaseWolf aiBaseWolf)
                    {
                        LegendaryWolvesManager.Instance?.Log($"Can't augment {spawnablePrefab}, baseAI is not aiBaseWolf!");
                        return;
                    }
                    LegendaryWolvesManager.Instance?.Log($"Augmenting {spawnablePrefab}, expect trouble!");
                    aiBaseWolf.m_RunSpeed *= 2;
                    aiBaseWolf.m_StalkSpeed *= 2;
                    aiBaseWolf.m_WalkSpeed *= 2;
                    aiBaseWolf.m_StalkSlowlySpeed *= 2;
                    aiBaseWolf.m_turnSpeed *= 2;
                    aiBaseWolf.m_StalkCatchUpSpeed *= 2;
                    spawnablePrefab.transform.get_localScale_Injected(out Vector3 currentScale);
                    currentScale = new Vector3(currentScale.x * 2, currentScale.y * 2, currentScale.z * 2);
                    spawnablePrefab.transform.set_localScale_Injected(ref currentScale);
                }
            }
        }
    }
}
