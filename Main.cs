using Il2Cpp;
using MelonLoader;


[assembly: MelonInfo(typeof(MonsieurMeh.Mods.TLD.LegendaryWolves.Main), "LegendaryWolves", "0.0.12", "MonsieurMeh", null)]
[assembly: MelonGame("Hinterland", "TheLongDark")]

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class Main : MelonMod
    {
        protected bool mInitialized = false;
        protected LegendaryWolvesManager mManager; 

        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg(Initialize() ? "Initialized Successfully!" : "Initialization Errors!");
        }

        public override void OnDeinitializeMelon()
        {
            LoggerInstance.Msg(Shutdown() ? "Shutdown Successfully!" : "Shutdown Errors!");
        }


        protected bool Initialize()
        {
            mManager = LegendaryWolvesManager.Instance;
            mManager.Initialize(new Settings(), (s) => LoggerInstance.Msg(s), (err) => LoggerInstance.Error(err));
            mInitialized = mManager != null;
            return mInitialized;
        }


        protected bool Shutdown()
        {
            mInitialized = mManager.Shutdown();
            return !mInitialized;
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