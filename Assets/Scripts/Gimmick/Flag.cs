using UnityEngine;
using DG.Tweening;

public class Flag : MonoBehaviour
{
    public bool isFlagActivated = false;

    public void OnFlagTouched()
    {
        if (isFlagActivated)
        {
            return;
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(1f, 0.5f).SetRelative().SetLoops(2, LoopType.Yoyo));
        sequence.Join(transform.DOLocalRotate(new Vector3(0, 720, 0), 1f, RotateMode.FastBeyond360)).SetRelative();
        isFlagActivated = true;
    }
}
