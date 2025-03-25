using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;

public class FukidashiManager : MonoBehaviour
{
    [SerializeField] private Sprite[] fukidashiSprites;
    [SerializeField] private Image fukidashiImage;
    [SerializeField] private float fukidashiChangeInterval = 0.4f;
    [SerializeField] private GameObject player;

    [SerializeField] private ReactiveProperty<bool> isFukidashiShowing = new ReactiveProperty<bool>(false);

    void Start()
    {
        Observable.Interval(System.TimeSpan.FromSeconds(fukidashiChangeInterval))
            .Where(_ => isFukidashiShowing.Value)
            .Subscribe(_ =>
            {
                int randomIndex = Random.Range(0, fukidashiSprites.Length);
                fukidashiImage.sprite = fukidashiSprites[randomIndex];
            })
            .AddTo(this);

        isFukidashiShowing
            .Where(isShowing => isShowing)
            .Subscribe(_ =>
            {
                fukidashiImage.DOFade(1.0f, 1.0f).SetEase(Ease.InQuint);
            }).AddTo(this);

        isFukidashiShowing
            .Where(isShowing => !isShowing)
            .Subscribe(_ =>
            {
                fukidashiImage.DOFade(0.0f, 1.0f);
            }).AddTo(this);
    }

    [ContextMenu("ShowFukidashi")]
    public void ShowFukidashi()
    {
        isFukidashiShowing.Value = true;
    }

    [ContextMenu("HideFukidashi")]
    public void HideFukidashi()
    {
        isFukidashiShowing.Value = false;
    }
}
