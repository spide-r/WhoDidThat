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
        var singleJob = this.Configuration.LogUniqueJobs;
        var filterRole = this.Configuration.ShouldFilterRoles;
        var logTank = this.Configuration.LogTank;
        var logMelee = this.Configuration.LogMelee;
        var logHealer = this.Configuration.LogHealer;
        var logRanged = this.Configuration.LogRanged;
        //todo maybe just list all jobs in a submenu
        
        ImGui.TextWrapped("Important! If \"Healing Actions\" is unchecked, Actions that grant a heal *and* an additional effect will not be tracked. " +
                          "This affects Medica II, E. Diag, Adloquium, etc.");
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
        
        if (ImGui.Checkbox("Unique Jobs", ref singleJob)) 
        {
            this.Configuration.LogUniqueJobs = singleJob;
            this.Configuration.Save();
        }
        
        ImGui.TextWrapped("Note:" +
                   "\nWhen this checkbox is enabled, jobs that aren't duplicated in the party will still have their actions logged." +
                   "\nFor example: If your party has one Astrologian, their Card actions will show up in your chat.");
        
        
        if (ImGui.Checkbox("Filter Certain Roles", ref filterRole))
        {
            this.Configuration.ShouldFilterRoles = filterRole;
            this.Configuration.Save();
        }

        if (Configuration.ShouldFilterRoles)
        {
            ImGui.Separator();
            if (ImGui.Checkbox("Tanks", ref logTank))
            {
                this.Configuration.LogTank = logTank;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Healers", ref logHealer))
            {
                this.Configuration.LogHealer = logHealer;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Melee", ref logMelee))
            {
                this.Configuration.LogMelee = logMelee;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Ranged", ref logRanged))
            {
                this.Configuration.LogRanged = logRanged;
                this.Configuration.Save();
            }
            ImGui.Separator();
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
