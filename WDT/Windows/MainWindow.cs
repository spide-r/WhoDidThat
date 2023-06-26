using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace WDT.Windows;

public class MainWindow : Window, IDisposable
{
    private WDTPlugin wdtPlugin;

    public MainWindow(WDTPlugin wdtPlugin) : base(
        "Who Did that?", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.wdtPlugin = wdtPlugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        
        ImGui.Text("Attributions");
        ImGui.BulletText("DeathRecap for critical backend hooking");
        ImGui.BulletText("Mutant Standard for the plugin icon (CC BY-NC-SA) - https://mutant.tech");
        ImGui.Spacing();
        if (ImGui.Button("Show Settings"))
        {
            this.wdtPlugin.DrawConfigUI();
        }
    }
}
