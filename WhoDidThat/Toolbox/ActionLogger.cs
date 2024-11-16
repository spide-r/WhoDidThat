using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Action = Lumina.Excel.Sheets.Action;

namespace WhoDidThat.Toolbox;

public class ActionLogger
{
    private readonly WhoDidThatPlugin plugin;

    public ActionLogger(WhoDidThatPlugin plugin) {
        this.plugin = plugin;
        
    }

    internal void LogAction(uint actionId, ulong sourceId)
    {
        Action? action = null;
        string? source = null;
        IGameObject? gameObject = null;
        
        action ??= Service.DataManager.Excel.GetSheet<Action>().GetRow(actionId);
        gameObject ??= Service.ObjectTable.SearchById(sourceId); 
        source ??= gameObject?.Name.ToString();
                        
        string? actionName = action.Value.Name.ToString();
                        
        SendActionToChat(source ?? "Unknown Source", actionName);
    }

    private void SendActionToChat(string source, string? actionName)
    {
        //right now this seems fine but in the future messageTag may become mandatory/very useful - change impl of the timer display?
       SeStringBuilder builder = new SeStringBuilder(); 

        if (plugin.Configuration.TextTag)
        {
            builder.AddUiForeground((ushort) plugin.Configuration.PrefixColor); //cast to short because ???
            builder.AddText("[WDT] ");
            builder.AddUiForegroundOff();
        }

        if (plugin.Configuration.CombatTimestamp && plugin.CombatTimer.inCombat())
        {
            builder.AddUiForeground((ushort) plugin.Configuration.CombatTimerColor); //cast to short because ???
            builder.AddText(plugin.CombatTimer.getCurrentCombatTime() + " ");
            builder.AddUiForegroundOff(); 
        }
        builder.Append(source + " used " + actionName);
        
        Service.ChatGui.Print(new XivChatEntry()
        {
            Message = builder.Build(),
            
            Type = plugin.Configuration.ChatType 
        });
    }
}
