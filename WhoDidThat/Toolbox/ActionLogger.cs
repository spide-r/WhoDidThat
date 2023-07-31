using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel.GeneratedSheets;

namespace WhoDidThat.Toolbox;

public class ActionLogger
{
    private readonly WhoDidThatPlugin plugin;

    public ActionLogger(WhoDidThatPlugin plugin) {
        this.plugin = plugin;
        
    }

    internal void LogAction(uint actionId, uint sourceId)
    {
        Action? action = null;
        string? source = null;
        GameObject? gameObject = null;
        
        action ??= Service.DataManager.Excel.GetSheet<Action>()?.GetRow(actionId);
        gameObject ??= Service.ObjectTable.SearchById(sourceId); 
        source ??= gameObject?.Name.ToString();
                        
        string actionName = action!.Name.RawString;
                        
        SeStringBuilder builder = new SeStringBuilder();
                        
        if (plugin.Configuration.TextTag)
        {
            builder.AddUiForeground((ushort) plugin.Configuration.PrefixColor); //cast to short because ???
            builder.AddText("[WDT] ");
            builder.AddUiForegroundOff();
        }
        builder.Append(source + " used " + actionName);
                        
        Service.ChatGui.Print(builder.Build());
    }
}
