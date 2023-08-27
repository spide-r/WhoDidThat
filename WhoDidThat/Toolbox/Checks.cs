using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

namespace WhoDidThat.Toolbox;

public class Checks
{
    private readonly WhoDidThatPlugin plugin;
    private readonly Tools tools;

    public Checks(WhoDidThatPlugin plugin) {
        this.plugin = plugin;
        tools = new Tools(plugin);
        
    }

    internal unsafe bool CheckLog(uint targets, int sourceId, IntPtr sourceCharacter, ActionEffect* effectArray, ulong* effectTrail, bool rescueEsuna)
    {
        if (targets == 0)
        {
            return false;
        }
        
        if (!plugin.Configuration.MultiTarget && targets > 1)
        {
            return false;
        }
        
        GameObject sourceActor = Service.ObjectTable.First(o => o.ObjectId == (uint) sourceId);
        uint localPlayerId = Service.ClientState.LocalPlayer!.ObjectId;
        if (sourceActor.ObjectKind != ObjectKind.Player)
        {
            return this.CheckNpc(targets, localPlayerId, effectArray, effectTrail);
        }

            
        if (sourceId == localPlayerId)
        {
            return this.CheckSelfLog(targets, localPlayerId, effectArray, effectTrail);
        }
            
        bool actorInParty = Service.PartyList.Count(member =>
        {
            return member.ObjectId == sourceId;
        }) > 0;

        if (actorInParty)
        {
            return this.CheckPartyMember(targets, localPlayerId, sourceCharacter, effectArray, effectTrail, rescueEsuna);
        }
        
        return this.CheckPcNotInParty(targets, localPlayerId, effectArray, effectTrail);
    
    }

    internal unsafe bool CheckSelfLog(uint targets, uint localPlayerId, ActionEffect* effectArray, ulong* effectTrail)
    {

            if (plugin.Configuration.SelfLog)
            {
                return tools.ShouldLogEffects(targets, effectTrail, effectArray, localPlayerId);
            }

            return false;
    }

    internal unsafe bool CheckPcNotInParty(uint targets, uint localPlayerId, ActionEffect* effectArray, ulong* effectTrail)
    {


        if (!plugin.Configuration.LogOutsideParty)
        {
            return false;
        }
        
        
        return tools.ShouldLogEffects(targets, effectTrail, effectArray, localPlayerId);

    }

    internal unsafe bool CheckPartyMember(
        uint targets, uint localPlayerId, IntPtr sourceCharacter, ActionEffect* effectArray, ulong* effectTrail,
        bool rescueEsuna)
    {

        ClassJob? originJob = Service.PartyList
                                     .First(p => p.GameObject != null && p.GameObject.Address == sourceCharacter)
                                     .ClassJob.GameData;

        Debug.Assert(originJob != null, nameof(originJob) + " != null");
        if (!tools.ShouldLogRole(originJob.PartyBonus))
        {
            return false;
        }

        bool isUnique = !tools.IsDuplicate(originJob);
        if (isUnique)
        {
            if (plugin.Configuration.FilterUniqueJobs) //Job is unique and we filter unique jobs
            {
                if (rescueEsuna && tools.twoOrMoreHealersPresent()) //if the action is rescue or esuna and two or more healers are present
                {
                    if (!plugin.Configuration.ShouldExemptRescueEsuna)
                    {
                        return false;
                    }
                } else {
                    return false;
                }

                
            }
            
        }
        return tools.ShouldLogEffects(targets, effectTrail, effectArray, localPlayerId);
    }


    internal unsafe bool CheckNpc(uint targets, uint localPlayerId,
                                  ActionEffect* effectArray, ulong* effectTrail)
    {
        if (plugin.Configuration.OnlyLogPlayerCharacters)
        {
            return false;
        }

        return tools.ShouldLogEffects(targets, effectTrail, effectArray, localPlayerId);
    }
}
