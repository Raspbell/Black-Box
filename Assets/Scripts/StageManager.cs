using UnityEngine;
using UniRx;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class StageManager : MonoBehaviour
{
    [SerializeField] private Image blackoutImage;
    [SerializeField] private int stageNumber;
    [SerializeField] private bool isLastStatge;
    [SerializeField] private GameObject player;
    [SerializeField] private ParticleSystem gameOverEffect;

    private Vector3 respawnPoint = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);
    private Renderer playerRenderer;

    private void Start()
    {
        playerRenderer = player.GetComponent<Renderer>();
        GameStateManager.CurrentGameState
            .Where(state => state == GameStateManager.GameState.GameOver)
            .Subscribe(_ =>
            {
                GameOver();
                Debug.Log("GameOver");
            })
            .AddTo(this);

        if (respawnPoint == new Vector3(int.MaxValue, int.MaxValue, int.MaxValue))
        {
            respawnPoint = player.transform.position;
        }

        GameStateManager.CurrentGameState.Value = GameStateManager.GameState.PreGame;
    }

    private void Update()
    {
        var center = player.transform.position;
        var halfExtents = player.transform.localScale / 2;
        var colliders = Physics.OverlapBox(center, halfExtents);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("trap"))
            {
                if (GameStateManager.CurrentGameState.Value == GameStateManager.GameState.GameOver)
                {
                    return;
                }
                if (respawnPoint != new Vector3(int.MaxValue, int.MaxValue, int.MaxValue))
                {
                    GameStateManager.CurrentGameState.Value = GameStateManager.GameState.GameOver;
                }
                break;
            }
        }

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("respawn"))
            {
                Flag flag = collider.GetComponent<Flag>();
                if (flag == null || flag.isFlagActivated)
                {
                    break;
                }
                respawnPoint = collider.transform.position;
                flag.OnFlagTouched();
                break;
            }
        }

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("key"))
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                playerController.hasKey = true;
                Destroy(collider.gameObject);
                break;
            }
        }

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("goal"))
            {
                PlayerController playerController = player.GetComponent<PlayerController>();
                if (playerController.hasKey)
                {
                    if (GameStateManager.CurrentGameState.Value == GameStateManager.GameState.InGame)
                    {
                        LoadNextStage();
                    }
                    GameStateManager.CurrentGameState.Value = GameStateManager.GameState.PostGame;
                    break;
                }
            }
        }

        if (player.transform.position.y < -30)
        {
            GameStateManager.CurrentGameState.Value = GameStateManager.GameState.GameOver;
        }
    }

    private void GameOver()
    {
        playerRenderer.enabled = false;
        player.GetComponent<Rigidbody>().isKinematic = true;
        gameOverEffect.transform.position = player.transform.position;
        gameOverEffect.Play();
        Observable.Timer(System.TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                player.transform.position = respawnPoint;
                playerRenderer.enabled = true;
                player.GetComponent<Rigidbody>().isKinematic = false;
                GameStateManager.CurrentGameState.Value = GameStateManager.GameState.InGame;
            })
            .AddTo(this);
    }

    private void LoadNextStage()
    {
        blackoutImage.gameObject.SetActive(true);
        blackoutImage.DOFade(1f, 2f)
            .OnComplete(() =>
            {
                if (!isLastStatge)
                {
                    SceneManager.LoadScene("Game_" + (stageNumber + 1));
                }
                else
                {
                    SceneManager.LoadScene("Ending");
                }
            });
    }
}
