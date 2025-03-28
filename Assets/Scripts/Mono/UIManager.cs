using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public struct UIManagerParameters
{
    [Header("Answers Options")] [SerializeField]
    private float margins;

    public float Margins => margins;

    [Header("Resolution Screen Options")] [SerializeField]
    private Color correctBGColor;

    public Color CorrectBGColor => correctBGColor;
    [SerializeField] private Color incorrectBGColor;
    public Color IncorrectBGColor => incorrectBGColor;
    [SerializeField] private Color finalBGColor;
    public Color FinalBGColor => finalBGColor;
}

[Serializable]
public struct UIElements
{
    [SerializeField] private RectTransform answersContentArea;
    public RectTransform AnswersContentArea => answersContentArea;

    [SerializeField] private TextMeshProUGUI questionInfoTextObject;
    public TextMeshProUGUI QuestionInfoTextObject => questionInfoTextObject;

    [SerializeField] private TextMeshProUGUI scoreText;
    public TextMeshProUGUI ScoreText => scoreText;

    [Space] [SerializeField] private Animator resolutionScreenAnimator;
    public Animator ResolutionScreenAnimator => resolutionScreenAnimator;

    [SerializeField] private Image resolutionBG;
    public Image ResolutionBG => resolutionBG;

    [SerializeField] private TextMeshProUGUI resolutionStateInfoText;
    public TextMeshProUGUI ResolutionStateInfoText => resolutionStateInfoText;

    [SerializeField] private TextMeshProUGUI resolutionScoreText;
    public TextMeshProUGUI ResolutionScoreText => resolutionScoreText;

    [Space] [SerializeField] private TextMeshProUGUI highScoreText;
    public TextMeshProUGUI HighScoreText => highScoreText;

    [SerializeField] private CanvasGroup mainCanvasGroup;
    public CanvasGroup MainCanvasGroup => mainCanvasGroup;

    [SerializeField] private RectTransform finishUIElements;
    public RectTransform FinishUIElements => finishUIElements;
}

public class UIManager : MonoBehaviour
{

    private bool _isPuzzleQuestion = false;

    private void UpdateQuestionUI(Question question)
    {
        uIElements.QuestionInfoTextObject.text = question.Info;
        CreateAnswers(question);
    }
    
    private void DisplayResolution(ResolutionScreenType type, int score)
    {
        UpdateResUI(type, score);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 2);
        uIElements.MainCanvasGroup.blocksRaycasts = false;

        if (type != ResolutionScreenType.Finish)
        {
            if (IE_DisplayTimedResolution != null) StopCoroutine(IE_DisplayTimedResolution);
            IE_DisplayTimedResolution = DisplayTimedResolution();
            StartCoroutine(IE_DisplayTimedResolution);
        }
    }

    private IEnumerator DisplayTimedResolution()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        uIElements.ResolutionScreenAnimator.SetInteger(resStateParaHash, 1);
        uIElements.MainCanvasGroup.blocksRaycasts = true;
    }
    
    private void UpdateResUI(ResolutionScreenType type, int score)
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey);

        switch (type)
        {
            case ResolutionScreenType.Correct:
                uIElements.ResolutionBG.color = parameters.CorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "CORRECT!";
                uIElements.ResolutionScoreText.text = "+" + score;
                break;
            case ResolutionScreenType.Incorrect:
                uIElements.ResolutionBG.color = parameters.IncorrectBGColor;
                uIElements.ResolutionStateInfoText.text = "WRONG!";
                uIElements.ResolutionScoreText.text = "-" + score;
                break;
            case ResolutionScreenType.Finish:
                uIElements.ResolutionBG.color = parameters.FinalBGColor;
                uIElements.ResolutionStateInfoText.text = "FINAL SCORE";

                StartCoroutine(CalculateScore());
                uIElements.FinishUIElements.gameObject.SetActive(true);
                uIElements.HighScoreText.gameObject.SetActive(true);
                uIElements.HighScoreText.text =
                    (highscore > events.StartupHighscore ? "<color=yellow>new </color>" : string.Empty) +
                    "Highscore: " + highscore;
                break;
        }
    }
    
    private IEnumerator CalculateScore()
    {
        var scoreValue = 0;
        while (scoreValue < events.CurrentFinalScore)
        {
            scoreValue++;
            uIElements.ResolutionScoreText.text = scoreValue.ToString();

            yield return null;
        }
    }
    
    private void CreateAnswers(Question question)
    {
        EraseAnswers();
        if (question.GetAnswerType == Question.AnswerType.Single || question.GetAnswerType == Question.AnswerType.Multi)
        {
            var offset = 0 - parameters.Margins;
            for (var i = 0; i < question.Answers.Length; i++)
            {
                var newAnswer = Instantiate(answerPrefab, uIElements.AnswersContentArea);
                newAnswer.UpdateData(question.Answers[i].Info, i);

                newAnswer.Rect.anchoredPosition = new Vector2(0, offset);

                offset -= newAnswer.Rect.sizeDelta.y + parameters.Margins;
                uIElements.AnswersContentArea.sizeDelta =
                    new Vector2(uIElements.AnswersContentArea.sizeDelta.x, offset * -1);

                currentAnswers.Add(newAnswer);
            }
        }
        else if (question.GetAnswerType == Question.AnswerType.Puzzle)
        {
            _isPuzzleQuestion = true;
            
            float offset = 0 - parameters.Margins;
            for (var i = 0; i < question.AnswerPuzzleLefts.Length; i++)
            {
                AnswerPuzzleLeftData newAnswer =
                    (AnswerPuzzleLeftData)Instantiate(answerPuzzleLeftPrefab, uIElements.AnswersContentArea);
                newAnswer.AnswerText.text = question.AnswerPuzzleLefts[i].Info;
  

                newAnswer.Rect.anchoredPosition = new Vector2(0, (question.AnswerPuzzleLefts[i].Index - 1)
                    * -(newAnswer.Rect.sizeDelta.y + parameters.Margins) - parameters.Margins);
                uIElements.AnswersContentArea.sizeDelta =
                    new Vector2(uIElements.AnswersContentArea.sizeDelta.x, ((i + 1)
                        * -(newAnswer.Rect.sizeDelta.y + parameters.Margins) - parameters.Margins) * -1);
                
                question.AnswerPuzzleLeftDatas.Add(newAnswer);
                currentAnswerPuzzleLeftDatas.Add(newAnswer);
            }

            offset = 0 - parameters.Margins;
            for (var i = 0; i < question.AnswerPuzzleRights.Length; i++)
            {
                AnswerPuzzleRightData newAnswer =
                    (AnswerPuzzleRightData)Instantiate(answerPuzzleRightPrefab, uIElements.AnswersContentArea);
                newAnswer.AnswerText.text = question.AnswerPuzzleRights[i].Info;
                newAnswer.CorrectIndex = question.AnswerPuzzleRights[i].CorrectIndex;

                newAnswer.Rect.anchoredPosition = new Vector2(500, i
                    * -(newAnswer.Rect.sizeDelta.y + parameters.Margins) - parameters.Margins);

                newAnswer.SetOriginalPosition();

                question.AnswerPuzzleRightDatas.Add(newAnswer);
                currentAnswerPuzzleRightDatas.Add(newAnswer);
            }
        }
    }
    
    private void EraseAnswers()
    {
        foreach (var answer in currentAnswers) Destroy(answer.gameObject);
        foreach (var answer in currentAnswerPuzzleLeftDatas.ToArray()) Destroy(answer.gameObject);
        foreach (var answer in currentAnswerPuzzleRightDatas.ToArray()) Destroy(answer.gameObject);
        currentAnswers.Clear();
        currentAnswerPuzzleLeftDatas.Clear();
        currentAnswerPuzzleRightDatas.Clear();
    }
    
    private void UpdateScoreUI()
    {
        uIElements.ScoreText.text = "Score: " + events.CurrentFinalScore;
    }

    #region Variables

    public enum ResolutionScreenType
    {
        Correct,
        Incorrect,
        Finish
    }

    [Header("References")] [SerializeField]
    private GameEvents events;

    [Header("UI Elements (Prefabs)")]
    [SerializeField]
    private AnswerData answerPrefab;
    [SerializeField]
    private AnswerPuzzleLeftData answerPuzzleLeftPrefab;
    [SerializeField]
    private AnswerPuzzleRightData answerPuzzleRightPrefab;

    [SerializeField] private UIElements uIElements;

    [Space] [SerializeField] private UIManagerParameters parameters;

    private readonly List<AnswerData> currentAnswers = new();
    private readonly List<AnswerPuzzleLeftData> currentAnswerPuzzleLeftDatas = new();
    private readonly List<AnswerPuzzleRightData> currentAnswerPuzzleRightDatas = new();
    private int resStateParaHash;

    private IEnumerator IE_DisplayTimedResolution;

    #endregion

    #region Default Unity methods
    
    private void OnEnable()
    {
        events.UpdateQuestionUI += UpdateQuestionUI;
        events.DisplayResolutionScreen += DisplayResolution;
        events.ScoreUpdated += UpdateScoreUI;
    }
    
    private void OnDisable()
    {
        events.UpdateQuestionUI -= UpdateQuestionUI;
        events.DisplayResolutionScreen -= DisplayResolution;
        events.ScoreUpdated -= UpdateScoreUI;
    }
    
    private void Start()
    {
        EraseAnswers();
        UpdateScoreUI();
        resStateParaHash = Animator.StringToHash("ScreenState");
    }

    #endregion
}