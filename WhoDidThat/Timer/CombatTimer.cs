﻿using System;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using WhoDidThat.Toolbox;
using Framework = Dalamud.Game.Framework;

namespace WhoDidThat.Timer;
// Thanks to EngageTimer for general structure
// https://github.com/xorus/EngageTimer/blob/main/Status/CombatStopwatch.cs
public class CombatTimer
{
    private ClientState clientState;
    private Condition condition;
    private DateTime startTime;
    
    public CombatTimer(WhoDidThatPlugin plugin)
    {
        clientState = Service.ClientState;
        condition = Service.Condition;
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
        double seconds = Math.Round(ts.TotalSeconds, 0);
        return "[" + minutes + ":" + seconds.ToString().PadLeft(2, '0') + "]";
    }

    public void onUpdateTimer(Framework framework)
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
