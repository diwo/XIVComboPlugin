using System;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class MCH
{
    public const byte JobID = 31;

    public const uint
        // Single target
        CleanShot = 2873,
        HeatedCleanShot = 7413,
        SplitShot = 2866,
        HeatedSplitShot = 7411,
        SlugShot = 2868,
        HeatedSlugshot = 7412,
        // Charges
        GaussRound = 2874,
        Ricochet = 2890,
        DoubleCheck = 36979,
        Checkmate = 36980,
        // AoE
        SpreadShot = 2870,
        AutoCrossbow = 16497,
        Scattergun = 25786,
        // Rook
        RookAutoturret = 2864,
        RookOverdrive = 7415,
        AutomatonQueen = 16501,
        QueenOverdrive = 16502,
        // Other
        Reassemble = 2876,
        Wildfire = 2878,
        Detonator = 16766,
        Hypercharge = 17209,
        BarrelStabilizer = 7414,
        HeatBlast = 7410,
        BlazingShot = 36978,
        HotShot = 2872,
        Drill = 16498,
        Bioblaster = 16499,
        AirAnchor = 16500,
        Chainsaw = 25788,
        Excavator = 36981,
        FullMetal = 36982,
        Tactician = 16889,
        Dismantle = 2887,
        ArmsLength = 7548,
        SecondWind = 7541;

    public static class Buffs
    {
        public const ushort
            Reassembled = 851,
            Overheated = 2688,
            HyperchargeReady = 3864,
            ExcavatorReady = 3865,
            FullMetalPrepared = 3866;
    }

    public static class Debuffs
    {
        public const ushort
            Wildfire = 861,
            Bioblaster = 1866;
    }

    public static class Levels
    {
        public const byte
            SlugShot = 2,
            HotShot = 4,
            Reassemble = 10,
            GaussRound = 15,
            SpreadShot = 18,
            CleanShot = 26,
            Hypercharge = 30,
            HeatBlast = 35,
            RookOverdrive = 40,
            Wildfire = 45,
            Ricochet = 50,
            AutoCrossbow = 52,
            HeatedSplitShot = 54,
            Tactician = 56,
            Drill = 58,
            HeatedSlugshot = 60,
            Dismantle = 62,
            HeatedCleanShot = 64,
            BarrelStabilizer = 66,
            BlazingShot = 68,
            Bioblaster = 72,
            ChargedActionMastery = 74,
            AirAnchor = 76,
            QueenOverdrive = 80,
            Chainsaw = 90,
            DoubleCheck = 92,
            CheckMate = 92,
            EnhancedMultiweapon1 = 94,
            EnhancedMultiweapon2 = 96,
            Excavator = 96,
            FullMetal = 100;
    }
}

internal abstract class MachinistCombo : CustomCombo
{
    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 1,
        DelayBurst = 1 << 2,
        UseTactician = 1 << 3,
        UseDismantle = 1 << 4,
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
        if (Gcd() < 0.6)
            return 0;

        var gauge = GetJobGauge<MCHGauge>();
        var nextGcd = GetGcdAction(lastComboMove, comboTime, level, options);

        if (level >= MCH.Levels.BarrelStabilizer && CanWeaveWithoutClip(MCH.BarrelStabilizer) && !options.HasFlag(GetActionOptions.DelayBurst))
            return MCH.BarrelStabilizer;

        if (level >= MCH.Levels.Reassemble
                && CanWeaveWithoutClip(MCH.Reassemble)
                && ShouldReassemble(nextGcd, level, options)
                && !HasEffect(MCH.Buffs.Reassembled))
        {
            if (TimeToCap(MCH.Reassemble, 55) <= Gcd())
                return MCH.Reassemble;

            if (options.HasFlag(GetActionOptions.MultiTarget))
                return MCH.Reassemble;

            if (GetRaidBuffWindowOffset() > 0)
                return MCH.Reassemble;

            if (GetCooldown(MCH.Reassemble).ChargeCooldownRemaining + GetRaidBuffWindowOffset() < 0)
                return MCH.Reassemble;
        }

        if (level >= MCH.Levels.Wildfire && CanWeaveWithoutClip(MCH.Wildfire)
                && !options.HasFlag(GetActionOptions.DelayBurst) && !options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (gauge.IsOverheated && (level < MCH.Levels.BarrelStabilizer || GetRaidBuffWindowOffset() > 0))
                return MCH.Wildfire;
        }

        if (gauge.Battery >= 50 && !gauge.IsRobotActive && !options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (level < MCH.Levels.BarrelStabilizer)
                return OriginalHook(MCH.AutomatonQueen);

            if (GetRaidBuffWindowOffset() > -5f)
            {
                if (gauge.Battery >= 90)
                    return OriginalHook(MCH.AutomatonQueen);

                var airAnchorSoon = level >= MCH.Levels.AirAnchor && GetCooldown(MCH.AirAnchor).CooldownRemaining < 2.5;
                var chainsawSoon = level >= MCH.Levels.Chainsaw && GetCooldown(MCH.Chainsaw).CooldownRemaining < 2.5;

                if (!airAnchorSoon && !chainsawSoon && !HasEffect(MCH.Buffs.ExcavatorReady))
                    return OriginalHook(MCH.AutomatonQueen);
            }
            else if (ShouldUseBatteryOutsideBuffs(level, lastComboMove))
            {
                return OriginalHook(MCH.AutomatonQueen);
            }
        }

        var hasHyperchargeResource = gauge.Heat >= 50 || HasEffect(MCH.Buffs.HyperchargeReady);
        var timeUntilComboBreak = TimeUntilComboBreak(lastComboMove, comboTime);
        var isNextGcdBigHit = nextGcd switch {
            MCH.AirAnchor or MCH.Drill or MCH.Chainsaw or MCH.Excavator or MCH.FullMetal => true,
            _ => false,
        };

        if (level >= MCH.Levels.Hypercharge && CanWeaveWithoutClip(MCH.Hypercharge)
                && hasHyperchargeResource && !gauge.IsOverheated
                && (timeUntilComboBreak == 0 || timeUntilComboBreak > 10))
        {
            var wildfire = GetTargetEffectRemainingTime(MCH.Debuffs.Wildfire);
            if (wildfire > 0)
            {
                if (!isNextGcdBigHit || wildfire - Gcd() < 7.5)
                    return MCH.Hypercharge;
            }

            if (options.HasFlag(GetActionOptions.DelayBurst) || options.HasFlag(GetActionOptions.MultiTarget))
            {
                if (gauge.Heat + HeatGenerated(nextGcd) > 100 && !HasEffect(MCH.Buffs.Reassembled))
                    return MCH.Hypercharge;
            }
            else if (level >= MCH.Levels.BarrelStabilizer)
            {
                if (GetRaidBuffWindowOffset() > 0)
                {
                    var soonAirAnchor = level >= MCH.Levels.AirAnchor && GetCooldown(MCH.AirAnchor).CooldownRemaining < 5;
                    var soonDrill = level >= MCH.Levels.Drill && TimeToCap(MCH.Drill, 20) < 5;
                    var soonChainsaw = level >= MCH.Levels.Chainsaw && GetCooldown(MCH.Chainsaw).CooldownRemaining < 5;
                    var soonBigHit = soonAirAnchor || soonDrill || soonChainsaw
                            || HasEffect(MCH.Buffs.ExcavatorReady) || HasEffect(MCH.Buffs.FullMetalPrepared);

                    if (!soonBigHit)
                    {
                        if (CanWeaveWithoutClip(MCH.Wildfire))
                            return MCH.Wildfire;
                        else
                            return MCH.Hypercharge;
                    }
                }
                else if (GetRaidBuffWindowOffset() < -5f && ShouldUseHyperchargeOutsideBuffs(level, lastComboMove))
                {
                    var soonAirAnchor = level >= MCH.Levels.AirAnchor && GetCooldown(MCH.AirAnchor).CooldownRemaining < 8;
                    var soonDrill = level >= MCH.Levels.Drill && TimeToCap(MCH.Drill, 20) < 8;
                    var soonChainsaw = level >= MCH.Levels.Chainsaw && GetCooldown(MCH.Chainsaw).CooldownRemaining < 8;

                    if (!soonAirAnchor && !soonDrill && !soonChainsaw)
                        return MCH.Hypercharge;
                }
            }
            else
            {
                if (level >= MCH.Levels.Wildfire && CanWeaveWithoutClip(MCH.Wildfire) && (hasHyperchargeResource || gauge.IsOverheated))
                    return MCH.Wildfire;

                if (!HasEffect(MCH.Buffs.Reassembled))
                {
                    if (gauge.Heat >= 100)
                        return MCH.Hypercharge;

                    if (gauge.Heat >= 50)
                    {
                        if (level < MCH.Levels.Wildfire || GetCooldown(MCH.Wildfire).CooldownRemaining > 30)
                            return MCH.Hypercharge;
                    }
                }
            }
        }

        if (level >= MCH.Levels.Tactician && options.HasFlag(GetActionOptions.UseTactician) && CanWeaveWithoutClip(MCH.Tactician))
            return MCH.Tactician;

        if (level >= MCH.Levels.Dismantle && options.HasFlag(GetActionOptions.UseDismantle) && CanWeaveWithoutClip(MCH.Dismantle))
            return MCH.Dismantle;

        if (options.HasFlag(GetActionOptions.UseArmsLength) && CanWeaveWithoutClip(MCH.ArmsLength))
            return MCH.ArmsLength;

        if (options.HasFlag(GetActionOptions.UseSecondWind) && CanWeaveWithoutClip(MCH.SecondWind))
            return MCH.SecondWind;

        var chargeAtk = GetChargeOGcd(level, 0);
        if (chargeAtk > 0)
            return chargeAtk;

        return 0;
    }

    private static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
            return GetMultiTargetGcdAction(lastComboMove, comboTime, level, options);

        return GetSingleTargetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static uint GetSingleTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<MCHGauge>();

        if (level >= MCH.Levels.HeatBlast && gauge.IsOverheated)
            return OriginalHook(MCH.HeatBlast);

        if (IsUsingComboDuringOpener(level, lastComboMove, comboTime) && !options.HasFlag(GetActionOptions.DelayBurst))
            return GetComboAction(lastComboMove, comboTime, level);

        if (level >= MCH.Levels.AirAnchor && IsGcdOffCooldown(MCH.AirAnchor))
            return MCH.AirAnchor;

        if (level >= MCH.Levels.Drill && IsGcdOffCooldown(MCH.Drill))
        {
            if (TimeToCap(MCH.Drill, 20) <= Gcd())
                return MCH.Drill;
        }

        if (level >= MCH.Levels.HotShot && level < MCH.Levels.AirAnchor && IsGcdOffCooldown(MCH.HotShot))
            return MCH.HotShot;

        if (HasEffect(MCH.Buffs.ExcavatorReady))
            return MCH.Excavator;

        if (level >= MCH.Levels.Chainsaw && IsGcdOffCooldown(MCH.Chainsaw))
        {
            if (GetCooldown(MCH.BarrelStabilizer).CooldownRemaining > 55)
                return MCH.Chainsaw;
        }

        if (HasEffect(MCH.Buffs.FullMetalPrepared))
            return MCH.FullMetal;

        var timeUntilComboBreak = TimeUntilComboBreak(lastComboMove, comboTime);
        if (timeUntilComboBreak > 0 && timeUntilComboBreak - Gcd() < 10)
            return GetComboAction(lastComboMove, comboTime, level);

        if (level >= MCH.Levels.Drill && IsGcdOffCooldown(MCH.Drill))
        {
            if (GetRaidBuffWindowOffset() < -25f)
                return MCH.Drill;

            if (GetRaidBuffWindowOffset() > -5f && !options.HasFlag(GetActionOptions.DelayBurst))
                return MCH.Drill;
        }

        return GetComboAction(lastComboMove, comboTime, level);
    }

    private static uint GetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (lastComboMove == MCH.SlugShot && level >= MCH.Levels.CleanShot)
                return OriginalHook(MCH.CleanShot);

            if (lastComboMove == MCH.SplitShot && level >= MCH.Levels.SlugShot)
                return OriginalHook(MCH.SlugShot);
        }

        return OriginalHook(MCH.SplitShot);
    }

    private static uint GetMultiTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<MCHGauge>();

        if (gauge.IsOverheated)
        {
            if (level >= MCH.Levels.AutoCrossbow)
                return MCH.AutoCrossbow;
            else
                return MCH.HeatBlast;
        }

        if (HasEffect(MCH.Buffs.ExcavatorReady))
            return MCH.Excavator;

        if (level >= MCH.Levels.Chainsaw && IsGcdOffCooldown(MCH.Chainsaw))
        {
            if (GetCooldown(MCH.BarrelStabilizer).CooldownRemaining > 55)
                return MCH.Chainsaw;
        }

        if (level >= MCH.Levels.Bioblaster && IsGcdOffCooldown(MCH.Bioblaster))
        {
            if (!TargetHasEffect(MCH.Debuffs.Bioblaster))
                return MCH.Bioblaster;
        }

        if (level >= MCH.Levels.SpreadShot)
            return OriginalHook(MCH.SpreadShot);

        return GetSingleTargetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static bool ShouldReassemble(uint nextGcd, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
            return nextGcd == MCH.Chainsaw || nextGcd == MCH.Excavator;

        if (level < MCH.Levels.Drill)
            return nextGcd == MCH.CleanShot;

        return nextGcd switch {
            MCH.AirAnchor or MCH.Drill or MCH.Chainsaw or MCH.Excavator => true,
            _ => false,
        };
    }

    private static float GetRaidBuffWindowOffset()
    {
        const float offset = -5f;

        var bs = GetCooldown(MCH.BarrelStabilizer);
        if (!bs.IsCooldown)
            return offset;

        var winElapsed = bs.CooldownElapsed + offset;
        if (winElapsed < 30)
            return winElapsed;

        return -bs.CooldownRemaining + offset;
    }

    private static bool IsUsingComboDuringOpener(byte level, uint lastComboMove, float comboTime)
    {
        var gauge = GetJobGauge<MCHGauge>();

        if (level < MCH.Levels.BarrelStabilizer || level >= MCH.Levels.EnhancedMultiweapon2)
            return false;

        if (gauge.Battery >= 50 || gauge.IsRobotActive)
            return false;

        if (lastComboMove == MCH.CleanShot && TimeSinceLastComboAction(comboTime) < 10)
            return false;

        if (GetRaidBuffWindowOffset() < -5f || GetRaidBuffWindowOffset() > 5)
            return false;

        return true;
    }

    private static float TimeUntilComboBreak(uint lastComboMove, float comboTime)
    {
        return lastComboMove == MCH.CleanShot ? 0f : comboTime;
    }

    private static float TimeSinceLastComboAction(float comboTime)
    {
        return 30 - comboTime;
    }

    private static uint HeatGenerated(uint actionID)
    {
        return actionID switch {
            MCH.SplitShot or MCH.HeatedSplitShot or MCH.SlugShot or MCH.HeatedSlugshot or MCH.CleanShot or MCH.HeatedCleanShot => 5,
            MCH.SpreadShot => 5,
            MCH.Scattergun => 10,
            _ => 0,
        };
    }

    private static bool ShouldUseBatteryOutsideBuffs(byte level, uint lastComboMove)
    {
        if (GetRaidBuffWindowOffset() > -5f)
            return false;

        var gauge = GetJobGauge<MCHGauge>();

        var timeUntilBuffsUse = -GetRaidBuffWindowOffset() - 5f;
        var (airAnchorUses, drillUses, chainsawUses, excavatorUses) = GetBigHitGcdUses(level, timeUntilBuffsUse);
        var nonComboActionUses = airAnchorUses + drillUses + chainsawUses + excavatorUses;
        uint[] comboActionUses = GetComboActionUses(lastComboMove, nonComboActionUses, timeUntilBuffsUse);
        var heatFromCombos = (comboActionUses[0] + comboActionUses[1] + comboActionUses[2]) * 5;
        var heatDuringBuffs = gauge.Heat + heatFromCombos;

        if (heatDuringBuffs >= 100)
        {
            const float heatBlastDuration = 1.5f;
            uint hyperchargeUses = (heatDuringBuffs - 50) / 50;
            var timeInHypercharge = hyperchargeUses * 5 * heatBlastDuration;
            if (gauge.IsOverheated)
                timeInHypercharge += FindEffect(MCH.Buffs.Overheated)!.StackCount * heatBlastDuration;

            comboActionUses = GetComboActionUses(lastComboMove, nonComboActionUses, timeUntilBuffsUse - timeInHypercharge);
        }

        var batteryGenerated = (airAnchorUses * 20) + (chainsawUses * 20) + (excavatorUses * 20) + (comboActionUses[2] * 10);
        var batteryDuringBuffs = gauge.Battery + batteryGenerated;

        if (batteryDuringBuffs <= 100)
            return false;

        if (gauge.Battery >= 90)
            return true;

        var excessBattery = batteryDuringBuffs - 100;
        return gauge.Battery >= excessBattery;
    }

    private static (uint AirAnchorUses, uint DrillUses, uint ChainsawUses, uint ExcavatorUses) GetBigHitGcdUses(byte level, float duration)
    {
        var airAnchorCd = GetCooldown(MCH.AirAnchor).CooldownRemaining;
        var airAnchorUses = level >= MCH.Levels.AirAnchor && airAnchorCd < duration ?
            Math.Floor((duration - airAnchorCd) / 40) + 1 : 0;

        var drill = GetCooldown(MCH.Drill);
        var drillCd = drill.HasCharges ? drill.ChargeCooldownRemaining : drill.CooldownRemaining;
        var drillUses = 0;
        if (level >= MCH.Levels.Drill)
        {
            drillUses = drill.RemainingCharges;
            drillCd = drillCd > 0 ? drillCd : 20;
            if (drillCd < duration)
                drillUses += (int)Math.Floor((duration - drillCd) / 20) + 1;
        }

        var chainsawCd = GetCooldown(MCH.Chainsaw).CooldownRemaining;
        var chainsawUses = level >= MCH.Levels.Chainsaw && chainsawCd < duration ?
            Math.Floor((duration - chainsawCd) / 60) + 1 : 0;

        var excavatorUses = level >= MCH.Levels.EnhancedMultiweapon2 ? chainsawUses : 0;

        return ((uint)airAnchorUses, (uint)drillUses, (uint)chainsawUses, (uint)excavatorUses);
    }

    private static uint[] GetComboActionUses(uint lastComboMove, uint nonComboActionUses, float duration)
    {
        var totalComboActionUses = Math.Max(0, Math.Floor(duration / 2.5) - nonComboActionUses);
        var baseComboActionUses = (uint)Math.Floor(totalComboActionUses / 3);
        uint[] comboActionUses = [baseComboActionUses, baseComboActionUses, baseComboActionUses];
        var nextComboActionIdx = lastComboMove switch {
            MCH.SplitShot => 1,
            MCH.SlugShot => 2,
            MCH.CleanShot => 0,
            _ => 0,
        };
        for (int i = 0; i < totalComboActionUses % 3; i++)
        {
            comboActionUses[(nextComboActionIdx + i) % 3] += 1;
        }

        return comboActionUses;
    }

    private static bool ShouldUseHyperchargeOutsideBuffs(byte level, uint lastComboMove)
    {
        if (GetRaidBuffWindowOffset() > -5f)
            return false;

        var gauge = GetJobGauge<MCHGauge>();

        var timeUntilBuffsUse = -GetRaidBuffWindowOffset() - 5f;
        var (airAnchorUses, drillUses, chainsawUses, excavatorUses) = GetBigHitGcdUses(level, timeUntilBuffsUse);
        var nonComboActionUses = airAnchorUses + drillUses + chainsawUses + excavatorUses;
        uint[] comboActionUses = GetComboActionUses(lastComboMove, nonComboActionUses, timeUntilBuffsUse);
        var heatFromCombos = (comboActionUses[0] + comboActionUses[1] + comboActionUses[2]) * 5;
        var totalHeat = gauge.Heat + heatFromCombos;

        return totalHeat > 100;
    }

    private static uint GetChargeOGcd(byte level, int minCharges)
    {
        var gaussCharges = GetCooldown(MCH.GaussRound).RemainingCharges;
        var ricochetCharges = GetCooldown(MCH.Ricochet).RemainingCharges;

        if (level < MCH.Levels.GaussRound)
            return 0;

        if (level < MCH.Levels.Ricochet)
        {
            if (gaussCharges <= minCharges)
                return 0;

            return OriginalHook(MCH.GaussRound);
        }

        if (gaussCharges <= minCharges && ricochetCharges <= minCharges)
            return 0;

        if (gaussCharges > ricochetCharges)
            return OriginalHook(MCH.GaussRound);

        if (ricochetCharges > gaussCharges)
            return OriginalHook(MCH.Ricochet);

        if (GetCooldown(MCH.Ricochet).ChargeCooldownRemaining < GetCooldown(MCH.GaussRound).ChargeCooldownRemaining)
            return OriginalHook(MCH.Ricochet);

        return OriginalHook(MCH.GaussRound);
    }

    private static float TimeToCap(uint actionID, float cooldown)
    {
        var cd = GetCooldown(actionID);

        if (!cd.HasCharges)
            return cd.CooldownRemaining;

        if (cd.MaxCharges == cd.RemainingCharges)
            return 0;

        var missingCharges = cd.MaxCharges - cd.RemainingCharges;
        return ((missingCharges - 1) * cooldown) + cd.ChargeCooldownRemaining;
    }

    private static float Gcd()
    {
        return GetCooldown(MCH.SplitShot).CooldownRemaining;
    }

    private static bool IsGcdOffCooldown(uint actionID)
    {
        var cd = GetCooldown(actionID);

        if (!cd.IsCooldown) return true;
        if (cd.HasCharges && cd.RemainingCharges > 0) return true;

        var cdRemaining = cd.HasCharges ? cd.ChargeCooldownRemaining : cd.CooldownRemaining;

        return cdRemaining <= Gcd();
    }

    private static bool CanWeaveWithoutClip(uint actionID)
    {
        var cd = GetCooldown(actionID);

        if (Gcd() < 0.6)
            return false;

        if (cd.RemainingCharges > 0)
            return true;

        if (Gcd() > 1.5)
            return false;

        var cdRemain = cd.HasCharges ? cd.ChargeCooldownRemaining : cd.CooldownRemaining;

        return cdRemain + 0.6 < Gcd();
    }
}

internal class MachinistRotation : MachinistCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MachinistOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        // AoE

        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        // Utility

        if (actionID == PLD.FightOrFlight)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseTactician);

        if (actionID == PLD.Requiescat)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseDismantle);

        if (actionID == PLD.HolySpirit)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseSecondWind);

        if (actionID == PLD.HolyCircle)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseArmsLength);

        return actionID;
    }
}

internal class MachinistRookAutoturret : CustomCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MchAny;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if (actionID == MCH.RookAutoturret || actionID == MCH.AutomatonQueen)
        {
            var gauge = GetJobGauge<MCHGauge>();

            if (level >= MCH.Levels.RookOverdrive && gauge.IsRobotActive)
                // Queen Overdrive
                return OriginalHook(MCH.RookOverdrive);
        }

        return actionID;
    }
}
