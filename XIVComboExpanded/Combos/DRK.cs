using System;

using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class DRK
{
    public const byte JobID = 32;

    public const uint
        HardSlash = 3617,
        Unleash = 3621,
        SyphonStrike = 3623,
        Grit = 3629,
        Souleater = 3632,
        BloodWeapon = 3625,
        SaltedEarth = 3639,
        AbyssalDrain = 3641,
        CarveAndSpit = 3643,
        Quietus = 7391,
        Bloodspiller = 7392,
        FloodOfDarkness = 16466,
        EdgeOfDarkness = 16467,
        StalwartSoul = 16468,
        FloodOfShadow = 16469,
        EdgeOfShadow = 16470,
        LivingShadow = 16472,
        SaltAndDarkness = 25755,
        Shadowbringer = 25757,
        GritRemoval = 32067,
        Delirium = 7390,
        ScarletDelirium = 36928,
        Comeuppance = 36929,
        Torcleaver = 36930,
        Impalement = 36931,
        Disesteem = 36932;

    public static class Buffs
    {
        public const ushort
            BloodWeapon = 742,
            Grit = 743,
            Darkside = 751,
            Delirium = 1972,
            ScarletDelirium = 3836,
            Scorn = 3837;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            SyphonStrike = 2,
            Grit = 10,
            Souleater = 26,
            FloodOfDarkness = 30,
            BloodWeapon = 35,
            EdgeOfDarkness = 40,
            StalwartSoul = 40,
            SaltedEarth = 52,
            AbyssalDrain = 56,
            CarveAndSpit = 60,
            Bloodspiller = 62,
            Quietus = 64,
            Delirium = 68,
            Shadow = 74,
            LivingShadow = 80,
            SaltAndDarkness = 86,
            Shadowbringer = 90,
            ScarletDelirium = 96,
            Comeuppance = 96,
            Torcleaver = 96,
            Impalement = 96,
            Disesteem = 100;
    }
}

internal abstract class DarkCombo : CustomCombo
{
    private static bool isOpenerState;

    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        UseResources = 1 << 2,
    }

    protected static uint GetRotationAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        UpdateState();

        var weaveAction = GetWeaveAction(lastComboMove, comboTime, level, options);
        if (weaveAction > 0)
            return weaveAction;

        return GetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static void UpdateState()
    {
        var gauge = GetJobGauge<DRKGauge>();

        var isOpenerStart =
            IsOffCooldown(DRK.LivingShadow) && IsOffCooldown(DRK.Delirium)
                && LocalPlayer?.CurrentMp == LocalPlayer?.MaxMp
                && gauge.DarksideTimeRemaining == 0;

        var isOpenerEnd = IsOnCooldown(DRK.LivingShadow) && gauge.ShadowTimeRemaining == 0;

        if (isOpenerStart) isOpenerState = true;
        if (isOpenerEnd) isOpenerState = false;
    }

    private static uint GetWeaveAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        if (Gcd() < 0.6)
            return 0;

        var gauge = GetJobGauge<DRKGauge>();
        var hasMpForDarkside = LocalPlayer?.CurrentMp >= 3000;
        var darksideOgcd = GetDarksideOgcd(level, options);
        var useBurst = gauge.ShadowTimeRemaining > 0 || options.HasFlag(GetActionOptions.UseResources);

        if (darksideOgcd > 0 && CanWeaveWithoutClip(darksideOgcd) && hasMpForDarkside)
        {
            if (gauge.DarksideTimeRemaining <= (Gcd() + 2.5) * 1000)
                return darksideOgcd;

            if (gauge.ShadowTimeRemaining == 0 && LocalPlayer?.CurrentMp > 9200)
                return darksideOgcd;
        }

        if (options.HasFlag(GetActionOptions.DelayBurst))
            return 0;

        if (level >= DRK.Levels.LivingShadow && CanWeaveWithoutClip(DRK.LivingShadow))
        {
            if (!isOpenerState || lastComboMove > 0)
                return DRK.LivingShadow;
        }

        if (level >= DRK.Levels.LivingShadow && !IsGcdsCountSinceActionUsed(DRK.LivingShadow, 2))
            return 0;

        if (level >= DRK.Levels.Delirium)
        {
            if (CanWeaveWithoutClip(DRK.Delirium))
                return DRK.Delirium;
        }
        else if (level >= DRK.Levels.BloodWeapon)
        {
            if (CanWeaveWithoutClip(DRK.BloodWeapon))
                return DRK.BloodWeapon;
        }

        if (level >= DRK.Levels.LivingShadow && !IsGcdsCountSinceActionUsed(DRK.LivingShadow, 3))
            return 0;

        if (level >= DRK.Levels.SaltedEarth && CanWeaveWithoutClip(DRK.SaltedEarth) && IsTargetInRadius(4))
            return DRK.SaltedEarth;

        if (darksideOgcd > 0 && CanWeaveWithoutClip(darksideOgcd))
        {
            var hasBloodweapon = HasEffect(DRK.Buffs.BloodWeapon) || HasEffect(DRK.Buffs.Delirium);
            if (hasBloodweapon && LocalPlayer?.CurrentMp >= 8000)
                return darksideOgcd;
        }

        if (level >= DRK.Levels.Shadowbringer && CanWeaveWithoutClip(DRK.Shadowbringer))
        {
            var chargesCapped = GetRemainingCharges(DRK.Shadowbringer) == GetMaxCharges(DRK.Shadowbringer);
            if (chargesCapped && useBurst)
                return DRK.Shadowbringer;
        }

        if (level >= DRK.Levels.AbyssalDrain && CanWeaveWithoutClip(DRK.AbyssalDrain))
        {
            if (level >= DRK.Levels.CarveAndSpit && !options.HasFlag(GetActionOptions.MultiTarget))
                return DRK.CarveAndSpit;

            return DRK.AbyssalDrain;
        }

        if (level >= DRK.Levels.Shadowbringer && CanWeaveWithoutClip(DRK.Shadowbringer))
        {
            if (HasCharges(DRK.Shadowbringer) && useBurst)
                return DRK.Shadowbringer;
        }

        if (darksideOgcd > 0 && CanWeaveWithoutClip(darksideOgcd))
        {
            if (gauge.HasDarkArts)
                return darksideOgcd;

            if (hasMpForDarkside && useBurst)
                return darksideOgcd;
        }

        var saltAndDarkness = OriginalHook(DRK.SaltedEarth);
        if (saltAndDarkness != DRK.SaltedEarth && CanWeaveWithoutClip(saltAndDarkness))
            return saltAndDarkness;

        return 0;
    }

    protected static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<DRKGauge>();
        var comboAction =
            !options.HasFlag(GetActionOptions.MultiTarget) ?
                GetSingleTargetComboAction(lastComboMove, comboTime, level) :
                GetMultiTargetComboAction(lastComboMove, comboTime, level);
        var bloodSpender =
            !options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(DRK.Bloodspiller) : OriginalHook(DRK.Quietus);

        if (level >= DRK.Levels.Disesteem && HasEffect(DRK.Buffs.Scorn))
            return DRK.Disesteem;

        if (level >= DRK.Levels.Bloodspiller)
        {
            if (HasEffect(DRK.Buffs.Delirium))
                return bloodSpender;

            if (gauge.Blood >= 50)
            {
                if (gauge.ShadowTimeRemaining > 0 && IsGcdsCountSinceActionUsed(DRK.LivingShadow, 2))
                    return bloodSpender;

                if (options.HasFlag(GetActionOptions.UseResources))
                    return bloodSpender;
            }

            if (level >= DRK.Levels.BloodWeapon)
            {
                var bloodweaponCd = GetCooldown(DRK.BloodWeapon).CooldownRemaining - Gcd();

                if (comboAction == DRK.Souleater || comboAction == DRK.StalwartSoul)
                {
                    if (gauge.Blood + 20 > 100)
                        return bloodSpender;

                    if (bloodweaponCd < 5 && gauge.Blood + 20 > 70)
                        return bloodSpender;
                }

                if (bloodweaponCd < 2.5 && gauge.Blood > 70)
                    return bloodSpender;
            }
        }

        return comboAction;
    }

    private static uint GetSingleTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (level >= DRK.Levels.Souleater && lastComboMove == DRK.SyphonStrike)
                return OriginalHook(DRK.Souleater);

            if (level >= DRK.Levels.SyphonStrike && lastComboMove == DRK.HardSlash)
                return DRK.SyphonStrike;
        }

        return DRK.HardSlash;
    }

    private static uint GetMultiTargetComboAction(uint lastComboMove, float comboTime, byte level)
    {
        if (comboTime > 0)
        {
            if (level >= DRK.Levels.StalwartSoul && lastComboMove == DRK.Unleash)
                return DRK.StalwartSoul;
        }

        return DRK.Unleash;
    }

    private static uint GetDarksideOgcd(byte level, GetActionOptions options)
    {
        if (!options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (level >= DRK.Levels.EdgeOfDarkness)
                return OriginalHook(DRK.EdgeOfDarkness);
        }

        if (level >= DRK.Levels.FloodOfDarkness)
            return OriginalHook(DRK.FloodOfDarkness);

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
        return GetCooldown(DRK.HardSlash).CooldownRemaining;
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

    private static bool IsGcdsCountSinceActionUsed(uint actionID, uint gcdCount)
    {
        return GetCooldown(actionID).CooldownElapsed + Gcd() > 2.5 * gcdCount;
    }
}

internal class DarkRotation : DarkCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.DarkOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == PLD.RageOfHalone)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseResources);

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
