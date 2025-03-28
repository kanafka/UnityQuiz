using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerData : MonoBehaviour
{

    public void Reset()
    {
        Checked = false;
        UpdateUI();
    }


    public void UpdateData(string info, int index)
    {
        infoTextObject.text = info;
        AnswerIndex = index;
    }


    public void SwitchState()
    {
        Checked = !Checked;
        UpdateUI();

        if (events.UpdateQuestionAnswer != null) events.UpdateQuestionAnswer(this);
    }


    private void UpdateUI()
    {
        if (toggle == null) return;

        toggle.sprite = Checked ? checkedToggle : uncheckedToggle;
    }



    [Header("UI Elements")] [SerializeField]
    private TextMeshProUGUI infoTextObject;

    [SerializeField] private Image toggle;

    [Header("Textures")] [SerializeField] private Sprite uncheckedToggle;

    [SerializeField] private Sprite checkedToggle;

    [Header("References")] [SerializeField]
    private GameEvents events;

    private RectTransform _rect;

    public RectTransform Rect
    {
        get
        {
            if (_rect == null) _rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            return _rect;
        }
    }

    public int AnswerIndex { get; private set; } = -1;

    private bool Checked;


}