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
        var enabled = this.Configuration.Enabled;
        var applyStatusEffect = this.Configuration.StatusEffects;
        var heal = this.Configuration.Healing;
        var buffCleanse = this.Configuration.BuffCleanse;
        var rescue = this.Configuration.RescueKB;
        var textTag = this.Configuration.TextTag;
        var multiTarget = this.Configuration.MultiTarget;
        var singleJob = this.Configuration.LogUniqueJobs;
        var outsideParty = this.Configuration.LogOutsideParty;
        var filterRole = this.Configuration.ShouldFilterRoles;
        var filterTank = this.Configuration.FilterTank;
        var filterMelee = this.Configuration.FilterMelee;
        var filterHealer = this.Configuration.FilterHealer;
        var filterRanged = this.Configuration.FilterRanged;
        //todo maybe just list all jobs in a submenu
        
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            this.Configuration.Enabled = enabled;
            this.Configuration.Save();
        }
        
        ImGui.Separator();
        
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
        ImGui.Indent();
        ImGui.TextWrapped("Important! If \"Healing Actions\" is unchecked, Actions that grant a heal *and* an additional effect will not be tracked. " +
                          "This affects Medica II, E. Diag, Adloquium, etc.");
        ImGui.Unindent();

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
        
        if (ImGui.Checkbox("Players outside your Party", ref outsideParty)) 
        {
            this.Configuration.LogOutsideParty = outsideParty;
            this.Configuration.Save();
        }
        
        if (ImGui.Checkbox("Unique Jobs", ref singleJob)) 
        {
            this.Configuration.LogUniqueJobs = singleJob;
            this.Configuration.Save();
        }
        ImGui.Indent();
        ImGui.TextWrapped("Note:" +
                   "\nWhen \"Unique Jobs\" is enabled, jobs that aren't duplicated in the party will still have their actions logged." +
                   "\nFor example: If your party has one Astrologian, their Card actions will show up in your chat.");
        ImGui.Unindent();
        
        if (ImGui.Checkbox("Filter Certain Roles", ref filterRole))
        {
            this.Configuration.ShouldFilterRoles = filterRole;
            this.Configuration.Save();
        }

        if (Configuration.ShouldFilterRoles)
        {
            ImGui.Separator();
            if (ImGui.Checkbox("Tanks", ref filterTank))
            {
                this.Configuration.FilterTank = filterTank;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Healers", ref filterHealer))
            {
                this.Configuration.FilterHealer = filterHealer;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Melee", ref filterMelee))
            {
                this.Configuration.FilterMelee = filterMelee;
                this.Configuration.Save();
            }
            if (ImGui.Checkbox("Ranged & Casters", ref filterRanged))
            {
                this.Configuration.FilterRanged = filterRanged;
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
