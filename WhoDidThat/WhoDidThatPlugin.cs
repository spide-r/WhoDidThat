using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using WhoDidThat.Timer;
using WhoDidThat.Toolbox;
using WhoDidThat.Windows;

namespace WhoDidThat
{
    //todo: Granular filtering
    public sealed class WhoDidThatPlugin : IDalamudPlugin
    {
        public string Name => "Who Did That?";
        private const string CommandName = "/pwdt";
        private const string CommandConfigName = "/pwdtc";

        private IDalamudPluginInterface PluginInterface { get; init; }
        public Configuration Configuration { get; init; }
        public ActionHook ActionHook { get; }
        public WindowSystem WindowSystem = new("WhoDidThat");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        private DebugWindow DebugWindow { get; init; }
        private ColorPickerWindow ColorPickerWindow { get; init; }
        private TimerColorPickerWindow TimerColorPickerWindow { get; init; }
        public CombatTimer CombatTimer { get; init; }
        
        public ExcelSheet<UIColor>? UiColors { get; init; }

        public WhoDidThatPlugin(
            IDalamudPluginInterface pluginInterface)
        {
            Service.Initialize(pluginInterface);

            UiColors = Service.DataManager.Excel.GetSheet<UIColor>();
            this.PluginInterface = pluginInterface;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            ActionHook = new ActionHook(this);
            
            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);
            DebugWindow = new DebugWindow(this);
            ColorPickerWindow = new ColorPickerWindow(this, Service.DataManager.Excel.GetSheet<UIColor>());
            TimerColorPickerWindow = new TimerColorPickerWindow(this, Service.DataManager.Excel.GetSheet<UIColor>());

            CombatTimer = new CombatTimer(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(DebugWindow);
            WindowSystem.AddWindow(ColorPickerWindow);
            WindowSystem.AddWindow(TimerColorPickerWindow);

            Service.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Type /pwdt to get started."
            });
            
            Service.CommandManager.AddHandler(CommandConfigName, new CommandInfo(OnConfigCommand)
            {
                HelpMessage = "Type /pwdtc for the plugin config."
            });
            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
            Service.Framework.Update += CombatTimer.onUpdateTimer;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            ActionHook.Dispose();
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            DebugWindow.Dispose();
            ColorPickerWindow.Dispose();
            
            Service.CommandManager.RemoveHandler(CommandName);
            Service.CommandManager.RemoveHandler(CommandConfigName);
        }

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = true;
        }
        
        private void OnConfigCommand(string command, string args)
        {
            DrawConfigUI();
        }
        
        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
        public void DrawColorPickerUI()
        {
            ColorPickerWindow.IsOpen = true;
        }
        
        public void DrawTimerColorPickerUI()
        {
            TimerColorPickerWindow.IsOpen = true;
        }
        public void DrawDebugUI()
        {
            DebugWindow.IsOpen = true;
        }
    }
}
