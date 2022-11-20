using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionsManager : MonoBehaviour
{
    public static QuestionsManager instance;
    public TMP_Text Text_Question;
    public List<TMP_Text> Text_Answers_UI;
    public List<TMP_Text> Text_Answers_Data;
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
        UpdateAnswersData(Text_Answers_UI);
    }

    private void UpdateAnswersData(List<TMP_Text> text_Answers_UI)
    {
        Text_Answers_Data = new List<TMP_Text>();
        foreach (var item in Text_Answers_UI)
        {
            Text_Answers_Data.Add(item);
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
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
                }
                else
                {
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
                Text_Answers_Data[y].text = AllQuestions[i].answers[y];
                Text_Answers_Data[y].gameObject.SetActive(true);
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
        if (Text_Answers_Data.Count > 0)
        {
            int _indexHighlighted = 0;
            for (int i = 0; i < Text_Answers_Data.Count; i++)
            {
                if (Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().isHighlighted)
                {
                    _indexHighlighted = i;
                    Text_Answers_Data[_indexHighlighted].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
            }



            if (_indexHighlighted == Text_Answers_UI.Count - 1)
            {
                _indexHighlighted = 0;
            }
            else
            {
                _indexHighlighted++;
            }
            Text_Answers_Data[_indexHighlighted].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
        }
        else
        {
            Debug.Log("list empty");
        }

    }

    public void Question_OnSwipeLeft()
    {
        if (Text_Answers_Data.Count > 0)
        {
            int _indexHighlighted = 0;
            for (int i = Text_Answers_Data.Count - 1; i >= 0; i--)
            {
                if (Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().isHighlighted)
                {
                    _indexHighlighted = i;
                    Text_Answers_Data[_indexHighlighted].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
            }



            if (_indexHighlighted == 0)
            {
                _indexHighlighted = Text_Answers_UI.Count - 1;
            }
            else
            {
                _indexHighlighted--;
            }
            Text_Answers_Data[_indexHighlighted].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
        }
        else
        {
            Debug.Log("list empty");
        }
    }

    public void Question_OnSwipeUp()
    {
        if (Text_Answers_Data.Count > 0)
        {
            int _indexToRemove = 0;
            for (int i = 0; i < Text_Answers_Data.Count; i++)
            {
                if (Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().isHighlighted)
                {
                    Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().OnChooseHighlighted();
                    _indexToRemove = i;
                    UpdateAnswerFields(Text_Answers_Data[i].text);
                }
            }
            Text_Answers_Data.RemoveAt(_indexToRemove);
            UpdateHighlights();
        }
        else
        {
            Debug.Log("list empty");
        }
    }

    void UpdateHighlights()
    {
        for (int i = 0; i < Text_Answers_Data.Count; i++)
        {
            if (!Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().isHighlighted)
            {
                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
                break;
            }
        }
    }

    private void UpdateAnswerFields(string text)
    {
        foreach (var field in Text_AnswerFields)
        {
            if (string.IsNullOrEmpty(field.text))
            {
                field.text = text;
                break;
            }
        }
    }

    public void Question_OnSwipeDown()
    {
        UpdateAnswersData(Text_Answers_UI);
        if (Text_Answers_Data.Count > 0)
        {
            for (int i = 0; i < Text_Answers_Data.Count; i++)
            {

                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleDisable(false);

            }

            UpdateHighlights();

            foreach (var field in Text_AnswerFields)
            {
                field.text = "";
            }
        }
        else
        {
            Debug.Log("list empty");
        }

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
