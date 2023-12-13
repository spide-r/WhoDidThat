/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using WhoDidThat.Toolbox;
using Action = Lumina.Excel.GeneratedSheets.Action;

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
        
        [Signature("40 55 53 57 41 54 41 55 41 56 41 57 48 8D AC 24 60 FF FF FF 48 81 EC A0 01 00 00",
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
                uint localPlayerId = Service.ClientState.LocalPlayer!.ObjectId;

                uint actionId = effectHeader->EffectDisplayType switch
                {
                    ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                    ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                    _ => effectHeader->ActionAnimationId
                };

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
                        Service.PluginLog.Information("S:" + sourceId + "|A: " + actionId + "|T: " + actionTargetId +
                                                      "|AN:" + Service.DataManager.Excel.GetSheet<Action>()
                                                                      ?.GetRow(actionId)?
                                                                      .Name.RawString);
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



                    //todo bard songs: 114,3359,116


                    int[] roleActionsWithPlayerTarget =
                        {(int)ClassJobActions.Esuna, (int)ClassJobActions.Rescue, (int)ClassJobActions.Shirk};
                    int[] debuffActionsWithNpcTarget = 
                    {
                        (int)ClassJobActions.LegGraze, (int)ClassJobActions.HeadGraze,
                        (int)ClassJobActions.LowBlow, (int)ClassJobActions.LegSweep, (int)ClassJobActions.Mug,
                        (int)ClassJobActions.Chain, (int)ClassJobActions.Interject, (int)ClassJobActions.FootGraze
                    };
                    int[] mitigationNpcTarget = new[]
                    {
                        (int)ClassJobActions.Addle, (int)ClassJobActions.Feint,
                        (int)ClassJobActions.Reprisal, (int)ClassJobActions.Dismantle
                    };
                    bool roleAction = roleActionsWithPlayerTarget.Contains<int>((int)actionId);
                    bool actionIsTargetingNpc = debuffActionsWithNpcTarget.Contains((int)actionId) ||
                                                mitigationNpcTarget.Contains((int)actionId) ||  actionId == (int) ClassJobActions.Provoke;

                    if (actionIsTargetingNpc)
                    {
                        //if not in an instance, return
                        //check if npc targeted crap is even allowed
                        //check if targeted mitigation is allowed - if so (and it is mit), go to A
                        //check if targeted debuff is allowed - if so (and it is debuff), go to A
                        //check if provoke is allowed- if so (and its provoke), go to A
                        //A: check if its from a job thats filtered, if not, continue
                        //check if the job is unique and do the checkUnique fuckery
                        //check if its from a person outside party, if not (or its enabled), continue
                        //use ShouldLogEffects()
                        //if true, then log that bad boy
                        //todo filter self
                        if (!Service.DutyState.IsDutyStarted) //only if in instances
                        {
                        // return;   todo re-add
                        //todo maybe instead of instance - check if the target npc AND the plugin user are in combat
                        }

                        if (!plugin.Configuration.TargetNpc)
                        {
                            return;
                        }

                        if (!plugin.Configuration.TargetedMit && mitigationNpcTarget.Contains((int)actionId)) 
                        {
                            return;   
                        }
                        
                        if (!plugin.Configuration.TargetedDebuffs && debuffActionsWithNpcTarget.Contains((int)actionId)) 
                        {
                            return;   
                        }


                        if (!plugin.Configuration.Provoke && actionId == (int)ClassJobActions.Provoke)
                        {
                            return;
                        }
                        
                        PlayerCharacter? player = Service.ObjectTable.SearchById((ulong)sourceId) as PlayerCharacter;
                        //todo check for null you nerd
                        if (!checks.ShouldLogEvenIfUnique(player.ClassJob.GameData, actionId))
                        {
                            return;
                        }

                        Tools tools = new Tools(plugin); //todo remove this when transposing
                        if (!tools.ShouldLogRole(player.ClassJob.GameData.PartyBonus))
                        {
                            return;
                        }
                        bool actorInParty = Service.PartyList.Count(member =>
                        {
                            return member.ObjectId == sourceId;
                        }) > 0;
                        
                        if (!plugin.Configuration.LogOutsideParty && !actorInParty)
                        {
                            return;
                        }

                        if (tools.ShouldLogEffects(tools.getEffects(0, effectArray)))
                        {
                            actionLogger.LogAction(actionId, (uint)sourceId);
                        }
                        return;
                    }

                    bool shouldLogAction = checks.CheckLog(targets, sourceId, sourceCharacter, effectArray, effectTrail,
                                                           roleAction, actionId);
                    if (shouldLogAction)
                    {
                        actionLogger.LogAction(actionId, (uint)sourceId);
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
