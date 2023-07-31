using System;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using WhoDidThat.Toolbox;

namespace WhoDidThat.Windows;

public class ColorPickerWindow : Window, IDisposable
{
    private Configuration Configuration;
    private WhoDidThatPlugin whoDidThatPlugin;
    public ColorPickerWindow(WhoDidThatPlugin whoDidThatPlugin) : base(
        "Prefix Color Picker", ImGuiWindowFlags.NoScrollbar |
                                    ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.whoDidThatPlugin = whoDidThatPlugin;

        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(500, 480),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.SizeCondition = ImGuiCond.Always;

        this.Configuration = whoDidThatPlugin.Configuration;
        
    }

    public override void Draw()
    {
        
        ImGui.Columns(10);
        if (whoDidThatPlugin.UiColors != null)
        {
            foreach (var z in whoDidThatPlugin.UiColors)
            {
                var temp = BitConverter.GetBytes(z.UIForeground);
                var x = (float)temp[3] / 255;
                var y = (float)temp[2] / 255;
                var zz = (float)temp[1] / 255;
                if (x + y + zz == 0)
                {
                    continue;
                }
                if (ImGui.ColorButton(z.RowId.ToString(), new Vector4(
                                          (float)temp[3] / 255,
                                          (float)temp[2] / 255,
                                          (float)temp[1] / 255,
                                          (float)temp[0] / 255)))
                {
                    Configuration.PrefixColor = z.RowId;
                    Configuration.PrefixColorPicker = false;
                    Configuration.Save();
                }

                ImGui.NextColumn();
            }
        }
        ImGui.Columns(1);
        ImGui.Separator();
        ImGui.NewLine();
        if (ImGui.Button("Test Color"))
        {
            SeStringBuilder builder = new SeStringBuilder();
            builder.AddUiForeground((ushort) Configuration.PrefixColor);
            builder.AddText("[WDT] ");
            builder.AddUiForegroundOff();
            
            builder.Append("Player used Test");
                        
            Service.ChatGui.Print(builder.Build());
        }

    }
    public void Dispose() { }

}
