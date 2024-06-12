using System;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class DRG
{
    public const byte ClassID = 4;
    public const byte JobID = 22;

    public const uint
        // Single Target
        TrueThrust = 75,
        VorpalThrust = 78,
        Disembowel = 87,
        FullThrust = 84,
        ChaosThrust = 88,
        PiercingTalon = 90,
        HeavensThrust = 25771,
        ChaoticSpring = 25772,
        WheelingThrust = 3556,
        FangAndClaw = 3554,
        RaidenThrust = 16479,
        Drakesbane = 36952,
        LanceBarrage = 36954,
        SpiralBlow = 36955,
        // AoE
        DoomSpike = 86,
        SonicThrust = 7397,
        CoerthanTorment = 16477,
        DraconianFury = 25770,
        // Combined
        Geirskogul = 3555,
        Nastrond = 7400,
        // Jumps
        Jump = 92,
        SpineshatterDive = 95,
        DragonfireDive = 96,
        HighJump = 16478,
        MirageDive = 7399,
        // Dragon
        Stardiver = 16480,
        WyrmwindThrust = 25773,
        RiseOfTheDragon = 36953,
        Starcross = 36956,
        // Buff abilities
        LifeSurge = 83,
        LanceCharge = 85,
        DragonSight = 7398,
        BattleLitany = 3557;

    public static class Buffs
    {
        public const ushort
            LifeSurge = 116,
            SharperFangAndClaw = 802,
            EnhancedWheelingThrust = 803,
            DiveReady = 1243,
            DraconianFire = 1863,
            LanceCharge = 1864,
            RightEye = 1910,
            PowerSurge = 2720,
            NastrondReady = 3844,
            DragonsFlight = 3845,
            StarcrossReady = 3846;
    }

    public static class Debuffs
    {
        public const ushort
            ChaosThrust = 118,
            ChaoticSpring = 2719;
    }

    public static class Levels
    {
        public const byte
            VorpalThrust = 4,
            LifeSurge = 6,
            PiercingTalon = 15,
            Disembowel = 18,
            FullThrust = 26,
            LanceCharge = 30,
            Jump = 30,
            SpineshatterDive = 45,
            DragonfireDive = 50,
            ChaosThrust = 50,
            BattleLitany = 52,
            HeavensThrust = 86,
            ChaoticSpring = 86,
            FangAndClaw = 56,
            WheelingThrust = 58,
            Geirskogul = 60,
            SonicThrust = 62,
            Drakesbane = 64,
            DragonSight = 66,
            MirageDive = 68,
            LifeOfTheDragon = 70,
            CoerthanTorment = 72,
            HighJump = 74,
            RaidenThrust = 76,
            Stardiver = 80,
            WyrmwindThrust = 90;
    }
}

internal abstract class DragoonCombo : CustomCombo
{
    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        DelayDives = 1 << 2,
        DelayRoot = 1 << 3,
    }

    protected static uint GetRotationAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var weaveAction = GetWeaveAction(lastComboMove, comboTime, level, options);
        if (weaveAction > 0)
            return weaveAction;

        return GetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static uint GetWeaveAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<DRGGauge>();
        bool hasHighPriorityLateWeave = false;

        if (Gcd() < 0.6)
            return 0;

        if (options.HasFlag(GetActionOptions.DelayBurst))
            return 0;

        if (!HasEffect(DRG.Buffs.PowerSurge)) // i.e. opener
        {
            if (!options.HasFlag(GetActionOptions.MultiTarget) || level >= DRG.Levels.SonicThrust)
                return 0;
        }

        if (level >= DRG.Levels.LanceCharge && CanWeaveWithoutClip(DRG.LanceCharge))
        {
            if (level < DRG.Levels.BattleLitany
                    || GetCooldown(DRG.BattleLitany).CooldownRemaining < 5
                    || GetCooldown(DRG.BattleLitany).CooldownRemaining > 55)
                return DRG.LanceCharge;
        }

        if (level >= DRG.Levels.BattleLitany)
        {
            if (CanWeaveWithoutClip(DRG.BattleLitany) && GetCooldown(DRG.LanceCharge).CooldownElapsed > (2.5 - Gcd()))
                return DRG.BattleLitany;

            if (IsOffCooldown(DRG.BattleLitany))
                return 0;
        }

        if (level >= DRG.Levels.Geirskogul)
        {
            if (CanWeaveWithoutClip(DRG.Geirskogul))
                return DRG.Geirskogul;
            else if (CanWeave(DRG.Geirskogul))
                hasHighPriorityLateWeave = true;
        }

        if (level >= DRG.Levels.LifeSurge && !HasEffect(DRG.Buffs.LifeSurge) && CanWeaveWithoutClip(DRG.LifeSurge))
        {
            var nextGcd = GetGcdAction(lastComboMove, comboTime, level, options);
            var isHeavensThrust = nextGcd == OriginalHook(DRG.HeavensThrust);
            var isCoerthanTorment = nextGcd == DRG.CoerthanTorment;
            var isDrakesbane = nextGcd == DRG.Drakesbane;

            if (GetCooldown(DRG.LifeSurge).MaxCharges >= 2)
            {
                if (HasEffect(DRG.Buffs.LanceCharge) && (isHeavensThrust || isDrakesbane || isCoerthanTorment))
                    return DRG.LifeSurge;

                if (TimeToCap(DRG.LifeSurge, 40) < 25 && (isHeavensThrust || isDrakesbane))
                    return DRG.LifeSurge;
            }
            else
            {
                if (isHeavensThrust || isDrakesbane || isCoerthanTorment)
                    return DRG.LifeSurge;

                if (level < DRG.Levels.FullThrust && nextGcd == DRG.VorpalThrust)
                    return DRG.LifeSurge;

                if (level < DRG.Levels.CoerthanTorment && nextGcd == DRG.SonicThrust)
                    return DRG.LifeSurge;

                if (level < DRG.Levels.SonicThrust && nextGcd == DRG.DoomSpike)
                    return DRG.LifeSurge;
            }
        }

        if (!options.HasFlag(GetActionOptions.DelayRoot))
        {
            if (level >= DRG.Levels.HighJump)
            {
                if (CanWeaveWithoutClip(DRG.HighJump))
                    return DRG.HighJump;
                else if (CanWeave(DRG.HighJump))
                    hasHighPriorityLateWeave = true;
            }
            else if (level >= DRG.Levels.Jump)
            {
                if (CanWeaveWithoutClip(DRG.Jump))
                    return DRG.Jump;
                else if (CanWeave(DRG.Jump))
                    hasHighPriorityLateWeave = true;
            }

            if (level >= DRG.Levels.DragonfireDive && CanWeaveWithoutClip(DRG.DragonfireDive, 0.8f))
            {
                if (!hasHighPriorityLateWeave && !options.HasFlag(GetActionOptions.DelayDives))
                    return DRG.DragonfireDive;
            }

            if (level >= DRG.Levels.Stardiver && gauge.IsLOTDActive && CanWeaveWithoutClip(DRG.Stardiver, 1.5f))
            {
                if (!hasHighPriorityLateWeave && !options.HasFlag(GetActionOptions.DelayDives))
                    return DRG.Stardiver;
            }
        }

        if (HasEffect(DRG.Buffs.StarcrossReady) && CanWeaveWithoutClip(DRG.Starcross))
            return DRG.Starcross;

        if (HasEffect(DRG.Buffs.DragonsFlight) && CanWeaveWithoutClip(DRG.RiseOfTheDragon))
            return DRG.RiseOfTheDragon;

        if (HasEffect(DRG.Buffs.NastrondReady) && CanWeaveWithoutClip(DRG.Nastrond))
            return DRG.Nastrond;

        if (HasEffect(DRG.Buffs.DiveReady))
            return DRG.MirageDive;

        if (level >= DRG.Levels.WyrmwindThrust && gauge.FirstmindsFocusCount == 2)
            return DRG.WyrmwindThrust;

        return 0;
    }

    protected static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
            return GetMultiTargetGcdAction(lastComboMove, comboTime, level, options);

        return GetSingleTargetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static uint GetSingleTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (level >= DRG.Levels.PiercingTalon && !IsActionInRange(DRG.TrueThrust))
        {
            if (options.HasFlag(GetActionOptions.DelayBurst) || options.HasFlag(GetActionOptions.DelayDives))
                return DRG.PiercingTalon;
        }

        if (comboTime > 0)
        {
            uint gcdsBeforeNextRefresh;
            if (level >= DRG.Levels.Drakesbane)
                gcdsBeforeNextRefresh = 5;
            else if (level >= DRG.Levels.FangAndClaw)
                gcdsBeforeNextRefresh = 4;
            else if (level >= DRG.Levels.FullThrust)
                gcdsBeforeNextRefresh = 3;
            else
                gcdsBeforeNextRefresh = 2;

            var buff = GetEffectRemainingTime(DRG.Buffs.PowerSurge);

            if (level >= DRG.Levels.Drakesbane && (lastComboMove == DRG.FangAndClaw || lastComboMove == DRG.WheelingThrust))
                return OriginalHook(DRG.Drakesbane);

            if (level >= DRG.Levels.FangAndClaw && (lastComboMove == DRG.FullThrust || lastComboMove == DRG.HeavensThrust))
                return OriginalHook(DRG.FangAndClaw);

            if (level >= DRG.Levels.WheelingThrust && (lastComboMove == DRG.ChaosThrust || lastComboMove == DRG.ChaoticSpring))
                return OriginalHook(DRG.WheelingThrust);

            if (level >= DRG.Levels.FullThrust && (lastComboMove == DRG.VorpalThrust || lastComboMove == DRG.LanceBarrage))
                return OriginalHook(DRG.FullThrust);

            if (level >= DRG.Levels.ChaosThrust && (lastComboMove == DRG.Disembowel || lastComboMove == DRG.SpiralBlow))
                return OriginalHook(DRG.ChaosThrust);

            if (lastComboMove == DRG.TrueThrust || lastComboMove == DRG.RaidenThrust)
            {
                if (level >= DRG.Levels.Disembowel && buff - Gcd() < gcdsBeforeNextRefresh * 2.5)
                    return OriginalHook(DRG.Disembowel);

                if (level >= DRG.Levels.VorpalThrust)
                    return OriginalHook(DRG.VorpalThrust);
            }
        }

        return OriginalHook(DRG.TrueThrust);
    }

    private static uint GetMultiTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (comboTime > 0)
        {
            if (level >= DRG.Levels.CoerthanTorment && lastComboMove == DRG.SonicThrust)
                return DRG.CoerthanTorment;

            if (level >= DRG.Levels.SonicThrust && (lastComboMove == DRG.DoomSpike || lastComboMove == DRG.DraconianFury))
                return DRG.SonicThrust;
        }

        return OriginalHook(DRG.DoomSpike);
    }

    private static float TimeToCap(uint actionID, float cooldown)
    {
        var cd = GetCooldown(actionID);

        if (cd.MaxCharges == cd.RemainingCharges)
            return 0;

        var missingCharges = cd.MaxCharges - cd.RemainingCharges;
        return ((missingCharges - 1) * cooldown) + cd.ChargeCooldownRemaining;
    }

    private static double GetPositionalAngle()
    {
        if (LocalPlayer is null || CurrentTarget is null)
            return 0;

        var dx = CurrentTarget.Position.X - LocalPlayer.Position.X;
        var dz = CurrentTarget.Position.Z - LocalPlayer.Position.Z;

        var angle = Math.Atan(dx / dz);
        if (dz > 0) angle += Math.PI;
        if (angle > Math.PI) angle -= Math.PI * 2;

        var relAngle = angle - CurrentTarget.Rotation;
        if (relAngle > Math.PI) relAngle -= Math.PI * 2;
        if (relAngle < -Math.PI) relAngle += Math.PI * 2;

        return relAngle;
    }

    private static bool IsFlank()
    {
        var angle = Math.Abs(GetPositionalAngle());
        return angle > Math.PI / 4 && angle < Math.PI * 3 / 4;
    }

    private static bool IsRear()
    {
        var angle = Math.Abs(GetPositionalAngle());
        return angle > Math.PI * 3 / 4;
    }

    private static unsafe bool IsActionInRange(uint actionId)
    {
        if (LocalPlayer == null || LocalPlayer.TargetObject == null)
            return false;

        var source = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.Address;
        var target = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.TargetObject.Address;
        return FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionInRangeOrLoS(actionId, source, target) != 566;
    }

    private static float Gcd()
    {
        return GetCooldown(DRG.TrueThrust).CooldownRemaining;
    }

    private static bool IsEarlyWeave()
    {
        return Gcd() > 1.0;
    }

    private static bool CanWeave(uint actionID, float anim = 0.6f)
    {
        if (Gcd() < anim)
            return false;

        if (IsOffCooldown(actionID))
            return true;

        return GetCooldown(actionID).CooldownRemaining + anim < Gcd();
    }

    private static bool CanWeaveWithoutClip(uint actionID, float anim = 0.6f)
    {
        if (Gcd() < anim)
            return false;

        if (IsOffCooldown(actionID) || HasCharges(actionID))
            return true;

        if (IsEarlyWeave())
            return false;

        return GetCooldown(actionID).CooldownRemaining + anim < Gcd();
    }
}

internal class DragoonRotation : DragoonCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.DragoonOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == PLD.RageOfHalone)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayDives);

        if (actionID == PLD.Atonement)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayRoot);

        // Display
        if (actionID == PLD.GoringBlade)
            return GetGcdAction(lastComboMove, comboTime, level, GetActionOptions.None);

        // AoE
        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        return actionID;
    }
}
