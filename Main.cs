using MelonLoader;


[assembly: MelonInfo(typeof(MonsieurMeh.Mods.TLD.LegendaryWolves.Main), "LegendaryWolves", "0.0.4", "MonsieurMeh", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class Main : MelonMod
    {
        protected bool mInitialized = false;
        protected LegendaryWolvesManager mManager; 

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg(Initialize() ? "Initialized Successfully!" : "Errors!");
        }


        protected bool Initialize()
        {
            mManager = LegendaryWolvesManager.Instance;
            mManager.Initialize(new Settings(), (s) => LoggerInstance.Msg(s));
            mInitialized = mManager != null;
            return mInitialized;
        }


        public override void OnUpdate()
        {
            if (mInitialized)
            {
                mManager.Update();
            }
        }
    }
}