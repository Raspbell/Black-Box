using UnityEngine;

public class TextEvent : MonoBehaviour
{
    public string eventText_ja;
    public string eventText_en;
    public bool isOneTimeOnly;
    public int priority;
    private bool isTextCalled = false;
    public bool IsTextCalled { get { return isTextCalled; } set { isTextCalled = value; } }

    private void Start()
    {
        if (GetComponent<Renderer>() != null)
        {
            GetComponent<Renderer>().enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerController>() != null)
        {
            FukidashiManager manager = FindFirstObjectByType<FukidashiManager>();
            if (manager != null)
            {
                manager.TriggerTextEvent(this);
            }
        }
    }
}
