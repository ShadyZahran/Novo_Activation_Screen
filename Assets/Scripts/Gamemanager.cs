using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Uduino;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager instance;

    string RegistrationClientID = "DF4066902FF7C2A1";
    public GameObject Menu_StartScreen, Menu_WelcomeScreen, Menu_TutorialScreen, Menu_QuestionScreen;
    public TMP_Text Text_WelcomeScreen;
    public int CheckRecordTimer = 5;
    public float _timer = 0f;
    public State GameState;
    public bool checking = true;

    List<PlayerInfo> Allplayers;

    //public int Pressure_A, Pressure_B, Pressure_C, Gesture_A, Gesture_B, Gesture_C;
    public int pressureThreshold = 1;
    public int gesture_SwipeRight = 1;
    public int gesture_SwipeLeft = 2;
    public int gesture_SwipeUp = 3;
    public int gesture_SwipeDown = 4;

    SensorValues mySensorValues;


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
        GameState = State.Login;
        Allplayers = new List<PlayerInfo>();
    }

    private void Update()
    {
        switch (GameState)
        {
            case State.Login:
                Debug.Log("inside Login state");
                break;
            case State.Start:
                if (checking)
                {
                    if (_timer < CheckRecordTimer)
                    {
                        _timer += Time.deltaTime;
                    }
                    else
                    {
                        Debug.Log("checking record");
                        checking = false;
                        _timer = 0;
                        GetRegistrationClientRecord();
                    }
                }

                break;
            case State.Welcome:
                //check weight sensor on 1st circle
                if (mySensorValues.Pressure_A >= pressureThreshold)
                {
                    Debug.Log("pressure A pressed");
                    LoadScreen(Menu_WelcomeScreen, Menu_TutorialScreen);
                    GameState = State.Tutorial;
                }
                else if (Input.GetKey(KeyCode.Alpha1))
                {
                    Debug.Log("fake pressure data");
                    OnDataReceived("1/0/0/0/0/0", null);
                }
                //move to tutorial
                break;
            case State.Tutorial:
                //check gesture sensor for 1st circle
                //swipe right, left, up and down
                if (mySensorValues.Gesture_A == gesture_SwipeRight)
                {
                    Debug.Log("tutorial: gesture A executed");
                    QuestionsManager.instance.UpdateQuestionScreen();
                    LoadScreen(Menu_TutorialScreen, Menu_QuestionScreen);
                    GameState = State.Question1;
                }
                else if (Input.GetKey(KeyCode.Alpha2))
                {
                    Debug.Log("fake gesture data");
                    OnDataReceived("1/0/0/1/0/0", null);
                }

                //goto 1st question
                break;
            case State.Question1:
                //check gesture sensor for 1st circle
                //swipe left, right to move highlight on the answers
                //swipe up, down to move the highlighted answer to the empty field
                if (mySensorValues.Gesture_A == gesture_SwipeRight)
                {
                    Debug.Log("Question1: gesture A swiperight");
                    QuestionsManager.instance.Question_OnSwipeRight();
                }
                else if (mySensorValues.Gesture_A == gesture_SwipeLeft)
                {
                    Debug.Log("Question1: gesture A swipeleft");
                    QuestionsManager.instance.Question_OnSwipeLeft();
                }
                else if (mySensorValues.Gesture_A == gesture_SwipeUp)
                {
                    Debug.Log("Question1: gesture A swipeup");
                    QuestionsManager.instance.Question_OnSwipeUp();
                }
                else if (mySensorValues.Gesture_A == gesture_SwipeDown)
                {
                    Debug.Log("Question1: gesture A swipedown");
                    QuestionsManager.instance.Question_OnSwipeDown();
                }


                ////////// FAKE DATA ///////////
                else if (Input.GetKey(KeyCode.Alpha3))
                {
                    Debug.Log("fake gesture A swipe right data");
                    OnDataReceived("1/0/0/1/0/0", null);
                }
                else if (Input.GetKey(KeyCode.Alpha4))
                {
                    Debug.Log("fake gesture A swipe left data");
                    OnDataReceived("1/0/0/2/0/0", null);
                }
                else if (Input.GetKey(KeyCode.Alpha5))
                {
                    Debug.Log("fake gesture A swipe up data");
                    OnDataReceived("1/0/0/3/0/0", null);
                }
                else if (Input.GetKey(KeyCode.Alpha6))
                {
                    Debug.Log("fake gesture A swipe down data");
                    OnDataReceived("1/0/0/4/0/0", null);
                }
                
                break;
            case State.Question2:
                break;
            case State.Question3:
                break;
            case State.Finish:
                break;
            default:
                break;
        }
    }



    private void GetRegistrationClientRecord()
    {
        PlayFabAuthService.Instance.GetUserData(RegistrationClientID, CheckRecord, OnGetRecordFail);
    }

    private void OnGetRecordFail(PlayFabError obj)
    {
        checking = true;
    }

    private void CheckRecord(GetUserDataResult obj)
    {
        Debug.Log("Got user data:");
        Debug.Log(obj.Data.Keys.Count);
        Debug.Log(PlayFabAuthService.PlayFabId);
        if (obj.Data == null || !obj.Data.ContainsKey("Flag"))
        {
            Debug.Log("No Data or no Flag");
            checking = true;
        }
        else if (obj.Data["Flag"].Value == "New")
        {
            Debug.Log("New record found ");
            Debug.Log("flag: " + obj.Data["Flag"].Value);
            //extract and add the player to the all players list
            AddPlayer(obj.Data);
            UpdateWelcomeScreen();
        }
        else
        {
            checking = true;
            Debug.Log("Old record found ");
            Debug.Log("flag: " + obj.Data["Flag"].Value);
        }
    }

    private void UpdateWelcomeScreen()
    {
        Text_WelcomeScreen.text = "Welcome Dr." + Allplayers[Allplayers.Count - 1].Name + ". Please step inside the first circle to start";
        LoadScreen(Menu_StartScreen, Menu_WelcomeScreen);
        GameState = State.Welcome;
    }

    private void LoadScreen(GameObject toDisable, GameObject toEnable)
    {
        toDisable.SetActive(false);
        toEnable.SetActive(true);
    }

    private void AddPlayer(Dictionary<string, UserDataRecord> data)
    {
        PlayerInfo NewPlayer = new PlayerInfo(data["Name"].Value, data["Email"].Value, data["Phone"].Value, data["Flag"].Value);
        NewPlayer.PrintPlayer();
        Allplayers.Add(NewPlayer);
        Debug.Log("Player: " + NewPlayer.Name + " added.");

    }

    public void StartGame()
    {
        //Menu_Registration.SetActive(true);
        //Menu_StartScreen.SetActive(false);
    }

    public void ResetMenus()
    {
        //Menu_StartScreen.SetActive(true);
        //Menu_Registration.SetActive(false);
    }


    [System.Serializable]
    public struct PlayerInfo
    {
        public string Name, Email, Phone, Flag;
        public PlayerInfo(string fName, string fEmail, string fPhone, string fFlag)
        {
            Name = fName;
            Email = fEmail;
            Phone = fPhone;
            Flag = fFlag;
        }
        public void PrintPlayer()
        {
            Debug.Log(Name + " " + Email + " " + Phone);
        }
    }

    public struct SensorValues
    {
        public int Pressure_A;
        public int Pressure_B;
        public int Pressure_C;
        public int Gesture_A;
        public int Gesture_B;
        public int Gesture_C;
    }


    void PrintAllPlayers()
    {
        Debug.Log("printing all players");
        foreach (var player in Allplayers)
        {
            player.PrintPlayer();
        }
    }

    public enum State
    {
        Login,
        Start,
        Welcome,
        Tutorial,
        Question1,
        Question2,
        Question3,
        Finish
    }

    public void UpdateArduinoValues()
    {
        Uduino.UduinoManager.Instance.sendCommand("doSomething", 20000);
    }

    public void OnDataReceived(string data, UduinoDevice deviceName)
    {
        //Output.text = "Arduino data received: " + data;
        Debug.Log("Arduino data received: " + data);
        ParseData(data);
    }
    // "Pressure_A/Pressure_B/Pressure_C/Gesture_A/Gesture_B/Gesture_C"
    void ParseData(string data)
    {
        string[] values = data.Split('/');
        if (values.Length < 6)
        {
            Debug.Log("data is not sensor values");
        }
        else
        {
            mySensorValues.Pressure_A = int.Parse(values[0]);
            mySensorValues.Pressure_B = int.Parse(values[1]);
            mySensorValues.Pressure_C = int.Parse(values[2]);
            mySensorValues.Gesture_A = int.Parse(values[3]);
            mySensorValues.Gesture_B = int.Parse(values[4]);
            mySensorValues.Gesture_C = int.Parse(values[5]);
            Debug.Log("Sensor values parsed successfully");
            ResetSensorValues();
        }

    }

    void ResetSensorValues()
    {
        mySensorValues.Pressure_A = 0;
        mySensorValues.Pressure_B = 0;
        mySensorValues.Pressure_C = 0;
        mySensorValues.Gesture_A = 0;
        mySensorValues.Gesture_B = 0;
        mySensorValues.Gesture_C = 0;
        Debug.Log("Sensor values reset for next data received");
    }
}
