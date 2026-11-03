
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] dialogueLines;

    private bool playerNearby;

    void Update()
    {
        if (!playerNearby)
            return;

        if (DialogueManager.Instance == null)
            return;

        if (DialogueManager.Instance.IsRunning)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            DialogueManager.Instance.StartDialogue(dialogueLines);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<Move_CHR>() != null)
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponentInParent<Move_CHR>() != null)
        {
            playerNearby = false;
        }
    }
}
