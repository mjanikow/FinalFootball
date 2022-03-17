using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;


public class GameManager : MonoBehaviour
{

    public GameObject PlayerPrefab;
    public GameObject BallPrefab;


    public float minX = 350f;
    public float maxX = 400f;
    private float y = 2f;
    public float minZ = -70f;
    public float maxZ = -120f;

    private void Start()
    {
        Vector3 randomPos = new Vector3(Random.Range(minX, maxX), y, Random.Range(minZ, maxZ));
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, randomPos, Quaternion.identity);



        //GuyController controller = player.GetComponent<GuyController>();
        //SimpleMouseOrbit camera = controller.getCam();
        //camera.SetTarget(player.transform);
    }
}

   


