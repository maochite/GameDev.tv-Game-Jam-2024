using System;
using UnityEngine;
using System.Collections.Generic;
using NaughtyAttributes;
using Unit.Entities;

namespace WaveSpawns
{
    [Serializable]
    public class Level
    {
        [SerializeField, MinValue(0), MaxValue(60)] public int startDelaySecs;
        [Space(5)]
        [SerializeField, ReorderableList] public List<Wave> waves;
    }

    [Serializable]
    public class Wave
    {
        [SerializeField, AllowNesting] public bool startAtGameTime;
        [ShowIf("startAtGameTime"), SerializeField, AllowNesting] public TimeManager.GameTime gameTime;
        [Space(5)]
        [SerializeField, MinValue(0), MaxValue(30), AllowNesting] public int startDelaySecs;
        [Space(5)]
        [SerializeField, AllowNesting] public bool endWaveOnPercentKilled;
        [ShowIf("endWaveOnPercentKilled"), SerializeField, MinValue(25f), MaxValue(100f), AllowNesting] public float percentKillsRequired;
        [SerializeField, AllowNesting] public bool endWaveOnTimeLimit;
        [ShowIf("endWaveOnTimeLimit"), SerializeField, MinValue(1f), MaxValue(180f), AllowNesting] public float timeLimitSecs;
        [SerializeField, AllowNesting] public bool despawnAllEnemiesOnWaveEnd;
        [Space(5)]
        [SerializeField, ReorderableList] public List<Group> groups;
    }

    [Serializable]
    public class Group
    {
        public enum SpawnFormation
        {
            Scatter,
            Circle,
            Rectangle,
        }
        [SerializeReference] public EnemySO enemy;
        [SerializeField, MinValue(1), MaxValue(50), AllowNesting] public int spawnCount;
        [Space(5)]
        [SerializeField] public float spawnDelaySecs;
        [Space(5)]
        public SpawnFormation spawnType;
        [ShowIf("spawnType", SpawnFormation.Scatter), SerializeField, AllowNesting] public RandomWaveSpawn scatterWaveSpawn = new();
        [ShowIf("spawnType", SpawnFormation.Circle), SerializeField, AllowNesting] public OscillatingWaveSpawn circleWaveSpawn = new();
        [ShowIf("spawnType", SpawnFormation.Rectangle), SerializeField, AllowNesting] public RectangleWaveSpawn rectangleWaveSpawn = new();
    }

    public class EnemyWaveHandler : MonoBehaviour
    {
        enum GameState
        {
            PreGame,
            StartLevel,
            StartWave,
            WaveInProgress,
            GroupSpawnDelay,
            PostGame
        }

        [SerializeField, ReorderableList] private List<Level> Levels;
        [Space(5)]
        [SerializeField, ReadOnly] private GameState CurrentState = GameState.PreGame;
        [SerializeField, ReadOnly] private int CurrentLevel = 0;
        [SerializeField, ReadOnly] private int CurrentWave = 0;
        [SerializeField, ReadOnly] private int CurrentGroup = 0;
        [SerializeField, ReadOnly] private int CurrentWaveTotalEnemies = 0;
        [SerializeField, ReadOnly] private int CurrentWaveEnemiesSpawned = 0;
        [SerializeField, ReadOnly] private int CurrentWaveEnemiesKilled = 0;
        [SerializeField, ReadOnly] private int CurrentWaveEnemiesAlive = 0;
        [SerializeField, ReadOnly] private uint LevelStartTick = 0;
        [SerializeField, ReadOnly] private uint WaveStartTick = 0;
        [SerializeField, ReadOnly] private uint LastGroupSpawnTick = 0;
        [Space(5)]
        [SerializeField] private bool DebugLogging = true;

        // For debug
        private GameState LastState = GameState.PreGame;

        private static EnemyWaveHandler _instance;
        public static EnemyWaveHandler Instance
        {
            get => _instance;
            set => _instance = value;
        }

        // May be null, check before use
        private List<Enemy> enemies;

        public void Start()
        {
            enemies = new();
            Instance = this;
        }

        public void OnEnable()
        {
            TimeManager.Instance.OnTick += TimeManager_OnTick;
        }

        public void OnDisable()
        {
            TimeManager.Instance.OnTick -= TimeManager_OnTick;
        }

        public void EnemyKilled()
        {
            CurrentWaveEnemiesKilled++;
            CurrentWaveEnemiesAlive--;
        }

        public void StartGame()
        {
            // Validate levels, waves, groups
            if (Levels.Count == 0)
            {
                Debug.LogError("EnemyWaveHandler::StartGame : There are no levels in the list!");
                return;
            }
            for (int i = 0; i<Levels.Count; i++)
            {
                if (Levels[i].waves.Count == 0)
                {
                    Debug.LogErrorFormat("EnemyWaveHandler::StartGame : Levels[{0}] has no waves in the list!", i);
                    return;
                }

                for (int j = 0; j<Levels[i].waves.Count; j++)
                {
                    if (!Levels[i].waves[j].endWaveOnPercentKilled && !Levels[i].waves[j].endWaveOnTimeLimit)
                    {
                        Debug.LogErrorFormat("EnemyWaveHandler::StartGame : Levels[{0}].waves[{1}] has no end condition!", i, j);
                        return;
                    }
                    if (Levels[i].waves[j].groups.Count == 0)
                    {
                        Debug.LogErrorFormat("EnemyWaveHandler::StartGame : Levels[{0}].waves[{1}] has no groups in the list!", i, j);
                        return;
                    }
                }
            }

            if (CurrentState == GameState.PreGame)
            {
                CurrentLevel = 0;
                CurrentWave = 0;
                CurrentGroup = 0;
                LevelStartTick = TimeManager.Instance.GetCurrentTick();
                CurrentState = GameState.StartLevel;
                CurrentWaveTotalEnemies = 0;
                CurrentWaveEnemiesSpawned = 0;
                CurrentWaveEnemiesKilled = 0;
                CurrentWaveEnemiesAlive = 0;
            }
            else
            {
                Debug.LogWarningFormat("EnemyWaveHandler::StartGame : Tried to start game but CurrentState was {0}", CurrentState);
            }
        }

        private void DebugLogStateChange()
        {
            if (LastState == CurrentState)
            {
                return;
            }

            if (!DebugLogging)
            {
                return;
            }

            LastState = CurrentState;

            switch (CurrentState)
            {
                case GameState.PreGame:
                    Debug.LogFormat("PreGame");
                    break;
                case GameState.StartLevel:
                    Level level = Levels[CurrentLevel];
                    if (level.startDelaySecs > 0)
                    {
                        Debug.LogFormat("StartLevel: Waiting {0} secs to start level {1}", level.startDelaySecs, CurrentLevel+1);
                    }
                    else
                    {
                        Debug.LogFormat("StartLevel: Starting level {0}", CurrentLevel+1);
                    }
                    break;
                case GameState.StartWave:
                    Wave wave = Levels[CurrentLevel].waves[CurrentWave];
                    if (wave.startDelaySecs > 0)
                    {
                        Debug.LogFormat("StartWave: Waiting {0} secs to start level {1} wave {2}", wave.startDelaySecs, CurrentLevel+1, CurrentWave+1);
                    }
                    else
                    {
                        Debug.LogFormat("StartWave: Starting level {0} wave {1}", CurrentLevel+1, CurrentWave+1);
                    }
                    break;
                case GameState.WaveInProgress:
                    Debug.LogFormat("WaveInProgress: wave {0}", CurrentWave+1);
                    break;
                case GameState.PostGame:
                    Debug.LogFormat("PostGame");
                    break;
            }
        }

        private bool WaveIsCompleted()
        {
            Wave wave = Levels[CurrentLevel].waves[CurrentWave];

            if (wave.endWaveOnTimeLimit && TimeManager.Instance.TimePassed(WaveStartTick) > wave.timeLimitSecs)
            {
                return true;
            }

            if (wave.endWaveOnPercentKilled && CurrentWaveEnemiesKilled >= (wave.percentKillsRequired / 100) * CurrentWaveTotalEnemies)
            {
                return true;
            }

            return false;
        }

        private void HandleWaveEnd()
        {
            if (Levels[CurrentLevel].waves[CurrentWave].despawnAllEnemiesOnWaveEnd)
            {
                DespawnRemainingEnemies();
            }

            // Reset group, mark this wave as completed
            CurrentGroup = 0;
            CurrentWave++;

            if (CurrentWave < Levels[CurrentLevel].waves.Count)
            {
                // Move to the next wave
                WaveStartTick = TimeManager.Instance.GetCurrentTick();
                CurrentState = GameState.StartWave;
            }
            else
            {
                // Current level has been completed
                DespawnRemainingEnemies();

                // Reset group and wave, mark this level as completed
                CurrentGroup = 0;
                CurrentWave = 0;
                CurrentLevel++;

                if (CurrentLevel < Levels.Count)
                {
                    // Move to the next level
                    LevelStartTick = TimeManager.Instance.GetCurrentTick();
                    CurrentState = GameState.StartLevel;
                }
                else
                {
                    // All levels have been completed
                    CurrentLevel = 0;
                    CurrentState = GameState.PostGame;
                    return;
                }
            }

            UpdateWaveEnemyTracking(); // Update stats used for checking wave completion
        }

        private void UpdateWaveEnemyTracking()
        {
            int totalEnemies = 0;
            foreach (Wave wave in Levels[CurrentLevel].waves)
            {
                foreach (Group group in wave.groups)
                {
                    totalEnemies += group.spawnCount;
                }
            }
            CurrentWaveTotalEnemies = totalEnemies;
            CurrentWaveEnemiesSpawned = 0;
            CurrentWaveEnemiesKilled = 0;
            CurrentWaveEnemiesAlive = 0;
        }

        private void TimeManager_OnTick()
        {
            DebugLogStateChange();

            switch (CurrentState)
            {
                case GameState.PreGame:
                    break;
                case GameState.StartLevel:
                    if (TimeManager.Instance.TimePassed(LevelStartTick) >= Levels[CurrentLevel].startDelaySecs)
                    {
                        WaveStartTick = TimeManager.Instance.GetCurrentTick();
                        CurrentState = GameState.StartWave;
                    }
                    break;
                case GameState.StartWave:
                    if (Levels[CurrentLevel].waves[CurrentWave].startAtGameTime)
                    {
                        TimeManager.GameTime time = TimeManager.Instance.GetGameTime();
                        if (time.day < Levels[CurrentLevel].waves[CurrentWave].gameTime.day)
                        {
                            break;
                        }
                        if (time.hour < Levels[CurrentLevel].waves[CurrentWave].gameTime.hour)
                        {
                            break;
                        }
                        if (time.minute < Levels[CurrentLevel].waves[CurrentWave].gameTime.minute)
                        {
                            break;
                        }
                        WaveStartTick = TimeManager.Instance.GetCurrentTick();
                    }
                    if (TimeManager.Instance.TimePassed(WaveStartTick) >= Levels[CurrentLevel].waves[CurrentWave].startDelaySecs)
                    {
                        LastGroupSpawnTick = TimeManager.Instance.GetCurrentTick();
                        UpdateWaveEnemyTracking(); // Update stats used for checking wave completion
                        CurrentState = GameState.WaveInProgress;
                    }
                    break;
                case GameState.WaveInProgress:
                    if (WaveIsCompleted())
                    {
                        HandleWaveEnd();
                        break;
                    }

                    if (CurrentGroup >= Levels[CurrentLevel].waves[CurrentWave].groups.Count)
                    {
                        // All groups in this wave are already spawned
                        // Do nothing and wait for wave completion
                        break;
                    }

                    if (TimeManager.Instance.TimePassed(LastGroupSpawnTick) >= Levels[CurrentLevel].waves[CurrentWave].groups[CurrentGroup].spawnDelaySecs)
                    {
                        if (DebugLogging)
                        {
                            Debug.LogFormat("WaveInProgress: Spawning group {0}/{1}", CurrentGroup+1, Levels[CurrentLevel].waves[CurrentWave].groups.Count);
                        }

                        SpawnCurrentGroup();
                    }
                    break;
                case GameState.PostGame:
                    break;
            }
        }

        private void SpawnCurrentGroup()
        {
            Group group = Levels[CurrentLevel].waves[CurrentWave].groups[CurrentGroup];
            switch (group.spawnType)
            {
                case Group.SpawnFormation.Scatter:
                    enemies.AddRange(group.scatterWaveSpawn.Spawn(group.enemy, group.spawnCount));
                    break;
                case Group.SpawnFormation.Circle:
                    enemies.AddRange(group.circleWaveSpawn.Spawn(group.enemy, group.spawnCount));
                    break;
                case Group.SpawnFormation.Rectangle:
                    enemies.AddRange(group.rectangleWaveSpawn.Spawn(group.enemy, group.spawnCount));
                    break;
            }

            CurrentWaveEnemiesSpawned += group.spawnCount;
            CurrentWaveEnemiesAlive += group.spawnCount;
            CurrentGroup++;
        }

        private void DespawnRemainingEnemies()
        {
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    EnemyManager.Instance.ReturnEnemyToPool(enemy);
                }
            }
            enemies.Clear();
        }
    }
}
