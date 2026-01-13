using UnityEngine;

public class TriggerNextDialogue : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueController.Instance.NextDialogue();
            gameObject.SetActive(false);
        }
    }
}