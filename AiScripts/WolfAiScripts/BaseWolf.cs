using Il2Cpp;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class BaseWolf : CustomAiBase, ICustomWolfAi
    {
        public override WolfTypes WolfType { get { return WolfTypes.Default; } }
        public BaseWolf(BaseAi baseAi) : base(baseAi) { }

        protected override float m_HoldGroundDistanceFromSpear { get { return 3f; } }
        protected override float m_HoldGroundOuterDistanceFromSpear { get { return 5f; } }
    }
}
