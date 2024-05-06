using System;
using System.Collections.Generic;
using System.Linq;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class BRD
{
    public const byte ClassID = 5;
    public const byte JobID = 23;

    public const uint
        HeavyShot = 97,
        StraightShot = 98,
        VenomousBite = 100,
        RagingStrikes = 101,
        QuickNock = 106,
        Barrage = 107,
        Bloodletter = 110,
        Windbite = 113,
        MagesBallad = 114,
        ArmysPaeon = 116,
        RainOfDeath = 117,
        BattleVoice = 118,
        EmpyrealArrow = 3558,
        WanderersMinuet = 3559,
        IronJaws = 3560,
        Sidewinder = 3562,
        PitchPerfect = 7404,
        Troubadour = 7405,
        CausticBite = 7406,
        Stormbite = 7407,
        NaturesMinne = 7408,
        SecondWind = 7541,
        ArmsLength = 7548,
        RefulgentArrow = 7409,
        Peloton = 7557,
        BurstShot = 16495,
        ApexArrow = 16496,
        Shadowbite = 16494,
        Ladonsbite = 25783,
        BlastArrow = 25784,
        RadiantFinale = 25785,
        WideVolley = 36974,
        ResonantArrow = 36976,
        RadiantEncore = 36977;

    public static class Buffs
    {
        public const ushort
            StraightShotReady = 122,
            RagingStrikes = 125,
            Barrage = 128,
            BattleVoice = 141,
            WanderersMinuet = 2009,
            BlastShotReady = 2692,
            RadiantFinaleDamageUp = 2964,
            ShadowbiteReady = 3002,
            HawksEye = 3861,
            ResonantArrowReady = 3862,
            RadiantEncoreReady = 3863;
    }

    public static class Debuffs
    {
        public const ushort
            VenomousBite = 124,
            Windbite = 129,
            CausticBite = 1200,
            Stormbite = 1201;
    }

    public static class Levels
    {
        public const byte
            StraightShot = 2,
            RagingStrikes = 4,
            VenomousBite = 6,
            Bloodletter = 12,
            WideVolley = 25,
            MagesBallad = 30,
            Windbite = 30,
            Barrage = 38,
            ArmysPaeon = 40,
            RainOfDeath = 45,
            BattleVoice = 50,
            WanderersMinuet = 52,
            PitchPerfect = 52,
            EmpyrealArrow = 54,
            IronJaws = 56,
            Sidewinder = 60,
            Troubadour = 62,
            BiteUpgrade = 64,
            NaturesMinne = 66,
            RefulgentArrow = 70,
            Shadowbite = 72,
            BurstShot = 76,
            ApexArrow = 80,
            Ladonsbite = 82,
            BlastShot = 86,
            RadiantFinale = 90;
    }
}

internal abstract class BardCombo : CustomCombo
{
    private const float GcdRecast = 2.5f;
    private const float EarlyWeaveGcdRemainingThreshold = 1.0f;
    private const float DefaultClipThreshold = 0.6f;
    private const float EmpyrealArrowClipThreshold = 0.4f;
    private const float PitchPerfectClipThreshold = 0.4f;

    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        ApplyFreshDoTs = 1 << 2,
        UseTroubadour = 1 << 3,
        UseNaturesMinne = 1 << 4,
        UseArmsLength = 1 << 5,
        UseSecondWind = 1 << 6,
    }

    protected static uint GetRotationAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<BRDGauge>();

        if (Gcd() > 0)
        {
            var weaveAction = GetWeaveAction(level, options);
            if (weaveAction > 0)
                return weaveAction;
        }

        if (InCombat() && HasNoTarget())
        {
            var songAction = GetSongAction(level, options, clipGcd: true);
            if (songAction > 0)
                return songAction;
        }

        if (!options.HasFlag(GetActionOptions.MultiTarget))
        {
            var dotAction = GetDoTAction(level, options);
            if (dotAction > 0)
                return dotAction;
        }

        if (HasEffect(BRD.Buffs.BlastShotReady))
            return BRD.BlastArrow;

        if (level >= BRD.Levels.ApexArrow && gauge.SoulVoice >= 80)
        {
            if (HasEffect(BRD.Buffs.RagingStrikes))
                return BRD.ApexArrow;

            if (GetCooldown(BRD.RagingStrikes).CooldownRemaining - Gcd() > 60)
            {
                if (gauge.SoulVoice >= 100)
                    return BRD.ApexArrow;

                if (GetCooldown(BRD.RagingStrikes).CooldownRemaining - Gcd() < 70)
                    return BRD.ApexArrow;
            }
        }

        if (HasEffect(BRD.Buffs.Barrage))
            return options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(BRD.WideVolley) : OriginalHook(BRD.StraightShot);

        if (HasEffect(BRD.Buffs.RadiantEncoreReady))
        {
            if (HasEffect(BRD.Buffs.RagingStrikes))
                return BRD.RadiantEncore;

            if (GetEffectRemainingTime(BRD.Buffs.RadiantEncoreReady) < GetCooldown(BRD.RagingStrikes).CooldownRemaining)
                return BRD.RadiantEncore;
        }

        if (HasEffect(BRD.Buffs.ResonantArrowReady))
        {
            if (HasEffect(BRD.Buffs.RagingStrikes))
                return BRD.ResonantArrow;

            if (GetEffectRemainingTime(BRD.Buffs.ResonantArrowReady) < GetCooldown(BRD.RagingStrikes).CooldownRemaining)
                return BRD.ResonantArrow;
        }

        if (!options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (HasEffect(BRD.Buffs.HawksEye))
                return OriginalHook(BRD.StraightShot);

            var earlyDotAction = GetDoTAction(level, options, allowOneGcdEarly: true);
            if (earlyDotAction > 0)
                return earlyDotAction;

            return OriginalHook(BRD.HeavyShot);
        }
        else
        {
            if (level >= BRD.Levels.WideVolley && HasEffect(BRD.Buffs.HawksEye))
                return OriginalHook(BRD.WideVolley);

            return OriginalHook(BRD.QuickNock);
        }
    }

    protected static uint GetDoTAction(byte level, GetActionOptions options, bool allowOneGcdEarly = false)
    {
        if (!ShouldUseDoT(level))
            return 0;

        if (CurrentTarget == null)
            return 0;

        ushort venomousDebuff, windbiteDebuff;
        if (level < BRD.Levels.BiteUpgrade)
        {
            venomousDebuff = BRD.Debuffs.VenomousBite;
            windbiteDebuff = BRD.Debuffs.Windbite;
        }
        else
        {
            venomousDebuff = BRD.Debuffs.CausticBite;
            windbiteDebuff = BRD.Debuffs.Stormbite;
        }

        if (level >= BRD.Levels.Windbite && !TargetHasEffect(windbiteDebuff))
        {
            if (options.HasFlag(GetActionOptions.ApplyFreshDoTs) || TargetHasEffect(venomousDebuff))
                return OriginalHook(BRD.Windbite);
        }

        if (level >= BRD.Levels.VenomousBite && !TargetHasEffect(venomousDebuff))
        {
            if (options.HasFlag(GetActionOptions.ApplyFreshDoTs) || TargetHasEffect(windbiteDebuff))
                return OriginalHook(BRD.VenomousBite);
        }

        var venomousRemainingTime = GetTargetEffectRemainingTime(venomousDebuff);
        var windbiteRemainingTime = GetTargetEffectRemainingTime(windbiteDebuff);
        var refreshThreshold = allowOneGcdEarly ? GcdRecast * 2 : GcdRecast;

        if (TargetHasEffect(windbiteDebuff) && windbiteRemainingTime - Gcd() < refreshThreshold)
        {
            if (level >= BRD.Levels.IronJaws)
                return BRD.IronJaws;
            else
                return OriginalHook(BRD.Windbite);
        }

        if (TargetHasEffect(venomousDebuff) && venomousRemainingTime - Gcd() < refreshThreshold)
        {
            if (level >= BRD.Levels.IronJaws)
                return BRD.IronJaws;
            else
                return OriginalHook(BRD.VenomousBite);
        }

        var shortestBuffRemainingTime = GetShortestBuffRemainingTime();
        if (shortestBuffRemainingTime > 0 && shortestBuffRemainingTime - Gcd() < refreshThreshold)
        {
            var venomousRefreshed = venomousRemainingTime > 35;
            var windbiteRefreshed = windbiteRemainingTime > 35;

            if (!venomousRefreshed || !windbiteRefreshed)
            {
                if (level >= BRD.Levels.IronJaws)
                    return BRD.IronJaws;
            }
        }

        return 0;
    }

    private static uint GetWeaveAction(byte level, GetActionOptions options)
    {
        var isOpener = IsOffCooldown(BRD.RagingStrikes) && IsOffCooldown(BRD.BattleVoice) && IsOffCooldown(BRD.RadiantFinale) && IsOffCooldown(BRD.EmpyrealArrow);

        if (level >= BRD.Levels.Bloodletter && TimeToCap(BRD.Bloodletter, 15) < 5 && isOpener)
            return GetBloodletterOrRainOfDeath(level, options);

        var songAction = GetSongAction(level, options);
        if (songAction > 0)
            return songAction;

        if (!options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (level >= BRD.Levels.RadiantFinale && CanWeaveWithoutBlock(BRD.RadiantFinale) && CanWeave(BRD.BattleVoice))
            {
                if (!isOpener || !IsEarlyWeave())
                    return BRD.RadiantFinale;
            }

            if (level >= BRD.Levels.BattleVoice && CanWeaveWithoutBlock(BRD.BattleVoice))
            {
                if (level < BRD.Levels.RadiantFinale || IsOnCooldown(BRD.RadiantFinale))
                    return BRD.BattleVoice;
            }

            if (level >= BRD.Levels.RagingStrikes && CanWeaveWithoutBlock(BRD.RagingStrikes) && !IsEarlyWeave())
            {
                if (level < BRD.Levels.BattleVoice || IsOnCooldown(BRD.BattleVoice))
                    return BRD.RagingStrikes;
            }
        }

        var ogcdDmgAction = GetOgcdDmgAction(level, options);

        switch (ogcdDmgAction)
        {
            case BRD.EmpyrealArrow:
            case BRD.PitchPerfect:
                return ogcdDmgAction;
        }

        if (HasEffect(BRD.Buffs.RagingStrikes))
        {
            if (level >= BRD.Levels.Bloodletter && CanWeaveWithoutBlock(BRD.Bloodletter))
            {
                if (GetRemainingCharges(BRD.Bloodletter) == GetMaxCharges(BRD.Bloodletter))
                    return GetBloodletterOrRainOfDeath(level, options);
            }

            if (level >= BRD.Levels.Barrage && CanWeaveWithoutBlock(BRD.Barrage))
                return BRD.Barrage;
        }

        if (level >= BRD.Levels.Troubadour && options.HasFlag(GetActionOptions.UseTroubadour) && CanWeaveWithoutBlock(BRD.Troubadour))
            return BRD.Troubadour;

        if (level >= BRD.Levels.NaturesMinne && options.HasFlag(GetActionOptions.UseNaturesMinne) && CanWeaveWithoutBlock(BRD.NaturesMinne))
            return BRD.NaturesMinne;

        if (options.HasFlag(GetActionOptions.UseArmsLength) && CanWeaveWithoutBlock(BRD.ArmsLength))
            return BRD.ArmsLength;

        if (options.HasFlag(GetActionOptions.UseSecondWind) && CanWeaveWithoutBlock(BRD.SecondWind))
            return BRD.SecondWind;

        if (ogcdDmgAction > 0)
            return ogcdDmgAction;

        return 0;
    }

    private static uint GetSongAction(byte level, GetActionOptions options, bool clipGcd = false)
    {
        var gauge = GetJobGauge<BRDGauge>();
        var clipThreshold = clipGcd ? 0 : DefaultClipThreshold;

        if (level >= BRD.Levels.WanderersMinuet && CanWeave(BRD.WanderersMinuet, noBlock: true, clipThreshold: clipThreshold)
                && !options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (gauge.Song == Song.ARMY && IsEarlyWeave())
                return BRD.ArmysPaeon;

            return BRD.WanderersMinuet;
        }

        var wandererCd = GetCooldown(BRD.WanderersMinuet).CooldownRemaining;

        if (level >= BRD.Levels.MagesBallad && CanWeave(BRD.MagesBallad, noBlock: true, clipThreshold: clipThreshold))
        {
            if (gauge.Song == Song.WANDERER && gauge.SongTimer < 3000 && gauge.Repertoire == 0)
                return BRD.MagesBallad;

            if (gauge.Song == Song.NONE)
            {
                if (level >= BRD.Levels.WanderersMinuet && wandererCd > 42)
                    return BRD.MagesBallad;

                if (level < BRD.Levels.WanderersMinuet && !options.HasFlag(GetActionOptions.DelayBurst))
                    return BRD.MagesBallad;
            }
        }

        if (level >= BRD.Levels.ArmysPaeon && CanWeave(BRD.ArmysPaeon, noBlock: true, clipThreshold: clipThreshold))
        {
            if (gauge.Song == Song.MAGE && gauge.SongTimer < 3000)
                return BRD.ArmysPaeon;

            if (gauge.Song == Song.NONE)
            {
                if (level >= BRD.Levels.WanderersMinuet && wandererCd > 24)
                    return BRD.ArmysPaeon;

                if (level < BRD.Levels.WanderersMinuet && !options.HasFlag(GetActionOptions.DelayBurst))
                    return BRD.ArmysPaeon;
            }
        }

        return 0;
    }

    private static uint GetOgcdDmgAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<BRDGauge>();

        var ogcdDmgAction = gauge.Song switch
        {
            Song.WANDERER => GetWandererOgcdDmgAction(level, options),
            Song.MAGE => GetMageOgcdDmgAction(level, options),
            _ => GetDefaultOgcdDmgAction(level, options),
        };

        if (ogcdDmgAction == BRD.EmpyrealArrow)
        {
            if (GetCooldown(BRD.RagingStrikes).CooldownRemaining < Gcd())
                ogcdDmgAction = 0;
        }

        return ogcdDmgAction;
    }

    private static uint GetWandererOgcdDmgAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<BRDGauge>();

        if (gauge.Repertoire == 3 && CanWeave(BRD.PitchPerfect, clipThreshold: PitchPerfectClipThreshold))
            return BRD.PitchPerfect;

        if (level >= BRD.Levels.EmpyrealArrow && CanWeave(BRD.EmpyrealArrow, noBlock: true, clipThreshold: EmpyrealArrowClipThreshold))
            return BRD.EmpyrealArrow;

        if (gauge.Repertoire == 2 && ShouldUsePitchPerfectAtTwoStacks(level)
                && CanWeave(BRD.PitchPerfect, clipThreshold: PitchPerfectClipThreshold))
            return BRD.PitchPerfect;

        var defaultOgcd = GetDefaultOgcdDmgAction(level, options);
        if (defaultOgcd > 0)
            return defaultOgcd;

        if (gauge.Repertoire > 0 && CanWeave(BRD.PitchPerfect, clipThreshold: PitchPerfectClipThreshold))
        {
            var shortestBuffRemainingTime = GetShortestBuffRemainingTime();
            if (shortestBuffRemainingTime > 0)
            {
                if (shortestBuffRemainingTime < Gcd())
                    return BRD.PitchPerfect;

                if (!IsEarlyWeave() && shortestBuffRemainingTime - Gcd() < (GcdRecast / 2))
                    return BRD.PitchPerfect;
            }

            if (gauge.SongTimer < 3000)
                return BRD.PitchPerfect;
        }

        return 0;
    }

    private static uint GetMageOgcdDmgAction(byte level, GetActionOptions options)
    {
        if (TimeToCap(BRD.Bloodletter, 15) < 8 && CanWeaveWithoutBlock(BRD.Bloodletter))
            return GetBloodletterOrRainOfDeath(level, options);

        var defaultOgcd = GetDefaultOgcdDmgAction(level, options);
        if (defaultOgcd > 0)
            return defaultOgcd;

        if (GetRemainingCharges(BRD.Bloodletter) > 0 && CanWeaveWithoutBlock(BRD.Bloodletter))
            return GetBloodletterOrRainOfDeath(level, options);

        return 0;
    }

    private static uint GetDefaultOgcdDmgAction(byte level, GetActionOptions options)
    {
        if (level >= BRD.Levels.EmpyrealArrow && CanWeave(BRD.EmpyrealArrow, noBlock: true, clipThreshold: EmpyrealArrowClipThreshold))
        {
            if (IsOnCooldown(BRD.RagingStrikes) || options.HasFlag(GetActionOptions.DelayBurst))
                return BRD.EmpyrealArrow;
        }

        if (level >= BRD.Levels.Sidewinder && CanWeaveWithoutBlock(BRD.Sidewinder) && ShouldUseSidewinder())
            return BRD.Sidewinder;

        if (level >= BRD.Levels.Bloodletter && GetRemainingCharges(BRD.Bloodletter) > 0 && CanWeaveWithoutBlock(BRD.Bloodletter))
        {
            if (HasEffect(BRD.Buffs.RagingStrikes))
                return GetBloodletterOrRainOfDeath(level, options);

            if (TimeToCap(BRD.Bloodletter, 15) < GetCooldown(BRD.RagingStrikes).CooldownRemaining)
                return GetBloodletterOrRainOfDeath(level, options);
        }

        return 0;
    }

    private static uint GetBloodletterOrRainOfDeath(byte level, GetActionOptions options)
    {
        if (level >= BRD.Levels.RainOfDeath && options.HasFlag(GetActionOptions.MultiTarget))
            return OriginalHook(BRD.RainOfDeath);
        else
            return OriginalHook(BRD.Bloodletter);
    }

    private static float GetShortestBuffRemainingTime()
    {
        List<Dalamud.Game.ClientState.Statuses.Status> buffs =
        [
            FindEffect(BRD.Buffs.RagingStrikes),
            FindEffect(BRD.Buffs.BattleVoice),
            FindEffect(BRD.Buffs.RadiantFinaleDamageUp)
        ];

        return buffs.FindAll(b => b != null)
            .Select(b => Math.Abs(b.RemainingTime))
            .Where(t => t > 0)
            .Order()
            .FirstOrDefault();
    }

    private static bool ShouldUseSidewinder()
    {
        if (HasEffect(BRD.Buffs.RagingStrikes))
            return true;

        return GetCooldown(BRD.RagingStrikes).CooldownRemaining > 45;
    }

    private static bool ShouldUsePitchPerfectAtTwoStacks(byte level)
    {
        if (level < BRD.Levels.EmpyrealArrow)
            return false;

        var gauge = GetJobGauge<BRDGauge>();
        var nextProcTime = (gauge.SongTimer % 3000) / 1000f;
        var nextEarlyWeaveTime = Gcd() + (GcdRecast / 2);
        var empCd = GetCooldown(BRD.EmpyrealArrow).CooldownRemaining;
        var canLateWeaveEmpThisGcd = CanWeave(BRD.EmpyrealArrow, clipThreshold: EmpyrealArrowClipThreshold);
        var canEarlyWeaveEmpNextGcd = GetCooldown(BRD.EmpyrealArrow).CooldownRemaining < nextEarlyWeaveTime;

        if (canLateWeaveEmpThisGcd && nextProcTime < nextEarlyWeaveTime)
            return true;

        if (!IsEarlyWeave() && canEarlyWeaveEmpNextGcd && nextProcTime < empCd + PitchPerfectClipThreshold)
            return true;

        return false;
    }

    private static unsafe bool ShouldUseDoT(byte level)
    {
        if (CurrentTarget == null || CurrentTarget is not IBattleChara chara)
            return false;

        if (chara.NameId == 541) // target dummy
            return true;

        var playerCount = Math.Max(1, Service.PartyList.Length);

        return chara.CurrentHp > GetEstimatedDps(level) * playerCount * 10;
    }

    private static float GetEstimatedDps(byte level)
    {
        if (level > 100)
            return GetEstimatedDps(100);

        if (level % 10 == 0)
        {
            return level switch
            {
                0 => 0,
                50 => 400,
                60 => 800,
                70 => 1700,
                80 => 2800,
                90 => 8000,
                100 => 14000,
                _ => 0,
            };
        }

        byte prev = (byte)(level / 10 * 10);
        byte next = (byte)(prev + 10);
        return GetEstimatedDps(prev) + ((GetEstimatedDps(next) - GetEstimatedDps(prev)) * (level - prev) / 10f);
    }

    private static float Gcd()
    {
        return GetCooldown(BRD.HeavyShot).CooldownRemaining;
    }

    private static float GcdElapsed()
    {
        return GcdRecast - Gcd();
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

    private static float TimeToCap(uint actionID, float cooldown)
    {
        var cd = GetCooldown(actionID);

        if (cd.MaxCharges == cd.RemainingCharges)
            return 0;

        var missingCharges = cd.MaxCharges - cd.RemainingCharges;
        return ((missingCharges - 1) * cooldown) + cd.ChargeCooldownRemaining;
    }
}

internal class BardRotation : BardCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.BardOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target

        if (actionID == PLD.FastBlade)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs | GetActionOptions.DelayBurst);

        // AoE

        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        // Utility

        if (actionID == PLD.FightOrFlight)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs | GetActionOptions.UseTroubadour);

        if (actionID == PLD.Requiescat)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs | GetActionOptions.UseNaturesMinne);

        if (actionID == PLD.HolySpirit)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs | GetActionOptions.UseSecondWind);

        if (actionID == PLD.HolyCircle)
            return GetRotationAction(level, GetActionOptions.ApplyFreshDoTs | GetActionOptions.UseArmsLength);

        return actionID;
    }
}
