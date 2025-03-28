using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Answer
{
    [SerializeField] private string _info;
    public string Info => _info;

    [SerializeField] private bool _isCorrect;
    public bool IsCorrect => _isCorrect;
}
[Serializable]
public struct AnswerPuzzleLeft
{
    [SerializeField] private string _info;
    public string Info => _info;

    [SerializeField] private int _index;
    public int Index => _index;
}
[Serializable]
public struct AnswerPuzzleRight
{
    [SerializeField] private string _info;
    public string Info => _info;

    [SerializeField] private int _correctIndex;
    public int CorrectIndex => _correctIndex;
}

[System.Serializable]
public class Question
{
    public enum AnswerType { Multi, Single, Puzzle }

    [SerializeField] private string _info;
    [SerializeField] private Answer[] _answers;
    [SerializeField] private AnswerType _answerType;
    [SerializeField] private int _addScore;
    [SerializeField] private AnswerPuzzleLeft[] _answerPuzzleLefts;
    [SerializeField] private AnswerPuzzleRight[] _answerPuzzleRights;

    public List<AnswerPuzzleLeftData> AnswerPuzzleLeftDatas = new List<AnswerPuzzleLeftData>();
    public List<AnswerPuzzleRightData> AnswerPuzzleRightDatas = new List<AnswerPuzzleRightData>();
    
    
    public string Info => _info;
    public Answer[] Answers => _answers;
    public AnswerType GetAnswerType => _answerType;
    public int AddScore => _addScore;
    public AnswerPuzzleLeft[] AnswerPuzzleLefts => _answerPuzzleLefts;
    public AnswerPuzzleRight[] AnswerPuzzleRights => _answerPuzzleRights;

    public List<int> GetCorrectAnswers()
    {
        List<int> correctAnswers = new List<int>();
        for (int i = 0; i < _answers.Length; i++)
        {
            if (_answers[i].IsCorrect)
                correctAnswers.Add(i);
        }
        return correctAnswers;
    }
}