using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace XIVComboExpandedPlugin.Combos;

internal static class ADV
{
    public const byte ClassID = 0;
    public const byte JobID = 0;

    public const uint
        LucidDreaming = 1204,
        Swiftcast = 7561,
        AngelWhisper = 18317,
        VariantRaise2 = 29734;

    public static class Buffs
    {
        public const ushort
            Medicated = 49,
            EurekaMoment = 2765;
    }

    public static class Debuffs
    {
        public const ushort
            Placeholder = 0;
    }

    public static class Levels
    {
        public const byte
            Swiftcast = 18,
            VariantRaise2 = 90;
    }
}

// Miner
internal static class MIN
{
    public const uint
        Collect = 240,
        Scour = 22182,
        BrazenProspector = 22183,
        MeticulousProspector = 22184,
        Scrutiny = 22185,
        SolidReason = 232,
        WiseToTheWorld = 26521;
}

// Botanist
internal static class BTN
{
    public const uint
        // Botanist
        Collect = 815,
        Scour = 22186,
        BrazenProspector = 22187,
        MeticulousProspector = 22188,
        Scrutiny = 22189,
        AgelessWords = 215,
        WiseToTheWorld = 26522;
}

internal class SwiftRaiseFeature : CustomCombo
{
    protected internal override CustomComboPreset Preset => CustomComboPreset.AdvSwiftcastFeature;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if ((actionID == AST.Ascend && level >= AST.Levels.Ascend) ||
            (actionID == SCH.Resurrection && level >= SCH.Levels.Resurrection) ||
            (actionID == SGE.Egeiro && level >= SGE.Levels.Egeiro) ||
            (actionID == WHM.Raise && level >= WHM.Levels.Raise) ||
            (actionID == BLU.AngelWhisper && level >= BLU.Levels.AngelWhisper))
        {
            if (level >= ADV.Levels.Swiftcast && IsOffCooldown(ADV.Swiftcast))
                return ADV.Swiftcast;
        }

        if (actionID == RDM.Verraise && level >= RDM.Levels.Verraise && !HasEffect(RDM.Buffs.Dualcast))
        {
            if (IsEnabled(CustomComboPreset.AdvVerRaiseToVerCureFeature))
            {
                if (level >= RDM.Levels.Vercure)
                    return RDM.Vercure;
            }
            else if (!IsEnabled(CustomComboPreset.AdvDisableVerRaiseFeature))
            {
                if (level >= ADV.Levels.Swiftcast && IsOffCooldown(ADV.Swiftcast))
                    return ADV.Swiftcast;
            }
        }

        return actionID;
    }
}

internal class VariantRaiseFeature : CustomCombo
{
    protected internal override CustomComboPreset Preset => CustomComboPreset.AdvAny;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if ((actionID == AST.Ascend && level >= AST.Levels.Ascend) ||
            (actionID == SCH.Resurrection && level >= SCH.Levels.Resurrection) ||
            (actionID == SGE.Egeiro && level >= SGE.Levels.Egeiro) ||
            (actionID == WHM.Raise && level >= WHM.Levels.Raise) ||
            (actionID == RDM.Verraise && level >= RDM.Levels.Verraise && !HasEffect(RDM.Buffs.Dualcast)) ||
            (actionID == BLU.AngelWhisper && level >= BLU.Levels.AngelWhisper))
        {
            // Per Splatoon:
            // 1069: solo
            // 1075: group
            // 1076: savage
            if (level >= ADV.Levels.VariantRaise2 && CurrentTerritory == 1075u)
                return ADV.VariantRaise2;
        }

        return actionID;
    }
}

internal class GathererCombo : CustomCombo
{
    protected internal override CustomComboPreset Preset => CustomComboPreset.AdvAny;

    protected override uint Invoke(uint actionID, uint lastComboMove, float comboTime, byte level)
    {
        if (actionID == MIN.SolidReason || actionID == BTN.AgelessWords)
        {
            if (IsGatheringCollectables())
            {
                var gp = LocalPlayer?.CurrentGp;
                var integrity = GetCollectableIntegrity();
                var missingIntegrity = GetCollectableIntegrityTotal() - integrity;
                var agelessRemaining = gp / 300;
                var collect = IsMiner() ? MIN.Collect : BTN.Collect;
                var scour = IsMiner() ? MIN.Scour : BTN.Scour;
                var isMaxedCollectability = CanUse(collect) && !CanUse(scour);

                if (HasEffect(ADV.Buffs.EurekaMoment) && missingIntegrity > 0)
                    return IsMiner() ? MIN.WiseToTheWorld : BTN.WiseToTheWorld;

                if (isMaxedCollectability)
                {
                    if (missingIntegrity == 0 || agelessRemaining == 0)
                        return collect;

                    if (integrity >= 2 && missingIntegrity < agelessRemaining * 2)
                        return collect;
                }
            }
            else
            {
                if (HasEffect(ADV.Buffs.EurekaMoment))
                    return IsMiner() ? MIN.WiseToTheWorld : BTN.WiseToTheWorld;
            }
        }

        return actionID;
    }

    private static unsafe bool CanUse(uint actionId)
    {
        return FFXIVClientStructs.FFXIV.Client.Game.ActionManager.Instance()->GetActionStatus(FFXIVClientStructs.FFXIV.Client.Game.ActionType.Action, actionId) == 0;
    }

    private static bool IsMiner()
    {
        return LocalPlayer?.ClassJob.GameData?.DohDolJobIndex == 0;
    }

    private static unsafe bool IsGatheringCollectables()
    {
        AddonGatheringMasterpiece* addon = GetAddonGatheringMasterpiece();
        return addon != null && addon->IsVisible && GetCollectableIntegrityTotal() > 0;
    }

    private static unsafe int GetCollectableIntegrity()
    {
        AddonGatheringMasterpiece* addon = GetAddonGatheringMasterpiece();
        if (addon != null && addon->IntegrityLeftover != null)
        {
            if (int.TryParse(addon->IntegrityLeftover->NodeText, out int integrity))
                return integrity;
        }

        return 0;
    }

    private static unsafe int GetCollectableIntegrityTotal()
    {
        AddonGatheringMasterpiece* addon = GetAddonGatheringMasterpiece();
        if (addon != null && addon->IntegrityTotal != null)
        {
            if (int.TryParse(addon->IntegrityTotal->NodeText, out int integrityTotal))
                return integrityTotal;
        }

        return 0;
    }

    private static unsafe AddonGatheringMasterpiece* GetAddonGatheringMasterpiece()
    {
        return (AddonGatheringMasterpiece*)AtkStage.Instance()->RaptureAtkUnitManager->GetAddonByName("GatheringMasterpiece");
    }
}