using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnswerField : MonoBehaviour
{
    public GameObject correct, wrong;
    public bool isCorrect = false;
    public bool isWrong = false;
    private void Awake()
    {
        correct = transform.GetChild(0).gameObject;
        wrong = transform.GetChild(1).gameObject;
    }

    public void ToggleCorrect(bool flag)
    {
        isCorrect = flag;
        correct.SetActive(flag);
    }

    public void ToggleWrong(bool flag)
    {
        isWrong = flag;
        wrong.SetActive(flag);
    }
    private void OnEnable()
    {
        correct.SetActive(isCorrect);
        correct.SetActive(isWrong);
    }

    
}
