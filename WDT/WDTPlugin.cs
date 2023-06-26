using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using WDT.Toolbox;
using WDT.Windows;

namespace WDT
{
    public sealed class WDTPlugin : IDalamudPlugin
    {
        public string Name => "Who Did That?";
        private const string CommandName = "/pwdt";

        private DalamudPluginInterface PluginInterface { get; init; }
        private CommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public ActionHook ActionHook { get; }
        public WindowSystem WindowSystem = new("WhoDidThat");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }
        private DebugWindow DebugWindow { get; init; }

        public WDTPlugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] CommandManager commandManager)
        {
            Service.Initialize(pluginInterface);
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            ActionHook = new ActionHook(this);
            
            ConfigWindow = new ConfigWindow(this);
            MainWindow = new MainWindow(this);
            DebugWindow = new DebugWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);
            WindowSystem.AddWindow(DebugWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Type /pwdt to get started."
            });
            

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            ActionHook.Dispose();
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            DebugWindow.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            MainWindow.IsOpen = true;
        }
        
        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
        
        public void DrawDebugUI()
        {
            DebugWindow.IsOpen = true;
        }
    }
}
