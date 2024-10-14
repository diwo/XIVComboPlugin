using System;
using Dalamud.Game.ClientState.JobGauge.Types;

namespace XIVComboExpandedPlugin.Combos;

internal static class VPR
{
    public const byte JobID = 41;

    public const uint
            SteelFangs = 34606,
            ReavingFangs = 34607,
            HuntersSting = 34608,
            SwiftskinsSting = 34609,
            FlankstingStrike = 34610,
            FlanksbaneFang = 34611,
            HindstingStrike = 34612,
            HindsbaneFang = 34613,

            SteelMaw = 34614,
            ReavingMaw = 34615,
            HuntersBite = 34616,
            SwiftskinsBite = 34617,
            JaggedMaw = 34618,
            BloodiedMaw = 34619,

            Vicewinder = 34620,
            HuntersCoil = 34621,
            SwiftskinsCoil = 34622,
            VicePit = 34623,
            HuntersDen = 34624,
            SwiftskinsDen = 34625,

            SerpentsTail = 35920,
            DeathRattle = 34634,
            LastLash = 34635,
            Twinfang = 35921,
            Twinblood = 35922,
            TwinfangBite = 34636,
            TwinfangThresh = 34638,
            TwinbloodBite = 34637,
            TwinbloodThresh = 34639,

            UncoiledFury = 34633,
            UncoiledTwinfang = 34644,
            UncoiledTwinblood = 34645,

            SerpentsIre = 34647,
            Reawaken = 34626,
            FirstGeneration = 34627,
            SecondGeneration = 34628,
            ThirdGeneration = 34629,
            FourthGeneration = 34630,
            Ouroboros = 34631,
            FirstLegacy = 34640,
            SecondLegacy = 34641,
            ThirdLegacy = 34642,
            FourthLegacy = 34643,

            WrithingSnap = 34632,
            Slither = 34646;

    public static class Buffs
    {
        public const ushort
            FlankstungVenom = 3645,
            FlanksbaneVenom = 3646,
            HindstungVenom = 3647,
            HindsbaneVenom = 3648,
            GrimhuntersVenom = 3649,
            GrimskinsVenom = 3650,
            HuntersVenom = 3657,
            SwiftskinsVenom = 3658,
            FellhuntersVenom = 3659,
            FellskinsVenom = 3660,
            PoisedForTwinfang = 3665,
            PoisedForTwinblood = 3666,
            HuntersInstinct = 3668, // Double check, might also be 4120
            Swiftscaled = 3669,     // Might also be 4121
            Reawakened = 3670,
            ReadyToReawaken = 3671,
            HonedSteel = 3672,
            HonedReavers = 3772;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            SteelFangs = 1,
            HuntersSting = 5,
            ReavingFangs = 10,
            WrithingSnap = 15,
            SwiftskinsSting = 20,
            SteelMaw = 25,
            Single3rdCombo = 30, // Includes Flanksting, Flanksbane, Hindsting, and Hindsbane
            ReavingMaw = 35,
            Slither = 40,
            HuntersBite = 40,
            SwiftskinsBite = 45,
            AoE3rdCombo = 50,    // Jagged Maw and Bloodied Maw
            DeathRattle = 55,
            LastLash = 60,
            Vicewinder = 65,     // Also includes Hunter's Coil and Swiftskin's Coil
            VicePit = 70,        // Also includes Hunter's Den and Swiftskin's Den
            TwinsSingle = 75,    // Twinfang Bite and Twinblood Bite
            TwinsAoE = 80,       // Twinfang Thresh and Twinblood Thresh
            UncoiledFury = 82,
            SerpentsIre = 86,
            EnhancedRattle = 88, // Third stack of Rattling Coil can be accumulated
            Reawaken = 90,       // Also includes First Generation through Fourth Generation
            UncoiledTwins = 92,  // Uncoiled Twinfang and Uncoiled Twinblood
            Ouroboros = 96,      // Also includes a 5th Anguine Tribute stack from Reawaken
            Legacies = 100;      // First through Fourth Legacy
    }
}

internal abstract class ViperCombo : CustomCombo
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

        return GetGcdAction(lastComboMove, comboTime, level, options);
    }

    private static uint GetWeaveAction(byte level, GetActionOptions options)
    {
        if (level >= VPR.Levels.SerpentsIre && CanWeaveWithoutClip(VPR.SerpentsIre))
        {
            if (!options.HasFlag(GetActionOptions.DelayBurst))
                return VPR.SerpentsIre;
        }

        var serpentsTail = GetMorphedAction(VPR.SerpentsTail);
        var twinfang = GetMorphedAction(VPR.Twinfang);
        var twinblood = GetMorphedAction(VPR.Twinblood);

        if (IsActionHighlighted(twinfang)) return twinfang;
        if (IsActionHighlighted(twinblood)) return twinblood;
        if (IsActionHighlighted(serpentsTail)) return serpentsTail;

        if (twinfang > 0) return twinfang;
        if (twinblood > 0) return twinblood;
        if (serpentsTail > 0) return serpentsTail;

        return 0;
    }

    protected static uint GetGcdAction(uint lastComboMove, float comboTime, byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<VPRGauge>();
        var rattlingCoilStacksCap = level >= VPR.Levels.EnhancedRattle ? 3 : 2;
        var viceAction =
            gauge.RattlingCoilStacks == rattlingCoilStacksCap ?
                VPR.UncoiledFury :
                level >= VPR.Levels.VicePit && options.HasFlag(GetActionOptions.MultiTarget) ?
                    OriginalHook(VPR.VicePit) :
                    OriginalHook(VPR.Vicewinder);

        var reawakenCombo = GetReawakenCombo(level, options);
        if (reawakenCombo > 0) return reawakenCombo;

        var viceFollowup = GetViceFollowup();
        if (viceFollowup > 0) return viceFollowup;

        if (!HasEffect(VPR.Buffs.Swiftscaled))
            return GetSteelOrReaving(options);

        if ((IsBurstWindow(level) && !options.HasFlag(GetActionOptions.DelayBurst)) || options.HasFlag(GetActionOptions.UseResources))
        {
            if (level >= VPR.Levels.Vicewinder && TimeToCap(VPR.Vicewinder, 40) - Gcd() < 20)
                return viceAction;

            if (level >= VPR.Levels.Reawaken && (gauge.SerpentOffering >= 50 || HasEffect(VPR.Buffs.ReadyToReawaken)))
                return VPR.Reawaken;

            if (level >= VPR.Levels.UncoiledFury && gauge.RattlingCoilStacks > 0)
                return VPR.UncoiledFury;

            if (level >= VPR.Levels.Vicewinder && HasCharges(VPR.Vicewinder))
                return viceAction;
        }

        if (level >= VPR.Levels.UncoiledFury)
        {
            var futureStacks = gauge.RattlingCoilStacks;
            if (level >= VPR.Levels.SerpentsIre && GetCooldown(VPR.SerpentsIre).CooldownRemaining - Gcd() < 5)
                futureStacks += 1;
            if (TimeToCap(VPR.Vicewinder, 40) - Gcd() < 8)
                futureStacks += 1;

            if (futureStacks > rattlingCoilStacksCap)
                return VPR.UncoiledFury;
        }

        if (level >= VPR.Levels.SerpentsIre && IsOnCooldown(VPR.SerpentsIre) && HasCharges(VPR.Vicewinder))
        {
            if (TimeToCap(VPR.Vicewinder, 40) < GetCooldown(VPR.SerpentsIre).CooldownRemaining + 20)
                return viceAction;
        }

        if (level >= VPR.Levels.Vicewinder && TimeToCap(VPR.Vicewinder, 40) - Gcd() < 5)
            return viceAction;

        if (level >= VPR.Levels.Reawaken && gauge.SerpentOffering >= 100)
            return VPR.Reawaken;

        return GetSteelOrReaving(options);
    }

    private static uint GetViceFollowup()
    {
        var huntersCoil = IsActionHighlighted(VPR.HuntersCoil) ? VPR.HuntersCoil : 0;
        var swiftskinsCoil = IsActionHighlighted(VPR.SwiftskinsCoil) ? VPR.SwiftskinsCoil : 0;
        var huntersDen = IsActionHighlighted(VPR.HuntersDen) ? VPR.HuntersDen : 0;
        var swiftskinsDen = IsActionHighlighted(VPR.SwiftskinsDen) ? VPR.SwiftskinsDen : 0;

        if (huntersCoil > 0 && swiftskinsCoil > 0)
            return IsFlank() ? huntersCoil : swiftskinsCoil;

        if (huntersCoil > 0) return huntersCoil;
        if (swiftskinsCoil > 0) return swiftskinsCoil;
        if (huntersDen > 0) return huntersDen;
        if (swiftskinsDen > 0) return swiftskinsDen;

        return 0;
    }

    private static uint GetReawakenCombo(byte level, GetActionOptions options)
    {
        var gauge = GetJobGauge<VPRGauge>();

        if (gauge.AnguineTribute == 0)
            return 0;

        if (level >= VPR.Levels.Ouroboros && gauge.AnguineTribute == 1)
            return VPR.Ouroboros;

        uint[] actions = [VPR.FirstGeneration, VPR.SecondGeneration, VPR.ThirdGeneration, VPR.FourthGeneration];
        foreach (var action in actions)
        {
            if (IsActionHighlighted(action))
                return action;
        }

        return 0;
    }

    private static uint GetSteelOrReaving(GetActionOptions options)
    {
        var steel = options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(VPR.SteelMaw) : OriginalHook(VPR.SteelFangs);
        var reaving = options.HasFlag(GetActionOptions.MultiTarget) ? OriginalHook(VPR.ReavingMaw) : OriginalHook(VPR.ReavingFangs);

        if (!HasEffect(VPR.Buffs.Swiftscaled))
        {
            if (reaving == VPR.SwiftskinsSting || reaving == VPR.SwiftskinsBite)
                return reaving;
        }

        if (IsActionHighlighted(steel) && !IsActionHighlighted(reaving))
            return steel;

        if (IsActionHighlighted(reaving) && !IsActionHighlighted(steel))
            return reaving;

        if (reaving == VPR.SwiftskinsSting || reaving == VPR.SwiftskinsBite)
        {
            if (GetEffectRemainingTime(VPR.Buffs.Swiftscaled) <= GetEffectRemainingTime(VPR.Buffs.HuntersInstinct))
                return reaving;
        }

        return steel;
    }

    protected static uint GetRangedAction(byte level)
    {
        var gauge = GetJobGauge<VPRGauge>();

        if (level >= VPR.Levels.UncoiledFury && gauge.RattlingCoilStacks > 0)
            return VPR.UncoiledFury;

        return VPR.WrithingSnap;
    }

    private static bool IsBurstWindow(byte level)
    {
        return level >= VPR.Levels.SerpentsIre && IsOnCooldown(VPR.SerpentsIre) && GetCooldown(VPR.SerpentsIre).CooldownElapsed < 30;
    }

    private static uint GetMorphedAction(uint actionId)
    {
        var morphed = OriginalHook(actionId);
        return morphed != actionId ? morphed : 0;
    }

    private static unsafe bool IsActionHighlighted(uint actionId)
    {
        var actionManager = FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance();
        return actionManager->IsActionHighlighted(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, actionId);
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
        return GetCooldown(VPR.SteelFangs).CooldownRemaining;
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

internal class ViperRotation : ViperCombo
{
    protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.ViperOneButton;

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
            return GetRangedAction(level);

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

        return actionID;
    }
}