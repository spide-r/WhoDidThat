using System;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using WhoDidThat.Toolbox;

namespace WhoDidThat.Timer;
// Thanks to EngageTimer for general structure
// https://github.com/xorus/EngageTimer/blob/main/Status/CombatStopwatch.cs
public class CombatTimer
{
    private DateTime startTime;
    
    public CombatTimer(WhoDidThatPlugin plugin)
    {
        Service.Condition.ConditionChange += onConditionChange;
    }

    public bool inCombat()
    {
        return startTime != DateTime.UnixEpoch;
    }

    public String getCurrentCombatTime()
    {

        if (startTime.Equals(DateTime.UnixEpoch)) //not in combat
        {
            return "[00:00]";
        }
        
        DateTime now = DateTime.Now;
        TimeSpan ts = now.Subtract(startTime);
        double minutes = Math.Round(ts.TotalMinutes, 0);
        double seconds = Math.Round((double)ts.Seconds, 0);
        return "[" + minutes + ":" + seconds.ToString().PadLeft(2, '0') + "]";
    }
    /*
 * [12:54 p.m.][WDT] [1:53] xxxxx used Leg Sweep
[12:54 p.m.][WDT] [1:01] xxxxx used Low Blow
todo combat timer is screwey when not entering in at the same time

 */

    public void onUpdateTimer(IFramework framework)
    {
        bool inCombat = Service.Condition[ConditionFlag.InCombat];
        if (!inCombat)
        {
            foreach (var partyMember in Service.PartyList)
            {
                if (partyMember.GameObject is Character character)
                {
                    if ((character.StatusFlags & StatusFlags.InCombat) != 0)
                    {
                        inCombat = true;
                        break;
                    }
                }
            }
        }

        if (inCombat)
        {
            if (startTime == DateTime.UnixEpoch)
            {
                startTime = DateTime.Now;
            }
        }
        else
        {
            startTime = DateTime.UnixEpoch;
        }
    }

    private void onConditionChange(ConditionFlag flag, bool value)
    {
        
    }
}
