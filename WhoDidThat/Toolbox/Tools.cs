using System.Collections.Generic;
using System.Linq;
using Lumina.Excel.Sheets;

namespace WhoDidThat.Toolbox;

public class Tools
{
    private readonly WhoDidThatPlugin plugin;
    

    public Tools(WhoDidThatPlugin plugin)
    {
        this.plugin = plugin;
    }


    //tank = 1
    //healer = 2
    //melee = 3
    //phys = 4
    //caster = 5
    
    //have to use party bonus because of square's indie game code
    public bool ShouldLogRole(byte partyBonus)
    {
        if (!plugin.Configuration.ShouldFilterRoles)
        {
            return true;
        }

        if (plugin.Configuration.FilterTank && partyBonus == 1)
        {
            return false;
        }

        if (plugin.Configuration.FilterMelee && partyBonus == 3)
        {
            return false;
        }

        if (plugin.Configuration.FilterRanged && partyBonus == 4)
        {
            return false;
        }
        
        if (plugin.Configuration.FilterHealer && partyBonus == 2)
        {
            return false;
        }
        if (plugin.Configuration.FilterCasters && partyBonus == 5)
        {
            return false;
        }

        return true;

    }

    internal bool IsDuplicate(ClassJob originJob)
    {
        string originJobName = originJob.Name.ToString();
        bool duplicate = Service.PartyList.Count(p =>
        {
            return p.ClassJob.Value.Name == originJobName;
        }) > 1;

        return duplicate;
    }
    
    public static bool twoOrMoreRolePresent(int role)
    {
        bool greaterThan1 = Service.PartyList.Count(p =>
        {
            return p.ClassJob.Value.PartyBonus == role;
        }) > 1;

        return greaterThan1;
    }
    
    internal bool twoOrMoreRoleActionUsersPresent(int roleAction)
    {
        switch (roleAction)
        {
            case (int)ClassJobActions.Addle:
                return twoOrMoreRolePresent(5); //caster
            case (int)ClassJobActions.Feint:
            case (int)ClassJobActions.LegSweep:
                return twoOrMoreRolePresent(3); //melee
            case (int)ClassJobActions.Reprisal:
            case (int)ClassJobActions.Interject:
            case (int)ClassJobActions.LowBlow:
            case (int)ClassJobActions.Provoke:
                return twoOrMoreRolePresent(1); // tank
            case (int)ClassJobActions.Rescue:
            case (int)ClassJobActions.Esuna:
                return twoOrMoreRolePresent(2); //healer
            case (int)ClassJobActions.LegGraze:
            case (int)ClassJobActions.FootGraze:
            case (int)ClassJobActions.Peloton:
            case (int)ClassJobActions.HeadGraze:
                return twoOrMoreRolePresent(4); //phys ranged
            
        }
        return false;
    }


    internal unsafe bool ShouldLogRaise(uint actionId)
    {
        HashSet<uint> actionIds = new HashSet<uint>();
        actionIds.Add(173); //resurrection 
        actionIds.Add(125); //raise
        actionIds.Add(7523); //verraise
        actionIds.Add(3603); //ascend
        actionIds.Add(24287); //egerio
        actionIds.Add(18317); //angel whisper
        
        return actionIds.Contains(actionId) && plugin.Configuration.Resurrections;
    }
    internal unsafe bool ShouldLogEffects(uint actionId, uint targets, ulong* effectTrail, ActionEffect* effectArray, ulong localPlayerId)
    {

        for (var i = 0; i < targets; i++)
        {

            var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
            if (actionTargetId != localPlayerId) 
            {
                continue;
            }

            var effects = getEffects(i, effectArray);
            return ShouldLogEffects(actionId, effects);
        }
        return false;

    }
    
    internal unsafe int[] getEffects(int targetIdx, ActionEffect* effectArray)
    {
        var effects = new int[8];
        for (var j = 0; j < 8; j++)
        {
            ref var actionEffect = ref effectArray[targetIdx * 8 + j];
            if (actionEffect.EffectType == 0)
            {
                continue;
            }
                            
            effects[j] = (int) actionEffect.EffectType;
            if (plugin.Configuration.Verbose)
            {
                Service.PluginLog.Information("Effect:" + actionEffect.EffectType);
            }
        }

        return effects;
    }
    
    
    public bool ShouldLogEffects(uint actionId, int[] effectArray)
    {
        //if the action is a heal, completely ignore all other effects and don't log
        for (var i = 0; i < effectArray.Length; i++)
        {
            Service.PluginLog.Information("Effect: " + effectArray[i]);
        }
        
        if (effectArray.Contains((int) ActionEffectType.Heal) && !plugin.Configuration.Healing) 
        {
            return false;
        }
        
        if (ShouldLogRaise(actionId))
        {
            return true;
        }
        
        if (effectArray.Contains((int) ActionEffectType.Heal) && plugin.Configuration.Healing)
        {
            return true;
        }

        if (effectArray.Contains((int)ActionEffectType.RecoveredFromStatusEffect) && plugin.Configuration.BuffCleanse)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.ApplyStatusEffectTarget) && plugin.Configuration.StatusEffects)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.Knockback) && plugin.Configuration.RescueKB)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.NoEffect) && plugin.Configuration.NoEffectMiss)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.Miss) && plugin.Configuration.NoEffectMiss)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.EnmityChange) && plugin.Configuration.Shirk)
        {
            //GOTCHA: this might case unexpected actions to be logged as some random actions have an enmity change effect 
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.ThreatPosition) && plugin.Configuration.Provoke)
        {
            return true;
        }
        
        if (effectArray.Contains((int)ActionEffectType.Interrupt) && plugin.Configuration.Interrupt)
        {
            return true;
        }
        return false;
        
        
    }
}
