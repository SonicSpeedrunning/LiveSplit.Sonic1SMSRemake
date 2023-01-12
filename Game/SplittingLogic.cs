using System;

namespace LiveSplit.Sonic1SMSRemake
{
    partial class Sonic1SMSRemakeComponent
    {
        private bool Start() => settings.Start && watchers.RoomID.Current == 6 && watchers.RoomID.Old == 3;

        private bool Split() => watchers.ActID.Current == 17
                ? watchers.RoomID.Changed && watchers.RoomID.Current == 14 && settings["c" + watchers.ActID.Current.ToString()]
                : watchers.ActCleared.Old && !watchers.ActCleared.Current && settings["c" + watchers.ActID.Current.ToString()];

        bool Reset() => settings.Reset && (watchers.RoomID.Current == 3 || watchers.RoomID.Current == 2) && watchers.RoomID.Changed;

        bool IsLoading() => watchers.GameMode.Current == GameMode.TimeAttack || watchers.IsInTimeBonus;

        private TimeSpan? GameTime() => watchers.GameMode.Current == GameMode.TimeAttack ? watchers.AccumulatedIGT + watchers.IGT.Current : null;
    }
}