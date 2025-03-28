using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{

    public void UpdateAnswers(AnswerData newAnswer)
    {
        if (Questions[currentQuestion].GetAnswerType == Question.AnswerType.Single)
        {
            foreach (var answer in PickedAnswers)
                if (answer != newAnswer)
                    answer.Reset();

            PickedAnswers.Clear();
            PickedAnswers.Add(newAnswer);
        }
        else
        {
            var alreadyPicked = PickedAnswers.Exists(x => x == newAnswer);
            if (alreadyPicked)
                PickedAnswers.Remove(newAnswer);
            else
                PickedAnswers.Add(newAnswer);
        }
    }

    public void EraseAnswers()
    {
        PickedAnswers = new List<AnswerData>();
    }

    private void Display()
    {
        EraseAnswers();
        var question = GetRandomQuestion();

        if (events.UpdateQuestionUI != null)
            events.UpdateQuestionUI(question);
        else
            Debug.LogWarning(
                "Ups! Something went wrong while trying to display new Question UI Data. GameEvents.UpdateQuestionUI is null. Issue occured in GameManager.Display() method.");
        
    }

    public void Accept()
    {
        if (!isTimerRunning) return;

        var isCorrect = CheckAnswers();
        FinishedQuestions.Add(currentQuestion);

        UpdateScore(isCorrect ? Questions[currentQuestion].AddScore : -Questions[currentQuestion].AddScore);

        if (IsFinished)
        {
            isTimerRunning = false;
            SetHighscore();
            events.DisplayResolutionScreen?.Invoke(UIManager.ResolutionScreenType.Finish, Questions[currentQuestion].AddScore);
            return;
        }

        var type = isCorrect 
            ? UIManager.ResolutionScreenType.Correct 
            : UIManager.ResolutionScreenType.Incorrect;

        events.DisplayResolutionScreen?.Invoke(type, Questions[currentQuestion].AddScore);

        if (IE_WaitTillNextRound != null) 
            StopCoroutine(IE_WaitTillNextRound);
        
        IE_WaitTillNextRound = WaitTillNextRound();
        StartCoroutine(IE_WaitTillNextRound);
    }

    private bool CheckAnswers()
    {
        if (Questions[currentQuestion].GetAnswerType == Question.AnswerType.Single ||
            Questions[currentQuestion].GetAnswerType == Question.AnswerType.Multi)
        {
            if (!CompareAnswers()) return false;
            return true;
        }
        else if (Questions[currentQuestion].GetAnswerType == Question.AnswerType.Puzzle)
        {
            return CheckPuzzleAnswers();
        }
        else
        {
            Debug.Log("Inccorect Type");
            return false;
        }
    }


    private bool CompareAnswers()
    {
        if (PickedAnswers.Count > 0)
        {
            var c = Questions[currentQuestion].GetCorrectAnswers();
            var p = PickedAnswers.Select(x => x.AnswerIndex).ToList();

            var f = c.Except(p).ToList();
            var s = p.Except(c).ToList();

            return !f.Any() && !s.Any();
        }

        return false;
    }

    private bool CheckPuzzleAnswers()
    {
        bool isCorrect = true;
        foreach (var puzzle in Questions[currentQuestion].AnswerPuzzleRightDatas)
        {
            if (!puzzle.CheckAnswer())
            {
                isCorrect = false;
                break;
            }
        }
        return isCorrect;
    }

    private void LoadQuestions()
    {
        TextAsset[] jsonFiles = Resources.LoadAll<TextAsset>("Questions");
        Questions = new Question[jsonFiles.Length];
    
        for (int i = 0; i < jsonFiles.Length; i++)
        {
            Questions[i] = JsonUtility.FromJson<Question>(jsonFiles[i].text);
        }
    }


    public void RestartGame()
    {
        LoadQuestions();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetHighscore()
    {
        var highscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey);
        if (highscore < events.CurrentFinalScore) PlayerPrefs.SetInt(GameUtility.SavePrefKey, events.CurrentFinalScore);
    }

    private void UpdateScore(int add)
    {
        events.CurrentFinalScore += add;

        if (events.ScoreUpdated != null) events.ScoreUpdated();
    }



    public Question[] Questions { get; private set; }

    [SerializeField] private GameEvents events;

    [SerializeField] private Animator timerAnimtor;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Color timerHalfWayOutColor = Color.yellow;
    [SerializeField] private Color timerAlmostOutColor = Color.red;
    private Color timerDefaultColor = Color.white;

    private List<AnswerData> PickedAnswers = new();
    private readonly List<int> FinishedQuestions = new();
    private int currentQuestion;

    private int timerStateParaHash;
    
    [SerializeField] private int totalQuizTime = 120; // 2 минуты в секундах
    private int currentQuizTime;
    private bool isTimerRunning;

    private IEnumerator IE_WaitTillNextRound;
    private IEnumerator IE_StartTimer;

    private bool IsFinished => FinishedQuestions.Count < Questions.Length ? false : true;




    private void OnEnable()
    {
        events.UpdateQuestionAnswer += UpdateAnswers;
    }


    private void OnDisable()
    {
        events.UpdateQuestionAnswer -= UpdateAnswers;
    }

    private void Awake()
    {
        events.CurrentFinalScore = 0;
    }

    private void Start()
    {
        events.StartupHighscore = PlayerPrefs.GetInt(GameUtility.SavePrefKey);
        timerDefaultColor = timerText.color;
        LoadQuestions();
        
        timerStateParaHash = Animator.StringToHash("TimerState");
        currentQuizTime = totalQuizTime;
        timerText.text = currentQuizTime.ToString();

        var seed = Random.Range(int.MinValue, int.MaxValue);
        Random.InitState(seed);

        StartTimer(); 
        Display();
    }




    
    private void StartTimer()
    {
        isTimerRunning = true;
        IE_StartTimer = QuizTimer();
        StartCoroutine(IE_StartTimer);
        timerAnimtor.SetInteger(timerStateParaHash, 2);
    }
    private IEnumerator QuizTimer()
    {
        while (currentQuizTime > 0 && isTimerRunning)
        {
            yield return new WaitForSeconds(1.0f);
            currentQuizTime--;

            // Обновление цвета текста таймера
            if (currentQuizTime < totalQuizTime / 2 && currentQuizTime > totalQuizTime / 4)
                timerText.color = timerHalfWayOutColor;
            else if (currentQuizTime <= totalQuizTime / 4)
                timerText.color = timerAlmostOutColor;
            else
                timerText.color = timerDefaultColor;

            timerText.text = currentQuizTime.ToString();

            if (currentQuizTime <= 0)
            {
                TimeExpired();
                yield break;
            }
        }
    }
    private void TimeExpired()
    {
        isTimerRunning = false;
        timerText.text = "0";
        Accept();
        SetHighscore();
        events.DisplayResolutionScreen?.Invoke(UIManager.ResolutionScreenType.Finish, 0);
    }

    private IEnumerator WaitTillNextRound()
    {
        yield return new WaitForSeconds(GameUtility.ResolutionDelayTime);
        Display();
    }




    private Question GetRandomQuestion()
    {
        var randomIndex = GetRandomQuestionIndex();
        currentQuestion = randomIndex;
        if (FinishedQuestions.Count == 0 && Questions[currentQuestion].GetAnswerType == Question.AnswerType.Puzzle)
        {
            return GetRandomQuestion();
        }
        return Questions[currentQuestion];
    }

    private int GetRandomQuestionIndex()
    {
        var random = 0;
        if (FinishedQuestions.Count < Questions.Length)
            do
            {
                random = Random.Range(0, Questions.Length);
            } while (FinishedQuestions.Contains(random) || random == currentQuestion);

        return random;
    }


}