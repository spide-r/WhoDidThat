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
        public bool Shirk { get; set; } = true;
        public bool Provoke { get; set; } = true;
        public bool Interrupt { get; set; } = true;
        public bool MultiTarget { get; set; } = false;
        public bool TargetNpc { get; set; } = false;
        public bool TargetedMit { get; set; } = true;
        public bool TargetedDebuffs { get; set; } = true;
        public bool NoEffectMiss { get; set; } = false;
        public bool TextTag { get; set; } = true;
        public bool CombatTimestamp { get; set; } = false;
        public bool FilterUniqueJobs { get; set; } = true;
        public bool LogOutsideParty { get; set; } = false;

        public bool ShouldFilterRoles { get; set; } = false;
        public bool ShouldExemptRoleActions { get; set; } = true;
        public bool FilterTank { get; set; } = false;
        public bool FilterHealer { get; set; } = false;
        public bool FilterMelee { get; set; } = false;
        public bool FilterRanged { get; set; } = false;
        public bool FilterCasters { get; set; } = false;
        public uint PrefixColor { get; set; } = 10;
        public uint CombatTimerColor { get; set; } = 10;
        public XivChatType ChatType { get; set; } = XivChatType.Debug;
        
        public bool SelfLog { get; set; } = false;
        public bool Verbose { get; set; } = false;
        public bool OnlyLogPlayerCharacters { get; set; } = true; //Battle NPC applied buffs are still found in the battle log so this toggle shouldn't be an issue

        

        [NonSerialized]
        private IDalamudPluginInterface? PluginInterface;

        public void Initialize(IDalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
