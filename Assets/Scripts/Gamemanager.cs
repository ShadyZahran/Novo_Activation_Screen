using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanager : MonoBehaviour
{
    public static Gamemanager instance;


    public GameObject Menu_Login, Menu_WelcomeScreen, Menu_Registration;


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
    }

    public void StartGame()
    {
        Menu_Registration.SetActive(true);
        Menu_WelcomeScreen.SetActive(false);
    }

    public void ResetMenus()
    {
        Menu_WelcomeScreen.SetActive(true);
        Menu_Registration.SetActive(false);
    }

    [System.Serializable]
    public struct PlayerInfo
    {
        public string Name, Email, Phone, Flag;
    }
}
