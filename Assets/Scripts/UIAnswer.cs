using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnswer : MonoBehaviour
{
    public GameObject highlighter, disabled;
    public bool isHighlighted = false;
    public bool isDisabled = false;
    private void Awake()
    {
        highlighter = transform.GetChild(0).gameObject;
        disabled = transform.GetChild(1).gameObject;
    }

    public void ToggleHighlight(bool flag)
    {
        isHighlighted = flag;
        highlighter.SetActive(flag);
    }

    public void ToggleDisable(bool flag)
    {
        isDisabled = flag;
        disabled.SetActive(flag);
    }
    private void OnEnable()
    {
        highlighter.SetActive(isHighlighted);
    }

    public void OnChooseHighlighted()
    {
        if (isHighlighted)
        {
            ToggleHighlight(false);
            ToggleDisable(true);
        }
        else
        {
            Debug.Log("not highlighted");
        }
    }
}
