using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    // ----- SingleTon ---------
    #region Singleton
    public static MainManager instance { get; private set; }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    public static MainManager GetInstance() => instance;

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
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