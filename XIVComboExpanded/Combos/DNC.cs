using System;
using System.Linq;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class DNC
{
    public const byte JobID = 38;

    public const uint
        // Single Target
        Cascade = 15989,
        Fountain = 15990,
        ReverseCascade = 15991,
        Fountainfall = 15992,
        // AoE
        Windmill = 15993,
        Bladeshower = 15994,
        RisingWindmill = 15995,
        Bloodshower = 15996,
        // Dancing
        StandardStep = 15997,
        TechnicalStep = 15998,
        Tillana = 25790,
        LastDance = 36983,
        FinishingMove = 36984,
        // Fans
        FanDance1 = 16007,
        FanDance2 = 16008,
        FanDance3 = 16009,
        FanDance4 = 25791,
        // Other
        SecondWind = 7541,
        ArmsLength = 7548,
        SaberDance = 16005,
        EnAvant = 16010,
        Devilment = 16011,
        ShieldSamba = 16012,
        Flourish = 16013,
        Improvisation = 16014,
        CuringWaltz = 16015,
        StarfallDance = 25792,
        DanceOfTheDawn = 36985;

    public static class Buffs
    {
        public const ushort
            FlourishingSymmetry = 3017,
            FlourishingFlow = 3018,
            FlourishingFinish = 2698,
            FlourishingStarfall = 2700,
            SilkenSymmetry = 2693,
            SilkenFlow = 2694,
            StandardStep = 1818,
            TechnicalStep = 1819,
            ThreefoldFanDance = 1820,
            StandardFinish = 1821,
            TechnicalFinish = 1822,
            Devilment = 1825,
            FourfoldFanDance = 2699,
            LastDanceReady = 3867,
            FinishingMoveReady = 3868,
            DanceOfTheDawnReady = 3869;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            Cascade = 1,
            Fountain = 2,
            Windmill = 15,
            StandardStep = 15,
            ReverseCascade = 20,
            Bladeshower = 25,
            RisingWindmill = 35,
            Fountainfall = 40,
            Bloodshower = 45,
            CuringWaltz = 52,
            ShieldSamba = 56,
            Devilment = 62,
            FanDance3 = 66,
            TechnicalStep = 70,
            Flourish = 72,
            Tillana = 82,
            FanDance4 = 86,
            StarfallDance = 90,
            LastDance = 92,
            FinishingMove = 96,
            DanceOfTheDawn = 100;
    }
}

internal abstract class DancerCombo : CustomCombo
{
    private const float GcdRecast = 2.5f;
    private const float EarlyWeaveGcdRemainingThreshold = 1.0f;
    private const float DefaultClipThreshold = 0.6f;

    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        UseResources = 1 << 2,
        UseShieldSamba = 1 << 3,
        UseCuringWaltz = 1 << 4,
        UseArmsLength = 1 << 5,
        UseSecondWind = 1 << 6,
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
        var gauge = GetJobGauge<DNCGauge>();
        var fanDance12 = !options.HasFlag(GetActionOptions.MultiTarget) ? DNC.FanDance1 : DNC.FanDance2;

        if (level >= DNC.Levels.FinishingMove && IsDevilmentUsed() && !HasEffect(DNC.Buffs.StandardFinish) && IsOffCooldown(DNC.Flourish))
            return DNC.Flourish;

        if (Gcd() < DefaultClipThreshold)
            return 0;

        if (level >= DNC.Levels.Devilment && CanWeaveWithoutBlock(DNC.Devilment))
        {
            if (HasEffect(DNC.Buffs.TechnicalFinish))
                return DNC.Devilment;

            if (level < DNC.Levels.TechnicalStep && !options.HasFlag(GetActionOptions.DelayBurst))
                return DNC.Devilment;
        }

        if (level >= DNC.Levels.FanDance3 && HasEffect(DNC.Buffs.ThreefoldFanDance))
            return DNC.FanDance3;

        if (gauge.Feathers == 4)
            return fanDance12;

        if (HasEffect(DNC.Buffs.FourfoldFanDance) && IsActionInRange(DNC.FanDance4))
            return DNC.FanDance4;

        if (level >= DNC.Levels.Flourish && CanWeaveWithoutBlock(DNC.Flourish))
        {
            if (IsDevilmentUsed() || GetCooldown(DNC.Devilment).CooldownRemaining > 50)
                return DNC.Flourish;

            if (options.HasFlag(GetActionOptions.UseResources))
                return DNC.Flourish;
        }

        if (level >= DNC.Levels.ShieldSamba && options.HasFlag(GetActionOptions.UseShieldSamba) && CanWeaveWithoutBlock(DNC.ShieldSamba))
            return DNC.ShieldSamba;

        if (level >= DNC.Levels.CuringWaltz && options.HasFlag(GetActionOptions.UseCuringWaltz) && CanWeaveWithoutBlock(DNC.CuringWaltz))
            return DNC.CuringWaltz;

        if (options.HasFlag(GetActionOptions.UseArmsLength) && CanWeaveWithoutBlock(DNC.ArmsLength))
            return DNC.ArmsLength;

        if (options.HasFlag(GetActionOptions.UseSecondWind) && CanWeaveWithoutBlock(DNC.SecondWind))
            return DNC.SecondWind;

        if (gauge.Feathers > 0 && CanWeaveWithoutBlock(fanDance12))
        {
            if (IsDevilmentUsed() || options.HasFlag(GetActionOptions.UseResources))
                return fanDance12;
        }

        return 0;
    }

    protected static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<DNCGauge>();
        var isTillanaReady = HasEffect(DNC.Buffs.FlourishingFinish) && GetEffectRemainingTime(DNC.Buffs.FlourishingFinish) < GetCooldown(DNC.Devilment).CooldownRemaining;

        if (gauge.IsDancing)
        {
            if (HasEffect(DNC.Buffs.StandardStep) && gauge.CompletedSteps == 2)
                return OriginalHook(DNC.StandardStep);

            if (HasEffect(DNC.Buffs.TechnicalStep) && gauge.CompletedSteps == 4)
                return OriginalHook(DNC.TechnicalStep);

            return gauge.NextStep;
        }

        if (level >= DNC.Levels.TechnicalStep && IsGcdActionOffCooldown(DNC.TechnicalStep) && !options.HasFlag(GetActionOptions.DelayBurst) && InCombat())
            return DNC.TechnicalStep;

        var isWaitForFlourish = level >= DNC.Levels.FinishingMove && IsDevilmentUsed() && !HasEffect(DNC.Buffs.FlourishingSymmetry);
        if (level >= DNC.Levels.StandardStep && IsGcdActionOffCooldown(DNC.StandardStep) && !isWaitForFlourish)
        {
            if (HasEffect(DNC.Buffs.FinishingMoveReady))
            {
                if (IsTargetInRadius(15))
                    return DNC.FinishingMove;
            }
            else if (!options.HasFlag(GetActionOptions.DelayBurst))
            {
                return DNC.StandardStep;
            }
        }

        if (HasEffect(DNC.Buffs.LastDanceReady) && GetEffectRemainingTime(DNC.Buffs.LastDanceReady) - Gcd() < GcdRecast)
            return DNC.LastDance;

        if (gauge.Esprit >= 50)
        {
            if (IsDevilmentUsed() || isTillanaReady || gauge.Esprit >= 80 || options.HasFlag(GetActionOptions.UseResources))
                return OriginalHook(DNC.SaberDance);

            if (!HasEffect(DNC.Buffs.DanceOfTheDawnReady) && HasEffect(DNC.Buffs.FlourishingFinish))
                return OriginalHook(DNC.SaberDance);
        }

        if (HasEffect(DNC.Buffs.LastDanceReady))
        {
            var canHoldForDevilment = GetEffectRemainingTime(DNC.Buffs.LastDanceReady) > GetCooldown(DNC.Devilment).CooldownRemaining + (2 * GcdRecast);
            if (IsDevilmentUsed() || !canHoldForDevilment || options.HasFlag(GetActionOptions.UseResources))
                return DNC.LastDance;
        }

        if (isTillanaReady && gauge.Esprit < 50 && IsTargetInRadius(15))
        {
            var isDevilmentEndSoon = GetEffectRemainingTime(DNC.Buffs.Devilment) - Gcd() < GcdRecast * 3;
            if (gauge.Esprit <= 40 || !IsDevilmentUsed() || isDevilmentEndSoon)
                return DNC.Tillana;
        }

        if (HasEffect(DNC.Buffs.FlourishingStarfall))
            return DNC.StarfallDance;

        return !options.HasFlag(GetActionOptions.MultiTarget) ?
            GetSingleTargetComboAction(lastComboMove, comboTime, level) :
            GetMultiTargetComboAction(lastComboMove, comboTime, level);
    }

    private static uint GetSingleTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (level >= DNC.Levels.Fountainfall && (HasEffect(DNC.Buffs.FlourishingFlow) || HasEffect(DNC.Buffs.SilkenFlow)))
            return DNC.Fountainfall;

        if (level >= DNC.Levels.ReverseCascade && (HasEffect(DNC.Buffs.FlourishingSymmetry) || HasEffect(DNC.Buffs.SilkenSymmetry)))
            return DNC.ReverseCascade;

        if (lastComboMove == DNC.Cascade && level >= DNC.Levels.Fountain)
            return DNC.Fountain;

        return DNC.Cascade;
    }

    private static uint GetMultiTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (level >= DNC.Levels.Bloodshower && (HasEffect(DNC.Buffs.FlourishingFlow) || HasEffect(DNC.Buffs.SilkenFlow)))
            return DNC.Bloodshower;

        if (level >= DNC.Levels.RisingWindmill && (HasEffect(DNC.Buffs.FlourishingSymmetry) || HasEffect(DNC.Buffs.SilkenSymmetry)))
            return DNC.RisingWindmill;

        if (lastComboMove == DNC.Windmill && level >= DNC.Levels.Bladeshower)
            return DNC.Bladeshower;

        return DNC.Windmill;
    }

    private static bool IsDevilmentUsed()
    {
        return IsOnCooldown(DNC.Devilment) && GetCooldown(DNC.Devilment).CooldownElapsed < 20;
    }

    private static float Gcd()
    {
        return GetCooldown(DNC.Cascade).CooldownRemaining;
    }

    private static float GcdElapsed()
    {
        return GcdRecast - Gcd();
    }

    private static bool IsGcdActionOffCooldown(uint actionID)
    {
        return GetCooldown(actionID).CooldownRemaining <= Gcd();
    }

    private static bool IsEarlyWeave()
    {
        return Gcd() > EarlyWeaveGcdRemainingThreshold;
    }

    private static bool CanWeave(uint actionID, uint gcdCount = 0, bool noBlock = false, float clipThreshold = DefaultClipThreshold)
    {
        var cd = GetCooldown(actionID);
        var cdRemaining = cd.HasCharges
            ? cd.RemainingCharges == 0
                ? cd.ChargeCooldownRemaining
                : 0
            : cd.CooldownRemaining;

        if (gcdCount == 0 && noBlock)
        {
            var canEarlyWeaveAfterCd = Gcd() - cdRemaining > EarlyWeaveGcdRemainingThreshold;
            if (IsEarlyWeave() && !canEarlyWeaveAfterCd)
                return false;
        }

        return cdRemaining + clipThreshold <= Gcd() + (gcdCount * GcdRecast);
    }

    private static bool CanWeaveWithoutBlock(uint actionID)
    {
        return CanWeave(actionID, noBlock: true);
    }

    private static unsafe bool IsActionInRange(uint actionId)
    {
        if (LocalPlayer == null || LocalPlayer.TargetObject == null)
            return false;

        var source = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.Address;
        var target = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.TargetObject.Address;
        return FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionInRangeOrLoS(actionId, source, target) != 566;
    }

    private static bool IsTargetInRadius(float radius)
    {
        if (LocalPlayer == null || LocalPlayer.TargetObject == null)
            return false;

        var target = LocalPlayer.TargetObject;
        var dx = LocalPlayer.Position.X - target.Position.X;
        var dz = LocalPlayer.Position.Z - target.Position.Z;
        var dist = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dz, 2));

        return dist - target.HitboxRadius < radius;
    }
}

internal class DancerRotation : DancerCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.DancerOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == PLD.SpiritsWithin)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseResources);

        // Display
        if (actionID == PLD.GoringBlade)
            return GetGcdAction(lastComboMove, comboTime, level, GetActionOptions.None);

        // AoE
        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        if (actionID == PLD.CircleOfScorn)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.UseResources);

        // Utility
        if (actionID == PLD.FightOrFlight)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseShieldSamba);

        if (actionID == PLD.Requiescat)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseCuringWaltz);

        if (actionID == PLD.HolySpirit)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseSecondWind);

        if (actionID == PLD.HolyCircle)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseArmsLength);

        return actionID;
    }
}

internal class DancerStandardStep : DancerCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.DancerOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if (actionID == DNC.StandardStep)
        {
            var gauge = GetJobGauge<DNCGauge>();

            if (gauge.IsDancing && HasEffect(DNC.Buffs.StandardStep))
            {
                if (gauge.CompletedSteps < 2)
                    return gauge.NextStep;

                return DNC.FinishingMove;
            }
        }

        return actionID;
    }
}