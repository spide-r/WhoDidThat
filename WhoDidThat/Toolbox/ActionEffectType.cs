﻿/*
 * Attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Game/ActionEffectType.cs
 */
namespace WhoDidThat.Toolbox;

internal enum ActionEffectType : byte {
    Nothing = 0,
    Miss = 1,
    Heal = 4,
    NoEffect = 8,
    ApplyStatusEffectTarget = 14,
    RecoveredFromStatusEffect = 16,
    StartActionCombo = 27,
    Knockback = 32, //sometime in 7.2 this changed from 33 to 32
    Interrupt = 76,
    ThreatPosition = 24, //voke
    EnmityChange = 62 //shirk
}
