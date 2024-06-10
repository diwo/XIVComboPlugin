using System;
using System.Linq;
using System.Runtime.InteropServices;

using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace XIVComboExpandedPlugin.Combos;

internal static class MNK
{
    public const byte ClassID = 2;
    public const byte JobID = 20;

    public const uint
        Bootshine = 53,
        TrueStrike = 54,
        SnapPunch = 56,
        TwinSnakes = 61,
        ArmOfTheDestroyer = 62,
        Mantra = 65,
        Demolish = 66,
        PerfectBalance = 69,
        Rockbreaker = 70,
        DragonKick = 74,
        Meditation = 3546,
        ForbiddenChakra = 3547,
        FormShift = 4262,
        RiddleOfEarth = 7394,
        RiddleOfFire = 7395,
        Brotherhood = 7396,
        SecondWind = 7541,
        BloodBath = 7542,
        TrueNorth = 7546,
        ArmsLength = 7548,
        Feint = 7549,
        FourPointFury = 16473,
        Enlightenment = 16474,
        SixSidedStar = 16476,
        HowlingFist = 25763,
        MasterfulBlitz = 25764,
        RiddleOfWind = 25766,
        ShadowOfTheDestroyer = 25767,
        ForbiddenMeditation = 36942,
        EarthsReply = 36944,
        LeapingOpo = 36945,
        RisingRaptor = 36946,
        PouncingCoeurl = 36947,
        WindsReply = 36949,
        FiresReply = 36950;

    public static class Buffs
    {
        public const ushort
            OpoOpoForm = 107,
            RaptorForm = 108,
            CoerlForm = 109,
            PerfectBalance = 110,
            RiddleOfFire = 1181,
            TrueNorth = 1250,
            LeadenFist = 1861,
            FormlessFist = 2513,
            DisciplinedFist = 3001,
            EarthsRumination = 3841,
            WindsRumination = 3842,
            FiresRumination = 3843;
    }

    public static class Debuffs
    {
        public const ushort
            Demolish = 246;
    }

    public static class Levels
    {
        public const byte
            Bootshine = 1,
            TrueStrike = 4,
            SnapPunch = 6,
            Meditation = 15,
            TwinSnakes = 18,
            ArmOfTheDestroyer = 26,
            Rockbreaker = 30,
            Demolish = 30,
            FourPointFury = 45,
            HowlingFist = 40,
            DragonKick = 50,
            PerfectBalance = 50,
            FormShift = 52,
            EnhancedPerfectBalance = 60,
            MasterfulBlitz = 60,
            RiddleOfFire = 68,
            Brotherhood = 70,
            Enlightenment = 70,
            RiddleOfWind = 72,
            SixSidedStar = 80,
            ShadowOfTheDestroyer = 82,
            RisingPhoenix = 86,
            FiresReply = 100;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 0x38)]
public unsafe struct BeastChakraGaugeData {
    [FieldOffset(0x0C)] public int BeastChakra1;
    [FieldOffset(0x10)] public int BeastChakra2;
    [FieldOffset(0x14)] public int BeastChakra3;
    [FieldOffset(0x18)] public bool LunarNadi;
    [FieldOffset(0x19)] public bool SolarNadi;
    [FieldOffset(0x1C)] public int BlitzTimeRemaining;
    [FieldOffset(0x20)] public int BlitzType;
    [FieldOffset(0x28)] public int OpoOpoFury;
    [FieldOffset(0x2C)] public int RaptorFury;
    [FieldOffset(0x30)] public int CoeurlFury;
}

public enum BeastChakraFixed : byte
{
    NONE = 0,
    OPOOPO = 1,
    RAPTOR = 2,
    COEURL = 3,
}

internal abstract class MonkCombo : CustomCombo
{
    private const float GcdRecast = 1.94f;
    private const float EarlyWeaveGcdRemainingThreshold = 1.0f;
    private const float DefaultClipThreshold = 0.6f;

    private static RofState rofInitState;
    private static bool isRofActiveState;
    private static bool isOpenerState;
    private static byte blitzUseCountState;
    private static bool hasBlitzState;

    private struct RofState
    {
        public float RofCooldownRemaining;
        public float RofCooldownElapsed;
        public float RofEffectRemainingTime;
        public ushort PbCharges;
        public float PbChargeCd;
        public byte PbEffectStackCount;
        public bool HasBlitz;
        public float BlitzTimeRemaining;
    }

    [Flags]
    protected enum GetActionOptions
    {
        None = 0,
        MultiTarget = 1 << 0,
        DelayBurst = 1 << 1,
        UseTrueNorth = 1 << 2,
        UseFeint = 1 << 3,
        UseRiddleOfEarth = 1 << 4,
        UseMantra = 1 << 5,
        UseSecondWind = 1 << 6,
        UseBloodBath = 1 << 7,
        UseArmsLength = 1 << 8,
    }

    protected static uint GetRotationAction(byte level, GetActionOptions options)
    {
        UpdateState();

        if (Gcd() > 0)
        {
            var weaveAction = GetWeaveAction(level, options);
            if (weaveAction > 0)
                return weaveAction;
        }

        return GetGcdAction(level, options);
    }

    private static void UpdateState()
    {
        bool wasRofActive = isRofActiveState;
        isRofActiveState = IsRiddleOfFireActive();
        if (isRofActiveState != wasRofActive)
            rofInitState = GetCurrentRofState();

        var isOpenerStart =
            IsOffCooldown(MNK.RiddleOfFire)
                && GetRemainingCharges(MNK.PerfectBalance) == GetMaxCharges(MNK.PerfectBalance)
                && HasEffect(MNK.Buffs.FormlessFist);
        var isOpenerEnd = IsOnCooldown(MNK.RiddleOfFire) || HasBlitz();
        if (isOpenerStart) isOpenerState = true;
        if (isOpenerEnd) isOpenerState = false;

        bool hadBlitz = hasBlitzState;
        hasBlitzState = HasBlitz();
        if (hadBlitz && !hasBlitzState && IsRiddleOfFireActive())
            blitzUseCountState += 1;
        if (!IsRiddleOfFireActive())
            blitzUseCountState = 0;

    }

    private static uint GetWeaveAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<MNKGauge>();
        var beastGauge = GetBeastGauge();
        var hasBothNadi = beastGauge.LunarNadi && beastGauge.SolarNadi;
        var isUsingInitialGcds = level >= MNK.Levels.RiddleOfFire && isOpenerState;

        if (level >= MNK.Levels.PerfectBalance
                && CanWeave(MNK.PerfectBalance, noBlock: true, clipThreshold: 0.2f)
                && !options.HasFlag(GetActionOptions.DelayBurst)
                && !HasEffect(MNK.Buffs.PerfectBalance) && !HasBlitz())
        {
            if (isUsingInitialGcds && GetCooldown(MNK.PerfectBalance).RemainingCharges == 2 && !HasEffect(MNK.Buffs.FormlessFist))
                return MNK.PerfectBalance;

            var mustUseKeyGcds = IsRiddleOfFireActive() && !CanPadGcdInWindow() && !HasEffect(MNK.Buffs.FiresRumination) && !HasEffect(MNK.Buffs.WindsRumination);

            if (options.HasFlag(GetActionOptions.MultiTarget))
            {
                var usedStrongestGcd = level >= MNK.Levels.ShadowOfTheDestroyer
                    ? HasEffect(MNK.Buffs.RaptorForm)
                    : HasEffect(MNK.Buffs.OpoOpoForm);

                if (usedStrongestGcd)
                {
                    if (level < MNK.Levels.RiddleOfFire)
                        return MNK.PerfectBalance;

                    if (IsEvenWindow())
                    {
                        if (IsRiddleOfFireActive() || CanWeave(MNK.RiddleOfFire, gcdCount: 3))
                            return MNK.PerfectBalance;
                    }

                    if (IsOddWindow() && blitzUseCountState == 0)
                    {
                        if (IsRiddleOfFireActive() || CanWeave(MNK.RiddleOfFire))
                            return MNK.PerfectBalance;
                    }
                }
            }
            else if (HasEffect(MNK.Buffs.RaptorForm) || mustUseKeyGcds)
            {
                if (level < MNK.Levels.RiddleOfFire)
                    return MNK.PerfectBalance;

                if (IsEvenWindow())
                {
                    if (IsRiddleOfFireActive() || CanWeave(MNK.RiddleOfFire, gcdCount: 3))
                        return MNK.PerfectBalance;
                }

                if (IsOddWindow() && blitzUseCountState == 0)
                {
                    if (IsRiddleOfFireActive() || CanWeave(MNK.RiddleOfFire))
                        return MNK.PerfectBalance;
                }

                if (TimeToCap(MNK.PerfectBalance, 40) < GetCooldown(MNK.RiddleOfFire).CooldownRemaining - (GcdRecast * 4))
                    return MNK.PerfectBalance;
            }
        }

        if (!isUsingInitialGcds && !options.HasFlag(GetActionOptions.DelayBurst))
        {
            if (level >= MNK.Levels.RiddleOfFire && CanWeave(MNK.RiddleOfFire, noBlock: true, clipThreshold: 0.2f))
            {
                if (!IsEarlyWeave())
                    return MNK.RiddleOfFire;
            }

            if (level >= MNK.Levels.Brotherhood && CanWeave(MNK.Brotherhood, noBlock: true))
            {
                if (IsEvenWindow() && (IsRiddleOfFireActive() || CanWeave(MNK.RiddleOfFire)))
                    return MNK.Brotherhood;
            }

            if (level >= MNK.Levels.RiddleOfWind && CanWeave(MNK.RiddleOfWind, noBlock: true))
                return MNK.RiddleOfWind;
        }

        if (level >= MNK.Levels.Meditation && gauge.Chakra >= 5 && CanWeave(MNK.ForbiddenChakra, noBlock: true, clipThreshold: 0.8f))
        {
            if (level >= MNK.Levels.HowlingFist && options.HasFlag(GetActionOptions.MultiTarget))
                return OriginalHook(MNK.HowlingFist);

            return OriginalHook(MNK.ForbiddenChakra);
        }

        if (options.HasFlag(GetActionOptions.UseTrueNorth) && CanWeave(MNK.TrueNorth, noBlock: true))
        {
            if (!HasEffect(MNK.Buffs.TrueNorth) || GetEffectRemainingTime(MNK.Buffs.TrueNorth) - Gcd() < GcdRecast * 2)
                return MNK.TrueNorth;
        }

        if (options.HasFlag(GetActionOptions.UseFeint) && CanWeave(MNK.Feint, noBlock: true))
            return MNK.Feint;

        if (options.HasFlag(GetActionOptions.UseRiddleOfEarth))
        {
            if (HasEffect(MNK.Buffs.EarthsRumination) && CanWeave(MNK.EarthsReply, noBlock: true))
                return MNK.EarthsReply;

            if (CanWeave(MNK.RiddleOfEarth, noBlock: true))
                return MNK.RiddleOfEarth;
        }

        if (options.HasFlag(GetActionOptions.UseMantra) && CanWeave(MNK.Mantra, noBlock: true))
            return MNK.Mantra;

        if (options.HasFlag(GetActionOptions.UseSecondWind) && CanWeave(MNK.SecondWind, noBlock: true))
            return MNK.SecondWind;

        if (options.HasFlag(GetActionOptions.UseBloodBath) && CanWeave(MNK.BloodBath, noBlock: true))
            return MNK.BloodBath;

        if (options.HasFlag(GetActionOptions.UseArmsLength) && CanWeave(MNK.ArmsLength, noBlock: true))
            return MNK.ArmsLength;

        return 0;
    }

    protected static uint GetGcdAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<MNKGauge>();
        var beastGauge = GetBeastGauge();

        var isOpoOpoForm = HasEffect(MNK.Buffs.OpoOpoForm) || HasEffect(MNK.Buffs.FormlessFist);
        var blitzTimeRemaining = (float)gauge.BlitzTimeRemaining / 1000;
        var isBlitzExpiring = HasBlitz() && blitzTimeRemaining - Gcd() < GcdRecast;
        var canCarryBlitz = HasBlitz() && blitzTimeRemaining > GetCooldown(MNK.RiddleOfFire).CooldownRemaining + GcdRecast;

        if (isBlitzExpiring)
            return OriginalHook(MNK.MasterfulBlitz);

        if (HasBlitz() && (level < MNK.Levels.RiddleOfFire || HasEffect(MNK.Buffs.RiddleOfFire) || isBlitzExpiring || !canCarryBlitz))
            return OriginalHook(MNK.MasterfulBlitz);

        if (HasEffect(MNK.Buffs.PerfectBalance))
        {
            if (level >= MNK.Levels.MasterfulBlitz)
            {
                var hasDifferentBeasts = gauge.BeastChakra[0] != gauge.BeastChakra[1] && gauge.BeastChakra[1] != (BeastChakra)BeastChakraFixed.NONE;
                if (hasDifferentBeasts)
                    return GetSolarBlitzAction(level, options);

                if (gauge.BeastChakra[0] == (BeastChakra)BeastChakraFixed.OPOOPO)
                    return GetOpoOpoGcdAction(level, options);

                // solar = raptor -> coeurl -> opoopo
                if (gauge.BeastChakra[0] == (BeastChakra)BeastChakraFixed.RAPTOR)
                    return GetSolarBlitzAction(level, options);

                // aoe lunar = rockbreaker x 3
                if (gauge.BeastChakra[0] == (BeastChakra)BeastChakraFixed.COEURL)
                    return GetCoeurlGcdAction(level, options);

                var isMissingSolar = beastGauge.LunarNadi && !beastGauge.SolarNadi;
                var useBlitzInWindow = IsRiddleOfFireActive() || GetCooldown(MNK.RiddleOfFire).CooldownRemaining < 20;
                if (isMissingSolar && useBlitzInWindow)
                {
                    if (IsEvenWindow())
                    {
                        // lunar only : ... -> lunar -> [solar]/lunar
                        // exception : lunar/[lunar] -> ...
                        if (blitzUseCountState == 0)
                            return GetSolarBlitzAction(level, options);
                    }

                    if (IsOddWindow())
                    {
                        // lunar/lunar -> [solar] -> lunar/lunar
                        return GetSolarBlitzAction(level, options);
                    }
                }
            }

            if (options.HasFlag(GetActionOptions.MultiTarget))
                return GetStrongestMultiTargetAction(level);
            else
                return GetOpoOpoGcdAction(level, options);
        }

        if (HasEffect(MNK.Buffs.WindsRumination))
            return MNK.WindsReply;

        if (HasEffect(MNK.Buffs.FiresRumination))
        {
            if (isOpoOpoForm && GetRemainingExtraGcdInWindow() > 1)
                return GetOpoOpoGcdAction(level, options);

            return MNK.FiresReply;
        }

        if (level >= MNK.Levels.SixSidedStar && !options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (IsRiddleOfFireActive() && GetRemainingGcdsInWindow() == 1 && CanPadGcdInWindow())
                return MNK.SixSidedStar;
        }

        return GetComboAction(level, options);
    }

    private static uint GetSolarBlitzAction(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<MNKGauge>();

        if (!gauge.BeastChakra.Contains((BeastChakra)BeastChakraFixed.RAPTOR))
            return GetRaptorGcdAction(level, options);

        if (!gauge.BeastChakra.Contains((BeastChakra)BeastChakraFixed.COEURL))
            return GetCoeurlGcdAction(level, options);

        return GetOpoOpoGcdAction(level, options);
    }

    private static uint GetComboAction(byte level, GetActionOptions options)
    {
        if (HasEffect(MNK.Buffs.RaptorForm))
            return GetRaptorGcdAction(level, options);
        else if (HasEffect(MNK.Buffs.CoerlForm))
            return GetCoeurlGcdAction(level, options);
        else
            return GetOpoOpoGcdAction(level, options);
    }

    private static uint GetOpoOpoGcdAction(byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (level >= MNK.Levels.ArmOfTheDestroyer)
                return OriginalHook(MNK.ArmOfTheDestroyer);
        }

        if (level >= MNK.Levels.DragonKick && GetBeastGauge().OpoOpoFury == 0)
            return MNK.DragonKick;

        return OriginalHook(MNK.Bootshine);
    }

    private static uint GetRaptorGcdAction(byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (level >= MNK.Levels.FourPointFury)
                return OriginalHook(MNK.FourPointFury);
        }

        if (level >= MNK.Levels.TwinSnakes && GetBeastGauge().RaptorFury == 0)
            return MNK.TwinSnakes;

        return OriginalHook(MNK.TrueStrike);
    }

    private static uint GetCoeurlGcdAction(byte level, GetActionOptions options)
    {
        if (options.HasFlag(GetActionOptions.MultiTarget))
        {
            if (level >= MNK.Levels.Rockbreaker)
                return OriginalHook(MNK.Rockbreaker);
        }

        if (level >= MNK.Levels.Demolish && GetBeastGauge().CoeurlFury == 0)
            return MNK.Demolish;

        return OriginalHook(MNK.SnapPunch);
    }

    private static uint GetStrongestMultiTargetAction(byte level)
    {
        if (level >= MNK.Levels.ShadowOfTheDestroyer)
            return MNK.ShadowOfTheDestroyer;
        else if (level >= MNK.Levels.Rockbreaker)
            return MNK.Rockbreaker;
        else if (level >= MNK.Levels.ArmOfTheDestroyer)
            return MNK.ArmOfTheDestroyer;
        else
            return MNK.Bootshine;
    }

    private static bool IsRiddleOfFireActive()
    {
        if (HasEffect(MNK.Buffs.RiddleOfFire))
            return true;

        return IsOnCooldown(MNK.RiddleOfFire) && GetCooldown(MNK.RiddleOfFire).CooldownElapsed < 20;
    }

    private static bool HasBlitz()
    {
        return OriginalHook(MNK.MasterfulBlitz) != MNK.MasterfulBlitz;
    }

    private static bool IsEvenWindow()
    {
        var rof = GetCooldown(MNK.RiddleOfFire);
        var bh = GetCooldown(MNK.Brotherhood);

        var isBrotherhoodWindow = IsRiddleOfFireActive()
            ? bh.CooldownElapsed < 25 || bh.CooldownRemaining < GetRemainingRofTime()
            : (20 - bh.CooldownElapsed) > rof.CooldownRemaining || bh.CooldownRemaining < rof.CooldownRemaining + 20;

        var blitzCount = IsRiddleOfFireActive()
            ? GetRemainingBlitzInWindow(rofInitState)
            : GetRemainingBlitzInWindow();

        return isBrotherhoodWindow && blitzCount >= 2;
    }

    private static bool IsOddWindow()
    {
        return !IsEvenWindow();
    }

    private static ushort GetRemainingBlitzInWindow()
    {
        return GetRemainingBlitzInWindow(GetCurrentRofState());
    }

    private static RofState GetCurrentRofState()
    {
        var gauge = GetJobGauge<MNKGauge>();
        return new()
        {
            RofCooldownRemaining = GetCooldown(MNK.RiddleOfFire).CooldownRemaining,
            RofCooldownElapsed = GetCooldown(MNK.RiddleOfFire).CooldownElapsed,
            RofEffectRemainingTime = GetEffectRemainingTime(MNK.Buffs.RiddleOfFire),
            PbCharges = GetCooldown(MNK.PerfectBalance).RemainingCharges,
            PbChargeCd = GetCooldown(MNK.PerfectBalance).ChargeCooldownRemaining,
            PbEffectStackCount = FindEffect(MNK.Buffs.PerfectBalance)?.StackCount ?? 0,
            HasBlitz = HasBlitz(),
            BlitzTimeRemaining = gauge.BlitzTimeRemaining,
        };
    }

    private static ushort GetRemainingBlitzInWindow(RofState state)
    {
        ushort blitzCount = 0;

        var isRofActive = state.RofEffectRemainingTime > 0 || state.RofCooldownElapsed < 20;

        var rofEndTime = isRofActive ?
            Math.Max(state.RofEffectRemainingTime, 20 - state.RofCooldownElapsed)
            : state.RofCooldownRemaining + 20;

        var timeBeforeLastPbUse = rofEndTime - (GcdRecast * 3);

        if (timeBeforeLastPbUse > 0)
            blitzCount += state.PbCharges;

        if (state.PbChargeCd > 0)
        {
            if (timeBeforeLastPbUse > state.PbChargeCd)
                blitzCount += 1;

            if (timeBeforeLastPbUse > state.PbChargeCd + 40)
                blitzCount += 1;
        }

        if (state.PbEffectStackCount > 0)
        {
            var blitzCarryTime = (GcdRecast * state.PbEffectStackCount) + 20;
            if (isRofActive || blitzCarryTime > state.RofCooldownRemaining)
                blitzCount += 1;
        }

        if (state.HasBlitz)
        {
            if (isRofActive || state.BlitzTimeRemaining > state.RofCooldownRemaining)
                blitzCount += 1;
        }

        return blitzCount;
    }

    private static int GetRemainingKeyGcdsInWindow()
    {
        if (!IsRiddleOfFireActive())
            return 0;

        int gcdCount = 0;

        if (HasEffect(MNK.Buffs.FiresRumination)) gcdCount += 1;
        if (HasEffect(MNK.Buffs.WindsRumination)) gcdCount += 1;
        if (HasEffect(MNK.Buffs.PerfectBalance)) gcdCount += FindEffect(MNK.Buffs.PerfectBalance)!.StackCount + 1;
        if (HasBlitz()) gcdCount += 1;

        var blitzCount = IsEvenWindow() ? 2 : 1;
        blitzCount -= blitzUseCountState;
        if (HasEffect(MNK.Buffs.PerfectBalance) || HasBlitz()) blitzCount -= 1;
        gcdCount += blitzCount * 4;

        return gcdCount;
    }

    private static float GetRemainingRofTime()
    {
        if (HasEffect(MNK.Buffs.RiddleOfFire))
            return GetEffectRemainingTime(MNK.Buffs.RiddleOfFire);

        if (IsOffCooldown(MNK.RiddleOfFire))
            return 20;

        return Math.Max(0, 20 - GetCooldown(MNK.RiddleOfFire).CooldownElapsed);
    }

    private static byte GetRemainingGcdsInWindow()
    {
        var remainingTime = GetRemainingRofTime() - Gcd();
        var gcds = Math.Floor(remainingTime / GcdRecast) + 1;
        return (byte)gcds;
    }

    private static byte GetRemainingExtraGcdInWindow()
    {
        return (byte)Math.Max(0, GetRemainingGcdsInWindow() - GetRemainingKeyGcdsInWindow());
    }

    private static bool CanPadGcdInWindow()
    {
        return GetRemainingExtraGcdInWindow() > 0;
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
        return GetCooldown(MNK.Bootshine).CooldownRemaining;
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

    private static unsafe BeastChakraGaugeData GetBeastGauge()
    {
        AddonJobHudMNK0* addon = (AddonJobHudMNK0*)AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("JobHudMNK0");
        BeastChakraGaugeData* ptr = (BeastChakraGaugeData*)addon->DataCurrentPointer;

        return new()
        {
            OpoOpoFury = ptr->OpoOpoFury,
            RaptorFury = ptr->RaptorFury,
            CoeurlFury = ptr->CoeurlFury,
            LunarNadi = ptr->LunarNadi,
            SolarNadi = ptr->SolarNadi,
        };
    }
}

internal class MonkRotation : MonkCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MonkOneButton;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        // Single target
        if (actionID == PLD.FastBlade)
            return GetRotationAction(level, GetActionOptions.None);

        if (actionID == PLD.RiotBlade)
            return GetRotationAction(level, GetActionOptions.DelayBurst);

        // Display
        if (actionID == PLD.GoringBlade)
            return GetGcdAction(level, GetActionOptions.None);

        // AoE
        if (actionID == PLD.TotalEclipse)
            return GetRotationAction(level, GetActionOptions.MultiTarget);

        if (actionID == PLD.Prominence)
            return GetRotationAction(level, GetActionOptions.MultiTarget | GetActionOptions.DelayBurst);

        // Utility
        if (actionID == PLD.FightOrFlight)
            return GetRotationAction(level, GetActionOptions.UseTrueNorth);

        if (actionID == PLD.Requiescat)
            return GetRotationAction(level, GetActionOptions.UseFeint);

        if (actionID == PLD.HolySpirit)
            return GetRotationAction(level, GetActionOptions.UseRiddleOfEarth);

        if (actionID == PLD.Confiteor)
            return GetRotationAction(level, GetActionOptions.UseMantra);

        if (actionID == PLD.Clemency)
            return GetRotationAction(level, GetActionOptions.UseSecondWind);

        if (actionID == PLD.SpiritsWithin)
            return GetRotationAction(level, GetActionOptions.UseBloodBath);

        if (actionID == PLD.CircleOfScorn)
            return GetRotationAction(level, GetActionOptions.UseArmsLength);

        return actionID;
    }
}

internal class MonkFormShiftMeditation : MonkCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.MnkAny;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if (actionID == MNK.FormShift)
        {
            var gauge = GetJobGauge<MNKGauge>();
            var formless = GetEffectRemainingTime(MNK.Buffs.FormlessFist);
            if (formless > 10 && gauge.Chakra < 5)
                return MNK.ForbiddenMeditation;
        }

        return actionID;
    }
}