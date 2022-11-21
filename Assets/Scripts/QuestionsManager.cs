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
    public GameObject UI_SwipeToNextQuestion;
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
        //UpdateAnswersData(Text_Answers_UI);
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
        UI_SwipeToNextQuestion.SetActive(false);
        UpdateAnswersData(Text_Answers_UI);


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
        ResetHighlights();
    }

    public void IsReadyToConfirm()
    {
        //all answer field are filled
    }

    public void CheckResult()
    {
        if (Text_Answers_Data.Count == 0)
        {
            //swipe XX to go to next question
            UI_SwipeToNextQuestion.SetActive(true);
        }
        else
        {
            UI_SwipeToNextQuestion.SetActive(false);
        }
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



            if (_indexHighlighted == Text_Answers_Data.Count - 1)
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
            if (_currentQuestionIndex<AllQuestions.Count-1)
            {
                Debug.Log("swipe right to go to next question, will reset data");
                _currentQuestionIndex++;
                UpdateQuestionScreen();
            }
            else
            {
                Debug.Log("no more questions");
            }
            
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
                _indexHighlighted = Text_Answers_Data.Count - 1;
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
                    PopulateAnswerFields(Text_Answers_Data[i].text);
                    UpdateAnswerFieldsResults();
                }
            }
            Text_Answers_Data.RemoveAt(_indexToRemove);
            UpdateHighlights();
            CheckResult();
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

    private void PopulateAnswerFields(string text)
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

    private bool IsQuestionReadyToEvaluate()
    {
        bool result = true;
        foreach (var field in Text_AnswerFields)
        {
            if (string.IsNullOrEmpty(field.text))
            {
                result = false;
            }
        }
        return result;
    }

    private void UpdateAnswerFieldsResults()
    {
        for (int i = 0; i < Text_AnswerFields.Count; i++)
        {
            if (!string.IsNullOrEmpty(Text_AnswerFields[i].text))
            {
                if (Text_AnswerFields[i].text == AllQuestions[_currentQuestionIndex].correctOrder[i])
                {
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(true);
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
                }
                else
                {
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(true);
                }
            }
            else
            {
                Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
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

            UpdateAnswerFieldsResults();
            CheckResult();
        }
        else
        {
            Debug.Log("list empty");
        }

    }

    void ResetHighlights()
    {
        Question_OnSwipeDown();
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
