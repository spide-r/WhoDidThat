/*
 * Attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Game/ActionEffectType.cs
 */
namespace WDT.Toolbox;

internal enum ActionEffectType : byte {
    Nothing = 0,
    Heal = 4,
    ApplyStatusEffectTarget = 14,
    RecoveredFromStatusEffect = 16,
    StartActionCombo = 27,
    Knockback = 33
}
