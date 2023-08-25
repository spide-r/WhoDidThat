using System;
using Dalamud.Configuration;
using Dalamud.Game.Text;
using Dalamud.Plugin;


namespace WhoDidThat
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;
        public bool Enabled { get; set; } = true;
        public bool StatusEffects { get; set; } = true;
        public bool Healing { get; set; } = false;
        public bool BuffCleanse { get; set; } = true;
        public bool RescueKB { get; set; } = true;
        public bool MultiTarget { get; set; } = false;
        public bool NoEffectMiss { get; set; } = false;
        public bool TextTag { get; set; } = true;
        public bool FilterUniqueJobs { get; set; } = true;
        public bool LogOutsideParty { get; set; } = false;

        public bool ShouldFilterRoles { get; set; } = false;
        public bool ShouldExemptRescueEsuna { get; set; } = true;

        public bool FilterTank { get; set; } = false;
        public bool FilterHealer { get; set; } = false;
        public bool FilterMelee { get; set; } = false;
        public bool FilterRanged { get; set; } = false;
        public bool FilterCasters { get; set; } = false;

        public bool PrefixColorPicker { get; set; } = false;

        public uint PrefixColor { get; set; } = 10;
        public XivChatType ChatType { get; set; } = XivChatType.Debug;
        
        public bool SelfLog { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public bool OnlyLogPlayerCharacters { get; set; } = true; //Battle NPC applied buffs are still found in the battle log so this toggle shouldn't be an issue

        

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
