using UniRx;
using UnityEngine;

public class SelfDestroyer : MonoBehaviour
{
    void Start()
    {
        Observable.Timer(System.TimeSpan.FromSeconds(2.0f)).Subscribe(_ => Destroy(gameObject)).AddTo(this);
    }
}
