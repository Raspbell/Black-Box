using UnityEngine;
using DG.Tweening;

public class FloorMoving : MonoBehaviour
{
    public float speed = 1.0f;
    public Vector3 targetPosition;
    public bool moveOnPlayerEnter = false;

    private Vector3 initialPosition;
    private Tween moveTween;

    private void Start()
    {
        initialPosition = transform.position;
        if (!moveOnPlayerEnter)
        {
            StartMoving();
        }
    }

    private void StartMoving()
    {
        if (moveTween != null && moveTween.IsActive())
            return;

        Vector3 targetWorldPosition = initialPosition + targetPosition;
        float distance = Vector3.Distance(initialPosition, targetWorldPosition);
        float duration = (speed > 0f) ? distance / speed : 0f;

        if (moveOnPlayerEnter)
        {
            moveTween = transform.DOMove(targetWorldPosition, duration)
                                 .SetEase(Ease.Linear)
                                 .SetLoops(2, LoopType.Yoyo)
                                 .OnComplete(() => moveTween = null);
        }
        else
        {
            moveTween = transform.DOMove(targetWorldPosition, duration)
                                 .SetEase(Ease.Linear)
                                 .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            if (moveOnPlayerEnter && (moveTween == null || !moveTween.IsActive()))
            {
                StartMoving();
            }
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.GetComponent<PlayerController>() != null)
        {
            if (collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }
    }
}
