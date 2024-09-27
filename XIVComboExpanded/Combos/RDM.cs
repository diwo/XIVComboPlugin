using System;

using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace XIVComboExpandedPlugin.Combos;

internal static class RDM
{
    public const byte JobID = 35;

    public const uint
        Jolt = 7503,
        Riposte = 7504,
        Verthunder = 7505,
        CorpsACorps = 7506,
        Veraero = 7507,
        Scatter = 7509,
        Verfire = 7510,
        Verstone = 7511,
        Zwerchhau = 7512,
        Moulinet = 7513,
        Vercure = 7514,
        Redoublement = 7516,
        Fleche = 7517,
        Acceleration = 7518,
        ContreSixte = 7519,
        Embolden = 7520,
        Manafication = 7521,
        Verraise = 7523,
        Jolt2 = 7524,
        Verflare = 7525,
        Verholy = 7526,
        EnchantedRiposte = 7527,
        EnchantedZwerchhau = 7528,
        EnchantedRedoublement = 7529,
        Swiftcast = 7561,
        LucidDreaming = 7562,
        Verthunder2 = 16524,
        Veraero2 = 16525,
        Impact = 16526,
        Engagement = 16527,
        Reprise = 16528,
        Scorch = 16530,
        Verthunder3 = 25855,
        Veraero3 = 25856,
        Resolution = 25858,
        Jolt3 = 37004,
        ViceOfThorns = 37005,
        GrandImpact = 37006,
        Prefulgence = 37007;

    public static class Buffs
    {
        public const ushort
            Swiftcast = 167,
            VerfireReady = 1234,
            VerstoneReady = 1235,
            Acceleration = 1238,
            Embolden = 1239,
            Dualcast = 1249,
            Manafication = 1971,
            LostChainspell = 2560,
            ThornedFlourish = 3876,
            GrandImpactReady = 3877,
            PrefulgenceReady = 3878;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            Jolt = 2,
            Verthunder = 4,
            CorpsACorps = 6,
            Veraero = 10,
            Scatter = 15,
            Verthunder2 = 18,
            Veraero2 = 22,
            Zwerchhau = 35,
            Engagement = 40,
            Fleche = 45,
            Redoublement = 50,
            Acceleration = 50,
            Vercure = 54,
            ContreSixte = 56,
            Embolden = 58,
            Manafication = 60,
            Jolt2 = 62,
            Verraise = 64,
            Impact = 66,
            Verflare = 68,
            Verholy = 70,
            EnchantedReprise = 76,
            Scorch = 80,
            Veraero3 = 82,
            Verthunder3 = 82,
            Resolution = 90,
            ViceOfThorns = 92,
            GrandImpact = 96,
            Prefulgence = 100;
    }
}

internal class RedMageCombo : CustomCombo
{
    private const float GcdRecast = 2.42f;
    private const float EarlyWeaveGcdRemainingThreshold = 1.0f;
    private const float DefaultClipThreshold = 0.6f;

    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RedMageOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single Target
        if (actionID == RDM.Jolt || actionID == RDM.Jolt2 || actionID == RDM.Jolt3)
            return Weave(GetSingleTargetSpellCombo, lastComboMove, level);

        if (actionID == RDM.Redoublement)
            return Weave(GetSingleTargetMeleeCombo, lastComboMove, level, useDash: true);

        // Display
        if (actionID == PLD.GoringBlade)
            return GetSingleTargetSpellCombo(lastComboMove, level);

        // AoE
        if (actionID == RDM.Scatter || actionID == RDM.Impact)
            return Weave(GetMultiTargetSpellCombo, lastComboMove, level);

        if (actionID == RDM.Moulinet)
            return Weave(GetMultiTargetMeleeCombo, lastComboMove, level, useDash: true);

        // Other
        if (actionID == RDM.Verraise)
            return GetVerraiseSwiftAction();

        return actionID;
    }

    private static uint GetSingleTargetSpellCombo(uint lastComboMove, byte level)
    {
        var manaSpellCombo = GetManaSpellCombo(lastComboMove, level);
        if (manaSpellCombo > 0)
            return manaSpellCombo;

        if (HasInstaCast() && level >= RDM.Levels.Verthunder)
            return GetVeraeroOrVerthunder(level);

        if (ShouldUseGrandImpact(level))
            return RDM.GrandImpact;

        if (Gcd() == 0 && IsPlayerMoving() && HasTarget() && !HasEffect(RDM.Buffs.GrandImpactReady))
        {
            var ogcdAction = GetWeaveAction(level, isNextGcdHardCast: true, useInGcdSlot: true);
            return ogcdAction > 0 ? ogcdAction : RDM.Swiftcast;
        }

        return GetVerstoneOrVerfireOrJolt(level);
    }

    private static uint GetMultiTargetSpellCombo(uint lastComboMove, byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        var manaSpellCombo = GetManaSpellCombo(lastComboMove, level);
        if (manaSpellCombo > 0)
            return manaSpellCombo;

        if (HasInstaCast())
            return OriginalHook(RDM.Scatter);

        if (ShouldUseGrandImpact(level))
            return RDM.GrandImpact;

        if (Gcd() == 0 && IsPlayerMoving() && HasTarget() && !HasEffect(RDM.Buffs.GrandImpactReady))
        {
            var ogcdAction = GetWeaveAction(level, isNextGcdHardCast: true, useInGcdSlot: true);
            return ogcdAction > 0 ? ogcdAction : RDM.Swiftcast;
        }

        if (level < RDM.Levels.Verthunder2)
            return OriginalHook(RDM.Scatter);

        if (level < RDM.Levels.Veraero2)
            return RDM.Verthunder2;

        return gauge.WhiteMana <= gauge.BlackMana ? RDM.Veraero2 : RDM.Verthunder2;
    }

    private static uint GetSingleTargetMeleeCombo(uint lastComboMove, byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        if (lastComboMove == RDM.Zwerchhau && level >= RDM.Levels.Redoublement)
            return OriginalHook(RDM.Redoublement);

        if ((lastComboMove == RDM.Riposte) && level >= RDM.Levels.Zwerchhau)
            return OriginalHook(RDM.Zwerchhau);

        var manaSpellCombo = GetManaSpellCombo(lastComboMove, level);
        if (manaSpellCombo > 0)
            return manaSpellCombo;

        if (ShouldUseGrandImpact(level))
            return RDM.GrandImpact;

        if (HasInstaCast())
            return GetSingleTargetSpellCombo(lastComboMove, level);

        if ((gauge.WhiteMana >= 50 && gauge.BlackMana >= 50) || HasEffect(RDM.Buffs.Manafication))
            return OriginalHook(RDM.Riposte);

        if (level >= RDM.Levels.EnchantedReprise && gauge.WhiteMana >= 5 && gauge.BlackMana >= 5)
            return OriginalHook(RDM.Reprise);

        return OriginalHook(RDM.Riposte);
    }

    private static uint GetMultiTargetMeleeCombo(uint lastComboMove, byte level)
    {
        var manaSpellCombo = GetManaSpellCombo(lastComboMove, level);
        if (manaSpellCombo > 0)
            return manaSpellCombo;

        if (ShouldUseGrandImpact(level))
            return RDM.GrandImpact;

        if (HasInstaCast())
            return GetMultiTargetSpellCombo(lastComboMove, level);

        return OriginalHook(RDM.Moulinet);
    }

    private static uint GetManaSpellCombo(uint lastComboMove, byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        if ((lastComboMove == RDM.Verflare || lastComboMove == RDM.Verholy) && level >= RDM.Levels.Scorch)
            return RDM.Scorch;

        if (lastComboMove == RDM.Scorch && level >= RDM.Levels.Resolution)
            return RDM.Resolution;

        if (gauge.ManaStacks == 3)
            return GetVerholyOrVerflare(level);

        return 0;
    }

    private static uint GetVerholyOrVerflare(byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        if (level < RDM.Levels.Verholy)
            return RDM.Verflare;

        // if (HasEffect(RDM.Buffs.VerstoneReady) && !HasEffect(RDM.Buffs.VerfireReady))
        //     return RDM.Verflare;

        // if (!HasEffect(RDM.Buffs.VerstoneReady) && HasEffect(RDM.Buffs.VerfireReady))
        //     return RDM.Verholy;

        return gauge.WhiteMana <= gauge.BlackMana ? RDM.Verholy : RDM.Verflare;
    }

    private static uint GetVeraeroOrVerthunder(byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        if (level < RDM.Levels.Veraero)
            return OriginalHook(RDM.Verthunder);

        if (HasEffect(RDM.Buffs.VerstoneReady) && !HasEffect(RDM.Buffs.VerfireReady))
            return OriginalHook(RDM.Verthunder);

        if (!HasEffect(RDM.Buffs.VerstoneReady) && HasEffect(RDM.Buffs.VerfireReady))
            return OriginalHook(RDM.Veraero);

        return gauge.WhiteMana <= gauge.BlackMana ? OriginalHook(RDM.Veraero) : OriginalHook(RDM.Verthunder);
    }

    private static uint GetVerstoneOrVerfireOrJolt(byte level)
    {
        var gauge = GetJobGauge<RDMGauge>();

        if (HasEffect(RDM.Buffs.VerstoneReady) && !HasEffect(RDM.Buffs.VerfireReady))
            return RDM.Verstone;

        if (!HasEffect(RDM.Buffs.VerstoneReady) && HasEffect(RDM.Buffs.VerfireReady))
            return RDM.Verfire;

        if (HasEffect(RDM.Buffs.VerstoneReady) && HasEffect(RDM.Buffs.VerfireReady))
            return gauge.WhiteMana <= gauge.BlackMana ? RDM.Verstone : RDM.Verfire;

        return OriginalHook(RDM.Jolt);
    }

    private static bool ShouldUseGrandImpact(byte level)
    {
        if (!HasEffect(RDM.Buffs.GrandImpactReady))
            return false;

        var isExpiring = GetEffectRemainingTime(RDM.Buffs.GrandImpactReady) - Gcd() < GcdRecast * 3;

        return IsPlayerMoving() || IsWeaveFlecheContreSixteNextGcd(level) || isExpiring;
    }

    private static bool IsWeaveFlecheContreSixteNextGcd(byte level)
    {
        var isWeaveFlecheNextGcd = level >= RDM.Levels.Fleche && CanWeaveNextGcd(RDM.Fleche);
        var isWeaveContreSixteNextGcd = level >= RDM.Levels.ContreSixte && CanWeaveNextGcd(RDM.ContreSixte);
        return isWeaveFlecheNextGcd || isWeaveContreSixteNextGcd;
    }

    private static uint Weave(Func<uint, byte, uint> getGcdAction, uint lastComboMove, byte level, bool useDash = false)
    {
        var gcdAction = getGcdAction(lastComboMove, level);
        var isNextGcdHardCast = gcdAction switch
        {
            RDM.Jolt or RDM.Jolt2 or RDM.Jolt3 => true,
            RDM.Verstone or RDM.Verfire => true,
            RDM.Veraero2 or RDM.Verthunder2 => true,
            _ => false,
        };

        var weaveAction = GetWeaveAction(level, isNextGcdHardCast: isNextGcdHardCast, useDash: useDash);
        if (weaveAction > 0)
            return weaveAction;

        return gcdAction;
    }

    private static uint GetWeaveAction(byte level, bool isNextGcdHardCast = false, bool useDash = false, bool useInGcdSlot = false)
    {
        if (level >= RDM.Levels.Fleche && CanUse(RDM.Fleche, useInGcdSlot))
            return RDM.Fleche;

        if (level >= RDM.Levels.ContreSixte && CanUse(RDM.ContreSixte, useInGcdSlot))
            return RDM.ContreSixte;

        if (isNextGcdHardCast && IsWeaveFlecheContreSixteNextGcd(level))
        {
            var accelerationOrSwiftcast = GetAccelerationOrSwiftcast(level, useInGcdSlot: useInGcdSlot);
            if (accelerationOrSwiftcast > 0)
                return accelerationOrSwiftcast;
        }

        if (level >= RDM.Levels.Engagement && CanUse(RDM.Engagement, useInGcdSlot) && IsActionInRange(RDM.Engagement))
        {
            if (GetCooldown(RDM.Embolden).CooldownRemaining > Gcd() && TimeToCap(RDM.Engagement, 35) - Gcd() < GcdRecast * 6)
                return RDM.Engagement;
        }

        if (useDash && level >= RDM.Levels.CorpsACorps && CanUse(RDM.CorpsACorps, useInGcdSlot))
        {
            if (GetCooldown(RDM.Embolden).CooldownRemaining > Gcd() && TimeToCap(RDM.CorpsACorps, 35) - Gcd() < GcdRecast * 6)
                return RDM.CorpsACorps;
        }

        if (HasEffect(RDM.Buffs.PrefulgenceReady) && CanUse(RDM.Prefulgence, useInGcdSlot))
            return RDM.Prefulgence;

        if (HasEffect(RDM.Buffs.ThornedFlourish) && CanUse(RDM.ViceOfThorns, useInGcdSlot))
            return RDM.ViceOfThorns;

        if (level >= RDM.Levels.Engagement && CanUse(RDM.Engagement, useInGcdSlot) && IsActionInRange(RDM.Engagement) && HasEffect(RDM.Buffs.Embolden))
            return RDM.Engagement;

        if (useDash && level >= RDM.Levels.CorpsACorps && CanUse(RDM.CorpsACorps, useInGcdSlot))
        {
            if (HasEffect(RDM.Buffs.Embolden) || !IsActionInRange(RDM.Riposte))
                return RDM.CorpsACorps;
        }

        if (CanUse(RDM.LucidDreaming, useInGcdSlot) && LocalPlayer?.CurrentMp <= 8000)
            return RDM.LucidDreaming;

        if (isNextGcdHardCast && useInGcdSlot)
        {
            var accelerationOrSwiftcast = GetAccelerationOrSwiftcast(level, useInGcdSlot: useInGcdSlot);
            if (accelerationOrSwiftcast > 0)
                return accelerationOrSwiftcast;
        }

        return 0;
    }

    private static uint GetAccelerationOrSwiftcast(byte level, bool useInGcdSlot = false)
    {
        if (level >= RDM.Levels.Acceleration && CanUse(RDM.Acceleration, useInGcdSlot) && TimeToCap(RDM.Acceleration, 55) - Gcd() < GcdRecast * 4)
            return RDM.Acceleration;

        if (CanUse(RDM.Swiftcast, useInGcdSlot))
            return RDM.Swiftcast;

        if (level >= RDM.Levels.Acceleration && CanUse(RDM.Acceleration, useInGcdSlot))
            return RDM.Acceleration;

        return 0;
    }

    private static uint GetVerraiseSwiftAction()
    {
        var isCasting = LocalPlayer?.IsCasting ?? false;

        if (isCasting || HasEffect(RDM.Buffs.Dualcast) || HasEffect(RDM.Buffs.Swiftcast))
            return RDM.Verraise;

        if (IsOffCooldown(RDM.Swiftcast))
            return RDM.Swiftcast;

        return RDM.Verraise;
    }

    private static bool CanUse(uint actionID, bool useInGcdSlot = false)
    {
        if (CanWeaveWithoutBlock(actionID))
            return true;

        if (!useInGcdSlot)
            return false;

        if (GetCooldown(actionID).HasCharges)
            return HasCharges(actionID);

        return IsOffCooldown(actionID);
    }

    private static bool CanWeaveNextGcd(uint actionID)
    {
        return !CanWeave(actionID) && CanWeave(actionID, gcdCount: 1);
    }

    private static bool HasInstaCast()
    {
        return HasEffect(RDM.Buffs.Dualcast) || HasEffect(RDM.Buffs.Acceleration) || HasEffect(RDM.Buffs.Swiftcast) || HasEffect(RDM.Buffs.LostChainspell);
    }

    private static float Gcd()
    {
        return GetCooldown(RDM.Riposte).CooldownRemaining;
    }

    private static bool IsEarlyWeave()
    {
        return Gcd() > EarlyWeaveGcdRemainingThreshold;
    }

    private static bool CanWeave(uint actionID, uint gcdCount = 0, bool noBlock = false, float clipThreshold = DefaultClipThreshold)
    {
        if (LocalPlayer?.IsCasting ?? true)
            return false;

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

    private static float TimeToCap(uint actionID, float cooldown)
    {
        var cd = GetCooldown(actionID);

        if (cd.MaxCharges == cd.RemainingCharges)
            return 0;

        var missingCharges = cd.MaxCharges - cd.RemainingCharges;
        return ((missingCharges - 1) * cooldown) + cd.ChargeCooldownRemaining;
    }

    private static unsafe bool IsActionInRange(uint actionId)
    {
        if (LocalPlayer == null || LocalPlayer.TargetObject == null)
            return false;

        var source = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.Address;
        var target = (FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject*)LocalPlayer.TargetObject.Address;
        return FFXIVClientStructs.FFXIV.Client.Game.ActionManager.GetActionInRangeOrLoS(actionId, source, target) != 566;
    }

    private static unsafe bool IsPlayerMoving()
    {
        var agentMap = AgentMap.Instance();
        if (agentMap == null)
            return false;

        return agentMap->IsPlayerMoving > 0;
    }
}