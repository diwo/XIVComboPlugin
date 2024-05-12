using System;
using System.Collections.Generic;
using System.Linq;

namespace XIVComboExpandedPlugin.Combos;

internal static class PLD
{
    public const byte ClassID = 1;
    public const byte JobID = 19;

    public const uint
        FastBlade = 9,
        RiotBlade = 15,
        ShieldBash = 16,
        FightOrFlight = 20,
        RageOfHalone = 21,
        CircleOfScorn = 23,
        ShieldLob = 24,
        SpiritsWithin = 29,
        GoringBlade = 3538,
        RoyalAuthority = 3539,
        Clemency = 3541,
        TotalEclipse = 7381,
        Requiescat = 7383,
        HolySpirit = 7384,
        LowBlow = 7540,
        Prominence = 16457,
        HolyCircle = 16458,
        Confiteor = 16459,
        Atonement = 16460,
        Expiacion = 25747,
        BladeOfFaith = 25748,
        BladeOfTruth = 25749,
        BladeOfValor = 25750,
        Supplication = 36918,
        Sepulchre = 36919,
        Imperator = 36921,
        BladeOfHonor = 36922;

    public static class Buffs
    {
        public const ushort
            FightOrFlight = 76,
            Requiescat = 1368,
            AtonementReady = 1902,
            DivineMight = 2673,
            ConfiteorReady = 3019,
            SupplicationReady = 3827,
            SepulchreReady = 3828,
            GoringBladeReady = 3847,
            BladeOfHonorReady = 3831;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            FightOrFlight = 2,
            RiotBlade = 4,
            LowBlow = 12,
            SpiritsWithin = 30,
            CircleOfScorn = 50,
            RageOfHalone = 26,
            Prominence = 40,
            GoringBlade = 54,
            RoyalAuthority = 60,
            HolySpirit = 64,
            DivineMagicMastery = 64,
            Requiescat = 68,
            HolyCircle = 72,
            Atonement = 76,
            Supplication = 76,
            Sepulchre = 76,
            Confiteor = 80,
            Expiacion = 86,
            BladeOfFaith = 90,
            BladeOfTruth = 90,
            BladeOfValor = 90,
            Imperator = 96,
            BladeOfHonor = 100;
    }
}

internal abstract class PaladinCombo : CustomCombo
{
    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 1,
        DelayBurst = 1 << 2,
        RestoreMP = 1 << 3,
    }

    protected static uint GetRotationAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.RestoreMP))
        {
            if (options.HasFlag(GetActionOptions.MultiTarget) && level >= PLD.Levels.Prominence)
                return GetMultiTargetRestoreMPAction(lastComboMove, comboTime);

            return GetSingleTargetRestoreMPAction(lastComboMove, comboTime);
        }

        var weaveAction = GetWeaveAction(lastComboMove, comboTime, level, options);
        if (weaveAction > 0)
            return weaveAction;

        return GetGcdAction(lastComboMove, comboTime, level, options);
    }

    protected static bool HasMp(uint spell)
    {
        var cost = spell switch
        {
            PLD.Clemency => 4000,
            PLD.HolySpirit or PLD.HolyCircle or PLD.Confiteor or PLD.BladeOfFaith or PLD.BladeOfTruth or PLD.BladeOfValor => 2000,
            _ => 0,
        };

        if (LocalPlayer?.Level >= PLD.Levels.DivineMagicMastery)
            cost /= 2;

        return LocalPlayer?.CurrentMp >= cost;
    }

    private static bool HasMpForConfiteorCombo()
    {
        if (LocalPlayer?.Level >= 90)
            return LocalPlayer?.CurrentMp >= 4000;

        return HasMp(PLD.Confiteor);
    }

    private static uint GetWeaveAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.DelayBurst) && !IsFightOrFlightUsed())
            return 0;

        if (Gcd() < 0.6)
            return 0;

        if (level >= PLD.Levels.FightOrFlight && CanWeaveWithoutClip(PLD.FightOrFlight))
        {
            if (level < PLD.Levels.Requiescat || IsActionInRange(PLD.Requiescat) || level >= PLD.Levels.Imperator)
                return PLD.FightOrFlight;
        }

        if (level >= PLD.Levels.Requiescat && CanWeaveWithoutClip(PLD.Requiescat))
        {
            if (IsOnCooldown(PLD.FightOrFlight) && GetCooldown(PLD.FightOrFlight).CooldownElapsed < 8)
            {
                if (level >= PLD.Levels.Imperator || IsActionInRange(PLD.Requiescat))
                    return OriginalHook(PLD.Requiescat);
            }
        }

        if (HasEffect(PLD.Buffs.BladeOfHonorReady))
            return PLD.BladeOfHonor;

        if (level >= PLD.Levels.SpiritsWithin && CanWeaveWithoutClip(PLD.SpiritsWithin) && IsActionInRange(PLD.SpiritsWithin))
            return OriginalHook(PLD.SpiritsWithin);

        if (level >= PLD.Levels.CircleOfScorn && CanWeaveWithoutClip(PLD.CircleOfScorn) && IsTargetInRadius(4))
            return PLD.CircleOfScorn;

        return 0;
    }

    protected static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
            return GetMultiTargetGcdAction(lastComboMove, comboTime, level, options);

        var action = GetSingleTargetGcdAction(lastComboMove, comboTime, level, options);
        if (action == 0)
            action = GetSingleTargetGcdAction(lastComboMove, comboTime, level, options, ignoreRangeCheck: true);

        return action;
    }

    private static uint GetSingleTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options, bool ignoreRangeCheck = false)
    {
        var expiringAction = GetExpiringGcdAction();
        if (expiringAction > 0)
        {
            if (HasMp(expiringAction) && (IsActionInRange(expiringAction) || ignoreRangeCheck))
                return expiringAction;
        }

        var replacedConfiteor = OriginalHook(PLD.Confiteor);
        var bladeCombo = replacedConfiteor != PLD.Confiteor ? replacedConfiteor : 0;
        var useBladeCombo = HasEffect(PLD.Buffs.ConfiteorReady) || bladeCombo > 0;

        var atonementComboReady = HasEffect(PLD.Buffs.AtonementReady) || HasEffect(PLD.Buffs.SupplicationReady) || HasEffect(PLD.Buffs.SepulchreReady);
        var atonementCombo = OriginalHook(PLD.Atonement);

        if (HasEffect(PLD.Buffs.FightOrFlight))
        {
            if (options.HasFlag(GetActionOptions.DelayBurst))
            {
                if (HasEffect(PLD.Buffs.GoringBladeReady) && (IsActionInRange(PLD.GoringBlade) || ignoreRangeCheck))
                    return PLD.GoringBlade;

                if (HasEffect(PLD.Buffs.SepulchreReady) && (IsActionInRange(PLD.Sepulchre) || ignoreRangeCheck))
                    return PLD.Sepulchre;

                if (HasEffect(PLD.Buffs.SupplicationReady) && (IsActionInRange(PLD.Supplication) || ignoreRangeCheck))
                    return PLD.Supplication;

                if (HasEffect(PLD.Buffs.AtonementReady) && (IsActionInRange(PLD.Atonement) || ignoreRangeCheck))
                    return PLD.Atonement;
            }

            if (HasEffect(PLD.Buffs.ConfiteorReady) && HasMpForConfiteorCombo())
                return PLD.Confiteor;

            if (bladeCombo > 0 && HasMp(bladeCombo))
                return bladeCombo;

            if (HasEffect(PLD.Buffs.GoringBladeReady) && (IsActionInRange(PLD.GoringBlade) || ignoreRangeCheck))
                return PLD.GoringBlade;

            if (HasEffect(PLD.Buffs.Requiescat) && HasMp(PLD.HolySpirit) && !useBladeCombo)
                return PLD.HolySpirit;

            if (HasEffect(PLD.Buffs.SepulchreReady) && (IsActionInRange(PLD.Sepulchre) || ignoreRangeCheck))
                return PLD.Sepulchre;

            if (HasEffect(PLD.Buffs.SupplicationReady) && (IsActionInRange(PLD.Supplication) || ignoreRangeCheck))
                return PLD.Supplication;

            if (!options.HasFlag(GetActionOptions.DelayBurst))
            {
                if (HasEffect(PLD.Buffs.DivineMight) && HasMp(PLD.HolySpirit) && !useBladeCombo)
                    return PLD.HolySpirit;
            }

            if (HasEffect(PLD.Buffs.AtonementReady) && (IsActionInRange(PLD.Atonement) || ignoreRangeCheck))
                return PLD.Atonement;

            var nextCombo = GetComboAction(lastComboMove, comboTime, level);
            if (nextCombo == PLD.RoyalAuthority && HasEffect(PLD.Buffs.DivineMight) && HasMp(PLD.HolySpirit))
                return PLD.HolySpirit;

            if (IsActionInRange(nextCombo) || ignoreRangeCheck)
                return nextCombo;
        }
        else
        {
            if (HasEffect(PLD.Buffs.GoringBladeReady) && (IsActionInRange(PLD.GoringBlade) || ignoreRangeCheck))
                return PLD.GoringBlade;

            if (HasEffect(PLD.Buffs.ConfiteorReady) && HasMpForConfiteorCombo())
                return PLD.Confiteor;

            if (bladeCombo > 0 && HasMp(bladeCombo))
                return bladeCombo;

            if (HasEffect(PLD.Buffs.Requiescat) && HasMp(PLD.HolySpirit) && !useBladeCombo)
                return PLD.HolySpirit;

            if (HasEffect(PLD.Buffs.AtonementReady) && (IsActionInRange(PLD.Atonement) || ignoreRangeCheck))
                return PLD.Atonement;

            var nextCombo = GetComboAction(lastComboMove, comboTime, level);
            if (nextCombo == PLD.RoyalAuthority)
            {
                if (atonementComboReady && (IsActionInRange(atonementCombo) || ignoreRangeCheck))
                    return atonementCombo;

                if (HasEffect(PLD.Buffs.DivineMight) && HasMp(PLD.HolySpirit))
                    return PLD.HolySpirit;
            }

            if (IsActionInRange(nextCombo) || ignoreRangeCheck)
                return nextCombo;

            if (HasEffect(PLD.Buffs.DivineMight) && HasMp(PLD.HolySpirit))
                return PLD.HolySpirit;
        }

        return 0;
    }

    private static uint GetExpiringGcdAction()
    {
        ushort expiringBuff = GetEarliestExpiringBuff(
            PLD.Buffs.GoringBladeReady, PLD.Buffs.DivineMight, PLD.Buffs.Requiescat,
            PLD.Buffs.AtonementReady, PLD.Buffs.SupplicationReady, PLD.Buffs.SepulchreReady);

        return expiringBuff switch
        {
            PLD.Buffs.GoringBladeReady => PLD.GoringBlade,
            PLD.Buffs.DivineMight => PLD.HolySpirit,
            PLD.Buffs.Requiescat => GetRequiescatGcd(),
            PLD.Buffs.AtonementReady => PLD.Atonement,
            PLD.Buffs.SupplicationReady => PLD.Supplication,
            PLD.Buffs.SepulchreReady => PLD.Sepulchre,
            _ => 0,
        };
    }

    private static ushort GetEarliestExpiringBuff(params ushort[] buffs)
    {
        List<(ushort Id, float Time)> buffTimes =
            Array.FindAll(buffs, HasEffect)
                .Select(buffId => (buffId, GetEffectRemainingTime(buffId) - Gcd()))
                .OrderBy(b => b.Item2)
                .ToList();

        float accTime = 0;
        for (int i = 0; i < buffTimes.Count; i++)
        {
            accTime += 2.5f * Math.Max((byte)1, FindEffect(buffTimes[i].Id)!.StackCount);
            if (buffTimes[i].Time < accTime)
                return buffTimes[0].Id;
        }

        return 0;
    }

    private static uint GetRequiescatGcd()
    {
        var replacedConfiteor = OriginalHook(PLD.Confiteor);
        var bladeCombo = replacedConfiteor != PLD.Confiteor ? replacedConfiteor : 0;
        var useBladeCombo = HasEffect(PLD.Buffs.ConfiteorReady) || bladeCombo > 0;

        if (useBladeCombo)
            return bladeCombo;

        return PLD.HolySpirit;
    }

    private static uint GetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (lastComboMove == PLD.RiotBlade && level >= PLD.Levels.RageOfHalone)
                return OriginalHook(PLD.RageOfHalone);

            if (lastComboMove == PLD.FastBlade && level >= PLD.Levels.RiotBlade)
                return PLD.RiotBlade;
        }

        return PLD.FastBlade;
    }

    private static uint GetMultiTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var replacedConfiteor = OriginalHook(PLD.Confiteor);
        var bladeCombo = replacedConfiteor != PLD.Confiteor ? replacedConfiteor : 0;
        var useBladeCombo = HasEffect(PLD.Buffs.ConfiteorReady) || bladeCombo > 0;

        if (HasEffect(PLD.Buffs.ConfiteorReady) && HasMpForConfiteorCombo())
            return PLD.Confiteor;

        if (bladeCombo > 0 && HasMp(bladeCombo))
            return bladeCombo;

        if (level >= PLD.Levels.HolyCircle && HasMp(PLD.HolyCircle) && !useBladeCombo)
        {
            if (HasEffect(PLD.Buffs.Requiescat) || HasEffect(PLD.Buffs.DivineMight))
                return PLD.HolyCircle;
        }

        if (comboTime > 0 && lastComboMove == PLD.TotalEclipse && level >= PLD.Levels.Prominence)
            return PLD.Prominence;

        return PLD.TotalEclipse;
    }

    private static uint GetSingleTargetRestoreMPAction(uint lastComboMove, float comboTime)
    {
        if (comboTime > 0 && lastComboMove == PLD.FastBlade)
            return PLD.RiotBlade;

        return PLD.FastBlade;
    }

    private static uint GetMultiTargetRestoreMPAction(uint lastComboMove, float comboTime)
    {
        if (comboTime > 0 && lastComboMove == PLD.TotalEclipse)
            return PLD.Prominence;

        return PLD.TotalEclipse;
    }

    private static bool IsFightOrFlightUsed()
    {
        if (HasEffect(PLD.Buffs.FightOrFlight))
            return true;

        return IsOnCooldown(PLD.FightOrFlight) && GetCooldown(PLD.FightOrFlight).CooldownElapsed < 20;
    }

    private static float Gcd()
    {
        return GetCooldown(PLD.FastBlade).CooldownRemaining;
    }

    private static bool CanWeaveWithoutClip(uint actionID)
    {
        if (IsOffCooldown(actionID))
            return true;

        if (Gcd() > 1.5)
            return false;

        return GetCooldown(actionID).CooldownRemaining + 0.6 < Gcd();
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

internal class PaladinRotation : PaladinCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PaladinOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == BRD.HeavyShot)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == BRD.StraightShot)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == BRD.IronJaws)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.RestoreMP);

        // Display
        if (actionID == BRD.ApexArrow)
            return GetGcdAction(lastComboMove, comboTime, level, GetActionOptions.None);

        // AoE
        if (actionID == BRD.QuickNock)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget);

        if (actionID == BRD.Shadowbite)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        return actionID;
    }
}