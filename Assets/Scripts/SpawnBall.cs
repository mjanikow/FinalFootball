using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnBall : MonoBehaviour
{
    public int spawnLimit = 5;
    public int counter = 0;
    public GameObject trainingBall;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (counter < spawnLimit)
        {
            Vector3 ballPos = transform.position;
            ballPos.x += 5f;
            GameObject theBall = PhotonNetwork.Instantiate(trainingBall.name, ballPos, Quaternion.identity);
        }
        counter++;
    }
}
