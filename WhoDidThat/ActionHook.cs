﻿/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using Lumina.Excel.GeneratedSheets;
using WhoDidThat.Toolbox;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace WhoDidThat
{

    public class ActionHook : IDisposable
    {
        private readonly WhoDidThatPlugin plugin;
        private readonly Tools tools;

        public ActionHook(WhoDidThatPlugin plugin) {
            this.plugin = plugin;
            tools = new Tools(plugin);

            SignatureHelper.Initialise(this);

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
            if (!plugin.Configuration.Enabled)
            {
                return;
            }

            GameObject targetingActor = Service.ObjectTable.First(o => o.ObjectId == (uint) sourceId);
            if (targetingActor.ObjectKind != ObjectKind.Player && plugin.Configuration.OnlyLogPlayerCharacters)
            {
                return;
            }

            try
            {

                if (Service.ClientState.IsPvP)
                {
                    if (!Service.ClientState.IsPvPExcludingDen) //player is in wolves den - exclude since dueling is jank
                    {
                        return;
                    }
                }

                if (Service.PartyList.Length == 0 && !plugin.Configuration.IgnoreParty) //empty party, no need to keep going, out of scope 
                {
                    return;
                }
             
                bool actorInParty = Service.PartyList.Count(member =>
                {
                    return member.ObjectId == sourceId;
                }) > 0;

                //todo need to manage in-party vs out-of-party shenanigans maybe
                if (!actorInParty)
                {
                    if (!plugin.Configuration.IgnoreParty)
                    {
                        return;
                    }              
                }
                
                uint targets = effectHeader->EffectCount;

                if (targets == 0)
                {
                    return;
                }

                if (!plugin.Configuration.MultiTarget && targets > 1)
                {
                    return;
                }
                

                var actionId = effectHeader->EffectDisplayType switch
                {
                    ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                    ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                    _ => effectHeader->ActionAnimationId
                };
                Action? action = null;
                string? source = null;
                GameObject? gameObject = null;
                

                uint localplayerid = Service.ClientState.LocalPlayer!.ObjectId;
                for (var i = 0; i < targets; i++)
                {
                    var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                    if (actionTargetId != localplayerid)
                    {
                        continue;
                    }
                    

                    if (sourceId == localplayerid)
                    {

                        if (!plugin.Configuration.SelfLog)
                        {
                            continue;
                        }
                    }
                    if (plugin.Configuration.Verbose)
                    {
                        PluginLog.Information("S:" + sourceId + "|A: " + actionId + "|T: " + actionTargetId + 
                                              "|AN:" + Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId)!.Name.RawString);
                    }
                   
                    ClassJob? originJob;
                    
                    //todo rework for overworld functionality
                    if (actorInParty)
                    {
                        
                        originJob = Service.PartyList.First(p => p.GameObject.Address == sourceCharacter).ClassJob.GameData;
                    }
                    else
                    {
                        originJob = Service.ClientState.LocalPlayer.ClassJob.GameData;
                    }

                    if (!tools.ShouldLogRole(originJob.Role))
                    {
                        return;
                    }
                    
                    string originJobName = originJob.Name;
                    bool duplicate = Service.PartyList.Count(p => p.ClassJob.GameData.Name== originJobName) > 1;
                    
                    if (!plugin.Configuration.LogUniqueJobs && !duplicate) //job is not duplicate and we don't log single jobs
                    {
                        return;
                    }
                    int[] effects = new int[8];
                    for (var j = 0; j < 8; j++)
                    {
                        ref var actionEffect = ref effectArray[i * 8 + j];
                        if (actionEffect.EffectType == 0)
                        {
                            continue;
                        }
                            
                        effects[j] = (int) actionEffect.EffectType;
                        if (plugin.Configuration.Verbose)
                        {
                            PluginLog.Information("E:" + actionEffect.EffectType);
                        }
                    }

                    bool shouldLog = tools.ShouldLog(effects);
                    if (shouldLog)
                    {
                               
                        action ??= Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
                        gameObject ??= Service.ObjectTable.SearchById((uint)sourceId); 
                        source ??= gameObject?.Name.ToString();
                        
                        string actionName = action!.Name.RawString;
                        
                        SeStringBuilder builder = new SeStringBuilder();
                        
                        if (plugin.Configuration.TextTag)
                        {
                            builder.AddUiForeground(10);
                            builder.AddText("[WDT] ");
                            builder.AddUiForegroundOff();
                        }
                        builder.Append(source + " used " + actionName);
                        
                        Service.ChatGui.Print(builder.Build());
                    }
                   
                 
                }
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Oops!");
            }
        }
        
   
 
        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();

        }
    }
}
