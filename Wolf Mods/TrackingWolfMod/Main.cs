﻿global using Il2Cpp;
global using MelonLoader;
global using ModSettings;


[assembly: MelonInfo(typeof(ExpandedAiFramework.TrackingWolfMod.Main), "ExpandedAiFramework.TrackingWolf", "1.0.0", "MonsieurMeh", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]


namespace ExpandedAiFramework.TrackingWolfMod
{
    public class Main : MelonMod
    {
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg(Initialize() ? "Initialized Successfully!" : "Initialization Errors!");
        }

        protected bool Initialize()
        {
            return EAFManager.Instance.RegisterSpawnableAi(typeof(TrackingWolf), TrackingWolf.Settings);
        }
    }
}