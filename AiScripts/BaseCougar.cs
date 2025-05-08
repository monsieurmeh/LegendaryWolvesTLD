#define DEV_BUILD
#define DEV_BUILD_LOG
//#define DEV_BUILD_LOG_VERBOSE

using Il2Cpp;
using Il2CppTLD.AI;
using UnityEngine;
using static Il2Cpp.BaseAi;
using static MonsieurMeh.Mods.TLD.LegendaryWolves.Helpers;
using static UnityEngine.GraphicsBuffer;
using System.Reflection;
using Il2CppSuperSplines;
using ModSettings;
using UnityEngine.Playables;
using HarmonyLib;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class BaseCougar : CustomAiBase, ICustomWolfAi
    {
        public BaseCougar(AiCougar baseAi) : base(baseAi) { }

        protected override float m_HoldGroundDistanceFromSpear { get { return 5f; } }
        protected override float m_HoldGroundOuterDistanceFromSpear { get { return 10f; } }
    }
}
