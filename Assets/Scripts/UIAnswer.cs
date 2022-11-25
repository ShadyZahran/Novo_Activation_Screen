using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAnswer : MonoBehaviour
{
    public GameObject highlighter, disabled;
    public bool isHighlighted = false;
    public bool isDisabled = false;
    

    public void ToggleHighlight(bool flag)
    {
        isHighlighted = flag;
        highlighter.SetActive(flag);
    }

    public void ToggleDisable(bool flag)
    {
        isDisabled = flag;
        //disabled.SetActive(flag);
        if (flag)
        {
            this.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.2f, 0.2f, 0.2f);
        }
        else
        {
            this.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(1.0f, 1.0f, 1.0f);
        }
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
            //ToggleDisable(true);
            this.GetComponent<TMPro.TextMeshProUGUI>().color = new Color(0.2f, 0.2f, 0.2f);
        }
        else
        {
            Debug.Log("not highlighted");
        }
    }
}
