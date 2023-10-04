using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
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

    //todo weird bug when enabling "Filter Unique Jobs" and "Players outside your party" - (1 ast in ally raid, no ast anywhere else, still saw notifs)
    internal unsafe bool CheckLog(uint targets, int sourceId, IntPtr sourceCharacter, ActionEffect* effectArray, ulong* effectTrail, bool roleAction, bool targetedAction, uint actionId)
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
            return this.CheckPartyMember(targets, localPlayerId, sourceCharacter, effectArray, effectTrail, roleAction, actionId);
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
        bool roleAction, uint roleActionId)
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
                if (roleAction && tools.twoOrMoreRoleActionUsersPresent(roleActionId)) //if the action is a role action and two or more of that role action user is present
                {
                    if (!plugin.Configuration.ShouldExemptRoleActions)
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
