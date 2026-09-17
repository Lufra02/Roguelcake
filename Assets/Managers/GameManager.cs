using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // ----- SingleTon ---------
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public static GameManager GetInstance() => Instance;

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
    // ------ Fin del singleton  ---------

    public GameState gameState;
    public Action<GameState> onChangeGameState;
    
    public bool canPause;

    
    private void Start()
    {
        gameState = GameState.Play;
        canPause = true;
    }

    public void PauseGame()
    {
        if (canPause)
        {
            if (gameState == GameState.Pause)
            {
                ChangeGameState(GameState.Play);
            }
            else if (gameState == GameState.Play)
            {
                ChangeGameState(GameState.Pause);
            }
        }
    }

    public void ChangeGameState(GameState newGameState)
    {
        if (!canPause) return;
        
        gameState = newGameState;
        onChangeGameState?.Invoke(gameState);
    }
    
}


public enum GameState
{
    Play,
    Pause,
    GameOver
}