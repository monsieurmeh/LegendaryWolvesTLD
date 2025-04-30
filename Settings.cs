using ModSettings;
using UnityEngine;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public class Settings : JsonModSettings
    {
        /*
        [Section("Main")]
        [Name("Activate Key")]
        [Description("Set this to re-map Survivor Sense activation.")]
        public KeyCode ActivateKey = KeyCode.CapsLock;

        [Name("Detection Cooldown")]
        [Slider(0, 300)]
        [Description("Set this to change the cooldown on Survivor Sense.")]
        public int DetectionCooldown = 1;

        [Name("Emission Duration")]
        [Slider(0, 1000)]
        [Description("Set this to change constant-brightness duration of Survivor Sense pulse emissions.")]
        public int ConstantDuration = 10;

        [Name("Emission Fade Duration")]
        [Slider(0, 1000)]
        [Description("Set this to change fade-out duration of Survivor Sense pulse emissions.")]
        public int FadeoutDuration = 10;

        [Name("Emission Intensity")]
        [Slider(0f, 10000f)]
        [Description("Set this to change the starting intensitty of Survivor Sense pulse emissions.")]
        public float EmissionIntensity = 1000;


        [Name("CullingMask")]
        [Slider(1, 32, 32)]
        [Description("Set this to change the culling mask of the light source - DEBUG ONLY!")]
        public int CullMask = 17;
        */

        public Settings()
        {
            Initialize();
        }


        protected void Initialize()
        {
            AddToModSettings("Legendary Woles");
            RefreshGUI();
        }
    }
}