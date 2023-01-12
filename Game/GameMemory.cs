using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiveSplit.ComponentUtil;
using LiveSplit.Model;

namespace LiveSplit.Sonic1SMSRemake
{
    partial class Watchers
    {
        // Game version
        private LiveSplitState state;
        private FakeMemoryWatcherList FakeWatchersList;
        private Dictionary<int, int> Acts = new Dictionary<int, int>()
        {
            { 17, 0 },
            { 18, 1 },
            { 19, 2 },
            { 20, 3 },
            { 21, 4 },
            { 22, 5 },
            { 26, 6 },
            { 27, 7 },
            { 28, 8 },
            { 32, 9 },
            { 33, 10 },
            { 34, 11 },
            { 35, 12 },
            { 36, 13 },
            { 37, 14 },
            { 41, 15 },
            { 42, 16 }, { 43, 16 },
            { 44, 17 },
            { 23, 18 },
            { 24, 19 },
            { 25, 20 },
            { 29, 21 },
            { 30, 22 },
            { 31, 23 },
            { 38, 24 },
            { 39, 25 },
            { 40, 26 }
        };


        // Watchers
        private IntPtr BaseAddress;

        public FakeMemoryWatcher<int> ActID { get; protected set; }
        public FakeMemoryWatcher<TimeSpan> IGT { get; protected set; }
        public FakeMemoryWatcher<GameMode> GameMode { get; protected set; }
        public FakeMemoryWatcher<bool> ActCleared { get; protected set; }
        public MemoryWatcher<int> RoomID { get; protected set; }
        private MemoryWatcher<double> TimeBonus { get; set; }
        private double TimeBonusStartValue { get; set; } = 0d;
        public bool IsInTimeBonus { get; protected set; } = false;
        public TimeSpan AccumulatedIGT { get; protected set; } = TimeSpan.Zero;


        public Watchers(LiveSplitState state)
        {
            this.state = state;

            ActID = new FakeMemoryWatcher<int>(() => Acts.ContainsKey(RoomID.Current) ? Acts[RoomID.Current] : ActID.Current);
            IGT = new FakeMemoryWatcher<TimeSpan>(() => TimeSpan.FromSeconds(game.ReadValue<double>(BaseAddress + 0xFFD80) * 3600 + game.ReadValue<double>(BaseAddress + 0xFFD90) * 60 + game.ReadValue<double>(BaseAddress + 0xFFDA0)));
            GameMode = new FakeMemoryWatcher<GameMode>(() => (GameMode)(int)game.ReadValue<double>(BaseAddress + 0xFFEA0));
            ActCleared = new FakeMemoryWatcher<bool>(() => game.ReadValue<double>(BaseAddress + 0xFFB70) == 1d);

            GameProcess = new ProcessHook("Sonic 1 SMS Remake");
        }

        public void Update()
        {
            WatcherList.UpdateAll(game);
            FakeWatchersList.UpdateAll();

            if (TimeBonus.Old == 0 && TimeBonus.Changed)
                TimeBonusStartValue = TimeBonus.Current;
            else if (TimeBonus.Current == 0)
                TimeBonusStartValue = 0;

            IsInTimeBonus = TimeBonusStartValue != 0 && TimeBonus.Current != TimeBonusStartValue;

            // If the timer is not running (eg. a run has been reset) these variables need to be reset
            if (state.CurrentPhase == TimerPhase.NotRunning && AccumulatedIGT != TimeSpan.Zero) AccumulatedIGT = TimeSpan.Zero;

            // When exiting a stage, or whenever the IGT resets, this will keep track of the time you accumulated so far
            if (IGT.Current < IGT.Old && IGT.Old != TimeSpan.Zero)
                AccumulatedIGT += IGT.Old;
        }

        /// <summary>
        /// This function is essentially equivalent of the init descriptor in script-based autosplitters.
        /// Everything you want to be executed when the game gets hooked needs to be put here.
        /// The main purpose of this function is to perform sigscanning and get memory addresses and offsets
        /// needed by the autosplitter.
        /// </summary>
        private void GetAddresses()
        {
            var Scanner = game.SigScanner();
            
            RoomID = new MemoryWatcher<int>(Scanner.ScanOrThrow(new SigScanTarget(2, "89 35 ???????? 7D 0B") { OnFound = (p, _, addr) => p.ReadPointer(addr) }));

            BaseAddress = game.MemoryPages(true).First(p => (int)p.RegionSize == 0x101000 && p.Type == MemPageType.MEM_PRIVATE && p.Protect == MemPageProtect.PAGE_READWRITE && p.State == MemPageState.MEM_COMMIT && p.AllocationProtect == MemPageProtect.PAGE_READWRITE).BaseAddress;
            TimeBonus = new MemoryWatcher<double>(BaseAddress + 0xFFA50);


            WatcherList = new MemoryWatcherList();
            WatcherList
                .AddRange(GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any())
                .Select(p => p.GetValue(this, null) as MemoryWatcher)
                .Where(p => p != null));

            FakeWatchersList = new();
            FakeWatchersList
                .AddRange(GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => !p.GetIndexParameters().Any())
                .Select(p => p.GetValue(this, null) as FakeMemoryWatcher)
                .Where(p => p != null));
        }
    }
}