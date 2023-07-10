using System.Linq;

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

        if (!plugin.Configuration.LogTank && role == 1)
        {
            return false;
        }

        if (!plugin.Configuration.LogMelee && role == 2)
        {
            return false;
        }

        if (!plugin.Configuration.LogRanged && role == 3)
        {
            return false;
        }
        
        if (!plugin.Configuration.LogHealer && role == 4)
        {
            return false;
        }

        return true;

    }
    
    
    public bool ShouldLog(int[] effectArray) //todo Missed Rescue - see if the effect array has anything besides "miss"
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
