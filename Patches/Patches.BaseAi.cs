using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public partial class Patches
    {
        [HarmonyPatch(typeof(BaseAi), "Update")]
        public class BaseAiPatches_Update
        {
            public static bool Prefix(BaseAi __instance)
            {
                return !Manager.TryUpdate(__instance);
            }
        }


        //This is more for external calls to SetAiMode
        //my own ICustomAi does most of the heavy lifting for BaseAi now
        //Ufortunately a lot of other systems like to call and try to route around my system
        //This will allow ICustomAi classes to catch these and adjust as needed

        [HarmonyPatch(typeof(BaseAi), "SetAiMode", new Type[] { typeof(AiMode) })]
        public class BaseAiPatches_SetAiMode
        {
            public static bool Prefix(BaseAi __instance, AiMode mode)
            {
                return !Manager.TrySetAiMode(__instance, mode);
            }
        }


        [HarmonyPatch(typeof(BaseAi), "ApplyDamage", new Type[] { typeof(float), typeof(DamageSource), typeof(string) })]
        public class BaseAiPatches_ApplyDamage
        {
            public static bool Prefix(BaseAi __instance, float damage, DamageSource damageSource, string collider)
            {
                return !Manager.TryApplyDamage(__instance, damage, 0.0f, damageSource);
            }
        }


        [HarmonyPatch(typeof(BaseAi), "ApplyDamage", new Type[] {typeof(float), typeof(float), typeof(DamageSource), typeof(string)})]
        public class BaseAiPatches_ApplyDamage2
        {
            //Thanks for misspelling bleedOutMinutes! Only took me 20 minutes to catch the typo /s
            public static bool Prefix(BaseAi __instance, float damage, float bleedOutMintues, DamageSource damageSource, string collider)
            {
                return !Manager.TryApplyDamage(__instance, damage, bleedOutMintues, damageSource);
            }
        }


        [HarmonyPatch(typeof(BaseAi), "DeserializeUsingBaseAiDataProxy", new Type[] {typeof(BaseAiDataProxy)})]
        public class BaseAiPatches_DeserializeUsingBaseAiDataProxy
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
    }
}
