using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace WhoDidThat.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;
    private WhoDidThatPlugin whoDidThatPlugin;
    public ConfigWindow(WhoDidThatPlugin whoDidThatPlugin) : base(
        "WhoDidThat Configuration", ImGuiWindowFlags.NoScrollbar |
                                            ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.whoDidThatPlugin = whoDidThatPlugin;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = whoDidThatPlugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        var applyStatusEffect = this.Configuration.StatusEffects;
        var heal = this.Configuration.Healing;
        var buffCleanse = this.Configuration.BuffCleanse;
        var rescue = this.Configuration.RescueKB;
        var textTag = this.Configuration.TextTag;
        var multiTarget = this.Configuration.MultiTarget;
        
        ImGui.TextWrapped("Important! If \"Healing Actions\" is unchecked, Actions that grant a heal *and* an additional effect will not be tracked. " +
                          "This affects Medica II, Regen, E. Diag, Adloquium, etc.");
        ImGui.Spacing();
        if (ImGui.Checkbox("Status Application", ref applyStatusEffect))
        {
            this.Configuration.StatusEffects = applyStatusEffect;
            this.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Healing Actions", ref heal))
        {
            this.Configuration.Healing = heal;
            this.Configuration.Save();
        }

        if (ImGui.Checkbox("Debuff Cleanse", ref buffCleanse))
        {
            this.Configuration.BuffCleanse = buffCleanse;
            this.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Rescue/Knockback", ref rescue))
        {
            this.Configuration.RescueKB = rescue;
            this.Configuration.Save();
        }

        if (ImGui.Checkbox("Multi-target Abilities", ref multiTarget))
        {
            this.Configuration.MultiTarget = multiTarget;
            this.Configuration.Save();
        }

        
        if (ImGui.Checkbox("[WDT] Tag Prefix", ref textTag))
        {
            this.Configuration.TextTag = textTag;
            this.Configuration.Save();
        }

        if (ImGui.Button("Open Debug Menu"))
        {
            this.whoDidThatPlugin.DrawDebugUI();
        }
        
        
        
    }
}
