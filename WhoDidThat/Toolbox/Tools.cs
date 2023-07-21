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
    //melee = 2
    //ranged = 3
    //healer = 4
    public bool ShouldLogRole(byte role)
    {
        if (!plugin.Configuration.ShouldFilterRoles)
        {
            return true;
        }

        if (plugin.Configuration.FilterTank && role == 1)
        {
            return false;
        }

        if (plugin.Configuration.FilterMelee && role == 2)
        {
            return false;
        }

        if (plugin.Configuration.FilterRanged && role == 3)
        {
            return false;
        }
        
        if (plugin.Configuration.FilterHealer && role == 4)
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


    internal unsafe bool ShouldLogEffects(uint targets, ulong* effectTrail, ActionEffect* effectArray, uint localPlayerId)
    {
        int[] effects;
        for (var i = 0; i < targets; i++)
        {
            var actionTargetId = (uint)(effectTrail[i] & uint.MaxValue);
            if (actionTargetId != localPlayerId)
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
                    PluginLog.Information("E:" + actionEffect.EffectType);
                }
            }
            return ShouldLogEffects(effects);
        }
        return false;

    }
    
    public bool ShouldLogEffects(int[] effectArray) //todo Missed Rescue - see if the effect array has anything besides "miss"
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
        return false;
        
        
    }
}
