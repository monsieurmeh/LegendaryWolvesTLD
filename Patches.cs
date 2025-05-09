using HarmonyLib;
using Il2Cpp;
using Il2CppTLD.AddressableAssets;
using Il2CppTLD.Placement;
using MelonLoader;
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
                Manager.TryAugment(__result, new System.Random().Next(100, 500) * 0.01f);
            }
        }

        

        [HarmonyPatch(typeof(BaseAi), "Update")]
        internal class BaseAiPatches_Update
        {
            public static bool Prefix(BaseAi __instance)
            {
                return !Manager.TryUpdate(__instance);
            }
        }


        //This is more for external calls to SetAiMode
        //my own ICustomAi does most of the heavy lifting for BaseAi now
        //Ufortunately a lot of other systems like to call and try to route around my system
        //This will allow ICustomAi classes to  catch these and adjust as needed

        //2: Mumble grumble, of course this is a stack overflow without a lock
        // and of course in order to check the lock it has to run again
        // at least dictionary checks are fast!
        [HarmonyPatch(typeof(BaseAi), "SetAiMode", new Type[] { typeof(AiMode) })]
        internal class BaseAiPatches_SetAiMode
        {
            public static bool Prefix(BaseAi __instance, AiMode mode)
            {
                return !Manager.TrySetAiMode(__instance, mode);
            }
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



        [HarmonyPatch(typeof(LoadScene), "Activate", new Type[] { typeof(bool) })]
        internal class LoadScenePatches_Activate
        {
            public static void Prefix()
            {
                Manager?.ClearAugments();
            }
        }



        [HarmonyPatch(typeof(ConsoleManager), "Initialize")]
        private static class ConsoleManagerPatches_Initialize
        {
            internal static void Postfix()
            {
                uConsole.RegisterCommand("follow_wandering_wolf", new Action(LegendaryWolvesManager.TryFollowWanderingWolf));
                uConsole.RegisterCommand("pos2", new Action(() => Log($"Pos: {GameManager.m_PlayerManager.m_LastPlayerPosition} / Dir: {GameManager.m_PlayerManager.m_LastPlayerAngle}")));
            }
        }
    }
}
