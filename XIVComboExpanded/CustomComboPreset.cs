using XIVComboExpandedPlugin.Attributes;
using XIVComboExpandedPlugin.Combos;

namespace XIVComboExpandedPlugin;

/// <summary>
/// Combo presets.
/// </summary>
public enum CustomComboPreset
{
    // ====================================================================================
    #region Misc

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", ADV.JobID)]
    AdvAny = 0,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", AST.JobID)]
    AstAny = AdvAny + AST.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", BLM.JobID)]
    BlmAny = AdvAny + BLM.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", BRD.JobID)]
    BrdAny = AdvAny + BRD.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", DNC.JobID)]
    DncAny = AdvAny + DNC.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", DOH.JobID)]
    DohAny = AdvAny + DOH.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", DOL.JobID)]
    DolAny = AdvAny + DOL.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", DRG.JobID)]
    DrgAny = AdvAny + DRG.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", DRK.JobID)]
    DrkAny = AdvAny + DRK.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", GNB.JobID)]
    GnbAny = AdvAny + GNB.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", MCH.JobID)]
    MchAny = AdvAny + MCH.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", MNK.JobID)]
    MnkAny = AdvAny + MNK.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", NIN.JobID)]
    NinAny = AdvAny + NIN.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", PLD.JobID)]
    PldAny = AdvAny + PLD.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", RDM.JobID)]
    RdmAny = AdvAny + RDM.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", RPR.JobID)]
    RprAny = AdvAny + RPR.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", SAM.JobID)]
    SamAny = AdvAny + SAM.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", SCH.JobID)]
    SchAny = AdvAny + SCH.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", SGE.JobID)]
    SgeAny = AdvAny + SGE.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", SMN.JobID)]
    SmnAny = AdvAny + SMN.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", WAR.JobID)]
    WarAny = AdvAny + WAR.JobID,

    [CustomComboInfo("Any", "This should not be displayed. This always returns true when used with IsEnabled.", WHM.JobID)]
    WhmAny = AdvAny + WHM.JobID,

    [CustomComboInfo("Disabled", "This should not be used.", ADV.JobID)]
    Disabled = 99999,

    #endregion
    // ====================================================================================
    #region ADV

    [CustomComboInfo("Swift Raise Feature", "Replace Ascend, Ressurection, Egeiro, Raise, Verraise, and Angel Whisper with Swiftcast when it is off cooldown (and Dualcast isn't up).", ADV.JobID)]
    AdvSwiftcastFeature = 1000,

    [ParentCombo(AdvSwiftcastFeature)]
    [ConflictingCombos(AdvVerRaiseToVerCureFeature)]
    [CustomComboInfo("Disable for VerRaise", "Doesn't apply this feature to RDM's VerRaise.", ADV.JobID)]
    AdvDisableVerRaiseFeature = 1002,

    [ParentCombo(AdvSwiftcastFeature)]
    [ConflictingCombos(AdvDisableVerRaiseFeature)]
    [CustomComboInfo("Replace VerRaise by Vercure instead", "Do those puny dead bodies really deserve you wasting 2 GCDs?", ADV.JobID)]
    AdvVerRaiseToVerCureFeature = 1003,

    [CustomComboInfo("Variant Raise Feature", "Replace Ascend, Ressurection, Egeiro, Raise, Verraise, and Angel Whisper with Variant Raise II when in a variant dungeon.", ADV.JobID)]
    AdvVariantRaiseFeature = 1001,

    #endregion
    // ====================================================================================
    #region ASTROLOGIAN

    //[CustomComboInfo("Malefic to Draw", "Replace Malefic with Draw when no card is drawn and a card is available.", AST.JobID)]
    //AstrologianMaleficDrawFeature = 3309,

    //[CustomComboInfo("Gravity to Draw", "Replace Gravity with Draw when no card is drawn and a card is available.", AST.JobID)]
    //AstrologianGravityDrawFeature = 3310,

    //[CustomComboInfo("Play to Draw", "Replace Play with Draw when no card is drawn and a card is available.", AST.JobID)]
    //AstrologianPlayDrawFeature = 3301,

    //[ParentCombo(AstrologianPlayDrawFeature)]
    //[CustomComboInfo("Play to Draw to Astrodyne", "Replace Play with Astrodyne when seals are full and Draw is on cooldown or a card is drawn.", AST.JobID)]
    //AstrologianPlayDrawAstrodyneFeature = 3307,

    //[CustomComboInfo("Play to Redraw", "Replace Play with Redraw if a card is drawn and would grant a seal you already have.", AST.JobID)]
    //AstrologianPlayRedrawFeature = 3311,

    //[CustomComboInfo("Play to Astrodyne", "Replace Play with Astrodyne when seals are full.", AST.JobID)]
    //AstrologianPlayAstrodyneFeature = 3304,

    //[CustomComboInfo("Draw Lockout", "Replace Draw (not Play to Draw) with Malefic when a card is drawn.", AST.JobID)]
    //AstrologianDrawLockoutFeature = 3306,

    [CustomComboInfo("Benefic 2 to Benefic Level Sync", "Replace Benefic 2 with Benefic when below level 26 in synced content.", AST.JobID)]
    AstrologianBeneficSyncFeature = 3303,

    #endregion
    // ====================================================================================
    #region BLACK MAGE

    [CustomComboInfo("Enochian Feature", "Replace Fire 4 and Blizzard 4 with whichever action you can currently use.", BLM.JobID)]
    BlackEnochianFeature = 2501,

    [SecretCustomCombo]
    [ParentCombo(BlackEnochianFeature)]
    [CustomComboInfo("Enochian Despair Feature", "Replace Fire 4 and Blizzard 4 with Despair when in Astral Fire with less than 2400 mana.", BLM.JobID)]
    BlackEnochianDespairFeature = 2510,

    [ParentCombo(BlackEnochianFeature)]
    [CustomComboInfo("Enochian No Sync Feature", "Fire 4 and Blizzard 4 will not sync to Fire 1 and Blizzard 1.", BLM.JobID)]
    BlackEnochianNoSyncFeature = 2518,

    [CustomComboInfo("Transpose into Umbral Soul", "Replace Transpose with Umbral Soul when Umbral Soul is usable.", BLM.JobID)]
    BlackTransposeUmbralSoulFeature = 2502,

    [CustomComboInfo("Umbral Soul into Transpose", "Replace Umbral Soul with Transpose when Umbral Soul is not usable.", BLM.JobID)]
    BlackUmbralSoulTransposeFeature = 2522,

    [CustomComboInfo("(Between the) Ley Lines", "Replace Ley Lines with BTL when Ley Lines is active.", BLM.JobID)]
    BlackLeyLinesFeature = 2503,

    [CustomComboInfo("Fire 1/3 Feature", "Fire 1 becomes Fire 3 outside of Astral Fire, and when Firestarter is up.", BLM.JobID)]
    BlackFireFeature = 2504,

    [ParentCombo(BlackFireFeature)]
    [CustomComboInfo("Fire 1/3 Option", "Fire 1 will stay Fire 3 when not at max Astral Fire.", BLM.JobID)]
    BlackFireOption = 2515,

    [ParentCombo(BlackFireFeature)]
    [CustomComboInfo("Fire 1/3 Option (2)", "Fire 1 does not become Fire 3 outside of Astral Fire.", BLM.JobID)]
    BlackFireOption2 = 2516,

    [CustomComboInfo("Blizzard 1/3 Feature", "Replace Blizzard 1 with Blizzard 3 when unlocked and becomes Paradox when available.", BLM.JobID)]
    BlackBlizzardFeature = 2505,

    [CustomComboInfo("Freeze/Flare Feature", "Freeze and Flare become whichever action you can currently use.", BLM.JobID)]
    BlackFreezeFlareFeature = 2506,

    [CustomComboInfo("Fire 2 Feature", "(High) Fire 2 becomes Flare in Astral Fire when only 1 Umbral Heart is active, less than 3000 mp, or during Enhanced Flare.", BLM.JobID)]
    BlackFire2Feature = 2508,

    [CustomComboInfo("Ice 2 Feature", "(High) Blizzard 2 becomes Freeze in Umbral Ice.", BLM.JobID)]
    BlackBlizzard2Feature = 2509,

    [CustomComboInfo("Fire 2/Ice 2 Option", "Fire 2 and Blizzard 2 will not change unless you're at max Astral Fire or Umbral Ice.", BLM.JobID)]
    BlackFireBlizzard2Option = 2514,

    [CustomComboInfo("Umbral Soul Feature", "Replace your ice spells with Umbral Soul, while in Umbral Ice and having no target.", BLM.JobID)]
    BlackSpellsUmbralSoulFeature = 2517,

    [CustomComboInfo("Scathe/Xenoglossy Feature", "Scathe becomes Xenoglossy when available.", BLM.JobID)]
    BlackScatheFeature = 2507,

    #endregion
    // ====================================================================================
    #region BARD

    [CustomComboInfo("Bard one button combo", "Bard one button combo", BRD.JobID)]
    BardOneButton = (BRD.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region DANCER

    [CustomComboInfo("Dancer one button combo", "Dancer one button combo", DNC.JobID)]
    DancerOneButton = (DNC.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region DARK KNIGHT

    [CustomComboInfo("Dark Knight one button combo", "Dark Knight one button combo", DRK.JobID)]
    DarkOneButton = (DRK.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region DRAGOON

    [CustomComboInfo("Dragoon one button combo", "Dragoon one button combo", DRG.JobID)]
    DragoonOneButton = (DRG.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region GUNBREAKER

    [CustomComboInfo("Gunbreaker one button combo", "Gunbreaker one button combo", GNB.JobID)]
    GunbreakerOneButton = (GNB.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region MACHINIST

    [CustomComboInfo("Machinist one button combo", "Machinist one button combo", MCH.JobID)]
    MachinistOneButton = (MCH.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region MONK

    [CustomComboInfo("Monk one button combo", "Monk one button combo", MNK.JobID)]
    MonkOneButton = (MNK.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region NINJA

    [CustomComboInfo("Aeolian Edge Combo", "Replace Aeolian Edge with its combo chain.", NIN.JobID)]
    NinjaAeolianEdgeCombo = 3002,

    //[SecretCustomCombo]
    //[CustomComboInfo("Auto-Refill Kazematoi / Huton Feature", "Replaces Aeolian Edge with Armor Crush when you don't have any Kazematoi left.", NIN.JobID)]
    //NinjaKazematoiFeature = 3019,

    [CustomComboInfo("Aeolian Edge / Ninjutsu Feature", "Replace Aeolian Edge with Ninjutsu if any Mudra are used.", NIN.JobID)]
    NinjaAeolianNinjutsuFeature = 3008,

    [CustomComboInfo("Aeolian Edge / Raiju Feature", "Replace the Aeolian Edge combo with Fleeting Raiju when available.", NIN.JobID)]
    NinjaAeolianEdgeRaijuFeature = 3013,

    [CustomComboInfo("Armor Crush Combo", "Replace Armor Crush with its combo chain.", NIN.JobID)]
    NinjaArmorCrushCombo = 3001,

    [CustomComboInfo("Armor Crush / Ninjutsu Feature", "Replace Armor Crush with Ninjutsu if any Mudra are used.", NIN.JobID)]
    NinjaArmorCrushNinjutsuFeature = 3015,

    [CustomComboInfo("Armor Crush / Raiju Feature", "Replace the Armor Crush combo with Forked Raiju when available.", NIN.JobID)]
    NinjaArmorCrushRaijuFeature = 3012,

    //[CustomComboInfo("Huraijin / Armor Crush Combo", "Replace Huraijin with Armor Crush after using Gust Slash when Huton is missing.", NIN.JobID)]
    //NinjaHuraijinArmorCrushCombo = 3010,

    //[CustomComboInfo("Huraijin / Ninjutsu Feature", "Replace Huraijin with Ninjutsu if any Mudra are used.", NIN.JobID)]
    //NinjaHuraijinNinjutsuFeature = 3009,

    //[ConflictingCombos(NinjaHuraijinFleetingRaijuFeature)]
    //[CustomComboInfo("Huraijin / Forked Raiju Feature", "Replace Huraijin with Forked Raiju when available.", NIN.JobID)]
    //NinjaHuraijinForkedRaijuFeature = 3011,

    //[ConflictingCombos(NinjaHuraijinForkedRaijuFeature)]
    //[CustomComboInfo("Huraijin / Fleeting Raiju Option", "Replace Huraijin with Fleeting Raiju when available.", NIN.JobID)]
    //NinjaHuraijinFleetingRaijuFeature = 3014,

    [CustomComboInfo("Hakke Mujinsatsu Combo", "Replace Hakke Mujinsatsu with its combo chain.", NIN.JobID)]
    NinjaHakkeMujinsatsuCombo = 3003,

    [CustomComboInfo("Hakke Mujinsatsu / Ninjutsu Feature", "Replace Hakke Mujinsatsu with Ninjutsu if any Mudra are used.", NIN.JobID)]
    NinjaHakkeMujinsatsuNinjutsuFeature = 3016,

    [ConflictingCombos(NinjaNinjitsuFleetingRaijuFeature)]
    [CustomComboInfo("Ninjitsu / Forked Raiju Feature", "Replace Ninjitsu with Forked Raiju when available and no Mudra are active.", NIN.JobID)]
    NinjaNinjitsuForkedRaijuFeature = 3017,

    [ConflictingCombos(NinjaNinjitsuForkedRaijuFeature)]
    [CustomComboInfo("Ninjitsu / Fleeting Raiju Feature", "Replace Ninjitsu with Fleeting Raiju when available and no Mudra are active.", NIN.JobID)]
    NinjaNinjitsuFleetingRaijuFeature = 3018,

    [CustomComboInfo("Kassatsu to Trick", "Replace Kassatsu with Trick Attack while Suiton or Hidden is up.\nCooldown tracking plugin recommended.", NIN.JobID)]
    NinjaKassatsuTrickFeature = 3004,

    [CustomComboInfo("Ten Chi Jin to Meisui", "Replace Ten Chi Jin (the move) with Meisui while Suiton is up.\nCooldown tracking plugin recommended.", NIN.JobID)]
    NinjaTCJMeisuiFeature = 3005,

    [CustomComboInfo("Kassatsu Chi/Jin Feature", "Replace Chi with Jin while Kassatsu is up if you have Enhanced Kassatsu.", NIN.JobID)]
    NinjaKassatsuChiJinFeature = 3006,

    [CustomComboInfo("Hide to Mug/Dokumori", "Replace Hide with Mug/Dokumori while in combat.", NIN.JobID)]
    NinjaHideMugFeature = 3007,

    [CustomComboInfo("Hide to Ninjutsu", "Replace Hide with Ninjutsu if any Mudra are active.", NIN.JobID)]
    NinjaHideNinjutsuFeature = 3020,

    #endregion
    // ====================================================================================
    #region PALADIN

    [CustomComboInfo("Paladin one button combo", "Paladin one button combo", PLD.JobID)]
    PaladinOneButton = (PLD.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region PICTOMANCER

    [CustomComboInfo("Subtractive Single-Target Combo", "Replace Blizzard in Cyan and its combo chain with Fire in Red and its combo chain when Subtractive Palette is not active.", PCT.JobID)]
    PictomancerSubtractiveSTCombo = 4201,

    [CustomComboInfo("Subtractive AoE Combo", "Replace Blizzard II in Cyan and its combo chain with Fire II in Red and its combo chain when Subtractive Palette is not active.", PCT.JobID)]
    PictomancerSubtractiveAoECombo = 4202,

    //[SecretCustomCombo]
    //[CustomComboInfo("Subtractive Autocast", "Replace Fire in Red and Fire II in Red, and their combo chains, with Subtractive Palette if the next cast in the chain would overcap the Palette Gauge.", PCT.JobID)]
    //PictomancerSubtractiveAutoCombo = 4205,

    [CustomComboInfo("Holy Comet Combo", "Replace Holy in White with Comet in Black when usable.", PCT.JobID)]
    PictomancerHolyCometCombo = 4203,

    //[SecretCustomCombo]
    //[CustomComboInfo("Holy Autocast", "Replace Fire in Red, Fire II in Red, Blizzard in Cyan, Blizzard II in Cyan, and their combo chains, with Holy or Comet if the next cast would overcap the Paint Gauge.", PCT.JobID)]
    //PictomancerHolyAutoCombo = 4204,

    //[CustomComboInfo("Creature Muse/Motif Combo", "Replace Creature Motif (Pom Motif etc) with Living Muse (Pom Muse etc) when the Creature Canvas is painted.", PCT.JobID)]
    //PictomancerCreatureMotifCombo = 4206,

    //[CustomComboInfo("Creature Muse/Mog of the Ages Combo", "Also replace Creature Motif (Pom Motif etc) with Mog of the Ages and Retribution of the Madeen when they are usable.", PCT.JobID)]
    //PictomancerCreatureMogCombo = 4207,

    //[CustomComboInfo("Weapon Muse/Motif Combo", "Replace Hammer Motif with Striking Muse when the Weapon Canvas is painted.", PCT.JobID)]
    //PictomancerWeaponMotifCombo = 4208,

    [CustomComboInfo("Hammer Time", "Replace Hammer Motif with Hammer Brush and its combo chain when they are usable.", PCT.JobID)]
    PictomancerWeaponHammerCombo = 4209,

    //[CustomComboInfo("Landscape Muse/Motif Combo", "Replace Starry Sky Motif with Starry Muse when the Landscape Canvas is painted.", PCT.JobID)]
    //PictomancerLandscapeMotifCombo = 4210,

    //[CustomComboInfo("Landscape Muse/Star Prism Combo", "Replace Starry Muse with Star Prism when it is usable.", PCT.JobID)]
    //PictomancerLandscapePrismCombo = 4211,

    #endregion
    // ====================================================================================
    #region REAPER

    [CustomComboInfo("Slice Combo", "Replace Infernal Slice with its combo chain.", RPR.JobID)]
    ReaperSliceCombo = 3901,

    [ConflictingCombos(ReaperSliceGallowsFeature)]
    [CustomComboInfo("Slice Gibbet Feature", "Replace Infernal Slice with Gibbet while Reaving or Enshrouded.", RPR.JobID)]
    ReaperSliceGibbetFeature = 3903,

    [ConflictingCombos(ReaperSliceGibbetFeature)]
    [CustomComboInfo("Slice Gallows Feature", "Replace Infernal Slice with Gallows while Reaving or Enshrouded.", RPR.JobID)]
    ReaperSliceGallowsFeature = 3904,

    [CustomComboInfo("Slice Enhanced Soul Reaver Feature", "Replace Infernal Slice with whichever of Gibbet or Gallows is currently enhanced while Reaving.", RPR.JobID)]
    ReaperSliceEnhancedSoulReaverFeature = 3913,

    [CustomComboInfo("Slice Enhanced Enshrouded Feature", "Replace Infernal Slice with whichever of Gibbet or Gallows is currently enhanced while Enshrouded.", RPR.JobID)]
    ReaperSliceEnhancedEnshroudedFeature = 3914,

    [CustomComboInfo("Slice Lemure's Feature", "Replace Infernal Slice with Lemure's Slice when two or more stacks of Void Shroud are active.", RPR.JobID)]
    ReaperSliceLemuresFeature = 3919,

    [CustomComboInfo("Slice Communio Feature", "Replace Infernal Slice with Communio when one stack of Shroud is left.", RPR.JobID)]
    ReaperSliceCommunioFeature = 3920,

    [CustomComboInfo("Slice Soulsow Feature", "Replace Infernal Slice with Soulsow when out of combat and not active.", RPR.JobID)]
    ReaperSliceSoulsowFeature = 3930,

    [ConflictingCombos(ReaperShadowGibbetFeature)]
    [CustomComboInfo("Shadow Gallows Feature", "Replace Shadow of Death with Gallows while Reaving or Enshrouded.", RPR.JobID)]
    ReaperShadowGallowsFeature = 3905,

    [ConflictingCombos(ReaperShadowGallowsFeature)]
    [CustomComboInfo("Shadow Gibbet Feature", "Replace Shadow of Death with Gibbet while Reaving or Enshrouded.", RPR.JobID)]
    ReaperShadowGibbetFeature = 3906,

    [CustomComboInfo("Shadow Lemure's Feature", "Replace Shadow of Death with Lemure's Slice when two or more stacks of Void Shroud are active.", RPR.JobID)]
    ReaperShadowLemuresFeature = 3923,

    [CustomComboInfo("Shadow Communio Feature", "Replace Shadow of Death with Communio when one stack of Shroud is left.", RPR.JobID)]
    ReaperShadowCommunioFeature = 3924,

    [CustomComboInfo("Shadow Soulsow Feature", "Replace Shadow of Death with Soulsow when out of combat, not active, and you have no target.", RPR.JobID)]
    ReaperShadowSoulsowFeature = 3929,

    [ConflictingCombos(ReaperSoulGibbetFeature)]
    [CustomComboInfo("Soul Gallows Feature", "Replace Soul Slice with Gallows while Reaving or Enshrouded.", RPR.JobID)]
    ReaperSoulGallowsFeature = 3925,

    [ConflictingCombos(ReaperSoulGallowsFeature)]
    [CustomComboInfo("Soul Gibbet Feature", "Replace Soul Slice with Gibbet while Reaving or Enshrouded.", RPR.JobID)]
    ReaperSoulGibbetFeature = 3926,

    [CustomComboInfo("Soul Lemure's Feature", "Replace Soul Slice with Lemure's Slice when two or more stacks of Void Shroud are active.", RPR.JobID)]
    ReaperSoulLemuresFeature = 3927,

    [CustomComboInfo("Soul Communio Feature", "Replace Soul Slice with Communio when one stack of Shroud is left.", RPR.JobID)]
    ReaperSoulCommunioFeature = 3928,

    [CustomComboInfo("Soul Overcap Feature", "Replace Soul Slice with Blood Stalk not Enshrouded and greater-than 50 Soul Gauge is present.", RPR.JobID)]
    ReaperSoulOvercapFeature = 3934,

    [CustomComboInfo("Soul (Scythe) Overcap Feature", "Replace Soul Scythe with Grim Swathe when not Enshrouded and greater-than 50 Soul Gauge is present.", RPR.JobID)]
    ReaperSoulScytheOvercapFeature = 3935,

    [CustomComboInfo("Scythe Combo", "Replace Nightmare Scythe with its combo chain.", RPR.JobID)]
    ReaperScytheCombo = 3902,

    [CustomComboInfo("Scythe Guillotine Feature", "Replace Nightmare Scythe with Guillotine while Reaving or Enshrouded.", RPR.JobID)]
    ReaperScytheGuillotineFeature = 3907,

    [CustomComboInfo("Scythe Lemure's Feature", "Replace Nightmare Scythe with Lemure's Scythe when two or more stacks of Void Shroud are active.", RPR.JobID)]
    ReaperScytheLemuresFeature = 3921,

    [CustomComboInfo("Scythe Communio Feature", "Replace Nightmare Scythe with Communio when one stack is left of Shroud.", RPR.JobID)]
    ReaperScytheCommunioFeature = 3922,

    [CustomComboInfo("Scythe Soulsow Feature", "Replace Nightmare Scythe with Soulsow when out of combat and not active.", RPR.JobID)]
    ReaperScytheSoulsowFeature = 3931,

    [CustomComboInfo("Scythe Harvest Moon Feature", "Replace Nightmare Scythe with Harvest Moon when Soulsow is active and you have a target.", RPR.JobID)]
    ReaperScytheHarvestMoonFeature = 3932,

    [CustomComboInfo("Enhanced Soul Reaver Feature", "Replace Gibbet and Gallows with whichever is currently enhanced while Reaving.", RPR.JobID)]
    ReaperEnhancedSoulReaverFeature = 3917,

    [CustomComboInfo("Enhanced Enshrouded Feature", "Replace Gibbet and Gallows with whichever is currently enhanced while Enshrouded.", RPR.JobID)]
    ReaperEnhancedEnshroudedFeature = 3918,

    [CustomComboInfo("Lemure's Soul Reaver Feature", "Replace Gibbet, Gallows, and Guillotine with Lemure's Slice or Scythe when two or more stacks of Void Shroud are active.", RPR.JobID)]
    ReaperLemuresSoulReaverFeature = 3911,

    [CustomComboInfo("Communio Soul Reaver Feature", "Replace Gibbet, Gallows, and Guillotine with Communio when one stack is left of Shroud.", RPR.JobID)]
    ReaperCommunioSoulReaverFeature = 3912,

    [CustomComboInfo("Enshroud Communio Feature", "Replace Enshroud with Communio when Enshrouded.", RPR.JobID)]
    ReaperEnshroudCommunioFeature = 3909,

    [CustomComboInfo("Blood Stalk Gluttony Feature", "Replace Blood Stalk with Gluttony when available and greater-than-or-equal-to 50 Soul Gauge is present.", RPR.JobID)]
    ReaperBloodStalkGluttonyFeature = 3915,

    [CustomComboInfo("Grim Swathe Gluttony Feature", "Replace Grim Swathe with Gluttony when available and greater-than-or-equal-to 50 Soul Gauge is present.", RPR.JobID)]
    ReaperGrimSwatheGluttonyFeature = 3916,

    [CustomComboInfo("Arcane Harvest Feature", "Replace Arcane Circle with Plentiful Harvest when you have stacks of Immortal Sacrifice.", RPR.JobID)]
    ReaperHarvestFeature = 3908,

    [CustomComboInfo("Regress Feature", "Replace Hell's Ingress and Egress turn with Regress when Threshold is active, instead of just the opposite of the one used.", RPR.JobID)]
    ReaperRegressFeature = 3910,

    [ParentCombo(ReaperRegressFeature)]
    [CustomComboInfo("Delayed Regress Option", "Replace the action used with Regress only after 1.5 seconds have elapsed on Threshold.", RPR.JobID)]
    ReaperRegressOption = 3933,

    [CustomComboInfo("Harpe Soulsow Feature", "Replace Harpe with Soulsow when not active and out of combat or you have no target.", RPR.JobID)]
    ReaperHarpeHarvestSoulsowFeature = 3936,

    [CustomComboInfo("Harpe Harvest Moon Feature", "Replace Harpe with Harvest Moon when Soulsow is active and you are in combat.", RPR.JobID)]
    ReaperHarpeHarvestMoonFeature = 3937,

    [ParentCombo(ReaperHarpeHarvestMoonFeature)]
    [CustomComboInfo("Enhanced Harpe Option", "Prevent replacing Harpe with Harvest Moon when Enhanced Harpe is active.", RPR.JobID)]
    ReaperHarpeHarvestMoonEnhancedFeature = 3939,

    [ParentCombo(ReaperHarpeHarvestMoonFeature)]
    [CustomComboInfo("Combat Option", "Prevent replacing Harpe with Harvest Moon when not in combat.", RPR.JobID)]
    ReaperHarpeHarvestMoonCombatFeature = 3938,

    #endregion
    // ====================================================================================
    #region RED MAGE

    [CustomComboInfo("Verstone/Verfire Feature", "Replace Verstone/Verfire with Jolt when no proc is available.", RDM.JobID)]
    RedMageVerprocFeature = 3504,

    [CustomComboInfo("Verstone/Verfire Plus Feature", "Replace Verstone/Verfire with Veraero/Verthunder when various instant-cast effects are active.", RDM.JobID)]
    RedMageVerprocPlusFeature = 3505,

    [ParentCombo(RedMageVerprocPlusFeature)]
    [CustomComboInfo("Deprioritize Grand Impact", "After using Acceleration, prioritize using Veraero/Verthunder over Grand Impact if both buffs are active.", RDM.JobID)]
    RedMageGrandImpactDeprioritize = 3517,

    [CustomComboInfo("Verstone/Verfire Plus Opener Feature (Stone)", "Replace Verstone with Veraero when out of combat.", RDM.JobID)]
    RedMageVerprocOpenerStoneFeature = 3506,

    [CustomComboInfo("Verstone/Verfire Plus Opener Feature (Fire)", "Replace Verfire with Verthunder when out of combat.", RDM.JobID)]
    RedMageVerprocOpenerFireFeature = 3507,

    [CustomComboInfo("Verstone/Verfire Mana Stacks Feature", "Replace Verstone/Verfire with Verflare/Verholy at 3 mana stacks.", RDM.JobID)]
    RedMageVerprocManaStacksFeature = 3515,

    [CustomComboInfo("Verstone/Verfire Capstone Combo", "Replace Verstone/Verfire with Scorch and Resolution when available.", RDM.JobID)]
    RedMageVerprocCapstoneCombo = 3513,

    [CustomComboInfo("Veraero/Verthunder Capstone Combo", "Replace Veraero/Verthunder with Scorch and Resolution when available.", RDM.JobID)]
    RedMageVeraeroVerthunderCapstoneCombo = 3512,

    [CustomComboInfo("AoE Combo", "Replace Veraero/Verthunder 2 with Impact when various instant-cast effects are active.", RDM.JobID)]
    RedMageAoEFeature = 3501,

    [CustomComboInfo("AoE Capstone Combo", "Replace Veraero/Verthunder 2 with Scorch and Resolution when available.", RDM.JobID)]
    RedMageAoECapstoneCombo = 3514,

    [CustomComboInfo("Melee combo", "Replace Redoublement with its combo chain, following enchantment rules.", RDM.JobID)]
    RedMageMeleeCombo = 3502,

    [CustomComboInfo("Melee Mana Stacks Feature", "Replace Redoublement and Moulinet with Verflare/Verholy at 3 mana stacks, using whichever mana color is lower.", RDM.JobID)]
    RedMageMeleeManaStacksFeature = 3516,

    [CustomComboInfo("Melee Capstone Combo", "Replace Redoublement and Moulinet with Scorch, Resolution and Prefulgence when available.", RDM.JobID)]
    RedMageMeleeCapstoneCombo = 3503,

    [CustomComboInfo("Acceleration into Grand Impact", "Replace Acceleration with Grand Impact when available.", RDM.JobID)]
    RedMageAccelerationGrandImpactFeature = 3518,

    [CustomComboInfo("Acceleration into Swiftcast", "Replace Acceleration with Swiftcast when on cooldown or synced.", RDM.JobID)]
    RedMageAccelerationSwiftcastFeature = 3509,

    [ParentCombo(RedMageAccelerationSwiftcastFeature)]
    [CustomComboInfo("Acceleration with Swiftcast first", "Replace Acceleration with Swiftcast when neither are on cooldown.", RDM.JobID)]
    RedMageAccelerationSwiftcastOption = 3511,

    [CustomComboInfo("Embolden to Manaification", "Replace Embolden with Manafication if the former is on cooldown and the latter is not.", RDM.JobID)]
    RedMageEmboldenFeature = 3510,

    [SecretCustomCombo]
    [CustomComboInfo("Contre Sixte / Fleche Feature", "Replace Contre Sixte and Fleche with whichever is available.", RDM.JobID)]
    RedMageContreFlecheFeature = 3508,

    #endregion
    // ====================================================================================
    #region SAGE

    [CustomComboInfo("Dosis Kardia Feature", "Replace Dosis with Kardia when missing Kardion.", SGE.JobID)]
    SageDosisKardiaFeature = 4010,

    [CustomComboInfo("Druochole into Rhizomata Feature", "Replace Druochole with Rhizomata when Addersgall is empty.", SGE.JobID)]
    SageDruocholeRhizomataFeature = 4003,

    [CustomComboInfo("Druochole into Taurochole Feature", "Replace Druochole with Taurochole when off cooldown.\nWarning: This will limit your abiility to use Druochole.", SGE.JobID)]
    SageDruocholeTaurocholeFeature = 4009,

    [CustomComboInfo("Ixochole into Rhizomata Feature", "Replace Ixochole with Rhizomata when Addersgall is empty.", SGE.JobID)]
    SageIxocholeRhizomataFeature = 4004,

    [CustomComboInfo("Kerachole into Rhizomata Feature", "Replace Kerachole with Rhizomata when Addersgall is empty.", SGE.JobID)]
    SageKeracholaRhizomataFeature = 4005,

    [CustomComboInfo("Phlegma into Dyskrasia", "Replace Phlegma with Dyskrasia when no charges remain or have no target.", SGE.JobID)]
    SagePhlegmaDyskrasia = 4008,

    [CustomComboInfo("Phlegma into Toxikon", "Replace Phlegma with Toxikon when no charges rmemain and have Addersting.\nThis takes priority over Phlegma into Dyskrasia.", SGE.JobID)]
    SagePhlegmaToxicon = 4007,

    [CustomComboInfo("Soteria Kardia Feature", "Replace Soteria with Kardia when off cooldown and missing Kardion.", SGE.JobID)]
    SageSoteriaKardionFeature = 4006,

    [CustomComboInfo("Taurochole into Druochole Feature", "Replace Taurochole with Druochole when on cooldown", SGE.JobID)]
    SageTaurocholeDruocholeFeature = 4001,

    [CustomComboInfo("Taurochole into Rhizomata Feature", "Replace Taurochole with Rhizomata when Addersgall is empty.", SGE.JobID)]
    SageTaurocholeRhizomataFeature = 4002,

    [CustomComboInfo("Toxikon into Phlegma Feature", "Replace Toxikon with Phlegma when charges are available.", SGE.JobID)]
    SageToxikonPhlegma = 4011,

    #endregion
    // ====================================================================================
    #region SAMURAI

    [CustomComboInfo("Yukikaze Combo", "Replace Yukikaze with its combo chain.", SAM.JobID)]
    SamuraiYukikazeCombo = 3401,

    [CustomComboInfo("Gekko Combo", "Replace Gekko with its combo chain.", SAM.JobID)]
    SamuraiGekkoCombo = 3402,

    [ParentCombo(SamuraiGekkoCombo)]
    [CustomComboInfo("Gekko Combo Option", "Start the Gekko combo chain with Jinpu instead of Hakaze.", SAM.JobID)]
    SamuraiGekkoOption = 3416,

    [CustomComboInfo("Kasha Combo", "Replace Kasha with its combo chain.", SAM.JobID)]
    SamuraiKashaCombo = 3403,

    [ParentCombo(SamuraiKashaCombo)]
    [CustomComboInfo("Kasha Combo Option", "Start the Kasha combo chain with Shifu instead of Hakaze.", SAM.JobID)]
    SamuraiKashaOption = 3417,

    [CustomComboInfo("Mangetsu Combo", "Replace Mangetsu with its combo chain.", SAM.JobID)]
    SamuraiMangetsuCombo = 3404,

    [CustomComboInfo("Oka Combo", "Replace Oka with its combo chain.", SAM.JobID)]
    SamuraiOkaCombo = 3405,

    [ConflictingCombos(SamuraiIaijutsuTsubameGaeshiFeature)]
    [CustomComboInfo("Tsubame-gaeshi to Iaijutsu", "Replace Tsubame-gaeshi with Iaijutsu when Sen is empty.", SAM.JobID)]
    SamuraiTsubameGaeshiIaijutsuFeature = 3407,

    [ConflictingCombos(SamuraiIaijutsuShohaFeature)]
    [CustomComboInfo("Tsubame-gaeshi to Shoha", "Replace Tsubame-gaeshi with Shoha when meditation is 3.", SAM.JobID)]
    SamuraiTsubameGaeshiShohaFeature = 3408,

    [ConflictingCombos(SamuraiTsubameGaeshiIaijutsuFeature)]
    [CustomComboInfo("Iaijutsu to Tsubame-gaeshi", "Replace Iaijutsu with Tsubame-gaeshi when Sen is not empty.", SAM.JobID)]
    SamuraiIaijutsuTsubameGaeshiFeature = 3409,

    [ConflictingCombos(SamuraiTsubameGaeshiShohaFeature)]
    [CustomComboInfo("Iaijutsu to Shoha", "Replace Iaijutsu with Shoha when meditation is 3.", SAM.JobID)]
    SamuraiIaijutsuShohaFeature = 3410,

    [CustomComboInfo("Shinten to Zanshin", "Replace Hissatsu: Shinten with Zanshin when available.", SAM.JobID)]
    SamuraiShintenZanshinFeature = 3420,

    [CustomComboInfo("Shinten to Shoha", "Replace Hissatsu: Shinten with Shoha when Meditation is full.", SAM.JobID)]
    SamuraiShintenShohaFeature = 3413,

    [CustomComboInfo("Shinten to Senei", "Replace Hissatsu: Shinten with Senei when available.", SAM.JobID)]
    SamuraiShintenSeneiFeature = 3414,

    [CustomComboInfo("Senei to Guren Level Sync", "Replace Hissatsu: Senei with Guren when level synced below 72.", SAM.JobID)]
    SamuraiSeneiGurenFeature = 3418,

    [CustomComboInfo("Kyuten to Guren", "Replace Hissatsu: Kyuten with Guren when available.", SAM.JobID)]
    SamuraiKyutenGurenFeature = 3415,

    //[CustomComboInfo("Kyuten to Shoha II", "Replace Hissatsu: Kyuten with Shoha II when Meditation is full.", SAM.JobID)]
    //SamuraiKyutenShoha2Feature = 3412,

    [CustomComboInfo("Ikishoten Namikiri Feature", "Replace Ikishoten with Ogi Namikiri and then Kaeshi Namikiri when available.", SAM.JobID)]
    SamuraiIkishotenNamikiriFeature = 3411,

    [ParentCombo(SamuraiIkishotenNamikiriFeature)]
    [CustomComboInfo("Ikishoten Shoha Feature", "Replace Ikishoten with Shoha when Meditation is full.", SAM.JobID)]
    SamuraiIkishotenShohaFeature = 3419,

    #endregion
    // ====================================================================================
    #region SCHOLAR

    [CustomComboInfo("Seraph Fey Blessing/Consolation", "Replace Fey Blessing with Consolation when Seraph is out.", SCH.JobID)]
    ScholarSeraphConsolationFeature = 2801,

    [CustomComboInfo("Lustrate to Recitation", "Replace Lustrate with Recitation when Recitation is off cooldown.", SCH.JobID)]
    ScholarLustrateRecitationFeature = 2807,

    [CustomComboInfo("Lustrate to Excogitation", "Replace Lustrate with Excogitation when Excogitation is off cooldown.", SCH.JobID)]
    ScholarLustrateExcogitationFeature = 2808,

    [CustomComboInfo("Excogitation to Recitation", "Replace Excogitation with Recitation when Recitation is off cooldown.", SCH.JobID)]
    ScholarExcogitationRecitationFeature = 2806,

    [CustomComboInfo("Excogitation to Lustrate", "Replace Excogitation with Lustrate when Excogitation is on cooldown.", SCH.JobID)]
    ScholarExcogitationLustrateFeature = 2809,

    [CustomComboInfo("ED Aetherflow", "Replace Energy Drain with Aetherflow when you have no more Aetherflow stacks.", SCH.JobID)]
    ScholarEnergyDrainAetherflowFeature = 2802,

    [CustomComboInfo("Lustrous Aetherflow", "Replace Lustrate with Aetherflow when you have no more Aetherflow stacks.", SCH.JobID)]
    ScholarLustrateAetherflowFeature = 2803,

    [CustomComboInfo("Indomitable Aetherflow", "Replace Indomitability with Aetherflow when you have no more Aetherflow stacks.", SCH.JobID)]
    ScholarIndomAetherflowFeature = 2804,

    [CustomComboInfo("Sacred Soil Aetherflow", "Replace Sacred Soil with Aetherflow when you have no more Aetherflow stacks.", SCH.JobID)]
    ScholarSacredSoilAetherflowFeature = 2811,

    [CustomComboInfo("Summon Seraph Feature", "Replace Summon Eos and Selene with Summon Seraph when a summon is out.", SCH.JobID)]
    ScholarSeraphFeature = 2805,

    [CustomComboInfo("Adloquium Level Sync", "Replace Adloquium with Physick when below level 30 in synced content.", SCH.JobID)]
    ScholarAdloquiumSyncFeature = 2810,

    #endregion
    // ====================================================================================
    #region SUMMONER

    [CustomComboInfo("Ruin Feature", "Change Ruin into Gemburst when attuned.", SMN.JobID)]
    SummonerRuinFeature = 2703,

    [CustomComboInfo("Outburst Feature", "Change Outburst into Precious Brilliance when attuned.", SMN.JobID)]
    SummonerOutburstFeature = 2704,

    [CustomComboInfo("Titan's Favor Ruin Feature", "Change Ruin into Mountain Buster (oGCD) when available.", SMN.JobID)]
    SummonerRuinTitansFavorFeature = 2713,

    [CustomComboInfo("Titan's Favor Outburst Feature", "Change Outburst into Mountain Buster (oGCD) when available.", SMN.JobID)]
    SummonerOutburstTitansFavorFeature = 2714,

    [CustomComboInfo("Gems Titan's Favor Feature", "Change Gemshine and Precious Brilliance into Mountain Buster (oGCD) when available.", SMN.JobID)]
    SummonerShinyTitansFavorFeature = 2707,

    [CustomComboInfo("Ruin 4 to Ruin Feature", "Change Ruin into Ruin4 when available and appropriate.", SMN.JobID)]
    SummonerFurtherRuinFeature = 2705,

    [CustomComboInfo("Ruin 4 to Outburst Feature", "Change Outburst into Ruin4 when available and appropriate.", SMN.JobID)]
    SummonerFurtherOutburstFeature = 2706,

    [CustomComboInfo("Gems Ruin 4 Feature", "Change Gemshine and Precious Brilliance into Ruin 4 when available and appropriate.", SMN.JobID)]
    SummonerFurtherShinyFeature = 2708,

    [CustomComboInfo("Gems Enkindle Feature", "Change Gemshine and Precious Brilliance to Enkindle when Bahamut, Phoenix or Summon Solar Bahamut are summoned.", SMN.JobID)]
    SummonerShinyEnkindleFeature = 2709,

    [CustomComboInfo("Demi Enkindle Feature", "Change Summon Bahamut and Summon Phoenix and Summon Solar Bahamut into Enkindle when Bahamut or Phoenix are summoned.", SMN.JobID)]
    SummonerDemiEnkindleFeature = 2710,

    [CustomComboInfo("Searing Demi Feature", "Change Summon Bahamut, Summon Phoenix and Summon Solar Bahamut into Searing Light when any of them is ready to be summoned, Searing Light is off cooldown, and you are in combat.", SMN.JobID)]
    SummonerDemiSearingLightFeature = 2715,

    [CustomComboInfo("ED Fester/Necrosis Feature", "Change Fester/Necrosis into Energy Drain when out of Aetherflow stacks.", SMN.JobID)]
    SummonerEDFesterFeature = 2701,

    [CustomComboInfo("ES Painflare Feature", "Change Painflare into Energy Syphon when out of Aetherflow stacks.", SMN.JobID)]
    SummonerESPainflareFeature = 2702,

    [CustomComboInfo("Radiant Carbuncle Feature", "Change Radiant Aegis into Summon Carbuncle when no pet has been summoned.", SMN.JobID)]
    SummonerRadiantCarbuncleFeature = 2711,

    [ParentCombo(SummonerRadiantCarbuncleFeature)]
    [CustomComboInfo("Radiant Lux Solaris Feature", "Change Radiant Aegis to Lux Solaris when you have Refulgent Lux ready.", SMN.JobID)]
    SummonerRadiantLuxSolarisFeature = 2718,

    [CustomComboInfo("Demi Carbuncle Feature", "Change Summon Bahamut into Summon Carbuncle when no pet has been summoned.", SMN.JobID)]
    SummonerDemiCarbuncleFeature = 2716,

    [CustomComboInfo("Summon Lux Solaris Feature", "Change Summon Bahamut to Lux Solaris when you have Refulgent Lux ready.", SMN.JobID)]
    SummonerSummonLuxSolarisFeature = 2717,


    #endregion
    // ====================================================================================
    #region VIPER

    [CustomComboInfo("Viper one button combo", "Viper one button combo", VPR.JobID)]
    ViperOneButton = (VPR.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region WARRIOR

    [CustomComboInfo("Warrior one button combo", "Warrior one button combo", WAR.JobID)]
    WarriorOneButton = (WAR.JobID * 10000) + 1,

    #endregion
    // ====================================================================================
    #region WHITE MAGE

    [CustomComboInfo("Solace into Misery", "Replace Afflatus Solace with Afflatus Misery when ready.", WHM.JobID)]
    WhiteMageSolaceMiseryFeature = 2401,

    [CustomComboInfo("Targeted Misery", "Only swap to Afflatus Misery when targeting an enemy.", WHM.JobID)]
    WhiteMageSolaceMiseryTargetFeature = 2406,

    [CustomComboInfo("Rapture into Misery", "Replace Afflatus Rapture with Afflatus Misery when ready and you have an enemy target.", WHM.JobID)]
    WhiteMageRaptureMiseryFeature = 2402,

    [CustomComboInfo("Holy into Misery", "Replace Holy/Holy 3 with Afflatus Misery when ready and you have an enemy target.", WHM.JobID)]
    WhiteMageHolyMiseryFeature = 2405,

    [CustomComboInfo("Cure 2 Level Sync", "Replace Cure 2 with Cure when below level 30 in synced content.", WHM.JobID)]
    WhiteMageCureFeature = 2403,

    [CustomComboInfo("Afflatus Feature", "Replace Cure 2 with Afflatus Solace and Medica with Afflatus Rapture when a Lily is available.", WHM.JobID)]
    WhiteMageAfflatusFeature = 2404,

    [ParentCombo(WhiteMageAfflatusFeature)]
    [CustomComboInfo("Medicafflatus Feature", "Also replaces Medica 2 & Medica 3 with Afflatus Rapture when a Lily is available.", WHM.JobID)]
    WhiteMageAfflatusMedicaPlusFeature = 2408,

    [CustomComboInfo("Glare4 Feature", "Replace Glare 3 with Glare 4 when a stack is available.", WHM.JobID)]
    WhiteMageGlare4Feature = 2407,

    #endregion
    // ====================================================================================
    #region DOH

    // [CustomComboInfo("Placeholder", "Placeholder.", DOH.JobID)]
    // DohPlaceholder = 50001,

    #endregion
    // ====================================================================================
    #region DOL

    [CustomComboInfo("Eureka Feature", "Replace Ageless Words and Solid Reason with Wise to the World when available.", DOL.JobID)]
    DolEurekaFeature = 51001,

    [CustomComboInfo("Cast / Hook Feature", "Replace Cast with Hook when fishing.", DOL.JobID)]
    DolCastHookFeature = 51002,

    [CustomComboInfo("Cast / Gig Feature", "Replace Cast with Gig when underwater.", DOL.JobID)]
    DolCastGigFeature = 51003,

    [CustomComboInfo("Surface Slap / Veteran Trade Feature", "Replace Surface Slap with Veteran Trade when underwater.", DOL.JobID)]
    DolSurfaceTradeFeature = 51004,

    [CustomComboInfo("Prize Catch / Nature's Bounty Feature", "Replace Prize Catch with Nature's Bounty when underwater.", DOL.JobID)]
    DolPrizeBountyFeature = 51005,

    [CustomComboInfo("Snagging / Salvage Feature", "Replace Snagging with Salvage when underwater.", DOL.JobID)]
    DolSnaggingSalvageFeature = 51006,

    [CustomComboInfo("Cast Light / Electric Current Feature", "Replace Cast Light with Electric Current when underwater.", DOL.JobID)]
    DolCastLightElectricCurrentFeature = 51007,

    #endregion
    // ====================================================================================
}
