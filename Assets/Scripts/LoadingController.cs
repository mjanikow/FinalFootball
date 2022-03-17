using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;


public class LoadingController : MonoBehaviourPunCallbacks
{



    private void Awake()
    {
    }

    private void Start()
    {
          PhotonNetwork.ConnectUsingSettings();

    } 

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("connected"); 
    }
    public override void OnJoinedLobby()
    {
        SceneManager.LoadScene("Menu");
    }

    
}
