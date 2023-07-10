using System;
using Dalamud.Configuration;
using Dalamud.Plugin;

namespace WhoDidThat
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool StatusEffects { get; set; } = true;
        public bool Healing { get; set; } = false;
        public bool BuffCleanse { get; set; } = true;
        public bool RescueKB { get; set; } = true;
        public bool MultiTarget { get; set; } = false;
        public bool TextTag { get; set; } = true;
        public bool LogUniqueJobs { get; set; } = true;

        public bool ShouldFilterRoles { get; set; } = false;

        public bool LogTank { get; set; } = true;
        public bool LogHealer { get; set; } = true;
        public bool LogMelee { get; set; } = true;
        public bool LogRanged { get; set; } = true;
        
        public bool SelfLog { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public bool IgnoreParty { get; set; } = false;

        

        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
