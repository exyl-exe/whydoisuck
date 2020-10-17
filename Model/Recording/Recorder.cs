﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Whydoisuck.DataModel;
using Whydoisuck.MemoryReading;

namespace Whydoisuck.DataSaving
{
    class Recorder
    {
        private Session CurrentSession { get; set; }
        private Attempt CurrentAttempt { get; set; }

        public Recorder()
        {
            GameWatcher.OnLevelEntered += CreateNewSession;
            GameWatcher.OnLevelStarted += UpdateCurrentSession;
            GameWatcher.OnLevelExited += PopSaveCurrentSession;
            GameWatcher.OnPlayerSpawns += CreateNewAttempt;
            GameWatcher.OnPlayerDies += PopSaveLosingAttempt;
            GameWatcher.OnPlayerRestarts += PopSaveLosingAttempt;
            GameWatcher.OnPlayerWins += PopSaveWinningAttempt;
        }

        public void StartRecording()
        {
            GameWatcher.StartWatching();
        }

        public void StopRecording()
        {
            GameWatcher.StopWatching();
        }

        //Called when entering a level, ensure a session is created before an attempt needs to be saved
        //However, while its metadata is fully loaded, the level is not
        //Therefore stuff like the level length, the start position etc. is updated when the level is fully loaded and not in this function
        public void CreateNewSession(GDLevelMetadata level)
        {
            CurrentSession = new Session
            {
                Attempts = new List<Attempt>(),
            };
        }

        public void CreateNewAttempt(GameState state)
        {
            CurrentAttempt = new Attempt()
            {
                StartTime = DateTime.Now,
                Number = state.LoadedLevel.AttemptNumber,
            };
        }

        public void PopSaveLosingAttempt(GameState state)
        {
            CreateAttemptIfNotExists(state);
            CurrentAttempt.EndPercent = 100 * state.PlayerObject.XPosition / state.LoadedLevel.PhysicalLength;
            CurrentAttempt.Duration = DateTime.Now - CurrentAttempt.StartTime;
            CurrentSession.AddAttempt(CurrentAttempt);
            CurrentAttempt = null;
        }

        public void PopSaveWinningAttempt(GameState state)
        {
            CreateAttemptIfNotExists(state);
            CurrentAttempt.EndPercent = 100;
            CurrentAttempt.Duration = DateTime.Now - CurrentAttempt.StartTime;
            CurrentSession.AddAttempt(CurrentAttempt);
            CurrentAttempt = null;
        }

        //Update values for the current session, is called when the level is fully loaded
        public void UpdateCurrentSession(GameState state)
        {
            CreateSessionIfNotExists(state);
            CurrentSession.Level = new Level(state);
            CurrentSession.IsCopyRun = state.LoadedLevel.IsTestmode;
            CurrentSession.StartPercent = 100 * state.LoadedLevel.StartPosition / state.LoadedLevel.PhysicalLength;
            CurrentSession.StartTime = DateTime.Now;
        }

        public void PopSaveCurrentSession(GDLevelMetadata level)
        {
            //Don't save if :
            //  -no session were created (= software launched while playing a level, and no attempts have been played before exiting)
            //  -The current level is unknown (= The level was left before it finished loading)
            //  -There are not attempts in the session (= useless data)
            if (CurrentSession == null || CurrentSession.Level == null || CurrentSession.Attempts.Count == 0) return;
            SessionSaver.SaveSession(CurrentSession);
            CurrentSession = null;
        }


        //Creates a session if there is no current session and initialize known values
        private void CreateSessionIfNotExists(GameState state)
        {
            if (CurrentSession != null) return;
            CurrentSession = new Session()
            {
                Attempts = new List<Attempt>(),
                StartTime = DateTime.MinValue,
            };

            if (state == null || state.LevelMetadata == null || state.LoadedLevel == null) return;
            CurrentSession.Level = new Level(state);
            CurrentSession.IsCopyRun = state.LoadedLevel.IsTestmode;
            CurrentSession.StartPercent = 100 * state.LoadedLevel.StartPosition / state.LoadedLevel.PhysicalLength;
        }

        //Creates an attempt if there is no current attempt and initialize known values
        private void CreateAttemptIfNotExists(GameState state)
        {
            if (CurrentAttempt != null) return;
            CreateSessionIfNotExists(state);
            CurrentAttempt = new Attempt()
            {
                StartTime = DateTime.MinValue,
                Number = state.LoadedLevel.AttemptNumber
            };

            if (state.LoadedLevel == null) return;
            CurrentAttempt.Number = state.LoadedLevel.AttemptNumber;
        }
    }
}