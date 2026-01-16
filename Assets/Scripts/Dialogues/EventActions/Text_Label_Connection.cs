using TMPro;
using UnityEngine;

public class Text_Label_Connection : MonoBehaviour
{
    [SerializeField] private TMP_Text label;

    void OnEnable()
    {
        label.gameObject.SetActive(true);
        SetOtherTextsDesactivate();
    }

    void OnDisable()
    {
        label.gameObject.SetActive(false);
    }

    private void SetOtherTextsDesactivate()
    {
        foreach (TMP_Text text in label.transform.parent.gameObject.GetComponentsInChildren<TMP_Text>())
        {
            if (text != label) text.gameObject.SetActive(false);
        }
    }
}
