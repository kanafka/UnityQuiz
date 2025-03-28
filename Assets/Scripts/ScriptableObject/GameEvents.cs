using UnityEngine;

[CreateAssetMenu(fileName = "GameEvents", menuName = "Quiz/new GameEvents")]
public class GameEvents : ScriptableObject
{
    public delegate void DisplayResolutionScreenCallback(UIManager.ResolutionScreenType type, int score);

    public delegate void ScoreUpdatedCallback();

    public delegate void UpdateQuestionAnswerCallback(AnswerData pickedAnswer);

    public delegate void UpdateQuestionUICallback(Question question);

    [HideInInspector] public int CurrentFinalScore;

    [HideInInspector] public int StartupHighscore;

    public DisplayResolutionScreenCallback DisplayResolutionScreen = null;
    public ScoreUpdatedCallback ScoreUpdated = null;
    public UpdateQuestionAnswerCallback UpdateQuestionAnswer = null;
    public UpdateQuestionUICallback UpdateQuestionUI = null;
}