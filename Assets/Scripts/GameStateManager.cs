using UnityEngine;
using UniRx;

[DefaultExecutionOrder(-5)]
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

    public enum Language
    {
        English,
        Japanese
    }

    public static ReactiveProperty<GameState> CurrentGameState { get; set; } = new ReactiveProperty<GameState>(GameState.None);
    public static Language CurrentLanguage { get; set; } = Language.Japanese;
    [SerializeField] private Language initialLanguage;

    // private void Start()
    // {
    //     CurrentLanguage = initialLanguage;
    // }

    [ContextMenu("SwitchToEnglish")]
    public void SwitchToEnglish()
    {
        CurrentLanguage = Language.English;
    }

    [ContextMenu("SwitchToJapanese")]
    public void SwitchToJapanese()
    {
        CurrentLanguage = Language.Japanese;
    }
}
