using Ability;
using Dialogue;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unit.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GameSceneState
{
    GameLoad = 0,
    MainMenu = 1,
    Intro = 2,
    PlayingGame = 3,
}

public class GameManager : PersistentSingleton<GameManager> 
{
    [field: SerializeField] public SceneTransitions SceneTransitions { get; private set; }

    public static event Action<GameSceneState> OnBeforeStateChanged;
    public static event Action<GameSceneState> OnAfterStateChanged;

    [field: NonSerialized] public bool ActiveGame { get; private set; } = false;
    public GameSceneState State { get; private set; } = GameSceneState.GameLoad;

    void Start() => ChangeState(GameSceneState.GameLoad);

    Coroutine waitCoroutine;

    public void ChangeState(GameSceneState newState) {
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState) {
            case GameSceneState.GameLoad:
                StartCoroutine(HandleGameLoad());
                break;
            case GameSceneState.MainMenu:
                StartCoroutine(HandleMainMenu());
                break;
            case GameSceneState.Intro:
                StartCoroutine(HandleGameIntro());
                break;
            case GameSceneState.PlayingGame:
                StartCoroutine(HandleMainGame());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
        
        Debug.Log($"New state: {newState}");
    }

    private IEnumerator HandleGameLoad() 
    {
        SceneManager.LoadScene((int)GameSceneState.MainMenu);
        ChangeState(GameSceneState.MainMenu);
        yield break;
    }

    private IEnumerator HandleMainMenu() 
    {
        yield return StartCoroutine(FadeInCoroutine(SceneTransitions.FadeDuration, SceneTransitions.FadeImage));
        SceneManager.LoadScene((int)GameSceneState.Intro);
        ChangeState(GameSceneState.Intro);
        yield break;
    }

    private IEnumerator HandleGameIntro()
    {
        DialogueManager.Instance.AssignTextBox(SceneTransitions.TextBox);
        DialogueManager.Instance.QueueDialogue(SceneTransitions.IntroDialogue);
        yield return StartCoroutine(WaitCoroutine(SceneTransitions.IntroDuration));
        SceneManager.LoadScene((int)GameSceneState.PlayingGame);
        ChangeState(GameSceneState.PlayingGame);
        yield break;
    }

    private IEnumerator HandleMainGame()
    {
        yield return StartCoroutine(WaitCoroutine(1));
        DialogueManager.Instance.AssignTextBox(Player.Instance.PlayerDialogue);
        yield return StartCoroutine(FadeOutCoroutine(SceneTransitions.FadeDuration, SceneTransitions.FadeImage));
        yield break;
    }


    public IEnumerator WaitCoroutine(float time)
    {
        yield return new WaitForSeconds(time);  
    }
    public IEnumerator FadeInCoroutine(float time, Image image)
    {
        float currentTime = 0f;
        Color originalColor = Color.black;
        float originalAlpha = originalColor.a;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            float normalizedTime = currentTime / time;
            Color newColor = new(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(0f, originalAlpha, normalizedTime));
            image.color = newColor;
            yield return null; 
        }

        image.color = new(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    public IEnumerator FadeOutCoroutine(float time, Image image)
    {
        float currentTime = 0f;
        Color originalColor = Color.black;
        float originalAlpha = originalColor.a;

        while (currentTime < time)
        {
            currentTime += Time.deltaTime;
            float normalizedTime = currentTime / time;
            Color newColor = new(originalColor.r, originalColor.g, originalColor.b, Mathf.Lerp(originalAlpha, 0f, normalizedTime));
            image.color = newColor;
            yield return null; 
        }

        image.color = new(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
