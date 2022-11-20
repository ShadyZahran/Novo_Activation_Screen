using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionsManager : MonoBehaviour
{
    public static QuestionsManager instance;
    public TMP_Text Text_Question;
    public List<TMP_Text> Text_Answers;
    public List<TMP_Text> Text_AnswerFields;
    public List<Question> AllQuestions;
    int _currentQuestionIndex = 0;
    //int _current
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public void UpdateQuestionScreen()
    {
        Text_Question.text = AllQuestions[_currentQuestionIndex].question;
        for (int i = 0; i < AllQuestions.Count; i++)
        {
            for (int y = 0; y < AllQuestions[i].answers.Length; y++)
            {
                if (y == 0)
                {
                    Text_Answers[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
                }
                else
                {
                    Text_Answers[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
                Text_Answers[y].text = AllQuestions[i].answers[y];
            }
           
        }
        _currentQuestionIndex++;
    }

    public void IsReadyToConfirm()
    {
        //all answer field are filled
    }

    public void CheckResult()
    {
        //return if answered correct or not
    }

    public void ChooseAnswer()
    {

    }

    public void Question_OnSwipeRight()
    {

    }

    public void Question_OnSwipeLeft()
    {

    }

    public void Question_OnSwipeUp()
    {

    }

    public void Question_OnSwipeDown()
    {

    }

    [System.Serializable]
    public struct Question
    {
        public string name;
        public string type;
        public string question;
        public string[] answers;
        public string[] correctOrder;
    }
}
