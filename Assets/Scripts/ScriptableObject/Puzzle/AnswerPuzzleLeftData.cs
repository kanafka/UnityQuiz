using TMPro;
using UnityEngine;

public class AnswerPuzzleLeftData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    public TextMeshProUGUI AnswerText => text;

    public TextMeshProUGUI Index{get; set;}
    private RectTransform rect;

    private string _info;
    private int _index;


    public RectTransform Rect
    {
        get
        {
            if (rect == null)
            {
                rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            }

            return rect;
        }
    }
}
