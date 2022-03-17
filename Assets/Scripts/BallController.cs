using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    PhotonView view;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public string xd = "siema";
    // Update is called once per frame
    void Update()
    {
        
    }
    public Player GetOwner()
    {
        view = GetComponent<PhotonView>();
        return view.Controller;
    }
    private void Awake()
    {
        view = GetComponent<PhotonView>();

    }
}
