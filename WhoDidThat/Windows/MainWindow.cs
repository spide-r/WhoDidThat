﻿using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace WhoDidThat.Windows;

public class MainWindow : Window, IDisposable
{
    private WhoDidThatPlugin whoDidThatPlugin;

    public MainWindow(WhoDidThatPlugin whoDidThatPlugin) : base(
        "Who Did that?", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(200, 200),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.whoDidThatPlugin = whoDidThatPlugin;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        if (ImGui.Button("Show Settings"))
        {
            this.whoDidThatPlugin.DrawConfigUI();
        }
        ImGui.Spacing();
        ImGui.Text("Attributions");
        ImGui.BulletText("DeathRecap for critical backend hooking");
        ImGui.BulletText("Mutant Standard for the plugin icon (CC BY-NC-SA) - https://mutant.tech");
        
    }
}
