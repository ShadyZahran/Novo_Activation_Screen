using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uduino;
using UnityEngine;
using UnityEngine.UI;

public class UduinoBtnCallback : MonoBehaviour
{
    public TMP_Text Output;
    public int Pressure_A, Pressure_B, Pressure_C, Gesture_A, Gesture_B, Gesture_C;
    void Awake()
    {
        UduinoManager.Instance.OnDataReceived += OnDataReceived; //Create the Delegate
        UduinoManager.Instance.alwaysRead = true; // This value should be On By Default
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnDataReceived(string data, UduinoDevice deviceName)
    {
        Output.text = "Arduino data received: " + data;
        //Debug.Log(data);
        //ParseData(data);
    }

    void ParseData(string data)
    {
        string[] values = data.Split('/');
        Pressure_A = int.Parse(values[0]);
        Pressure_B = int.Parse(values[1]);
        Pressure_C = int.Parse(values[2]);
        Gesture_A = int.Parse(values[3]);
        Gesture_B = int.Parse(values[4]);
        Gesture_C = int.Parse(values[5]);
    }
}
