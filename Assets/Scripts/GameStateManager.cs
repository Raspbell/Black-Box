using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public enum GameState
    {
        None,
        PreGame,
        InGame,
        PostGame,
        GameOver
    }

    public static GameState CurrentGameState { get; set; } = GameState.None;
}
