using Il2Cpp;


namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    public partial class CustomAiBase : ICustomAi
    {
        // Return "true" on any of these in subclasses to divert flow away from main class logic, or return false to continue after
        // I know, inversion of prefix patching, I might reverse but Im so used to this already. sue me.

        /// <summary>
        /// Intercept changing of AI modes at the very beginning. Useful if you want to preclude certain behaviors altogether, such as
        /// a wolf that won't attack by turning any "attack", "stalk", "HoldGround", etc state into flee
        /// </summary>
        /// <param name="mode">Incoming AiMode</param>
        /// <param name="newMode">New AiMode to inject</param>
        /// <returns>Return false to allow original method to interpret newMode as if it was the original mode; return true to force set new mode</returns>
        protected virtual bool PreprocesSetAiModeCustom(AiMode mode, out AiMode newMode)
        {
            newMode = AiMode.None;
            return false;
        }

        /// <summary>
        /// Allows intercepting of preprocessing logic. 
        /// </summary>
        /// <returns>Return true to prevent top level preprocessing</returns>
        protected virtual bool PreProcessCustom() => false;

        /// <summary>
        /// Allows intercepting of current mode processing logic. Necessary if you are using any of the new AI states, as well as useful if you want to override any base behaviors like attacking or wandering
        /// </summary>
        /// <returns>Return true to prevent handling of base modes. Doesnt matter as much with custom modes, very important to return false if you are overriding base behavior in any way!</returns>
        protected virtual bool ProcessCustom() => false;

        /// <summary>
        /// Allows intercepting of postprocessing logic. 
        /// </summary>
        /// <returns>Return true to prevent top level postprocessing</returns>
        protected virtual bool PostProcessCustom() => false;

        /// <summary>
        /// Allows intercepting of on-enter events for base methods and functionality for on-enter events with custom ai modes
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>Return true to prevent base on-enter event firing. Especially important if it's not a base AiMode.</returns>
        protected virtual bool EnterAiModeCustom(AiMode mode) => false;


        /// <summary>
        /// Allows intercepting of on-exit events for base methods and functionality for on-exit events with custom ai modes
        /// </summary>
        /// <param name="mode"></param>
        /// <returns>Return true to prevent base on-exit event firing. Especially important if it's not a base AiMode.</returns>
        protected virtual bool ExitAiModeCustom(AiMode mode) => false;


        /// <summary>
        /// Allows intercepting of top-level application of imposter status to an AI. 
        /// Classic logic sets m_Imposter to true and disables collider. 
        /// Return true and do your own logic to change this.
        /// </summary>
        /// <returns>Return true to prevent overlap with default behavior if needed</returns>
        protected virtual bool ApplyImposterStatusCustom() => false;


        /// <summary>
        /// Allows intercepting of top-level classic camera distance check for ai imposter qualification.
        /// Intercept or inject logic here to affect whether the Ai is considered an imposter at any point.
        /// This is the ideal place to create a never-imposter AI by returning true with isImposter set to false.
        /// </summary>
        /// <returns>Return true to abort camera distance check in favor of your own result</returns>
        protected virtual bool TestIsImposterCustom(out bool isImposter)
        {
            isImposter = false;
            return false;
        }


        /// <summary>
        /// Allows intercepting of top-level first frame logic. 
        /// Vanilla logic applies difficulty modifiers and sticks character to ground if not dead.
        /// </summary>
        /// <returns>Return true to prevent base logic from firing</returns>
        protected virtual bool FirstFrameCustom() => false;


        /// <summary>
        /// Allows intercepting of base game AiAnimationState mapping from input AiMode, and for functional routing of custom ai modes to existing (or new?) AiAnimationState values.
        /// </summary>
        /// <param name="mode">Incoming mode. Usually CurrentMode but leaving the parameter open for calculation purposes</param>
        /// <param name="overrideState">Return your own state here. Required for custom states, optionally can override base game states as well.</param>
        /// <returns>Return true to override base mapping with your own overrideState</returns>
        protected virtual bool GetAiAnimationStateCustom(AiMode mode, out AiAnimationState overrideState)
        {
            overrideState = AiAnimationState.Invalid;
            return false;
        }

        /// <summary>
        /// Allows intercepting of base game move state flag mapping from input AiMode, and for functional routing of custom ai modes to move state flag.
        /// Setting this to false will preclude any AI movement, be careful!
        /// </summary>
        /// <param name="mode">Incoming mode. Usually CurrentMode but leaving the parameter open for calculation purposes</param>
        /// <param name="overrideState">Return your own preference here</param>
        /// <returns>Return true to override base mapping with your own move state</returns>
        protected virtual bool IsMoveStateCustom(AiMode mode, out bool isMoveState)
        {
            isMoveState = false;
            return false;
        }

        /// <summary>
        /// Allows intercepting of base game logic for wound processing. Vanilla logic increments BaseAi.m_ElapsedWoundedMinutes.
        /// </summary>
        /// <param name="deltaTime">frame time</param>
        /// <returns>return true to prevent base game logic from firing</returns>
        protected bool ProcessWoundsCustom(float deltaTime) => false;


        /// <summary>
        /// Allows intercepting of base game logic for bleeding out. Vanilla logic increments BaseAi.m_ElapsedWoundedMinutes.
        /// </summary>
        /// <param name="deltaTime">frame time</param>
        /// <returns>return true to prevent base game logic from firing</returns>
        protected bool ProcessBleedingOutCustom(float deltaTime) => false;


        /// <summary>
        /// Allows intercepting of base game logic for bleed out qualification. 
        /// Vanilla logic is surprisingly obfuscated, but we all pretty much know that Moose (and i think cougar?) can't bleed out in vanilla.
        /// </summary>
        /// <param name="canBleedOut">Return your own value here</param>
        /// <returns>Return true to intercept call and return your own instead</returns>
        protected virtual bool CanBleedOutCustom(out bool canBleedOut)
        {
            canBleedOut = false;
            return false;
        }
    }
}
