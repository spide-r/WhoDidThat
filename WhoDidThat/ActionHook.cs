/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using WhoDidThat.Toolbox;
using Action = Lumina.Excel.Sheets.Action;

namespace WhoDidThat
{

    public class ActionHook : IDisposable
    {
        private readonly WhoDidThatPlugin plugin;
        private readonly Checks checks;
        private readonly ActionLogger actionLogger;

        public ActionHook(WhoDidThatPlugin plugin) {
            this.plugin = plugin;
            checks = new Checks(plugin);
            actionLogger = new ActionLogger(plugin);
            Service.GameInteropProvider.InitializeFromAttributes(this);
            receiveAbilityEffectHook.Enable();
        }
        
        [Signature("40 55 56 57 41 54 41 55 41 56 48 8D AC 24 68 FF FF FF 48 81 EC 98 01 00 00",
                   DetourName = nameof(ReceiveAbilityEffectDetour))]
        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityEffectHook = null!;

        private unsafe delegate void ReceiveAbilityDelegate(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail);


        private unsafe void ReceiveAbilityEffectDetour(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail)
        {
            receiveAbilityEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);
            try
            {
                if (!plugin.Configuration.Enabled)
                {
                    return;
                }

                if (Service.ClientState.IsPvP)
                {
                    return;
                }

                uint targets = effectHeader->EffectCount;

                uint actionId = effectHeader->EffectDisplayType switch
                {
                    ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                    ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                    _ => effectHeader->ActionAnimationId
                };
                
                ulong gameObjectID = Service.ObjectTable.SearchById((uint)sourceId).GameObjectId;

                for (var i = 0; i < targets; i++)
                {
                    var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                    bool targetNotInParty = Service.PartyList.Count(p => { return p.ObjectId == actionTargetId; }) == 0;
                    if (plugin.Configuration.Verbose)
                    {
                        if (actionId == 7)
                        {
                            continue;
                        }
                        Service.PluginLog.Information("S:" + sourceId + " GOID: " + gameObjectID  +  "|A: " + actionId + "|T: " + actionTargetId +
                                                      "|AN:" + Service.DataManager.Excel.GetSheet<Action>()
                                                                      .GetRow(actionId).Name.ToString());
                        for (var j = 0; j < 8; j++)
                        {
                            ref var actionEffect = ref effectArray[i * 8 + j];
                            if (actionEffect.EffectType == 0)
                            {
                                continue;
                            }

                            Service.PluginLog.Information("E:" + actionEffect.EffectType);

                        }
                    }
                }



                /*
                  Role Actions:
                     provoke: 7533
                 */

                    int[] roleActionsWithPlayerTarget =
                        [(int)ClassJobActions.Esuna, (int)ClassJobActions.Rescue, (int)ClassJobActions.Shirk];
                    int[] debuffActionsWithNpcTarget =
                    [
                        (int)ClassJobActions.LegGraze, (int)ClassJobActions.HeadGraze,
                        (int)ClassJobActions.LowBlow, (int)ClassJobActions.LegSweep, (int)ClassJobActions.Mug,
                        (int)ClassJobActions.Chain, (int)ClassJobActions.Interject, (int)ClassJobActions.FootGraze,
                        (int)ClassJobActions.Dokumori
                    ];
                    int[] mitigationNpcTarget =
                    [
                        (int)ClassJobActions.Addle, (int)ClassJobActions.Feint,
                        (int)ClassJobActions.Reprisal, (int)ClassJobActions.Dismantle
                    ];
                    bool roleAction = roleActionsWithPlayerTarget.Contains((int)actionId);
                    bool actionIsTargetingNpc = debuffActionsWithNpcTarget.Contains((int)actionId) ||
                                                mitigationNpcTarget.Contains((int)actionId) ||  actionId == (int) ClassJobActions.Provoke;
                    bool shouldLogAction;
                    if (actionIsTargetingNpc)
                    {
                        shouldLogAction = checks.CheckLogNPCTarget(gameObjectID, effectArray, actionId, mitigationNpcTarget, debuffActionsWithNpcTarget);
                    }
                    else
                    {
                        shouldLogAction = checks.CheckLog(targets, gameObjectID, sourceCharacter, effectArray, effectTrail,
                                                          roleAction, actionId);
                    }
                    
                    if (shouldLogAction)
                    {
                        actionLogger.LogAction(actionId, gameObjectID);
                    }
            }
            catch (Exception e)
            {
                Service.PluginLog.Error(e, "oops!");
            }
        }

        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();
            receiveAbilityEffectHook.Dispose();
        }
    }
}
