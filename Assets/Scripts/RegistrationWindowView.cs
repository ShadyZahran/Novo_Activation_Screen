using PlayFab;
using PlayFab.ClientModels;
using PlayFab.PfEditor.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static Gamemanager;

public class RegistrationWindowView : UIScreen
{
    public TMP_InputField Name;
    public TMP_InputField Email;
    public TMP_InputField Phone;

    Dictionary<string, string> userDataToSend;

    //shady DF4066902FF7C2A1
    //Test F0A7306CF96769F8
    public string targetPlayerID = "F0A7306CF96769F8";




    public void RegisterUser()
    {
        userDataToSend = new Dictionary<string, string>
        {
            {"PlayerID", PlayFabAuthService.PlayFabId},
            {"Name", Name.text},
            {"Email", Email.text},
            {"Phone", Phone.text},
            {"Flag", "New"}
        };
        PlayFabAuthService.Instance.SetUserData(userDataToSend, OnRegisterUserSuccessful, OnRegisterUserFail);
    }

    public void GetUserData()
    {
        //PlayFabAuthService.PlayerInfo result = 
        PlayFabAuthService.Instance.GetUserData(targetPlayerID, OnGetUserDataSuccess, OnGetUserDataFail);

        //if (string.IsNullOrEmpty(result.Name))
        //{
        //    Debug.Log("Empty record");
        //}
        //else if (result.Flag=="New")
        //{
        //    Debug.Log("New record");
        //}
        //else
        //{
        //    Debug.Log("Old record");
        //}
    }

    public void UpdateUser()
    {
        userDataToSend = new Dictionary<string, string>
        {
            {"PlayerID", targetPlayerID},
            {"Name", Name.text},
            {"Email", Email.text},
            {"Phone", Phone.text},
            {"Flag", "Old"}
        };
        PlayFabAuthService.Instance.SetUserData(userDataToSend, OnRegisterUserSuccessful, OnRegisterUserFail);
    }

    public void OnRegisterUserSuccessful(ExecuteCloudScriptResult obj)
    {

        Debug.Log("Successfull setting user data");
        // CloudScript (Legacy) returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        Debug.Log(JsonWrapper.SerializeObject(obj.FunctionResult));
        Debug.Log(obj.FunctionResult);


        ResetFields();
        Gamemanager.instance.ResetMenus();
    }

    private void ResetFields()
    {
        Name.text = string.Empty;
        Email.text = string.Empty;
        Phone.text = string.Empty;
    }

    public void OnRegisterUserFail(PlayFabError obj)
    {

        Debug.Log("Got error setting user data");
        Debug.Log(obj);
        Debug.Log(obj.GenerateErrorReport());
    }

    public void OnGetUserDataSuccess(GetUserDataResult obj)
    {
        PlayerInfo NewPlayer;
        Debug.Log("Got user data:");
        Debug.Log(obj.Data.Keys.Count);
        if (obj.Data == null || !obj.Data.ContainsKey("Flag")) Debug.Log("No flag assigned");
        else
        {
            Debug.Log("Flag: " + obj.Data["Flag"].Value);

        }
        NewPlayer = new PlayerInfo
        {
            Name = obj.Data["Name"].Value,
            Email = obj.Data["Email"].Value,
            Phone = obj.Data["Phone"].Value,
            Flag = obj.Data["Flag"].Value
        };
        if (string.IsNullOrEmpty(NewPlayer.Name))
        {
            Debug.Log("Empty record");
        }
        else if (NewPlayer.Flag == "New")
        {
            Debug.Log("New record");
        }
        else
        {
            Debug.Log("Old record");
        }


    }

    public void OnGetUserDataFail(PlayFabError obj)
    {

        Debug.Log("Got error retrieving user data:");
        Debug.Log(obj.GenerateErrorReport());
    }
}

    

