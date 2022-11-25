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
    string RegistrationClientID = "DF4066902FF7C2A1";
    public AudioClip clip_wrong, clip_correct;
    public int CurrentQuestionIndex { get => _currentQuestionIndex; set => _currentQuestionIndex = value; }

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
        foreach (var item in text_Answers_UI)
        {
            Text_Answers_Data.Add(item);
        }
    }

    public void UpdateQuestionScreen()
    {
        UI_SwipeToNextQuestion.SetActive(false);
        UpdateAnswersData(Text_Answers_UI);

        Text_Question.text = AllQuestions[CurrentQuestionIndex].question;
        if (CurrentQuestionIndex == 2)
        {
            for (int y = 0; y < AllQuestions[CurrentQuestionIndex].answers.Length; y++)
            {
                if (y == 0)
                {
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
                }
                else
                {
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
                Text_Answers_Data[y].text = AllQuestions[CurrentQuestionIndex].answers[y];
                Text_Answers_Data[y].gameObject.SetActive(true);
            }
            for (int i = 0; i < Text_AnswerFields.Count; i++)
            {
                if (i == 1 || i == 2)
                {
                    Text_AnswerFields[i].gameObject.SetActive(true);
                }
                else
                {
                    Text_AnswerFields[i].gameObject.SetActive(false);
                }
            }
        }
        else
        {
            for (int y = 0; y < AllQuestions[CurrentQuestionIndex].answers.Length; y++)
            {
                if (y == 0)
                {
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(true);
                }
                else
                {
                    Text_Answers_Data[y].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                }
                Text_Answers_Data[y].text = AllQuestions[CurrentQuestionIndex].answers[y];
                Text_Answers_Data[y].gameObject.SetActive(true);
                Text_AnswerFields[y].gameObject.SetActive(true);
            }
        }



        ResetHighlights();
    }


    public void ShouldMoveToNextQuestion()
    {
        if (IsAllAnswerFieldFilled())
        {
            //swipe XX to go to next question
            UI_SwipeToNextQuestion.SetActive(true);
        }
        else
        {
            UI_SwipeToNextQuestion.SetActive(false);
        }
    }

    private bool IsAllAnswerFieldFilled()
    {
        bool result = true;
        foreach (var field in Text_AnswerFields)
        {
            if (field.gameObject.activeInHierarchy && string.IsNullOrEmpty(field.text))
            {
                result = false;
            }
        }
        return result;
    }


    public void Question_OnSwipeRight()
    {
        //if there are still answers to swipe through
        //update highlights on answers
        //if (Text_Answers_Data.Count > 0)
        //{
        if (!IsAllAnswerFieldFilled())
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
        //no more answers to swipe through
        else
        {
            if (CurrentQuestionIndex < AllQuestions.Count - 1)
            {
                Debug.Log("swipe right to go to next question, will reset data");
                CurrentQuestionIndex++;
                Gamemanager.instance.LoadTransitionScreen("Please move on to the next goal");
                //UpdateQuestionScreen();
            }
            else
            {
                Debug.Log("no more questions, moving to finish screen");
                Gamemanager.instance.LoadFinishScreen();
                PlayFabAuthService.Instance.FinalizeUser(RegistrationClientID, Gamemanager.instance.CurrentPlayer);
            }

        }

    }

    public void Question_OnSwipeLeft()
    {
        //if (Text_Answers_Data.Count > 0)
        //{
        if (!IsAllAnswerFieldFilled())
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
        if (CurrentQuestionIndex == 2)
        {
            if (IsAllAnswerFieldFilled())
            {
                ShouldMoveToNextQuestion();
            }
            else
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
                    UpdateAnswersHighlights();
                    ShouldMoveToNextQuestion();
                }
                else
                {
                    Debug.Log("list empty");
                }
            }

        }
        else
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
                UpdateAnswersHighlights();
                ShouldMoveToNextQuestion();
            }
            else
            {
                Debug.Log("list empty");
            }
        }

    }

    public void Question_OnSwipeDown()
    {
        isBlocking = false;
        UpdateAnswersData(Text_Answers_UI);
        if (Text_Answers_Data.Count > 0)
        {
            for (int i = 0; i < Text_Answers_Data.Count; i++)
            {

                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleDisable(false);

            }

            UpdateAnswersHighlights();

            foreach (var field in Text_AnswerFields)
            {
                field.text = "";
            }

            UpdateAnswerFieldsResults();
            ShouldMoveToNextQuestion();
        }
        else
        {
            Debug.Log("list empty");
        }

    }

    void UpdateAnswersHighlights()
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
            if (field.gameObject.activeInHierarchy && string.IsNullOrEmpty(field.text))
            {
                field.text = text;
                break;
            }
        }
    }

    private bool isBlocking = false;
    private void UpdateAnswerFieldsResults()
    {
        if (CurrentQuestionIndex == 2)
        {
            for (int i = 0; i < Text_AnswerFields.Count; i++)
            {
                if (!string.IsNullOrEmpty(Text_AnswerFields[i].text))
                {
                    if (HasCorrectAnswer(Text_AnswerFields[i].text))
                    {
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(true);
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
                    }
                    else
                    {
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(true);
                        isBlocking = true;
                    }
                }
                else
                {
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
                }
            }
        }
        else
        {
            for (int i = 0; i < Text_AnswerFields.Count; i++)
            {
                if (!string.IsNullOrEmpty(Text_AnswerFields[i].text))
                {
                    if (Text_AnswerFields[i].text == AllQuestions[CurrentQuestionIndex].correctOrder[i])
                    {
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(true);
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
                    }
                    else
                    {
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                        Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(true);
                        isBlocking = true;
                    }
                }
                else
                {
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleCorrect(false);
                    Text_AnswerFields[i].gameObject.GetComponent<UIAnswerField>().ToggleWrong(false);
                }
            }
        }

    }

    private bool HasCorrectAnswer(string text)
    {
        bool result = false;
        foreach (var answer in AllQuestions[CurrentQuestionIndex].correctOrder)
        {
            if (text == answer)
            {
                result = true;
            }
        }
        return result;
    }

    private void IsAnswerCorrect()
    {

    }
    void ResetHighlights()
    {
        //Question_OnSwipeDown();
        UpdateAnswersData(Text_Answers_UI);
        if (Text_Answers_Data.Count > 0)
        {
            for (int i = 0; i < Text_Answers_Data.Count; i++)
            {

                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleHighlight(false);
                Text_Answers_Data[i].gameObject.GetComponent<UIAnswer>().ToggleDisable(false);

            }

            UpdateAnswersHighlights();

            foreach (var field in Text_AnswerFields)
            {
                field.text = "";
            }

            UpdateAnswerFieldsResults();
            UI_SwipeToNextQuestion.SetActive(false);
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
