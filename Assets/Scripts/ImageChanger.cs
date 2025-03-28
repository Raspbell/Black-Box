using UnityEngine;
using UniRx;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class ImageChanger : MonoBehaviour
{
    [SerializeField] private Sprite[] buttonSprites;
    [SerializeField] private float spriteChangeInterval = 0.75f;
    [SerializeField] private Image blackOutImage;

    private void Start()
    {
        Observable.Interval(System.TimeSpan.FromSeconds(spriteChangeInterval))
            .Subscribe(_ =>
            {
                int randomIndex = Random.Range(0, buttonSprites.Length);
                GetComponent<Image>().sprite = buttonSprites[randomIndex];
            })
            .AddTo(this);
    }

    public void GameStart(int lang)
    {
        if (lang == 0)
        {
            GameStateManager.CurrentLanguage = GameStateManager.Language.English;
        }
        else
        {
            GameStateManager.CurrentLanguage = GameStateManager.Language.Japanese;
        }
        blackOutImage.gameObject.SetActive(true);
        blackOutImage.DOFade(1.0f, 1.0f).OnComplete(() => SceneManager.LoadScene("Game_1"));
    }
}
