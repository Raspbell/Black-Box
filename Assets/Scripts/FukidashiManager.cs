using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using DG.Tweening;
using TMPro;

public class FukidashiManager : MonoBehaviour
{
    [SerializeField] private Sprite[] fukidashiSprites;
    [SerializeField] private Image fukidashiImage;
    [SerializeField] private TextMeshProUGUI fukidashiText;
    [SerializeField] private float fukidashiChangeInterval = 0.4f;
    [SerializeField] private CanvasGroup fukidashiCanvasGroup;
    [SerializeField] private GameObject player;
    [SerializeField] private bool initialIsFukidashiShowing = false;
    [SerializeField] private SO_CatText_Click catText_en;
    [SerializeField] private SO_CatText_Click catText_ja;
    [SerializeField] private float letterInterval_en = 0.03f;
    [SerializeField] private float letterInterval_ja = 0.1f;
    private bool continueConversationFlag = false;
    public ReactiveProperty<bool> isFukidashiShowing;

    [SerializeField] private float textEventLetterInterval_en = 0.02f;
    [SerializeField] private float textEventLetterInterval_ja = 0.05f;
    [SerializeField] private float textEventDisplayDuration = 2.0f;

    private int currentTextEventPriority = int.MinValue;
    private Coroutine currentTextEventCoroutine = null;
    private float letterInterval;
    private float textEventLetterInterval;

    void Awake()
    {
        isFukidashiShowing = new ReactiveProperty<bool>(initialIsFukidashiShowing);
    }

    void Start()
    {
        if (GameStateManager.CurrentLanguage == GameStateManager.Language.English)
        {
            textEventLetterInterval = textEventLetterInterval_en;
            letterInterval = letterInterval_en;
            Debug.Log("English");
        }
        else
        {
            textEventLetterInterval = textEventLetterInterval_ja;
            letterInterval = letterInterval_ja;
            Debug.Log("Japanese");
        }

        Debug.Log($"letterInterval: {letterInterval}, textEventLetterInterval: {textEventLetterInterval}");

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
            .Take(1)
            .Subscribe(_ =>
            {
                fukidashiCanvasGroup.DOFade(1.0f, 1.0f)
                    .SetEase(Ease.InQuint)
                    .OnComplete(() =>
                    {
                        ShowOpeningText();
                    });
            }).AddTo(this);

        isFukidashiShowing
            .Where(isShowing => isShowing)
            .Skip(1)
            .Subscribe(_ =>
            {
                fukidashiCanvasGroup.DOFade(1.0f, 1.0f).SetEase(Ease.InQuint);
            }).AddTo(this);

        isFukidashiShowing
            .Where(isShowing => !isShowing)
            .Subscribe(_ =>
            {
                fukidashiCanvasGroup.DOFade(0.0f, 1.0f);
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

    private void ShowOpeningText()
    {
        StartCoroutine(ShowTextCoroutine());
    }

    private IEnumerator ShowTextCoroutine()
    {
        string[] texts;
        if (GameStateManager.CurrentLanguage == GameStateManager.Language.English)
        {
            texts = catText_en.catTexts;
        }
        else
        {
            texts = catText_ja.catTexts;
        }

        for (int i = 0; i < texts.Length; i++)
        {
            string currentText = texts[i];
            if (currentText.StartsWith("Event:"))
            {
                string idString = currentText.Substring("Event:".Length);
                int eventId;
                if (int.TryParse(idString, out eventId))
                {
                    StartCoroutine(ProcessEventCoroutine(eventId));
                }
                else
                {
                    Debug.LogWarning("不正なイベントID: " + idString);
                }
                yield return new WaitUntil(() => continueConversationFlag);
                continueConversationFlag = false;
                continue;
            }
            fukidashiText.text = "";
            for (int j = 0; j < currentText.Length; j++)
            {
                fukidashiText.text += currentText[j];
                yield return new WaitForSeconds(letterInterval);
            }
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }
        GameStateManager.CurrentGameState.Value = GameStateManager.GameState.InGame;
        Debug.Log("InGame");
        isFukidashiShowing.Value = false;
    }

    private IEnumerator ProcessEventCoroutine(int eventId)
    {
        switch (eventId)
        {
            case 0:
                PlayerController playerController = player.GetComponent<PlayerController>();
                playerController.ShowQuestionAnimation(this);
                break;
            default:
                Debug.LogWarning("未定義のイベントID: " + eventId);
                break;
        }
        yield break;
    }

    public void ContinueConversation()
    {
        continueConversationFlag = true;
    }

    public void TriggerTextEvent(TextEvent textEvent)
    {
        if (textEvent.isOneTimeOnly && textEvent.IsTextCalled)
        {
            return;
        }
        if (currentTextEventCoroutine != null)
        {
            if (currentTextEventPriority <= textEvent.priority)
            {
                StopCoroutine(currentTextEventCoroutine);
            }
            else
            {
                return;
            }
        }
        if (textEvent.isOneTimeOnly)
        {
            textEvent.IsTextCalled = true;
        }
        currentTextEventPriority = textEvent.priority;
        currentTextEventCoroutine = StartCoroutine(ShowTextEventCoroutine(textEvent));
    }

    private IEnumerator ShowTextEventCoroutine(TextEvent textEvent)
    {
        ShowFukidashi();
        fukidashiText.text = "";
        if (GameStateManager.CurrentLanguage == GameStateManager.Language.English)
        {
            for (int i = 0; i < textEvent.eventText_en.Length; i++)
            {
                fukidashiText.text += textEvent.eventText_en[i];
                yield return new WaitForSeconds(textEventLetterInterval);
            }
        }
        else
        {
            for (int i = 0; i < textEvent.eventText_ja.Length; i++)
            {
                fukidashiText.text += textEvent.eventText_ja[i];
                yield return new WaitForSeconds(textEventLetterInterval);
            }
        }

        yield return new WaitForSeconds(textEventDisplayDuration);
        HideFukidashi();
        Debug.Log("HideFukidashi");
        currentTextEventPriority = int.MinValue;
        currentTextEventCoroutine = null;
    }
}
