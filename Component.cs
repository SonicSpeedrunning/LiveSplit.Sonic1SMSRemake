using System.Xml;
using System.Windows.Forms;
using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;

namespace LiveSplit.Sonic1SMSRemake
{
    partial class Sonic1SMSRemakeComponent : LogicComponent
    {
        public override string ComponentName => "Sonic 1 SMS Remake - Autosplitter";
        private readonly Settings settings = new Settings();
        private readonly Watchers watchers;
        private readonly TimerModel timer;

        public Sonic1SMSRemakeComponent(LiveSplitState state)
        {
            timer = new TimerModel { CurrentState = state };
            watchers = new Watchers(state);
            timer.InitializeGameTime();
            
            if (timer.CurrentState.CurrentTimingMethod == TimingMethod.RealTime)
            {
                var question = MessageBox.Show("""
                This autosplitter supports Time without Time Bonus (RTA-TB) or in-game time (IGT), depending on the game mode you select.
                Would you like to set the timing method to Game Time?
                """, "LiveSplit - Sonic 1 SMS Remake", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (question == DialogResult.Yes)
                    timer.CurrentState.CurrentTimingMethod = TimingMethod.GameTime;
            }
        }

        public override void Dispose()
        {
            settings.Dispose();
            watchers.Dispose();
        }

        public override XmlNode GetSettings(XmlDocument document) => this.settings.GetSettings(document);

        public override Control GetSettingsControl(LayoutMode mode) => this.settings;

        public override void SetSettings(XmlNode settings) => this.settings.SetSettings(settings);

        public override void Update(IInvalidator invalidator, LiveSplitState state, float width, float height, LayoutMode mode)
        {
            // If LiveSplit is not connected to the game, of course there's no point in going further
            if (!watchers.Init()) return;

            // Main update logic is inside the watcher class in order to avoid exposing unneded stuff to the outside
            watchers.Update();

            if (timer.CurrentState.CurrentPhase == TimerPhase.Running || timer.CurrentState.CurrentPhase == TimerPhase.Paused)
            {
                timer.CurrentState.IsGameTimePaused = IsLoading();
                if (GameTime() != null) timer.CurrentState.SetGameTime(GameTime());
                if (Reset()) timer.Reset();
                else if (Split()) timer.Split();
            }

            if (timer.CurrentState.CurrentPhase == TimerPhase.NotRunning)
            {
                if (Start()) timer.Start();
            }
        }
    }
}
