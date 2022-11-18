using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uduino;
using UnityEngine;
using UnityEngine.UI;

public class UduinoBtnCallback : MonoBehaviour
{
    public TMP_Text Output;
    void Awake()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived; //Create the Delegate
        UduinoManager.Instance.alwaysRead = true; // This value should be On By Default
    }

    void OnDataReceived(string data, UduinoDevice deviceName)
    {
        Output.text = "Arduino data received: "+ data;
    }
}
