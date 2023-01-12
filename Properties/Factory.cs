using System;
using System.Reflection;
using LiveSplit.Model;
using LiveSplit.UI.Components;
using LiveSplit.Sonic1SMSRemake;

[assembly: ComponentFactory(typeof(Sonic1SMSRemakeFactory))]

namespace LiveSplit.Sonic1SMSRemake
{
    public class Sonic1SMSRemakeFactory : IComponentFactory
    {
        public string ComponentName => "Sonic 1 SMS Remake - Autosplitter";
        public string Description => "Autosplitter";
        public ComponentCategory Category => ComponentCategory.Control;
        public string UpdateName => ComponentName;
        public string UpdateURL => "https://raw.githubusercontent.com/SonicSpeedrunning/LiveSplit.Sonic1SMSRemake/master/";
        public Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string XMLURL => UpdateURL + "Components/LiveSplit.Sonic1SMSRemake.xml";
        public IComponent Create(LiveSplitState state) => new Sonic1SMSRemakeComponent(state);
    }
}
