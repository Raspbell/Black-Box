using UnityEngine;
using UniRx;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private SO_PlayerStatus playerStatus;

    private bool isGrounded = false;
    private float currentBaseFacing = 180f;
    private Tween turningTween = null;
    private BoxCollider boxCollider;
    private bool isJumpCharging = false;
    private float jumpChargeStartTime = 0f;

    private void Start()
    {
        if (GameStateManager.CurrentGameState != GameStateManager.GameState.InGame)
        {
            return;
        }

        boxCollider = GetComponent<BoxCollider>();

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                Vector3 movement = new Vector3(horizontalInput * playerStatus.walkSpeed * Time.deltaTime, 0, 0);
                transform.Translate(movement, Space.World);
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Space) && isGrounded)
            .Subscribe(_ =>
            {
                jumpChargeStartTime = Time.time;
                isJumpCharging = true;
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => isJumpCharging && Input.GetKeyUp(KeyCode.Space))
            .Subscribe(_ =>
            {
                float chargeTime = Mathf.Min(Time.time - jumpChargeStartTime, playerStatus.maxJumpChargeTime);
                float jumpForce = Mathf.Lerp(playerStatus.minJumpPower, playerStatus.maxJumpPower, chargeTime / playerStatus.maxJumpChargeTime);
                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                Debug.Log("Jumped " + chargeTime + " seconds charge" + " " + jumpForce + " force");
                isJumpCharging = false;
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Where(_ => isJumpCharging && (Time.time - jumpChargeStartTime) >= playerStatus.maxJumpChargeTime)
            .Subscribe(_ =>
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * playerStatus.maxJumpPower, ForceMode.Impulse);
                isJumpCharging = false;
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                float horizontalInput = Input.GetAxis("Horizontal");
                if (horizontalInput > 0)
                {
                    float targetFacing = 180f;
                    if (Mathf.Abs(currentBaseFacing - targetFacing) > 0.1f)
                    {
                        if (turningTween != null && turningTween.IsActive())
                        {
                            turningTween.Kill();
                        }
                        turningTween = DOTween.To(() => currentBaseFacing, x => currentBaseFacing = x, targetFacing, playerStatus.turnDuration);
                    }
                }
                else if (horizontalInput < 0)
                {
                    float targetFacing = 0f;
                    if (Mathf.Abs(currentBaseFacing - targetFacing) > 0.1f)
                    {
                        if (turningTween != null && turningTween.IsActive())
                        {
                            turningTween.Kill();
                        }
                        turningTween = DOTween.To(() => currentBaseFacing, x => currentBaseFacing = x, targetFacing, playerStatus.turnDuration);
                    }
                }
                float offset = (Mathf.Abs(horizontalInput) > 0.01f)
                    ? Mathf.Sin(Time.time * playerStatus.oscillationFrequency) * playerStatus.oscillationAmplitude
                    : 0f;
                transform.rotation = Quaternion.Euler(0, currentBaseFacing + offset, 0);
            })
            .AddTo(this);

        Observable.EveryUpdate()
            .Subscribe(_ =>
            {
                Vector3 localBottomCenter = boxCollider.center - new Vector3(0, boxCollider.size.y * 0.5f, 0);
                Vector3 bottomCenter = transform.TransformPoint(localBottomCenter) + Vector3.up * playerStatus.rayOriginHeightOffset;
                Vector3 frontOffset = transform.TransformVector(new Vector3(boxCollider.size.x * 0.5f, 0, 0));
                Vector3 backOffset = transform.TransformVector(new Vector3(-boxCollider.size.x * 0.5f, 0, 0));
                Vector3 bottomFront = bottomCenter + frontOffset;
                Vector3 bottomBack = bottomCenter + backOffset;
                bool hitDetected = false;
                RaycastHit hit;

                if (Physics.Raycast(bottomCenter, Vector3.down, out hit, playerStatus.groundCheckDistance))
                {
                    Debug.DrawRay(bottomCenter, Vector3.down * hit.distance, Color.green);
                    if (hit.collider.CompareTag("floor"))
                        hitDetected = true;
                }
                else
                {
                    Debug.DrawRay(bottomCenter, Vector3.down * playerStatus.groundCheckDistance, Color.red);
                }

                if (Physics.Raycast(bottomFront, Vector3.down, out hit, playerStatus.groundCheckDistance))
                {
                    Debug.DrawRay(bottomFront, Vector3.down * hit.distance, Color.green);
                    if (hit.collider.CompareTag("floor"))
                        hitDetected = true;
                }
                else
                {
                    Debug.DrawRay(bottomFront, Vector3.down * playerStatus.groundCheckDistance, Color.red);
                }

                if (Physics.Raycast(bottomBack, Vector3.down, out hit, playerStatus.groundCheckDistance))
                {
                    Debug.DrawRay(bottomBack, Vector3.down * hit.distance, Color.green);
                    if (hit.collider.CompareTag("floor"))
                        hitDetected = true;
                }
                else
                {
                    Debug.DrawRay(bottomBack, Vector3.down * playerStatus.groundCheckDistance, Color.red);
                }

                isGrounded = hitDetected;
            })
            .AddTo(this);
    }
}
