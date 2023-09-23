/*
 * Main Structure attributed to Kouzukii/ffxiv-deathrecap
 * https://github.com/Kouzukii/ffxiv-deathrecap/blob/master/Events/CombatEventCapture.cs
 */

using System;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
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

                    /*
                     Role Actions:
                        addle: 7560
                        feint: 7549
                        rep: 7535
                        leg graze: 7554
                        head graze: 7551
                        low blow 7540
                        interject: 7538
                        leg sweep: 7863
                        rescue: 7571
                        esuna: 7568
                    */

                    /*
                     Targeted Actions:
                        mage ballad: 114
                        wanderers minne: 3559
                        armys paeon: 116
                        mug: 2248
                        dismantle: 2887
                        chain: 7436
                    */
                    var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
                    if (actionTargetId != localPlayerId)
                    {
                        continue;
                    }

                    if (plugin.Configuration.Verbose)
                    {
                        Service.PluginLog.Information("S:" + sourceId + "|A: " + actionId + "|T: " + actionTargetId +
                                              "|AN:" + Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId)?
                                                  .Name.RawString);
                    }
                }

                int[] roleActions = new[] { 7571, 7568, 7560, 7549, 7535, 7554, 7551, 7540, 7538, 7863 };
                bool roleAction = roleActions.Contains<int>((int) actionId);

                int[] targetedActions = new[] {114,3359,116,2248,2887,7436 };
                bool targetedAction = targetedActions.Contains<int>( (int) actionId);


                bool shouldLogAction = checks.CheckLog(targets, sourceId, sourceCharacter, effectArray, effectTrail, roleAction, targetedAction, actionId);
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
