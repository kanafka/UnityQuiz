using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AnswerPuzzleRightData : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private static readonly List<AnswerPuzzleRightData> allDraggables = new();
    [SerializeField] private TextMeshProUGUI answerText;
    [SerializeField] private Image imageBackground;

    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private bool isDragging;
    private Vector3 originalPosition;


    private RectTransform rectTransform;
    private float startY;
    private bool _isConnected = false;
    public TextMeshProUGUI AnswerText => answerText;

    public int Index { get; private set; }

    public int CorrectIndex { get; set; }

    public RectTransform Rect
    {
        get
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();

            return rectTransform;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Debug.Log(rectTransform.localPosition);

        if (canvas == null) Debug.LogError("UI элемент должен быть внутри Canvas");

        allDraggables.Add(this);
    }

    private void OnDisable()
    {
        allDraggables.Clear();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag started!");
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, eventData.position,
            eventData.pressEventCamera, out var localPoint);
        startY = rectTransform.localPosition.y - localPoint.y;

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
                eventData.position, eventData.pressEventCamera, out localPoint))
        {
            var newY = localPoint.y + startY;
            rectTransform.localPosition =
                new Vector3(rectTransform.localPosition.x, newY, rectTransform.localPosition.z);
        }

        NearestSlot();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        isDragging = false;
        if (_isConnected)
        {
            rectTransform.localPosition = originalPosition;
        }
        else
        {
            _isConnected = true;
            originalPosition.x = originalPosition.x - 150;
            rectTransform.localPosition = originalPosition;
        }
    }

    public bool CheckAnswer()
    {
        return Index == CorrectIndex;
    }

    public void SetOriginalPosition()
    {
        originalPosition = rectTransform.localPosition;
        allDraggables.Sort((b, a) => a.rectTransform.localPosition.y.CompareTo(b.rectTransform.localPosition.y));
        for (var i = 0; i < allDraggables.Count; i++)
        {
            allDraggables[i].Index = i + 1;
            Debug.Log(allDraggables[i].answerText.text + " " + (i + 1));
        }
    }


    private void NearestSlot()
    {
        AnswerPuzzleRightData closest = null;
        float closestDistance = 50;

        foreach (var draggable in allDraggables)
        {
            if (draggable == this) continue;

            var distance = Mathf.Abs(rectTransform.localPosition.y - draggable.originalPosition.y);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = draggable;
            }
        }

        if (closest != null)
        {
            if (_isConnected && !closest._isConnected)
            {
                closest.originalPosition.x -= 150;
            }   
            if (!_isConnected && closest._isConnected)
            {
                originalPosition.x -= 150;
            }  
            (originalPosition, closest.originalPosition) = (closest.originalPosition, originalPosition);
            (Index, closest.Index) = (closest.Index, Index);
            closest.rectTransform.localPosition = closest.originalPosition;
            if (closest._isConnected)
            {
                _isConnected = true;
            }

            if (_isConnected)
            {
                closest._isConnected = true;
            }


        }
    }
}