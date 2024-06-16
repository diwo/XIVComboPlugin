using System;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class GNB
{
    public const byte JobID = 37;

    public const uint
        KeenEdge = 16137,
        NoMercy = 16138,
        BrutalShell = 16139,
        DemonSlice = 16141,
        DangerZone = 16144,
        SolidBarrel = 16145,
        GnashingFang = 16146,
        DemonSlaughter = 16149,
        SonicBreak = 16153,
        Continuation = 16155,
        JugularRip = 16156,
        AbdomenTear = 16157,
        EyeGouge = 16158,
        BowShock = 16159,
        BurstStrike = 16162,
        FatedCircle = 16163,
        Bloodfest = 16164,
        BlastingZone = 16165,
        Hypervelocity = 25759,
        DoubleDown = 25760,
        FatedBrand = 36936,
        ReignOfBeasts = 36937;

    public static class Buffs
    {
        public const ushort
            NoMercy = 1831,
            ReadyToRip = 1842,
            ReadyToTear = 1843,
            ReadyToGouge = 1844,
            ReadyToBlast = 2686,
            ReadyToFated = 3839,
            ReadyToReign = 3840,
            ReadyToBreak = 3886;
    }

    public static class Debuffs
    {
        public const ushort
            BowShock = 1838;
    }

    public static class Levels
    {
        public const byte
            NoMercy = 2,
            BrutalShell = 4,
            DangerZone = 18,
            SolidBarrel = 26,
            BurstStrike = 30,
            DemonSlaughter = 40,
            SonicBreak = 54,
            GnashingFang = 60,
            BowShock = 62,
            Continuation = 70,
            FatedCircle = 72,
            Bloodfest = 76,
            BlastingZone = 80,
            EnhancedContinuation = 86,
            CartridgeCharge2 = 88,
            DoubleDown = 90;
    }
}

internal abstract class GunbreakerCombo : CustomCombo
{
    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
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
        var gauge = GetJobGauge<GNBGauge>();

        if (options.HasFlag(GetActionOptions.DelayBurst))
            return 0;

        if (GetCooldown(GNB.NoMercy).CooldownRemaining < Gcd() && !IsEarlyWeave())
            return GNB.NoMercy;

        if (Gcd() < 0.6)
            return 0;

        if (OriginalHook(GNB.Continuation) != GNB.Continuation)
            return OriginalHook(GNB.Continuation);

        if (IsOnCooldown(GNB.NoMercy))
        {
            if (level >= GNB.Levels.Bloodfest && CanWeaveWithoutClip(GNB.Bloodfest) && gauge.Ammo == 0)
            {
                if (GetRemainingNoMercyTime() - Gcd() > 10)
                    return GNB.Bloodfest;
            }

            if (level >= GNB.Levels.BowShock && CanWeaveWithoutClip(GNB.BowShock) && IsTargetInRadius(4))
                return GNB.BowShock;

            if (level >= GNB.Levels.BlastingZone)
            {
                if (CanWeaveWithoutClip(GNB.BlastingZone))
                    return GNB.BlastingZone;
            }
            else if (level >= GNB.Levels.DangerZone)
            {
                if (CanWeaveWithoutClip(GNB.DangerZone))
                    return GNB.DangerZone;
            }
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
        var gauge = GetJobGauge<GNBGauge>();
        var comboAction = GetSingleTargetComboAction(lastComboMove, comboTime, level);
        var maxAmmo = level >= GNB.Levels.CartridgeCharge2 ? 3 : 2;

        if (!options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (IsNoMercyUsed())
            {
                var hasUnusedBloodfest = level >= GNB.Levels.Bloodfest && GetCooldown(GNB.Bloodfest).CooldownRemaining < GetRemainingNoMercyTime() - 10;
                var hasUnusedGnashingFang = level >= GNB.Levels.GnashingFang && GetCooldown(GNB.GnashingFang).CooldownRemaining < GetRemainingNoMercyTime();
                var hasUnusedDoubleDown = level >= GNB.Levels.DoubleDown && GetCooldown(GNB.DoubleDown).CooldownRemaining < GetRemainingNoMercyTime();
                var availableAmmoInBurst = hasUnusedBloodfest ? gauge.Ammo + maxAmmo : gauge.Ammo;
                var surplusAmmo = availableAmmoInBurst - (hasUnusedGnashingFang ? 1 : 0) - (hasUnusedDoubleDown ? 2 : 0);

                if (level >= GNB.Levels.DoubleDown && IsGcdActionOffCooldown(GNB.DoubleDown) && gauge.Ammo >= 2 && IsTargetInRadius(4))
                    return GNB.DoubleDown;

                var isNextGcdDoubleDown = hasUnusedDoubleDown && gauge.Ammo == 2 && GetCooldown(GNB.DoubleDown).CooldownRemaining < Gcd();
                var isOnlyHasAmmoForDoubleDown = hasUnusedDoubleDown && availableAmmoInBurst == 2;
                var saveForDoubleDown = isNextGcdDoubleDown || isOnlyHasAmmoForDoubleDown;
                if (!saveForDoubleDown && gauge.Ammo > 0)
                {
                    if (level >= GNB.Levels.GnashingFang && IsGcdActionOffCooldown(GNB.GnashingFang))
                        return GNB.GnashingFang;

                    if (surplusAmmo > 0 && hasUnusedBloodfest)
                        return GNB.BurstStrike;
                }

                if (HasEffect(GNB.Buffs.ReadyToBreak))
                    return GNB.SonicBreak;

                if (OriginalHook(GNB.GnashingFang) != GNB.GnashingFang)
                    return OriginalHook(GNB.GnashingFang);

                if (HasEffect(GNB.Buffs.ReadyToReign) || OriginalHook(GNB.ReignOfBeasts) != GNB.ReignOfBeasts)
                    return OriginalHook(GNB.ReignOfBeasts);

                if (surplusAmmo > 0 && gauge.Ammo > 0)
                    return GNB.BurstStrike;
            }

            if (level >= GNB.Levels.GnashingFang && IsGcdActionOffCooldown(GNB.GnashingFang) && gauge.Ammo >= 1)
            {
                if (GetCooldown(GNB.NoMercy).CooldownRemaining > 20)
                    return GNB.GnashingFang;
            }

            if (HasEffect(GNB.Buffs.ReadyToReign))
                return GNB.ReignOfBeasts;
        }

        if (OriginalHook(GNB.GnashingFang) != GNB.GnashingFang)
            return OriginalHook(GNB.GnashingFang);

        if (OriginalHook(GNB.ReignOfBeasts) != GNB.ReignOfBeasts)
            return OriginalHook(GNB.ReignOfBeasts);

        if (comboAction == GNB.SolidBarrel && gauge.Ammo == maxAmmo)
        {
            if (level >= GNB.Levels.BurstStrike)
                return GNB.BurstStrike;
        }

        return comboAction;
    }

    private static uint GetSingleTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (level >= GNB.Levels.SolidBarrel && lastComboMove == GNB.BrutalShell)
                return OriginalHook(GNB.SolidBarrel);

            if (level >= GNB.Levels.BrutalShell && lastComboMove == GNB.KeenEdge)
                return GNB.BrutalShell;
        }

        return GNB.KeenEdge;
    }

    private static uint GetMultiTargetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<GNBGauge>();
        var comboAction = GetMultiTargetComboAction(lastComboMove, comboTime, level);
        var maxAmmo = level >= GNB.Levels.CartridgeCharge2 ? 3 : 2;

        if (!options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (IsNoMercyUsed())
            {
                var hasUnusedBloodfest = level >= GNB.Levels.Bloodfest && GetCooldown(GNB.Bloodfest).CooldownRemaining < GetRemainingNoMercyTime() - 10;
                var hasUnusedDoubleDown = level >= GNB.Levels.DoubleDown && GetCooldown(GNB.DoubleDown).CooldownRemaining < GetRemainingNoMercyTime();
                var availableAmmoInBurst = hasUnusedBloodfest ? gauge.Ammo + maxAmmo : gauge.Ammo;
                var surplusAmmo = availableAmmoInBurst - (hasUnusedDoubleDown ? 2 : 0);

                if (level >= GNB.Levels.DoubleDown && IsGcdActionOffCooldown(GNB.DoubleDown) && gauge.Ammo >= 2 && IsTargetInRadius(4))
                    return GNB.DoubleDown;

                if (HasEffect(GNB.Buffs.ReadyToReign) || OriginalHook(GNB.ReignOfBeasts) != GNB.ReignOfBeasts)
                    return OriginalHook(GNB.ReignOfBeasts);

                if (level >= GNB.Levels.FatedCircle && surplusAmmo > 0 && gauge.Ammo > 0)
                    return GNB.FatedCircle;

                if (HasEffect(GNB.Buffs.ReadyToBreak))
                    return GNB.SonicBreak;
            }
        }

        if (level >= GNB.Levels.FatedCircle && comboAction == GNB.DemonSlaughter && gauge.Ammo == maxAmmo)
            return GNB.FatedCircle;

        return comboAction;
    }

    private static uint GetMultiTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (level >= GNB.Levels.DemonSlaughter && lastComboMove == GNB.DemonSlice)
                return GNB.DemonSlaughter;
        }

        return GNB.DemonSlice;
    }

    private static bool IsNoMercyUsed()
    {
        if (HasEffect(GNB.Buffs.NoMercy))
            return true;

        return IsOnCooldown(GNB.NoMercy) && GetCooldown(GNB.NoMercy).CooldownElapsed < 20;
    }

    private static float GetRemainingNoMercyTime()
    {
        if (HasEffect(GNB.Buffs.NoMercy))
            return GetEffectRemainingTime(GNB.Buffs.NoMercy);

        if (IsOffCooldown(GNB.NoMercy) || GetCooldown(GNB.NoMercy).CooldownElapsed < 20)
            return 20;

        return 0;
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

    private static float Gcd()
    {
        return GetCooldown(GNB.KeenEdge).CooldownRemaining;
    }

    private static bool IsGcdActionOffCooldown(uint actionID)
    {
        return GetCooldown(actionID).CooldownRemaining <= Gcd();
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

internal class GunbreakerRotation : GunbreakerCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.GunbreakerOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

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
