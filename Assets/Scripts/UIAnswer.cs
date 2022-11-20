using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnswer : MonoBehaviour
{
    public GameObject highlighter;
    public bool isHighlighted = false;

    private void Awake()
    {
        highlighter = transform.GetChild(0).gameObject;
    }

    public void ToggleHighlight(bool flag)
    {
        isHighlighted = flag;
        highlighter.SetActive(flag);
    }
    private void OnEnable()
    {
        highlighter.SetActive(isHighlighted);
    }
}
