using Il2Cpp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonsieurMeh.Mods.TLD.LegendaryWolves
{
    [Flags]
    public enum AiModeFlags : uint
    {
        None = 1U << (int)AiMode.None, //0
        Attack = 1U << (int)AiMode.Attack, //1
        Dead = 1U << (int)AiMode.Dead, //2
        Feeding = 1U << (int)AiMode.Feeding, //3
        Flee = 1U << (int)AiMode.Flee, //4
        FollowWaypoints = 1U << (int)AiMode.FollowWaypoints, //5
        HoldGround = 1U << (int)AiMode.HoldGround, //6
        Idle = 1U << (int)AiMode.Idle, //7
        Investigate = 1U << (int)AiMode.Investigate, //8
        InvestigateFood = 1U << (int)AiMode.InvestigateFood, //9
        InvestigateSmell = 1U << (int)AiMode.InvestigateSmell, //a (10)
        Rooted = 1U << (int)AiMode.Rooted, //b (11)
        Sleep = 1U << (int)AiMode.Sleep, //c (12)
        Stalking = 1U << (int)AiMode.Stalking, //d (13)
        Struggle = 1U << (int)AiMode.Struggle, //e (14)
        Wander = 1U << (int)AiMode.Wander, //f (15)
        WanderPaused = 1U << (int)AiMode.WanderPaused, //10 (16)
        GoToPoint = 1U << (int)AiMode.GoToPoint, //11 (17)
        InteractWithProp = 1U << (int)AiMode.InteractWithProp, //12 (18)
        ScriptedSequence = 1U << (int)AiMode.ScriptedSequence, //13 (19)
        Stunned = 1U << (int)AiMode.Stunned, //14 (20)
        ScratchingAntlers = 1U << (int)AiMode.ScratchingAntlers, //15 (21)
        PatrolPointsOfInterest = 1U << (int)AiMode.PatrolPointsOfInterest, //16 (22)
        HideAndSeek = 1U << (int)AiMode.HideAndSeek, //17 (23)
        JoinPack = 1U << (int)AiMode.JoinPack, //18 (24)
        PassingAttack = 1U << (int)AiMode.PassingAttack, //19 (25) 
        Howl = 1U << (int)AiMode.Howl, //1a (26)
        Disabled = 1U << (int)AiMode.Disabled, //1b (27)


        EarlyOutMaybeHoldGround =
              None
            | Dead
            | Feeding
            | Flee
            | HoldGround
            | Idle
            | Rooted
            | Sleep
            | Struggle
            | WanderPaused
            | GoToPoint
            | InteractWithProp
            | ScriptedSequence
            | Stunned
            | ScratchingAntlers
            | PatrolPointsOfInterest
            | JoinPack
            | PassingAttack
            | Howl
            | Disabled,
    }

    public interface ICustomAi
    {
        BaseAi BaseAi { get; }
        void Update();
        void Augment();
        void UnAugment();
    }
}
