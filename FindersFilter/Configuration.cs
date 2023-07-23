using System;
using Dalamud.Configuration;

namespace FindersFilter
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public bool PartyMakeupFilter { get; set; }

        public void Save()
        {
            Dalamud.PluginInterface.SavePluginConfig(this);
        }
    }
}
