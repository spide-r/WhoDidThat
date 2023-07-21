/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
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

            SignatureHelper.Initialise(this);

            receiveAbilityEffectHook.Enable();
        }
        
        [Signature("40 55 53 57 41 54 41 55 41 56 41 57 48 8D AC 24 60 FF FF FF 48 81 EC A0 01 00 00",
                   DetourName = nameof(ReceiveAbilityEffectDetour))]
        private readonly Hook<ReceiveAbilityDelegate> receiveAbilityEffectHook = null!;

        private unsafe delegate void ReceiveAbilityDelegate(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail);

        /*
         * !! Incoming Action !!
Should we be doing something (check plugin activation)
Who is this coming from? NPC? Party Member? Outside Party Member? Ourself?
Does it target only 1 person or multiple? (Check if we log aoe actions or not)
         */
        private unsafe void ReceiveAbilityEffectDetour(
            int sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader,
            ActionEffect* effectArray, ulong* effectTrail)
        {
            receiveAbilityEffectHook.Original(sourceId, sourceCharacter, pos, effectHeader, effectArray, effectTrail);
            if (!plugin.Configuration.Enabled)
            {
                return;
            }
            
            uint targets = effectHeader->EffectCount;
            uint localPlayerId = Service.ClientState.LocalPlayer!.ObjectId;
            
            var actionId = effectHeader->EffectDisplayType switch
            {
                ActionEffectDisplayType.MountName => 0xD000000 + effectHeader->ActionId,
                ActionEffectDisplayType.ShowItemName => 0x2000000 + effectHeader->ActionId,
                _ => effectHeader->ActionAnimationId
            };
            
            for (var i = 0; i < targets; i++)
            {
                var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                if (actionTargetId != localPlayerId)
                {
                    continue;
                }
                    
                if (plugin.Configuration.Verbose)
                {
                    PluginLog.Information("S:" + sourceId + "|A: " + actionId + "|T: " + actionTargetId + 
                                          "|AN:" + Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId)!.Name.RawString);
                }
            }
            
            if (Service.ClientState.IsPvP)
            {
                if (!Service.ClientState.IsPvPExcludingDen) //player is in wolves den - exclude since dueling is jank
                {
                    return;
                }
            }
            
            if (targets == 0)
            {
                return;
            }

            if (!plugin.Configuration.MultiTarget && targets > 1)
            {
                return;
            }
            
            bool shouldLogAction = checks.CheckLog(targets, sourceId, sourceCharacter, effectArray, effectTrail);
            PluginLog.Information("Result: " + shouldLogAction);
            if (shouldLogAction)
            {
                actionLogger.LogAction(actionId, (uint) sourceId);
            }
        }
        
   
 
        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();

        }
    }
}
