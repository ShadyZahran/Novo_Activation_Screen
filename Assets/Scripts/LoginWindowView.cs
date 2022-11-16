using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

#if FACEBOOK
using Facebook.Unity;
#endif

#if GOOGLEGAMES
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif


public class LoginWindowView : UIScreen
{
    //Debug Flag to simulate a reset
    public bool ClearPlayerPrefs;

    //Meta fields for objects in the UI
    public TMP_InputField Username;
    public TMP_InputField Password;
    public TMP_InputField ConfirmPassword;
    public TMP_InputField DisplayName;

    public Button LoginButton;
    public Button RegisterButton;
    public Button CancelRegisterButton;
    public Toggle RememberMe;
    public Button TutorialContinueButton;
    //public PlayFab.ProgressBarView ProgressBar;

    //Meta references to panels we need to show / hide
    public GameObject RegisterPanel;
    public GameObject Panel;
    public GameObject Next;

    public TMPro.TextMeshProUGUI LoginStatusText;
    public TMPro.TextMeshProUGUI RegisterStatusText;

    public const string RegisterMessage = "User account not found, If you would like to register with these credentials instead, enter your display name and retype your password below and click register.";

    //Settings for what data to get from playfab on login.
    public GetPlayerCombinedInfoRequestParams InfoRequestParams;

    //Reference to our Authentication service
    private PlayFabAuthService _AuthService = PlayFabAuthService.Instance;


    public void Awake()
    {
        if (ClearPlayerPrefs)
        {
            _AuthService.UnlinkSilentAuth();
            _AuthService.ClearRememberMe();
            _AuthService.AuthType = Authtypes.None;
        }

        //Set our remember me button to our remembered state.
        RememberMe.isOn = _AuthService.RememberMe;

        //Subscribe to our Remember Me toggle
        RememberMe.onValueChanged.AddListener((toggle) =>
        {
            _AuthService.RememberMe = toggle;
        });
    }

    public void Start()
    {

        //Hide all our panels until we know what UI to display
        //Panel.SetActive(false);
        //Next.SetActive(false);
        //RegisterPanel.SetActive(false);

        //Subscribe to events that happen after we authenticate
        PlayFabAuthService.OnDisplayAuthentication += OnDisplayAuthentication;
        PlayFabAuthService.OnLoginSuccess += OnLoginSuccess;
        PlayFabAuthService.OnPlayFabError += OnPlayFaberror;


        //Bind to UI buttons to perform actions when user interacts with the UI.
        LoginButton.onClick.AddListener(OnLoginClicked);
        //PlayAsGuestButton.onClick.AddListener(OnPlayAsGuestClicked);
        //LoginWithFacebook.onClick.AddListener(OnLoginWithFacebookClicked);
        //LoginWithGoogle.onClick.AddListener(OnLoginWithGoogleClicked);
        RegisterButton.onClick.AddListener(OnRegisterButtonClicked);
        CancelRegisterButton.onClick.AddListener(OnCancelRegisterButtonClicked);
        //Set the data we want at login from what we chose in our meta data.
        _AuthService.InfoRequestParams = InfoRequestParams;

        //Start the authentication process.
        _AuthService.Authenticate();
    }


    /// <summary>
    /// Login Successfully - Goes to next screen.
    /// </summary>
    /// <param name="result"></param>
    private void OnLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        Debug.LogFormat("Logged In as: {0}", result.PlayFabId);
        LoginStatusText.text = "Login Success";
        //Show our next screen if we logged in successfully.
        Panel.SetActive(false);
        Next.SetActive(true);
        Gamemanager.instance.GameState = Gamemanager.State.Start;
        //UIScreenController.Instance.Show(UIScreenController.MainScreenId, false, true);
        //UIScreenController.Instance.Show(UIScreenController.TutorialScreenId, false, true, false, Tween.TweenStyle.EaseOut, OnTutorialScreenShown);
    }

    private void OnTutorialScreenShown()
    {
        TutorialContinueButton.onClick.RemoveAllListeners();
        TutorialContinueButton.onClick.AddListener(ShowMainMenu);
    }

    private void ShowMainMenu()
    {
        //UIScreenController.Instance.Show(UIScreenController.MainScreenId, false, true);
    }

    /// <summary>
    /// Error handling for when Login returns errors.
    /// </summary>
    /// <param name="error"></param>
    private void OnPlayFaberror(PlayFabError error)
    {
        //There are more cases which can be caught, below are some
        //of the basic ones.
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidEmailAddress:
            case PlayFabErrorCode.InvalidPassword:
            case PlayFabErrorCode.InvalidEmailOrPassword:
                Debug.Log("Invalid Email or Password");
                LoginStatusText.text = "Invalid Email or Password";
                break;

            case PlayFabErrorCode.AccountNotFound:
                RegisterPanel.SetActive(true);
                return;
            default:
                Debug.Log(error.GenerateErrorReport());
                LoginStatusText.text = error.GenerateErrorReport();
                break;

        }

        //Also report to debug console, this is optional.
        Debug.Log(error.ErrorMessage);
        Debug.LogError(error.GenerateErrorReport());

    }

    /// <summary>
    /// Choose to display the Auth UI or any other action.
    /// </summary>
    private void OnDisplayAuthentication()
    {

#if FACEBOOK
        if (FB.IsInitialized)
        {
            Debug.LogFormat("FB is Init: AccessToken:{0} IsLoggedIn:{1}",AccessToken.CurrentAccessToken.TokenString, FB.IsLoggedIn);
            if (AccessToken.CurrentAccessToken == null || !FB.IsLoggedIn)
            {
                Panel.SetActive(true);
            }
        }
        else
        {
            Panel.SetActive(true);
            Debug.Log("FB Not Init");
        }
#else
        //Here we have choses what to do when AuthType is None.
        Panel.SetActive(true);
        //UIScreenController.Instance.Show(UIScreenController.LoginScreenId, false, false);
#endif
        /*
         * Optionally we could Not do the above and force login silently
         * 
         * _AuthService.Authenticate(Authtypes.Silent);
         * 
         * This example, would auto log them in by device ID and they would
         * never see any UI for Authentication.
         * 
         */
    }


    /// <summary>
    /// Login Button means they've selected to submit a username (email) / password combo
    /// Note: in this flow if no account is found, it will ask them to register.
    /// </summary>
    private void OnLoginClicked()
    {
        //ProgressBar.UpdateLabel(string.Format("Logging In As {0} ...", Username.text));
        //ProgressBar.UpdateProgress(0f);
        //ProgressBar.AnimateProgress(0, 1, () =>
        //{
        //    second loop
        //    ProgressBar.UpdateProgress(0f);
        //    ProgressBar.AnimateProgress(0, 1, () =>
        //    {
        //        ProgressBar.UpdateLabel(string.Empty);
        //        ProgressBar.UpdateProgress(0f);
        //    });
        //});

        _AuthService.Email = Username.text;
        _AuthService.Password = Password.text;
        _AuthService.Authenticate(Authtypes.EmailAndPassword);
    }

    /// <summary>
    /// No account was found, and they have selected to register a username (email) / password combo.
    /// </summary>
    private void OnRegisterButtonClicked()
    {

        if (Password.text != ConfirmPassword.text)
        {
            Debug.Log("Passwords do not Match.");
            RegisterStatusText.text = "Passwords do not Match.";
            return;
        }

        if (string.IsNullOrEmpty(DisplayName.text))
        {
            Debug.Log("Display name is empty.");
            RegisterStatusText.text = "Display name is empty.";
            return;
        }

        Debug.Log(string.Format("Registering User {0} ...", Username.text));
        //ProgressBar.UpdateProgress(0f);
        //ProgressBar.AnimateProgress(0, 1, () =>
        //{
        //    //second loop
        //    ProgressBar.UpdateProgress(0f);
        //    ProgressBar.AnimateProgress(0, 1, () =>
        //    {
        //        ProgressBar.UpdateLabel(string.Empty);
        //        ProgressBar.UpdateProgress(0f);
        //    });
        //});

        _AuthService.Email = Username.text;
        _AuthService.Password = Password.text;
        _AuthService.DisplayName = DisplayName.text;
        _AuthService.Authenticate(Authtypes.RegisterPlayFabAccount);
    }

    /// <summary>
    /// They have opted to cancel the Registration process.
    /// Possibly they typed the email address incorrectly.
    /// </summary>
    private void OnCancelRegisterButtonClicked()
    {
        //Reset all forms
        Username.text = string.Empty;
        Password.text = string.Empty;
        ConfirmPassword.text = string.Empty;
        DisplayName.text = string.Empty;
        LoginStatusText.text = string.Empty;
        RegisterStatusText.text = RegisterMessage;
        RememberMe.isOn = false;
        //Show panels
        RegisterPanel.SetActive(false);
        //Next.SetActive(false);
    }



    public void OnSelectRegisterInputField()
    {
        RegisterStatusText.text = RegisterMessage;
    }

    public override void OnShowing(object data)
    {
        base.OnShowing(data);
        //SoundManager.instance.PlayFade(SoundManager.MainScreenMusicId);
    }


}
