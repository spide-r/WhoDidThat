using System.Diagnostics;
using System.Linq;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

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
        string originJobName = originJob.Name;
        bool duplicate = Service.PartyList.Count(p =>
        {
            Debug.Assert(p.ClassJob.GameData != null, "p.ClassJob.GameData != null");
            return p.ClassJob.GameData.Name == originJobName;
        }) > 1;

        return duplicate;
    }
    
    internal bool twoOrMoreRolePresent(int role)
    {
        bool greaterThan1 = Service.PartyList.Count(p =>
        {
            Debug.Assert(p.ClassJob.GameData != null, "p.ClassJob.GameData != null");
            return p.ClassJob.GameData.PartyBonus == role;
        }) > 1;

        return greaterThan1;
    }
    
    internal bool twoOrMoreRoleActionUsersPresent(uint roleAction)
    {
        switch (roleAction)
        {
            case 7560: //addle
                return twoOrMoreRolePresent(5);
            case 7549: //feint
            case 7863: //leg sweep
                return twoOrMoreRolePresent(3);
            case 7535: //rep
            case 7538: //interject
            case 7540: //low blow
                return twoOrMoreRolePresent(1);
            case 7571: //rescue
            case 7568: //esuna
                return twoOrMoreRolePresent(2);
            case 7554: //leg graze
            case 7553: //foot graze
            case 7557: //peloton
            case 7551: //head graze
                return twoOrMoreRolePresent(4);
            
            
        }
        return false;
    }


    internal unsafe bool ShouldLogEffects(uint targets, ulong* effectTrail, ActionEffect* effectArray, uint localPlayerId)
    {
        int[] effects;
        for (var i = 0; i < targets; i++)
        {
            var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
            if (actionTargetId != localPlayerId) //todo might need to change this when checking targeted mit and actions
            {
                continue;
            }
            
            effects = new int[8];
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
                    Service.PluginLog.Information("E:" + actionEffect.EffectType);
                }
            }
            return ShouldLogEffects(effects);
        }
        return false;

    }
    
    public bool ShouldLogEffects(int[] effectArray)
    {
        //if the action is a heal, completely ignore all other effects and don't log
        if (effectArray.Contains((int) ActionEffectType.Heal) && !plugin.Configuration.Healing) 
        {
            return false;
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
        
        return false;
        
        
    }
}
