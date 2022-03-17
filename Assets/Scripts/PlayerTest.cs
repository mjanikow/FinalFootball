using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTest : MonoBehaviour
{
   // public PhotonView photonView;
    public Rigidbody rb;
    public SimpleMouseOrbit playerCamera;

    private void Awake()
    {
        
        
            playerCamera.SetTarget(this.transform);
        
    }
    private void Update()
    {
        
    }
    private void CheckInput()
    {
        var move = new Vector3(Input.GetAxisRaw("Horizontal"), 0);
        transform.position += move * 5f * Time.deltaTime;
        
    }
}
