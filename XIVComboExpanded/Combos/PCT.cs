using System;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace XIVComboExpandedPlugin.Combos;

internal static class PCT
{
    public const byte JobID = 42;

    public const uint
        Swiftcast = 7561,
        LucidDreaming = 7562,
        FireRed = 34650,
        AeroGreen = 34651,
        WaterBlue = 34652,
        BlizzardCyan = 34653,
        EarthYellow = 34654,
        ThunderMagenta = 34655,
        FireRed2 = 34656,
        AeroGreen2 = 34657,
        WaterBlue2 = 34658,
        BlizzardCyan2 = 34659,
        EarthYellow2 = 34660,
        ThunderMagenta2 = 34661,
        HolyInWhite = 34662,
        CometBlack = 34663,
        PomMotif = 34664,
        WingMotif = 34665,
        ClawMotif = 34666,
        MawMotif = 34667,
        HammerMotif = 34668,
        StarrySkyMotif = 34669,
        PomMuse = 34670,
        WingedMuse = 34671,
        ClawedMuse = 34672,
        FangedMuse = 34673,
        StrikingMuse = 34674,
        StarryMuse = 34675,
        MogOftheAges = 34676,
        Retribution = 34677,
        HammerStamp = 34678,
        HammerBrush = 34679,
        PolishingHammer = 34680,
        StarPrism1 = 34681,
        StarPrism2 = 34682,
        SubstractivePalette = 34683,
        Smudge = 34684,
        TemperaCoat = 34685,
        TemperaGrassa = 34686,
        RainbowDrip = 34688,
        CreatureMotif = 34689,
        WeaponMotif = 34690,
        LandscapeMotif = 34691,
        LivingMuse = 35347,
        SteelMuse = 35348,
        ScenicMuse = 35349;

    public static class Buffs
    {
        public const ushort
            SubstractivePalette = 3674,
            Chroma2Ready = 3675,
            Chroma3Ready = 3676,
            RainbowReady = 3679,
            HammerReady = 3680,
            StarPrismReady = 3681,
            Hyperphantasia = 3688,
            ArtisticInstallation = 3689,
            SubstractivePaletteReady = 3690,
            MonochromeTones = 3691;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            FireRed = 1,
            AeroGreen = 5,
            TemperaCoat = 10,
            WaterBlue = 15,
            Smudge = 20,
            FireRed2 = 25,
            CreatureMotif = 30,
            PomMotif = 30,
            WingMotif = 30,
            PomMuse = 30,
            WingedMuse = 30,
            MogOftheAges = 30,
            AeroGreen2 = 35,
            WaterBlue2 = 45,
            HammerMotif = 50,
            HammerStamp = 50,
            WeaponMotif = 50,
            StrikingMuse = 50,
            SubstractivePalette = 60,
            BlizzardCyan = 60,
            EarthYellow = 60,
            ThunderMagenta = 60,
            BlizzardCyan2 = 60,
            EarthYellow2 = 60,
            ThunderMagenta2 = 60,
            StarrySkyMotif = 70,
            LandscapeMotif = 70,
            MiracleWhite = 80,
            HammerBrush = 86,
            PolishingHammer = 86,
            TemperaGrassa = 88,
            CometBlack = 90,
            RainbowDrip = 92,
            ClawMotif = 96,
            MawMotif = 96,
            ClawedMuse = 96,
            FangedMuse = 96,
            StarryMuse = 70,
            Retribution = 96,
            StarPrism1 = 100,
            StarPrism2 = 100;
    }
}

internal abstract class PictomancerCombo : CustomCombo
{
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
        var weaveAction = GetWeaveAction(level, options);
        if (weaveAction > 0)
            return weaveAction;

        return GetGcdAction(level, options);
    }

    private static uint GetWeaveAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<PCTGauge>();
        var useStarryMuse = gauge.LandscapeMotifDrawn && CanWeaveWithoutClip(PCT.StarryMuse) && !options.HasFlag(GetActionOptions.DelayBurst);
        var hasMogOrMadeen = gauge.CreatureFlags.HasFlag(CreatureFlags.MooglePortait) || gauge.CreatureFlags.HasFlag(CreatureFlags.MadeenPortrait);

        if (Gcd() < 0.6)
            return 0;

        if (InCombat())
        {
            if (gauge.WeaponMotifDrawn && CanWeaveWithoutClip(PCT.StrikingMuse))
            {
                var hasNoBurstGcd = gauge.Paint == 0 && !HasEffect(PCT.Buffs.SubstractivePalette);
                if (!useStarryMuse || hasNoBurstGcd)
                    return OriginalHook(PCT.StrikingMuse);
            }

            if (useStarryMuse)
                return PCT.StarryMuse;
        }

        if (IsBurstWindow())
        {
            if (CanUseSubtractivePalette())
                return PCT.SubstractivePalette;

            if (hasMogOrMadeen && CanWeaveWithoutClip(PCT.MogOftheAges))
                return OriginalHook(PCT.MogOftheAges);

            if (gauge.CreatureMotifDrawn && CanWeaveWithoutClip(PCT.LivingMuse))
                return OriginalHook(PCT.LivingMuse);
        }

        if (hasMogOrMadeen && CanWeaveWithoutClip(PCT.MogOftheAges))
        {
            var burstDeadline = GetCooldown(PCT.StarryMuse).CooldownRemaining + 15;
            var willBeReadyForBurst = TimeToNextMog() < burstDeadline && burstDeadline > 30;
            if (level < PCT.Levels.StarryMuse || willBeReadyForBurst || options.HasFlag(GetActionOptions.UseResources))
                return OriginalHook(PCT.MogOftheAges);
        }

        if (gauge.CreatureMotifDrawn && CanWeaveWithoutClip(PCT.LivingMuse))
        {
            var willBeReadyForBurst = GetCooldown(PCT.LivingMuse).ChargeCooldownRemaining <= GetCooldown(PCT.StarryMuse).CooldownRemaining;
            if (level < PCT.Levels.StarryMuse || willBeReadyForBurst || options.HasFlag(GetActionOptions.UseResources))
                return OriginalHook(PCT.LivingMuse);
        }

        if (CanUseSubtractivePalette())
        {
            if (gauge.PalleteGauge == 100 || options.HasFlag(GetActionOptions.UseResources))
                return PCT.SubstractivePalette;
        }

        if (CanWeaveWithoutClip(PCT.LucidDreaming) && LocalPlayer?.CurrentMp <= 8000)
            return PCT.LucidDreaming;

        return 0;
    }

    protected static uint GetGcdAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<PCTGauge>();
        var movementGcd = GetMovementGcd();
        var weaveWeapon = gauge.WeaponMotifDrawn && GetCooldown(PCT.SteelMuse).CooldownRemaining - Gcd() < 1.5;
        var weaveLandscape = gauge.LandscapeMotifDrawn && GetCooldown(PCT.ScenicMuse).CooldownRemaining - Gcd() < 1.5 && !options.HasFlag(GetActionOptions.DelayBurst);
        var isCombatWeave = InCombat() && (weaveWeapon || weaveLandscape);

        if (isCombatWeave || IsPlayerMoving())
        {
            if (movementGcd > 0)
                return movementGcd;
        }

        if (IsBurstWindow() || options.HasFlag(GetActionOptions.UseResources))
        {
            if (CanUseSubtractivePalette() && movementGcd > 0)
                return movementGcd;

            if (HasEffect(PCT.Buffs.Hyperphantasia) && FindEffect(PCT.Buffs.Hyperphantasia)!.StackCount > 2)
                return GetComboAction(options);

            if (HasEffect(PCT.Buffs.MonochromeTones) && gauge.Paint > 0)
                return PCT.CometBlack;

            if (HasEffect(PCT.Buffs.StarPrismReady))
                return PCT.StarPrism1;

            if (HasEffect(PCT.Buffs.RainbowReady))
                return PCT.RainbowDrip;

            if (HasEffect(PCT.Buffs.HammerReady) && !HasEffect(PCT.Buffs.SubstractivePalette))
                return OriginalHook(PCT.HammerStamp);
        }

        if (IsHammerExpiring())
            return OriginalHook(PCT.HammerStamp);

        return GetComboAction(options);
    }

    private static uint GetComboAction(GetActionOptions options)
    {
        if (HasEffect(PCT.Buffs.SubstractivePalette))
            return options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(PCT.BlizzardCyan2) : OriginalHook(PCT.BlizzardCyan);

        return options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(PCT.FireRed2) : OriginalHook(PCT.FireRed);
    }

    protected static uint GetMovementGcd()
    {
        var gauge = GetJobGauge<PCTGauge>();

        if (HasEffect(PCT.Buffs.MonochromeTones) && gauge.Paint > 0)
            return PCT.CometBlack;

        if (HasEffect(PCT.Buffs.StarPrismReady))
            return PCT.StarPrism1;

        if (HasEffect(PCT.Buffs.RainbowReady))
            return PCT.RainbowDrip;

        if (HasEffect(PCT.Buffs.HammerReady))
            return OriginalHook(PCT.HammerStamp);

        if (gauge.Paint > 0)
            return PCT.HolyInWhite;

        return 0;
    }

    private static bool IsHammerExpiring()
    {
        if (!HasEffect(PCT.Buffs.HammerReady))
            return false;

        var remainingTime = GetEffectRemainingTime(PCT.Buffs.HammerReady);
        var stackCount = FindEffect(PCT.Buffs.HammerReady)!.StackCount;

        return remainingTime - Gcd() < (stackCount + 1) * 2.5;
    }

    private static bool CanUseSubtractivePalette()
    {
        var gauge = GetJobGauge<PCTGauge>();
        var hasBlackComet = HasEffect(PCT.Buffs.MonochromeTones) && gauge.Paint > 0;

        if (HasEffect(PCT.Buffs.SubstractivePalette) || hasBlackComet)
            return false;

        return gauge.PalleteGauge >= 50 || HasEffect(PCT.Buffs.SubstractivePaletteReady);
    }

    private static float TimeToNextMog()
    {
        var gauge = GetJobGauge<PCTGauge>();
        var hasPom = gauge.CreatureFlags.HasFlag(CreatureFlags.Pom);
        var hasClaw = gauge.CreatureFlags.HasFlag(CreatureFlags.Claw);

        if (hasPom || hasClaw)
            return HasCharges(PCT.LivingMuse) ? 0 : GetCooldown(PCT.LivingMuse).ChargeCooldownRemaining;

        if (IsOffCooldown(PCT.LivingMuse))
            return 0;

        return GetCooldown(PCT.LivingMuse).ChargeCooldownRemaining + (HasCharges(PCT.LivingMuse) ? 0 : 30);
    }

    private static bool IsBurstWindow()
    {
        return IsOnCooldown(PCT.StarryMuse) && GetCooldown(PCT.StarryMuse).CooldownElapsed < 30;
    }

    private static uint GetMorphedAction(uint actionId)
    {
        var morphed = OriginalHook(actionId);
        return morphed != actionId ? morphed : 0;
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
        return GetCooldown(PCT.FireRed).CooldownRemaining;
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

    private static unsafe bool IsPlayerMoving()
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return false;

        return agentMap->IsPlayerMoving > 0;
    }
}

internal class PictomancerRotation : PictomancerCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PictomancerOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.DelayBurst);

        if (actionID == PLD.RageOfHalone)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.UseResources);

        if (actionID == PLD.ShieldLob)
            return GetMovementGcd() > 0 ? GetMovementGcd() : PCT.HolyInWhite;

        // AoE
        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        if (actionID == PLD.CircleOfScorn)
            return GetRotationAction(lastComboMove, comboTime, level, GetActionOptions.MultiTarget | GetActionOptions.UseResources);

        return actionID;
    }
}