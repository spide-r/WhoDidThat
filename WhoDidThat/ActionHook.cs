/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
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
                                              "|AN:" + Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId)?
                                                  .Name.RawString);
                    }
                }


                bool shouldLogAction = checks.CheckLog(targets, sourceId, sourceCharacter, effectArray, effectTrail);
                if (shouldLogAction)
                {
                    actionLogger.LogAction(actionId, (uint)sourceId);
                }
            }
            catch (Exception e)
            {
             PluginLog.Error(e, "oops!");
            }
        }
        
   
 
        public void Dispose()
        {
            receiveAbilityEffectHook.Disable();
            receiveAbilityEffectHook.Dispose();
        }
    }
}
