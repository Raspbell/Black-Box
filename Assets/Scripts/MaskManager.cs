using UnityEngine;
using DG.Tweening;

public class MaskManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] float maskScale = 1.0f;
    private RectTransform rectTransform;

    public void SetTarget(Transform target, Vector3 offset)
    {
        this.target = target;
        rectTransform = GetComponent<RectTransform>();
        RefreshPosition();
    }

    public void SetTarget(Transform target)
    {
        SetTarget(target, Vector3.zero);
    }

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.DOScale(maskScale, 1.5f)
            .OnComplete(() =>
            {
                FukidashiManager fukidashiManager = FindObjectOfType<FukidashiManager>();
                fukidashiManager.isFukidashiShowing.Value = true;
            });
    }

    void Update()
    {
        RefreshPosition();
    }

    private void RefreshPosition()
    {
        if (target)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(target.position);
            rectTransform.position = screenPos;
        }
    }
}