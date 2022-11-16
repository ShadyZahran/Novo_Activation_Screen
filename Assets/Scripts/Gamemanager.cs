using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager instance;

    string RegistrationClientID = "DF4066902FF7C2A1";
    public GameObject Menu_StartScreen, Menu_WelcomeScreen;
    public int CheckRecordTimer = 5;
    public float _timer = 0f;
    public State GameState;
    public bool checking = true;

    List<PlayerInfo> Allplayers;

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
                        checking = false;
                        _timer = 0;
                        GetRegistrationClientRecord();
                        
                    }
                }
               
                break;
            case State.Welcome:
                //check weight sensor on 1st circle
                //move to tutorial
                break;
            case State.Tutorial:
                //check gesture sensor for 1st circle
                //swipe right, left, up and down
                //display 1st question
                break;
            case State.Question1:
                //check gesture sensor for 1st circle
                //swipe left, right to move highlight on the answers
                //swipe up, down to move the highlighted answer to the empty field
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
        }
        else
        {
            checking = true;
            Debug.Log("Old record found ");
            Debug.Log("flag: " + obj.Data["Flag"].Value);
        }
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
}
