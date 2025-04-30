using HarmonyLib;
using Il2Cpp;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SpawnRegion), "Spawn", new Type[] { typeof(WildlifeMode) })]
        internal class SpawnRegion_SpawnPatch
        {
            public static void Prefix()
            {
                LegendaryWolvesManager.Instance?.Log($"Starting SpawnRegion.Spawn!\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}");
            }


            public static void Postfix()
            {
                LegendaryWolvesManager.Instance?.Log($"Finishing call of SpawnRegion.Spawn!\n{UnityEngine.StackTraceUtility.ExtractStackTrace()}");
            }
        }
    }
}
