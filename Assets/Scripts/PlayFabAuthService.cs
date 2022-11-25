using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using LoginResult = PlayFab.ClientModels.LoginResult;
using System;
using static Gamemanager;
//using PlayFab.Json;

#if FACEBOOK
using Facebook.Unity;
#endif

/// <summary>
/// Supported Authentication types
/// Note: Add types to there to support more AuthTypes
/// See - https://api.playfab.com/documentation/client#Authentication
/// </summary>
public enum Authtypes
{
    None,
    Silent,
    UsernameAndPassword,
    EmailAndPassword,
    RegisterPlayFabAccount,
    Steam,
    Facebook,
    Google
}

public class PlayFabAuthService
{
    public string targetPlayerID = "F0A7306CF96769F8";
    //Events to subscribe to for this service
    public delegate void DisplayAuthenticationEvent();
    public static event DisplayAuthenticationEvent OnDisplayAuthentication;

    public delegate void LoginSuccessEvent(LoginResult success);
    public static event LoginSuccessEvent OnLoginSuccess;

    public delegate void PlayFabErrorEvent(PlayFabError error);
    public static event PlayFabErrorEvent OnPlayFabError;

    //These are fields that we set when we are using the service.
    public string Email;
    public string Username;
    public string Password;
    public string DisplayName;
    public string AuthTicket;
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    //this is a force link flag for custom ids for demoing
    public bool ForceLink = false;

    //Accessbility for PlayFab ID & Session Tickets
    public static string PlayFabId { get { return _playFabId; } }
    private static string _playFabId;
    public static string SessionTicket { get { return _sessionTicket; } }
    private static string _sessionTicket;

    private const string _LoginRememberKey = "PlayFabLoginRemember";
    private const string _PlayFabRememberMeIdKey = "PlayFabIdPassGuid";
    private const string _PlayFabAuthTypeKey = "PlayFabAuthType";

    public static PlayFabAuthService Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PlayFabAuthService();
            }
            return _instance;
        }
    }
    private static PlayFabAuthService _instance;

    public PlayFabAuthService()
    {
        _instance = this;
    }


    /// <summary>
    /// Remember the user next time they log in
    /// This is used for Auto-Login purpose.
    /// </summary>
    public bool RememberMe
    {
        get
        {
            return PlayerPrefs.GetInt(_LoginRememberKey, 0) == 0 ? false : true;
        }
        set
        {
            PlayerPrefs.SetInt(_LoginRememberKey, value ? 1 : 0);
        }
    }

    /// <summary>
    /// Remember the type of authenticate for the user
    /// </summary>
    public Authtypes AuthType
    {
        get
        {
            return (Authtypes)PlayerPrefs.GetInt(_PlayFabAuthTypeKey, 0);
        }
        set
        {

            PlayerPrefs.SetInt(_PlayFabAuthTypeKey, (int)value);
        }
    }

    /// <summary>
    /// Generated Remember Me ID
    /// Pass Null for a value to have one auto-generated.
    /// </summary>
    private string RememberMeId
    {
        get
        {
            return PlayerPrefs.GetString(_PlayFabRememberMeIdKey, "");
        }
        set
        {
            var guid = string.IsNullOrEmpty(value) ? Guid.NewGuid().ToString() : value;
            PlayerPrefs.SetString(_PlayFabRememberMeIdKey, guid);
        }
    }

    public void ClearRememberMe()
    {
        PlayerPrefs.DeleteKey(_LoginRememberKey);
        PlayerPrefs.DeleteKey(_PlayFabRememberMeIdKey);
    }

    /// <summary>
    /// Kick off the authentication process by specific authtype.
    /// </summary>
    /// <param name="authType"></param>
    public void Authenticate(Authtypes authType)
    {
        AuthType = authType;
        Authenticate();
    }

    /// <summary>
    /// Authenticate the user by the Auth Type that was defined.
    /// </summary>
    public void Authenticate()
    {
        var authType = AuthType;
        Debug.Log(authType);
        switch (authType)
        {
            case Authtypes.None:
                if (OnDisplayAuthentication != null)
                {
                    OnDisplayAuthentication.Invoke();
                }
                break;

            case Authtypes.Silent:
                SilentlyAuthenticate(TestCallback);
                break;

            case Authtypes.EmailAndPassword:
                AuthenticateEmailPassword();
                break;
            case Authtypes.RegisterPlayFabAccount:
                AddAccountAndPassword();
                break;
            case Authtypes.Steam:
                AuthenticateSteam();
                break;
            case Authtypes.Facebook:
                AuthenticateFacebook();
                break;

            case Authtypes.Google:
                AuthenticateGooglePlayGames();
                break;

        }


    }


    /// <summary>
    /// Authenticate a user in PlayFab using an Email & Password combo
    /// </summary>
    private void AuthenticateEmailPassword()
    {
        //Check if the users has opted to be remembered.
        if (RememberMe && !string.IsNullOrEmpty(RememberMeId))
        {
            //If the user is being remembered, then log them in with a customid that was 
            //generated by the RememberMeId property
            PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                CustomId = RememberMeId,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;

                if (OnLoginSuccess != null)
                {
                    //report login result back to subscriber
                    OnLoginSuccess.Invoke(result);
                }
            }, (error) =>
            {
                if (OnPlayFabError != null)
                {
                    //report error back to subscriber
                    OnPlayFabError.Invoke(error);
                }
            });
            return;
        }

        //a good catch: If username & password is empty, then do not continue, and Call back to Authentication UI Display 
        if (!RememberMe && string.IsNullOrEmpty(Email) && string.IsNullOrEmpty(Password))
        {
            OnDisplayAuthentication.Invoke();
            return;
        }


        //We have not opted for remember me in a previous session, so now we have to login the user with email & password.
        PlayFabClientAPI.LoginWithEmailAddress(new LoginWithEmailAddressRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            Email = Email,
            Password = Password,
            InfoRequestParameters = InfoRequestParams
        }, (result) =>
        {
            //store identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //Note: At this point, they already have an account with PlayFab using a Username (email) & Password
            //If RememberMe is checked, then generate a new Guid for Login with CustomId.
            if (RememberMe)
            {
                RememberMeId = Guid.NewGuid().ToString();
                AuthType = Authtypes.EmailAndPassword;
                //Fire and forget, but link a custom ID to this PlayFab Account.
                PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest()
                {
                    CustomId = RememberMeId,
                    ForceLink = ForceLink
                }, null, null);
            }

            if (OnLoginSuccess != null)
            {
                //report login result back to subscriber
                OnLoginSuccess.Invoke(result);
            }
        }, (error) =>
        {
            if (OnPlayFabError != null)
            {
                //Report error back to subscriber
                OnPlayFabError.Invoke(error);
            }
        });
    }

    /// <summary>
    /// Register a user with an Email & Password
    /// Note: We are not using the RegisterPlayFab API
    /// </summary>
    private void AddAccountAndPassword()
    {
        //Any time we attempt to register a player, first silently authenticate the player.
        //This will retain the players True Origination (Android, iOS, Desktop)
        SilentlyAuthenticate((result) =>
        {

            if (result == null)
            {
                //something went wrong with Silent Authentication, Check the debug console.
                OnPlayFabError.Invoke(new PlayFabError()
                {
                    Error = PlayFabErrorCode.UnknownError,
                    ErrorMessage = "Silent Authentication by Device failed"
                });
            }

            //Note: If silent auth is success, which is should always be and the following 
            //below code fails because of some error returned by the server ( like invalid email or bad password )
            //this is okay, because the next attempt will still use the same silent account that was already created.

            //Now add our username & password.
            PlayFabClientAPI.AddUsernamePassword(new AddUsernamePasswordRequest()
            {
                Username = !string.IsNullOrEmpty(Username) ? Username : result.PlayFabId, //Because it is required & Unique and not supplied by User.
                Email = Email,
                Password = Password,

            }, (addResult) =>
            {
                if (OnLoginSuccess != null)
                {
                    //Store identity and session
                    _playFabId = result.PlayFabId;
                    _sessionTicket = result.SessionTicket;

                    //If they opted to be remembered on next login.
                    if (RememberMe)
                    {
                        //Generate a new Guid 
                        RememberMeId = Guid.NewGuid().ToString();
                        //Fire and forget, but link the custom ID to this PlayFab Account.
                        PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest()
                        {
                            CustomId = RememberMeId,
                            ForceLink = ForceLink
                        }, null, null);
                    }

                    //Override the auth type to ensure next login is using this auth type.
                    AuthType = Authtypes.EmailAndPassword;

                    //Report login result back to subscriber.
                    OnLoginSuccess.Invoke(result);

                    //update display name
                    UpdateDisplayName();

                }
            }, (error) =>
            {
                if (OnPlayFabError != null)
                {
                    //Report error result back to subscriber
                    OnPlayFabError.Invoke(error);
                }
            });

        });
    }

    private void AuthenticateFacebook()
    {
#if FACEBOOK
        if (FB.IsInitialized && FB.IsLoggedIn && !string.IsNullOrEmpty(AuthTicket))
        {
            PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest()
            {
                TitleId = PlayFabSettings.TitleId,
                AccessToken = AuthTicket,
                CreateAccount = true,
                InfoRequestParameters = InfoRequestParams
            }, (result) =>
            {
                //Store Identity and session
                _playFabId = result.PlayFabId;
                _sessionTicket = result.SessionTicket;

                //check if we want to get this callback directly or send to event subscribers.
                if (OnLoginSuccess != null)
                {
                    //report login result back to the subscriber
                    OnLoginSuccess.Invoke(result);
                }
            }, (error) =>
            {

                //report errro back to the subscriber
                if (OnPlayFabError != null)
                {
                    OnPlayFabError.Invoke(error);
                }
            });
        }
        else
        {
            if (OnDisplayAuthentication != null)
            {
                OnDisplayAuthentication.Invoke();
            }
        }
#endif 
    }

    private void AuthenticateGooglePlayGames()
    {
#if GOOGLEGAMES
        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            ServerAuthCode = AuthTicket,
            InfoRequestParameters = InfoRequestParams,
            CreateAccount = true
        }, (result) =>
        {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }
        }, (error) =>
        {

            //report errro back to the subscriber
            if (OnPlayFabError != null)
            {
                OnPlayFabError.Invoke(error);
            }
        });
#endif
    }

    private void AuthenticateSteam()
    {

    }


    private void SilentlyAuthenticate(System.Action<LoginResult> callback = null)
    {
#if UNITY_ANDROID  && !UNITY_EDITOR

        //Get the device id from native android
        AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
        AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
        string deviceId = secure.CallStatic<string>("getString", contentResolver, "android_id");

        //Login with the android device ID
        PlayFabClientAPI.LoginWithAndroidDeviceID(new LoginWithAndroidDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            AndroidDevice = SystemInfo.deviceModel,
            OS = SystemInfo.operatingSystem,
            AndroidDeviceId = deviceId,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {

            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });

#elif  UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
        PlayFabClientAPI.LoginWithIOSDeviceID(new LoginWithIOSDeviceIDRequest() {
            TitleId = PlayFabSettings.TitleId,
            DeviceModel = SystemInfo.deviceModel, 
            OS = SystemInfo.operatingSystem,
            DeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) => {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) => {
            //report errro back to the subscriber
            if(callback == null && OnPlayFabError != null){
                OnPlayFabError.Invoke(error);
            }else{
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }
        });
#else
        PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest()
        {
            TitleId = PlayFabSettings.TitleId,
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true,
            InfoRequestParameters = InfoRequestParams
        }, (result) =>
        {
            //Store Identity and session
            _playFabId = result.PlayFabId;
            _sessionTicket = result.SessionTicket;

            //check if we want to get this callback directly or send to event subscribers.
            if (callback == null && OnLoginSuccess != null)
            {
                //report login result back to the subscriber
                OnLoginSuccess.Invoke(result);
            }
            else if (callback != null)
            {
                //report login result back to the caller
                callback.Invoke(result);
            }
        }, (error) =>
        {
            //report errro back to the subscriber
            if (callback == null && OnPlayFabError != null)
            {
                OnPlayFabError.Invoke(error);
            }
            else
            {
                //make sure the loop completes, callback with null
                callback.Invoke(null);
                //Output what went wrong to the console.
                Debug.LogError(error.GenerateErrorReport());
            }

        });
#endif
    }

    public void UnlinkSilentAuth()
    {
        SilentlyAuthenticate((result) =>
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            //Get the device id from native android
            AndroidJavaClass up = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = up.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject contentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");
            AndroidJavaClass secure = new AndroidJavaClass("android.provider.Settings$Secure");
            string deviceId = secure.CallStatic<string>("getString", contentResolver, "android_id");

            //Fire and forget, unlink this android device.
            PlayFabClientAPI.UnlinkAndroidDeviceID(new UnlinkAndroidDeviceIDRequest() {
                AndroidDeviceId = deviceId
            }, null, null);

#elif UNITY_IPHONE || UNITY_IOS && !UNITY_EDITOR
            PlayFabClientAPI.UnlinkIOSDeviceID(new UnlinkIOSDeviceIDRequest()
            {
                DeviceId = SystemInfo.deviceUniqueIdentifier
            }, null, null);
#else
            PlayFabClientAPI.UnlinkCustomID(new UnlinkCustomIDRequest()
            {
                CustomId = SystemInfo.deviceUniqueIdentifier
            }, null, null);
#endif

        });
    }


    void TestCallback(LoginResult result)
    {
        Debug.Log(result.PlayFabId);
    }

    private void UpdateDisplayName()
    {
        Debug.Log("inside update display name");
        //update display name
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest()
        {
            DisplayName = DisplayName,
        }, (resultUpdateDisplay) =>
        {
            //Report login result back to subscriber.
            Debug.Log("display name update success: " + resultUpdateDisplay.ToString());
        }, (error) =>
        {
            if (OnPlayFabError != null)
            {
                //Report error result back to subscriber
                OnPlayFabError.Invoke(error);
            }
        });
    }

    public void GetLeaderboard()
    {
        Debug.Log("inside get leaderboard");
        PlayFabClientAPI.GetLeaderboard(new GetLeaderboardRequest()
        {
            StatisticName = "HighScore",
            MaxResultsCount = 20
        }, (resultLeaderboard) =>
        {
            //Debug.Log(resultLeaderboard.Leaderboard);
            //LeaderboardManager.instance.InitLeaderboard(resultLeaderboard.Leaderboard);
            //foreach (var entry in resultLeaderboard.Leaderboard)
            //{
            //    Debug.Log("name: " + entry.DisplayName + " rank: " + entry.Position + " score: " + entry.StatValue);
            //}
        }, (error) =>
        {
            if (OnPlayFabError != null)
            {
                //Report error result back to subscriber
                OnPlayFabError.Invoke(error);
            }
        });
    }

    public void SetUserData(Dictionary<string, string> userData, Action<ExecuteCloudScriptResult> OnSuccess, Action<PlayFabError> OnFail)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()

        {
            FunctionName = "userRegistered",
            FunctionParameter = new
            {
                Name = userData["Name"],
                PlayerID = userData["PlayerID"],
                //Email = userData["Email"],
                //Phone = userData["Phone"],
                Flag = userData["Flag"]
            }
        }, OnSuccess, OnFail);

    }
    

    //public void GetUserData()
    //{
    //    PlayFabClientAPI.GetUserData(new GetUserDataRequest()
    //    {
    //        PlayFabId = "DF4066902FF7C2A1",
    //        //Keys = null
    //    }, result =>
    //    {
    //        Debug.Log("Got user data:");
    //        Debug.Log(result.Data.Keys.Count);
    //        Debug.Log(PlayFabAuthService.PlayFabId);
    //        if (result.Data == null || !result.Data.ContainsKey("damage")) Debug.Log("No damage");
    //        else Debug.Log("damage: " + result.Data["damage"].Value);
    //    }, (error) =>
    //    {
    //        Debug.Log("Got error retrieving user data:");
    //        Debug.Log(error.GenerateErrorReport());
    //    });

    //}


    public void GetUserData(string playerID, Action<GetUserDataResult> OnSuccess, Action<PlayFabError> OnFail)
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest()
        {
            PlayFabId = playerID,
        }, OnSuccess, OnFail);
    }

    public void UpdateUser(string playerID, PlayerInfo player)
    {
        Dictionary<string, string> userDataToSend = new Dictionary<string, string>
        {
            {"PlayerID", playerID},
            {"Name", player.Name},
            //{"Email", player.Email},
            //{"Phone", player.Phone},
            {"Flag", "InProgress"}
        };
        PlayFabAuthService.Instance.SetUserData(userDataToSend, OnUpdateUserSuccessful, OnUpdateUserFail);
    }

    public void OnUpdateUserSuccessful(ExecuteCloudScriptResult obj)
    {

        Debug.Log("Successfull updating user data");
        // CloudScript (Legacy) returns arbitrary results, so you have to evaluate them one step and one parameter at a time
        //Debug.Log(JsonWrapper.SerializeObject(obj.FunctionResult));
        Debug.Log(obj.FunctionResult);


        //ResetFields();
        Gamemanager.instance.ResetMenus();
    }

    public void OnUpdateUserFail(PlayFabError obj)
    {
        Debug.Log("Got error updating user data");
        Debug.Log(obj);
        Debug.Log(obj.GenerateErrorReport());
    }

    public void FinalizeUser(string playerID, PlayerInfo player)
    {
        Dictionary<string, string> userDataToSend = new Dictionary<string, string>
        {
            {"PlayerID", playerID},
            {"Name", player.Name},
            //{"Email", player.Email},
            //{"Phone", player.Phone},
            {"Flag", "Old"}
        };
        PlayFabAuthService.Instance.SetUserData(userDataToSend, OnUpdateUserSuccessful, OnUpdateUserFail);
    }

}
