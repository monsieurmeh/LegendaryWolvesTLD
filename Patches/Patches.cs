using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public partial class Patches
    {

        [HarmonyPatch(typeof(SpawnRegion), "InstantiateSpawnInternal", new Type[] { typeof(GameObject), typeof(WildlifeMode), typeof(Vector3), typeof(Quaternion) })]
        public class SpawnRegionPatches_InstantiateSpawnInternal
        {
            public static void Postfix(BaseAi __result)
            { 
                Manager.TryAugment(__result, new System.Random().Next(100, 500) * 0.01f);
            }
        }


        [HarmonyPatch(typeof(LoadScene), "Activate", new Type[] { typeof(bool) })]
        public class LoadScenePatches_Activate
        {
            public static void Prefix()
            {
                Manager?.ClearAugments();
            }
        }



        [HarmonyPatch(typeof(ConsoleManager), "Initialize")]
        private static class ConsoleManagerPatches_Initialize
        {
            public static void Postfix()
            {
                uConsole.RegisterCommand("follow_wandering_wolf", new Action(LegendaryWolvesManager.TryFollowWanderingWolf));
                uConsole.RegisterCommand("pos2", new Action(() => Log($"Pos: {GameManager.m_PlayerManager.m_LastPlayerPosition} / Dir: {GameManager.m_PlayerManager.m_LastPlayerAngle}")));
            }
        }
    }
}
