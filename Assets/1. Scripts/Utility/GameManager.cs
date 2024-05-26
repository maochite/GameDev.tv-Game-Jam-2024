using Ability;
using System;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    GameStartup = 0,
    MainMenu = 1,
    PlayingGame = 2,
}

public class GameManager : StaticInstance<GameManager> 
{


    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    [field: NonSerialized] public bool ActiveGame { get; private set; } = false;
    public GameState State { get; private set; }

    void Start() => ChangeState(GameState.GameStartup);

    public void ChangeState(GameState newState) {
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState) {
            case GameState.GameStartup:
                HandleGameStartup();
                break;
            case GameState.MainMenu:
                HandleMainMenu();
                break;
            case GameState.PlayingGame:
                HandlePlayingGame();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
        
        Debug.Log($"New state: {newState}");
    }

    private void HandleGameStartup() 
    {
        PlayerManager.Instance.AssignPlayer();

        ChangeState(GameState.MainMenu);
    }

    private void HandleMainMenu() 
    {
        ChangeState(GameState.PlayingGame);
    }

    private void HandlePlayingGame()
    {

    }

    public void StartGame() 
    {

    }
}
