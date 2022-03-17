using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class MenuController : MonoBehaviourPunCallbacks
{

    [SerializeField] private GameObject usernameMenu;
    [SerializeField] private GameObject controlPanel;

    [SerializeField] private InputField UsernameInput;
    [SerializeField] private InputField CreateGameInput;
    [SerializeField] private InputField JoinGameInput;

    [SerializeField] private GameObject StartButton;





    // Start is called before the first frame update
    void Start()
    {
        usernameMenu.SetActive(true);
    }

    // Update is called once per frame
    public void ChangeUserNameInput()
    {

        if (UsernameInput.text.Length >= 2)
            StartButton.SetActive(true);
        else
            StartButton.SetActive(false);
    }
    public void SetUserName()
    {
        usernameMenu.SetActive(false);
        PlayerPrefs.SetString("username", UsernameInput.text);
    }
    
    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(CreateGameInput.text);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(JoinGameInput.text);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("russianPitch");
    }
}
