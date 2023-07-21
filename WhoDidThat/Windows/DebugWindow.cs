using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace WhoDidThat.Windows;

public class DebugWindow : Window, IDisposable
{
    private Configuration Configuration;
    public DebugWindow(WhoDidThatPlugin whoDidThatPlugin) : base(
        "WhoDidThat Debug", ImGuiWindowFlags.NoScrollbar |
                                            ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 100),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = whoDidThatPlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var selfLog = this.Configuration.SelfLog;
        var verbose = this.Configuration.Verbose;
        var pcs = this.Configuration.OnlyLogPlayerCharacters;

        if (ImGui.Checkbox("Log Own Abilities", ref selfLog))
        {
            this.Configuration.SelfLog = selfLog;
            this.Configuration.Save();
        }

        if (ImGui.Checkbox("Verbose Logging", ref verbose))
        {
            this.Configuration.Verbose = verbose;
            this.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Only Log Player Characters", ref pcs))
        {
            this.Configuration.OnlyLogPlayerCharacters = pcs;
            this.Configuration.Save();
        }

        
        
    }
}
