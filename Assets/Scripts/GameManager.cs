using UnityEngine;
using CustomUtils;
using System;

public class GameManager : SingletonMono<GameManager>
{
    [Header("State")]
    public GameState CurrentGameState = GameState.Playing;

    [Header("Manager Container")]
    public Transform ManagerContainer;

    private void Start()
    {
        UIManager.Instance.OpenUI<HomeUI>();
    }

    private GameObject CreateObject(string module, string nameModule)
    {
        GameObject Obj = GameObject.Instantiate(Resources.Load<GameObject>(module), ManagerContainer);
        Obj.name = nameModule;

        return Obj;
    }

    public void WinGame()
    {
        
    }

    public void OpenWinUI()
    {
        
    }

    public void LoseGame()
    {
        
    }

    public void OpenLoseUI()
    {
        
    }

    public void PauseGame()
    {
        
    }
}

[Serializable]
public enum GameState
{
    Playing,
    Paused,
    GameOver,
}
