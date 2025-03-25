using UnityEngine;
using UniRx;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private Vector3 respawnPoint = new Vector3(int.MaxValue, int.MaxValue, int.MaxValue);

    private void Update()
    {
        var center = player.transform.position;
        var halfExtents = player.transform.localScale / 2;
        var colliders = Physics.OverlapBox(center, halfExtents);

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("trap"))
            {
                Debug.Log("Player is dead");
                if (respawnPoint != new Vector3(int.MaxValue, int.MaxValue, int.MaxValue))
                {
                    GameStateManager.CurrentGameState = GameStateManager.GameState.GameOver;
                    player.transform.position = respawnPoint;
                }
                break;
            }
        }

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("respawn"))
            {
                respawnPoint = collider.transform.position;
                break;
            }
        }

        foreach (var collider in colliders)
        {
            if (collider.CompareTag("goal"))
            {
                GameStateManager.CurrentGameState = GameStateManager.GameState.PostGame;
                break;
            }
        }
    }
}
