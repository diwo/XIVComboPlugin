using System;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class WAR
{
    public const byte ClassID = 3;
    public const byte JobID = 21;

    public const uint
        HeavySwing = 31,
        Maim = 37,
        Berserk = 38,
        ThrillOfBattle = 40,
        Overpower = 41,
        StormsPath = 42,
        StormsEye = 45,
        Tomahawk = 46,
        InnerBeast = 49,
        SteelCyclone = 51,
        Infuriate = 52,
        FellCleave = 3549,
        Decimate = 3550,
        RawIntuition = 3551,
        Equilibrium = 3552,
        Onslaught = 7386,
        Upheaval = 7387,
        InnerRelease = 7389,
        MythrilTempest = 16462,
        ChaoticCyclone = 16463,
        NascentFlash = 16464,
        InnerChaos = 16465,
        Bloodwhetting = 25751,
        Orogeny = 25752,
        PrimalRend = 25753,
        PrimalWrath = 36924,
        PrimalRuination = 36925;

    public static class Buffs
    {
        public const ushort
            Berserk = 86,
            InnerRelease = 1177,
            NascentChaos = 1897,
            PrimalRendReady = 2624,
            InnerStrength = 2663,
            SurgingTempest = 2677,
            Wrathful = 3901,
            PrimalRuinationReady = 3834;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            Maim = 4,
            Berserk = 6,
            Tomahawk = 15,
            StormsPath = 26,
            ThrillOfBattle = 30,
            InnerBeast = 35,
            MythrilTempest = 40,
            SteelCyclone = 45,
            StormsEye = 50,
            Infuriate = 50,
            FellCleave = 54,
            RawIntuition = 56,
            Equilibrium = 58,
            Decimate = 60,
            Onslaught = 62,
            Upheaval = 64,
            InnerRelease = 70,
            MythrilTempestTrait = 74,
            NascentFlash = 76,
            InnerChaos = 80,
            Bloodwhetting = 82,
            Orogeny = 86,
            PrimalRend = 90,
            PrimalWrath = 96,
            PrimalRuination = 100;
    }
}

internal abstract class WarriorCombo : CustomCombo
{
    private static float gcdInternal = 2.5f;

    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        UseResources = 1 << 2,
        UseCooldowns = 1 << 3,
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
        var gauge = GetJobGauge<WARGauge>();
        var nextCombo = options.HasFlag(GetActionOptions.MultiTarget) ?
            GetMultiTargetComboAction(lastComboMove, comboTime, level, options) :
            GetSingleTargetComboAction(lastComboMove, comboTime, level);
        var nextGcd = GetGcdAction(lastComboMove, comboTime, level, options);

        if (Gcd() < 0.6)
            return 0;

        if (options.HasFlag(GetActionOptions.DelayBurst))
            return 0;

        if (level >= WAR.Levels.StormsEye && !HasEffect(WAR.Buffs.SurgingTempest))
        {
            if (TimeToCapInfuriate() == 0)
            {
                var gaugeFromComboChain = nextCombo == WAR.StormsEye ? 10 : 20;
                if (gauge.BeastGauge + gaugeFromComboChain <= 50)
                    return WAR.Infuriate;
            }

            return 0;
        }

        if (level >= WAR.Levels.InnerRelease)
        {
            if (GetCooldown(WAR.Infuriate).RemainingCharges > 0 && gauge.BeastGauge <= 50)
            {
                if (TimeToCapInfuriate() - Gcd() < 10)
                    return WAR.Infuriate;

                if (nextGcd == WAR.FellCleave && TimeToCapInfuriate() - Gcd() < 5)
                    return WAR.Infuriate;

                if (options.HasFlag(GetActionOptions.UseCooldowns))
                    return WAR.Infuriate;
            }

            if (IsOffCooldown(WAR.InnerRelease))
            {
                var willOvercapInfuriate = TimeToCapInfuriate() - Gcd() < 30;
                if (gauge.BeastGauge <= 50 || !willOvercapInfuriate)
                    return WAR.InnerRelease;
            }
        }
        else if (level >= WAR.Levels.Infuriate)
        {
            if (GetCooldown(WAR.Infuriate).RemainingCharges > 0 && gauge.BeastGauge <= 50)
            {
                if (TimeToCapInfuriate() - Gcd() < 2.5)
                    return WAR.Infuriate;

                if (HasEffect(WAR.Buffs.Berserk) && gauge.BeastGauge < 50)
                    return WAR.Infuriate;

                if (options.HasFlag(GetActionOptions.UseCooldowns))
                    return WAR.Infuriate;
            }

            if (IsOffCooldown(WAR.Berserk))
            {
                if (options.HasFlag(GetActionOptions.MultiTarget) && nextCombo == WAR.MythrilTempest)
                    return WAR.Berserk;

                var bigHits = 0;

                var beastGauge = gauge.BeastGauge;
                if (nextCombo == WAR.StormsPath)
                    beastGauge += 20;
                else if (nextCombo == WAR.StormsEye)
                    beastGauge += 10;

                bigHits += beastGauge / 50;

                bigHits += GetCooldown(WAR.Infuriate).RemainingCharges;

                if (IsOnCooldown(WAR.Infuriate) && GetCooldown(WAR.Infuriate).ChargeCooldownRemaining - Gcd() + 0.6 < 5)
                    bigHits += 1;

                if (nextCombo == WAR.StormsPath || nextCombo == WAR.StormsEye)
                    bigHits += 1;

                if (bigHits >= 3 || options.HasFlag(GetActionOptions.UseCooldowns))
                    return WAR.Berserk;
            }
        }
        else if (level >= WAR.Levels.InnerBeast)
        {
            if (IsOffCooldown(WAR.Berserk))
            {
                if (!options.HasFlag(GetActionOptions.MultiTarget))
                {
                    if (gauge.BeastGauge >= 80 && nextCombo == WAR.StormsPath)
                        return WAR.Berserk;
                }
                else
                {
                    if (level < WAR.Levels.MythrilTempest || nextCombo == WAR.MythrilTempest)
                        return WAR.Berserk;
                }

                if (options.HasFlag(GetActionOptions.UseCooldowns))
                    return WAR.Berserk;
            }
        }
        else
        {
            if (IsOffCooldown(WAR.Berserk))
            {
                if (options.HasFlag(GetActionOptions.MultiTarget) || level >= WAR.Levels.StormsPath || nextCombo == WAR.Maim)
                    return WAR.Berserk;
            }
        }

        if (level >= WAR.Levels.Orogeny && IsOffCooldown(WAR.Orogeny)
                && options.HasFlag(GetActionOptions.MultiTarget) && IsTargetInRadius(4))
            return WAR.Orogeny;

        if (level >= WAR.Levels.Upheaval && IsOffCooldown(WAR.Upheaval) && IsActionInRange(WAR.Upheaval))
            return WAR.Upheaval;

        if (HasEffect(WAR.Buffs.Wrathful) && IsTargetInRadius(4))
        {
            if (options.HasFlag(GetActionOptions.UseResources) || GetEffectRemainingTime(WAR.Buffs.Wrathful) - Gcd() < 5)
                return WAR.PrimalWrath;

            if (options.HasFlag(GetActionOptions.MultiTarget))
                return WAR.PrimalWrath;
        }

        if (options.HasFlag(GetActionOptions.UseCooldowns))
        {
            if (level >= WAR.Levels.Onslaught && GetCooldown(WAR.Onslaught).RemainingCharges > 0)
                return WAR.Onslaught;
        }

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
        var gauge = GetJobGauge<WARGauge>();
        var nextCombo = GetSingleTargetComboAction(lastComboMove, comboTime, level);

        if (level >= WAR.Levels.StormsEye && !HasEffect(WAR.Buffs.SurgingTempest))
            return nextCombo;

        if (level >= WAR.Levels.InnerRelease)
        {
            if (HasEffect(WAR.Buffs.PrimalRendReady))
            {
                if (options.HasFlag(GetActionOptions.UseResources))
                    return WAR.PrimalRend;
            }

            if (HasEffect(WAR.Buffs.PrimalRuinationReady))
            {
                if (options.HasFlag(GetActionOptions.UseResources) || GetEffectRemainingTime(WAR.Buffs.PrimalRuinationReady) - Gcd() < 5)
                    return WAR.PrimalRuination;
            }

            if (HasEffect(WAR.Buffs.InnerRelease))
                return FellCleave(level);

            if (gauge.BeastGauge > 50 && !options.HasFlag(GetActionOptions.DelayBurst))
            {
                if (GetCooldown(WAR.InnerRelease).CooldownRemaining - Gcd() < 2.5)
                {
                    if (TimeToCapInfuriate() - Gcd() < 30)
                        return FellCleave(level);
                }

                if (TimeToCapInfuriate() - Gcd() < 10)
                    return FellCleave(level);
            }

            if (gauge.BeastGauge >= 50 && options.HasFlag(GetActionOptions.UseResources))
                return FellCleave(level);
        }
        else if (level >= WAR.Levels.InnerBeast)
        {
            if (HasEffect(WAR.Buffs.Berserk))
            {
                if (comboTime - Gcd() < 2.5 && (nextCombo == WAR.StormsPath || nextCombo == WAR.StormsEye))
                    return nextCombo;

                if (level >= WAR.Levels.FellCleave && gauge.BeastGauge >= 50)
                    return FellCleave(level);

                if (nextCombo == WAR.StormsPath || nextCombo == WAR.StormsEye)
                {
                    if (!IsNextComboActionOvercaps(lastComboMove, comboTime, level))
                        return nextCombo;
                }

                if (gauge.BeastGauge >= 50)
                    return FellCleave(level); // inner beast
            }

            if (gauge.BeastGauge >= 50 && !options.HasFlag(GetActionOptions.DelayBurst))
            {
                if (level >= WAR.Levels.Infuriate && TimeToCapInfuriate() - Gcd() < 2.5)
                    return FellCleave(level);

                if (options.HasFlag(GetActionOptions.UseResources))
                    return FellCleave(level);
            }
        }

        return GetSingleTargetComboActionWithoutOvercap(lastComboMove, comboTime, level);
    }

    private static bool IsNextComboActionOvercaps(uint lastComboMove, float comboTime, byte level)
    {
        if (level < WAR.Levels.InnerBeast)
            return false;

        var gauge = GetJobGauge<WARGauge>();
        var nextCombo = GetSingleTargetComboAction(lastComboMove, comboTime, level);

        if (nextCombo == WAR.StormsPath && gauge.BeastGauge + 20 > 100)
            return true;

        if (nextCombo == WAR.StormsEye && gauge.BeastGauge + 10 > 100)
            return true;

        if (nextCombo == WAR.Maim && gauge.BeastGauge + 10 > 100)
            return true;

        if (level >= WAR.Levels.MythrilTempestTrait && nextCombo == WAR.MythrilTempest
                && gauge.BeastGauge + 20 > 100)
            return true;

        return false;
    }

    private static uint GetSingleTargetComboActionWithoutOvercap(uint lastComboMove, float comboTime, byte level)
    {
        return IsNextComboActionOvercaps(lastComboMove, comboTime, level) ?
            FellCleave(level) :
            GetSingleTargetComboAction(lastComboMove, comboTime, level);
    }

    private static uint GetSingleTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (level >= WAR.Levels.StormsPath && lastComboMove == WAR.Maim)
            {
                if (level >= WAR.Levels.StormsEye)
                {
                    var skill = level >= WAR.Levels.InnerRelease ? WAR.InnerRelease : WAR.Berserk;
                    var innerRelease = GetCooldown(skill).CooldownRemaining;
                    var surgingTempest = GetEffectRemainingTime(WAR.Buffs.SurgingTempest);
                    if (ShouldRefreshSurgingTempest(surgingTempest, innerRelease))
                        return WAR.StormsEye;
                }

                return WAR.StormsPath;
            }

            if (level >= WAR.Levels.Maim && lastComboMove == WAR.HeavySwing)
                return WAR.Maim;
        }

        return WAR.HeavySwing;
    }

    private static uint GetMultiTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<WARGauge>();
        var nextCombo = GetMultiTargetComboAction(lastComboMove, comboTime, level, options);

        if (level >= WAR.Levels.MythrilTempest && !HasEffect(WAR.Buffs.SurgingTempest))
            return nextCombo;

        if (HasEffect(WAR.Buffs.PrimalRendReady) && !options.HasFlag(GetActionOptions.DelayBurst))
            return WAR.PrimalRend;

        if (HasEffect(WAR.Buffs.PrimalRuinationReady) && !options.HasFlag(GetActionOptions.DelayBurst))
            return WAR.PrimalRuination;

        if (HasEffect(WAR.Buffs.InnerRelease))
            return OriginalHook(WAR.Decimate);

        if (level >= WAR.Levels.SteelCyclone && gauge.BeastGauge >= 50)
        {
            if (!options.HasFlag(GetActionOptions.DelayBurst))
                return OriginalHook(WAR.Decimate);
        }

        return nextCombo;
    }

    private static uint GetMultiTargetComboAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (level >= WAR.Levels.MythrilTempest && comboTime > 0 && lastComboMove == WAR.Overpower)
            return WAR.MythrilTempest;

        return WAR.Overpower;
    }

    private static uint FellCleave(byte level)
    {
        if (HasEffect(WAR.Buffs.NascentChaos) && level < WAR.Levels.InnerChaos)
        {
            if (IsTargetInRadius(4))
                return OriginalHook(WAR.ChaoticCyclone);
        }

        return OriginalHook(WAR.FellCleave);
    }

    private static bool ShouldRefreshSurgingTempest(float surgingTempestRemainingTime, float innerReleaseCooldownRemaining)
    {
        var surgingTempestFromIR = Math.Max(0, 10 - innerReleaseCooldownRemaining);
        return surgingTempestRemainingTime + 30 + surgingTempestFromIR < 60;
    }

    private static float TimeToCapInfuriate()
    {
        return TimeToCap(WAR.Infuriate, 60);
    }

    private static float TimeToCap(uint actionID, float cooldown)
    {
        var cd = GetCooldown(actionID);

        if (cd.MaxCharges == cd.RemainingCharges)
            return 0;

        var missingCharges = cd.MaxCharges - cd.RemainingCharges;
        return ((missingCharges - 1) * cooldown) + cd.ChargeCooldownRemaining;
    }

    private static float Gcd()
    {
        return GetCooldown(WAR.HeavySwing).CooldownRemaining;
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

internal class WarriorRotation : WarriorCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.WarriorOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == PLD.RageOfHalone)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseResources);

        if (actionID == PLD.Atonement)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseResources | GetActionOptions.UseCooldowns);

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
