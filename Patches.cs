using HarmonyLib;
using Il2Cpp;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SpawnRegion), "Spawn", new Type[] { typeof(WildlifeMode) })]
        internal class SpawnRegionPatches_Spawn
        {
            public static void Prefix(SpawnRegion __instance)
            {
                LegendaryWolvesManager.Instance?.Log($"Starting SpawnRegion.Spawn! Logging intended spawns...");
                int randIndex = new Random().Next(0, __instance.m_Spawns?.Count ?? 0);
                for (int i = 0, iMax = __instance.m_Spawns?.Count ?? 0; i < iMax; i++)
                {
                    if (i == randIndex)
                    {

                        LegendaryWolvesManager.Instance?.Log($"[Spawn #{i}] {__instance.m_Spawns[i]} - Selected as guinea pig!");
                    }
                    else
                    {
                        LegendaryWolvesManager.Instance?.Log($"[Spawn #{i}] {__instance.m_Spawns[i]}");
                    }
                }
            }
        }
    }
}
