using System;
using System.Diagnostics;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Lumina.Excel.Sheets;

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
    internal unsafe bool CheckLog(uint targets, ulong sourceId, IntPtr sourceCharacter, ActionEffect* effectArray, ulong* effectTrail, bool roleAction, uint actionId)
    {
        if (targets == 0)
        {
            return false;
        }
        
        if (!plugin.Configuration.MultiTarget && targets > 1)
        {
            return false;
        }
        

        IGameObject sourceActor = Service.ObjectTable.First(o => o.GameObjectId == (uint) sourceId);
        ulong localPlayerId = Service.ClientState.LocalPlayer!.GameObjectId;
        if (sourceActor.ObjectKind != ObjectKind.Player)
        {
            return this.CheckNpc(actionId, targets, localPlayerId, effectArray, effectTrail);
        }


        if (sourceId == localPlayerId)
        {
            return this.CheckSelfLog(actionId, targets, localPlayerId, effectArray, effectTrail);
        }
            
        bool actorInParty = Service.PartyList.Count(member =>
        {
            return member.ObjectId == sourceId;
        }) > 0;
        
        if (actorInParty)
        {

            return this.CheckPartyMember(targets, actionId, sourceCharacter, effectArray, effectTrail, localPlayerId);
        }
        
        
        return this.CheckPcNotInParty(targets, actionId, localPlayerId, effectArray, effectTrail);
    }


    internal unsafe bool CheckLogNPCTarget(ulong sourceId, ActionEffect* effectArray, uint actionId, int[] mitigationNpcTarget, int[] debuffActionsWithNpcTarget)
    {

                        if ((Service.ClientState.LocalPlayer.StatusFlags & StatusFlags.InCombat) == 0)
                        {
                            return false;
                        }
                        
                        if (!plugin.Configuration.TargetNpc)
                        {

                            return false;
                        }

                        if (!plugin.Configuration.TargetedMit && mitigationNpcTarget.Contains((int)actionId)) 
                        {

                            return false;   
                        }
                        
                        if (!plugin.Configuration.TargetedDebuffs && debuffActionsWithNpcTarget.Contains((int)actionId)) 
                        {

                            return false;   
                        }


                        if (!plugin.Configuration.Provoke && actionId == (int)ClassJobActions.Provoke)
                        {
                            return false;
                        }
                        
                        IPlayerCharacter? player = Service.ObjectTable.SearchById(sourceId) as IPlayerCharacter;
                        
                        if (!ShouldLogEvenIfUnique(player.ClassJob.Value, actionId))
                        {
                            return false;
                        }

                        if (!tools.ShouldLogRole(player.ClassJob.Value.PartyBonus))
                        {
                            return false;
                        }
                        bool isInParty = Service.PartyList.Any();
                        bool actorInParty = Service.PartyList.Count(member =>
                        {
                            return member.ObjectId == sourceId;
                        }) > 0;
                        if (isInParty)
                        {

                            if (!plugin.Configuration.LogOutsideParty && !actorInParty)
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (!plugin.Configuration.SelfLog)
                            {
                                return false;
                            }
                        }
                        
                        ulong localPlayerId = Service.ClientState.LocalPlayer!.GameObjectId;
                        if (sourceId == localPlayerId && !plugin.Configuration.SelfLog)
                        {
                            return false;
                        }
                        
                        if (tools.ShouldLogEffects(actionId, tools.getEffects(0, effectArray)))
                        {
                            return true;
                        }

                        return false;
    }

    internal unsafe bool CheckSelfLog(uint actionId, uint targets, ulong localPlayerId, ActionEffect* effectArray, ulong* effectTrail)
    {

            if (plugin.Configuration.SelfLog)
            {
                return tools.ShouldLogEffects(actionId, targets, effectTrail, effectArray, localPlayerId);
            }

            return false;
    }

    internal unsafe bool CheckPcNotInParty(uint targets, uint actionId, ulong localPlayerId, ActionEffect* effectArray, ulong* effectTrail)
    {


        if (!plugin.Configuration.LogOutsideParty)
        {
            return false;
        }
        
        
        return tools.ShouldLogEffects(actionId, targets, effectTrail, effectArray, localPlayerId);

    }

    internal unsafe bool CheckPartyMember(
        uint targets, uint actionId, IntPtr sourceCharacter, ActionEffect* effectArray, ulong* effectTrail, ulong localPlayerId)
    {

        ClassJob? originJob = Service.PartyList
                                     .First(p => p.GameObject != null && p.GameObject.Address == sourceCharacter)
                                     .ClassJob.Value;

        Debug.Assert(originJob != null, nameof(originJob) + " != null");
        if (!tools.ShouldLogRole(originJob.Value.PartyBonus))
        {
            return false;
        }

        bool shouldLogUnique = ShouldLogEvenIfUnique(originJob.Value, actionId);
        
        if (shouldLogUnique)
        {
            return tools.ShouldLogEffects(actionId, targets, effectTrail, effectArray, localPlayerId);
        }

        return false;
    }
    
    

    public bool ShouldLogEvenIfUnique(ClassJob originJob, uint actionId)
    {
        bool isUnique = !tools.IsDuplicate(originJob);
        if (isUnique)
        {
            if (plugin.Configuration.FilterUniqueJobs) //Job is unique and we filter unique jobs
            {
                if (tools.twoOrMoreRoleActionUsersPresent((int)actionId)) //if the action is a role action and two or more of that role action user is present
                {
                    if (!plugin.Configuration.ShouldExemptRoleActions) //if we shouldn't exempt role actions from this filtration, dont even bother tracking effects
                    {
                        return false;
                    }
                } else {
                    return false;
                }

                
            }
            
        }
        
        return true; //log if its not unique

    }


    internal unsafe bool CheckNpc(uint actionId, uint targets, ulong localPlayerId,
                                  ActionEffect* effectArray, ulong* effectTrail)
    {
        if (plugin.Configuration.OnlyLogPlayerCharacters)
        {
            return false;
        }

        return tools.ShouldLogEffects(actionId, targets, effectTrail, effectArray, localPlayerId);
    }
}
