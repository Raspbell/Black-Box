using UnityEngine;
using DG.Tweening;
using UniRx;

public class PlayerController : MonoBehaviour
{
    public bool hasKey = false;

    [SerializeField] private SO_PlayerStatus playerStatus;
    [SerializeField] private SpriteRenderer questionSpriteRenderer;
    [SerializeField] private float maxHorizontalSpeed = 5f;

    public bool isGrounded = false;
    private float currentBaseFacing = 180f;
    private Tween turningTween = null;
    private BoxCollider boxCollider;
    private Rigidbody rb;

    private bool isJumpCharging = false;
    private float jumpChargeStartTime = 0f;

    private float horizontalInputValue = 0f;
    private bool jumpKeyReleased = false;
    private bool jumpAutoRelease = false;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();

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

        if (GameStateManager.CurrentGameState.Value != GameStateManager.GameState.InGame)
        {
            return;
        }
    }

    private void Update()
    {
        if (GameStateManager.CurrentGameState.Value != GameStateManager.GameState.InGame)
        {
            return;
        }

        horizontalInputValue = Input.GetAxis("Horizontal");

        if (!isJumpCharging && isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            jumpChargeStartTime = Time.time;
            isJumpCharging = true;
        }

        if (isJumpCharging && Input.GetKeyUp(KeyCode.Space))
        {
            jumpKeyReleased = true;
        }

        if (isJumpCharging && (Time.time - jumpChargeStartTime) >= playerStatus.maxJumpChargeTime)
        {
            jumpAutoRelease = true;
        }

        if (horizontalInputValue > 0)
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
        else if (horizontalInputValue < 0)
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
        float offset = (Mathf.Abs(horizontalInputValue) > 0.01f)
            ? Mathf.Sin(Time.time * playerStatus.oscillationFrequency) * playerStatus.oscillationAmplitude
            : 0f;
        transform.rotation = Quaternion.Euler(0, currentBaseFacing + offset, 0);
    }

    private void FixedUpdate()
    {
        if (GameStateManager.CurrentGameState.Value != GameStateManager.GameState.InGame)
        {
            return;
        }

        if (Mathf.Abs(horizontalInputValue) > 0.01f)
        {
            float appliedSpeed = isGrounded ? playerStatus.walkSpeed : playerStatus.walkSpeed * playerStatus.airControlMultiplier;
            Vector3 force = new Vector3(horizontalInputValue * appliedSpeed, 0, 0);
            rb.AddForce(force, ForceMode.Force);
        }

        if (Mathf.Abs(rb.linearVelocity.x) > maxHorizontalSpeed)
        {
            rb.linearVelocity = new Vector3(Mathf.Sign(rb.linearVelocity.x) * maxHorizontalSpeed, rb.linearVelocity.y, rb.linearVelocity.z);
        }

        if (jumpKeyReleased)
        {
            float chargeTime = Mathf.Min(Time.time - jumpChargeStartTime, playerStatus.maxJumpChargeTime);
            float jumpForce = Mathf.Lerp(playerStatus.minJumpPower, playerStatus.maxJumpPower, chargeTime / playerStatus.maxJumpChargeTime);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log("Jumped " + chargeTime + " seconds charge" + " " + jumpForce + " force");
            jumpKeyReleased = false;
            isJumpCharging = false;
        }
        else if (jumpAutoRelease)
        {
            rb.AddForce(Vector3.up * playerStatus.maxJumpPower, ForceMode.Impulse);
            jumpAutoRelease = false;
            isJumpCharging = false;
        }
    }

    public void ShowQuestionAnimation(FukidashiManager fukidashiManager)
    {
        questionSpriteRenderer.enabled = true;
        Vector3 originalScale = questionSpriteRenderer.transform.localScale;
        Sequence sequence = DOTween.Sequence();
        sequence.Append(questionSpriteRenderer.transform.DOScale(0.3f, 0.5f));
        sequence.AppendInterval(1f);
        sequence.Append(questionSpriteRenderer.transform.DOScale(originalScale, 0.5f));
        sequence.OnComplete(() =>
        {
            questionSpriteRenderer.enabled = false;
            fukidashiManager.ContinueConversation();
        });
    }
}
