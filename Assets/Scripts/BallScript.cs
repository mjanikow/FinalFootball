using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    GameObject hand = null;
    Rigidbody rb;
    public float rotationForce = 10f;
    public float rotationForceRotate = 10f;
    public float rotationDecrease = 1f;
    public float rotationRotateDecrease = 1f;
    public float rotationLowForce = 10f;
    public float gkDivingForce = 100f;
    float lobPower = 100f;
    public float lobpowerSave;
    public float lobDecrease = 50f;
    public float lobVelocityDecreaseVal = 0.75f;
    float lobVelocityDecreaseValv2 = 0f;
    public float kickUpForce = 1f;
    public float kickUpSlow = 50f;
    PhotonView view;
    public string spinState;

    public float kickForce = 10f;
    public float sprintKickForce = 10f;
    public float kickPassForce = 10f;
    public float kickJogaForce = 10f;
    public float isGroundedHeight = 1f;
    public float rollBonus = 2f;
    public GameObject shadow;

    Vector3 previousPosition;
    Vector3 lastMoveDirection;
    Vector3 heightDirection;
    Vector3 moveRotation;


    bool isGrounded = true;
    bool isLobbed = false;

    public float thePower = 0f;
    private float theLowPower = 10f;
    private float theSecondPower = 0f;
    private float dribCounter = 0f;
    private float dribKickTime = 0f;
    private float lastDribKickTime = 0f;
    private float LastPlayerDribID = -1f;
    private float rotationDecrease_safe;
    private float lastKickTime = 0f;
    private bool isGoingUp = false;
    private float lastHeight = 0f;
    private bool IsKnuckleBall;

    private Vector3 positionBeforeShoot;
    private bool wasFPressed = true;
    Vector3 LastMoveRotation; float lastKickForce; float LastKickPower; float LastSpeedBonus; string lastRotateState; float lastRotationForce;
    Vector3 savedPos;
    Rigidbody shadowRb;
    private bool wasKicked = false;
    // Start is called before the first frame update
    void Start()
    {
        shadowRb = shadow.GetComponent<Rigidbody>();
        view = GetComponent<PhotonView>();

        rb = GetComponent<Rigidbody>();
        previousPosition = transform.position;
        lastMoveDirection = Vector3.zero;
        rotationDecrease_safe = rotationDecrease;
        positionBeforeShoot = transform.position;
        lobPower = lobpowerSave;
        lobVelocityDecreaseValv2 = lobVelocityDecreaseVal;
    }
    // Update is called once per frame
    bool canIRotatePlease = false;
    float timer = 0f;
    string rotatePowerState = "high";
    GuyController lastClosestPlayer = null;
    Player owner;
    float rotationForceSave = 0;
    bool owningTransfer = true;
    public void setValues(bool isLobbedX, bool isKnuckleballX, float rotationForceX, string rotateStateX)
    {
        isLobbed = isLobbedX; IsKnuckleBall = isKnuckleballX; rotationForce = rotationForceX; rotateState = rotateStateX;
    }
    void Update()
    {

        var foundPlayers = FindObjectsOfType<GuyController>();
        GuyController closestPlayer = GetClosestPlayer(foundPlayers);


        if (!closestPlayer.Equals(lastClosestPlayer))
        {
            view.TransferOwnership(closestPlayer.GetPlayer());
            //Debug.Log("prosba o transfer" + dribCounter++ + ": " + view.Owner);

            lastClosestPlayer = closestPlayer;
            owner = closestPlayer.GetPlayer();
            owningTransfer = true;
        }
        if (owner.Equals(view.Owner) && owningTransfer)
        {
            //closestPlayer.zrobMiLoda(this,isLobbed, IsKnuckleBall, rotationForce, rotateState);
            //Debug.Log("zrealizowany transfer" + dribCounter++ + ": " + view.Owner);
            owningTransfer = false;
        }



        if (Input.GetMouseButtonDown(0)) isCatched = false;

        //if (transform.position.y < 13)
         //   shadow.SetActive(false);
       // else
        //{
          //  shadow.SetActive(true);
            shadow.transform.position = transform.position;
            shadowRb.transform.rotation = Quaternion.Euler(90, 0, 0);
       // }
        



        if (Input.GetKeyDown("f"))
            wasFPressed = true;

        timer = Time.time;

        if (Input.GetKeyDown("f"))
            Invoke("savePosition", 0.2f);
        //Debug.DrawRay(transform.position, direction * 10f, Color.red);

        if (isCatched)
        {
            rb.angularVelocity = Vector3.zero;

            Vector3 pos = hand.transform.position;
            pos.x -= 1.5f;
            transform.position = pos;
        }
        if(isGrounded)
            rb.maxAngularVelocity = 7f;

    }
    void savePosition() { savedPos = transform.position; }

    private float checkCountDown = 0.2f;
    string rotateState = "noRotate";
    public Vector3 rotationDir;
    public Vector3 torqueDir;
    bool isFirstLob = false;

    GuyController GetClosestPlayer(GuyController[] enemies)
    {
        GuyController tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GuyController t in enemies)
        {
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                tMin = t;
                minDist = dist;
            }
        }
        return tMin;
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.5f);
        //Vector3 shadowPos = transform.position;
        //.y += 10f;
        
        if(isGrounded && !wasKicked)
        {
            rb.maxAngularVelocity = 7f;
        }


        if (transform.position != previousPosition)
        {
            lastMoveDirection = (transform.position - previousPosition).normalized;
            previousPosition = transform.position;
        }


        heightDirection = lastMoveDirection;
        heightDirection.x = 0;
        heightDirection.z = 0;
        isGoingUp = (heightDirection.y > 0);

        Vector3 rotationDirTest = Quaternion.Euler(0, 0, 0) * lastMoveDirection;
        rotationDirTest.x = 0;

        lastMoveDirection.y = 0;



        //Debug.DrawRay(transform.position, rotationDirTest * 10f, Color.red);
        if (isLobbed)
        {
            


            Vector3 lobRotDir;
            if (isGoingUp)
            {
                rb.velocity = rb.velocity * lobVelocityDecreaseVal;
                lobVelocityDecreaseVal += 0.05f;
                lobRotDir = Quaternion.Euler(0, -90f, 0) * lastMoveDirection;
                rb.maxAngularVelocity = 20f;
                rb.AddForce(Vector3.up * lobPower);
                rb.AddTorque(lobRotDir * kickPower/3 * 50f);
                lobPower = lobPower - rotationDecrease * 100;

            }
            if (lobPower < 0) isLobbed = false;
            
        }

        if (wasKicked && rotationForce > 4)
        {

            


            if (rotateState == "rotateLeft")
            {
                rotationDir = Quaternion.Euler(0, 90f, 0) * lastMoveDirection;
                torqueDir = Quaternion.Euler(90f, 0, 0) * lastMoveDirection;
            }
            else
            {
                rotationDir = Quaternion.Euler(0, -90f, 0) * lastMoveDirection;
                torqueDir = Quaternion.Euler(-90f, 0, 0) * lastMoveDirection;
            }

            //rotationDir = Vector3.down;
            //Quaternion deltaRotation = Quaternion.Euler(Quaternion.Euler(0, -90f, 0) * lastMoveDirection * Time.fixedDeltaTime * theSecondPower);



            if (isGoingUp && !isGrounded)
            {


                if (rotatePowerState == "high")
                {
                    if (IsKnuckleBall)
                    {
                        rb.maxAngularVelocity = 10f;
                        rb.AddForce(rotationDir * thePower/1.8f);
                        rb.AddTorque(torqueDir * thePower/2f * 3 * (thePower / 30f) * (thePower / 30f) * (thePower / 30f));
                    }
                    else
                    {
                        //Debug.Log("krenceUP" + dribCounter++);
                        GuyController closestPlayer = GetClosestPlayer(FindObjectsOfType<GuyController>());

                        //closestPlayer.spinMeDaddy(thePower, rotationDir,torqueDir);
                        spinState = "spinMeDaddy";        
                        //rb.maxAngularVelocity = 20f;                        
                        //rb.AddForce(rotationDir * thePower);
                        //rb.AddTorque(torqueDir * thePower * 3 * (thePower / 30f) * (thePower / 30f) * (thePower / 30f));
                    }
                    

                }
                

            }
            else if (isGrounded && thePower > 0)
            {
                rb.maxAngularVelocity = 7f;

                
                    thePower = thePower - rotationDecrease * 2;
                    //rb.AddTorque(lastMoveDirection * thePower);
                    GuyController closestPlayer = GetClosestPlayer(FindObjectsOfType<GuyController>());
                    //closestPlayer.spinMeLowDaddy(thePower, rotationDir);
                    spinState = "spinMeLowDaddy";

                //rb.AddForce(rotationDir * thePower);




            }
            else if (!isGrounded)
            {
                rb.maxAngularVelocity = 20f;

                if (IsKnuckleBall)
                {
                    if (rotateState.Equals("rotateRight"))
                    {
                        rb.angularVelocity = Vector3.zero;
                        rb.AddTorque(torqueDir * thePower * 1.5f);
                        thePower = thePower / 1.3f;
                        rotateState = "rotateLeft";
                    }
                    else
                    {
                        rb.angularVelocity = Vector3.zero;
                        rb.AddTorque(torqueDir * thePower * 1.5f);
                        thePower = thePower / 1.3f;
                        rotateState = "rotateRight";
                    }
                    IsKnuckleBall = false;
                }

                
                    thePower = thePower - rotationDecrease;
                    if (thePower < 0f) thePower = 0f;
                    // rb.AddTorque(lastMoveDirection * thePower * 1000f);
                    // Debug.Log("krenceDOWN" + dribCounter++); 
                    GuyController closestPlayer = GetClosestPlayer(FindObjectsOfType<GuyController>());
                //closestPlayer.spinMeLowDaddy(thePower, rotationDir);
                spinState = "spinMeLowDaddy";

                //rb.AddForce(rotationDir * thePower);


            }

            if (thePower == 0) 
            {
                spinState = "dontSpinMeDaddy";
                wasKicked = false;
            }
            if (theLowPower == 0)
            {
                spinState = "dontSpinMeDaddy";
                wasKicked = false;
            }



        }




    }
    float startKickTime = 0f;
    public void ReapeatLastKick()
    {
        try
        {
            rotationForce = lastRotationForce;
            thePower = rotationForce;
            theLowPower = rotationLowForce;
            transform.position = positionBeforeShoot;
            rb.velocity = Vector3.zero;
            rotateState = lastRotateState;
            rb.AddForce(LastMoveRotation * lastKickForce * LastKickPower * LastSpeedBonus);
            wasKicked = true;
            lastKickTime = Time.time;
        }catch(Exception e)
        {

        }
        


    }
    public void SetRotation(string rotation)
    {
        rotateState = rotation;
    }

    Vector3 heading;
    float distance;
    Vector3 direction;
    float shootPower;
    GuyController playerer;
    public void RandomShoot(GuyController player)
    {

        try
        {

            playerer = player;
            transform.position = savedPos;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;


            shootPower = UnityEngine.Random.Range(1900, 2400);
            int rotationRandom = UnityEngine.Random.Range(0, 3);
            if (rotationRandom == 0)
                rotateState = "noRotate";
            else if (rotationRandom == 1)
                rotateState = "rotateLeft";
            else
                rotateState = "rotateRight";


            Invoke("Shoot", 1f);
        }catch(Exception e)
        {

        }

        

        
    }
    bool tackleBusy = false;

    void TackeNotBusy()
    {
        tackleBusy = false;
    }
    private void Shoot()
    {
        Vector3 shootDir = playerer.transform.position;

        shootDir.y += 7f;

        float randomY = UnityEngine.Random.Range(0f, 20f);
        float randomX = UnityEngine.Random.Range(-15f, 15f);

        shootDir.x += randomX;
        shootDir.y += randomY;


        heading = shootDir - transform.position;
        distance = heading.magnitude;
        direction = heading / distance;

        rb.AddForce(direction * shootPower);
        wasKicked = true;
        lastKickTime = Time.time;
    }
    public bool isCatched = false;

    bool isDribLocked = false;
    bool isDribLockedL = false;
    bool isSpecialShoot = false;


    void UnlockDrib()
    {
        isDribLocked = false;
        isDribLockedL = false;
    }
    void UnlockSpecialShoot()
    {
        isSpecialShoot = false;
    }
    float kickPower = 0f;

    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "upLeg" && !isSpecialShoot)
        {

            GuyController player = other.GetComponentInParent<GuyController>();

            string playerDribState = player.getDribState();
            string playerKickState = player.getKickState();
            SimpleMouseOrbit cam = player.getCam();
            float speedBonus = 1 - (rb.velocity.magnitude / 300);


            

            if (playerKickState.Equals("kickup e") || playerKickState.Equals("kickup q"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
            }
            else if (playerKickState.Equals("kickup W"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                rb.AddForce(player.transform.forward * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup S"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -180f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup AW"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -45f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup DW"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 45f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup A"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup D"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup AS"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -135f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup DS"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 135f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }

        }



        if (other.tag == "rightFoot" || other.tag == "upLeg" && !isSpecialShoot)
        {
            GuyController player = other.GetComponentInParent<GuyController>();

            string playerDribState = player.getDribState();
            string playerKickState = player.getKickState();
            SimpleMouseOrbit cam = player.getCam();
            float speedBonus = 1 - (rb.velocity.magnitude / 300);



            

            if (playerKickState.Equals("kickup") && transform.position.y > 3.6f)
                {
                    
                dribKickTime = Time.time;

                if (dribKickTime - lastDribKickTime > 0.12f)
                {
                    thePower = thePower / 4f;

                    if (!player.IsSprinting())
                    {
                        if (!player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.up;
                            rb.velocity = rb.velocity * 0.05f;

                            rb.AddForce(moveRotation * kickForce * player.kickupForce / 6);
                        }
                        else if (!player.IsA_PPressed() && player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            Debug.Log("kickup S " + dribCounter++);
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(180, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (player.IsA_PPressed() && player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-45, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (!player.IsA_PPressed() && player.IsW_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(45, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(90, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-90, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(135, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                        else if (player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-60, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-135, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 6);
                        }
                    }else
                    {
                        if (!player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.up;
                            rb.velocity = rb.velocity * 0.05f;

                            rb.AddForce(moveRotation * kickForce * player.kickupForce / 5);
                        }
                        else if (!player.IsA_PPressed() && player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(180, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (player.IsA_PPressed() && player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-45, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (!player.IsA_PPressed() && player.IsW_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(45, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(90, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-90, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (!player.IsA_PPressed() && !player.IsW_PPressed() && player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(135, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                        else if (player.IsA_PPressed() && !player.IsW_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                        {
                            player.DisableRightFoot();
                            player.ResetDribState();
                            moveRotation = player.transform.forward * -1;
                            rb.velocity = rb.velocity * 0.05f;
                            Vector3 newDir = Quaternion.AngleAxis(-30, player.transform.right) * player.transform.forward;
                            newDir = Quaternion.AngleAxis(-135, player.transform.up) * newDir;
                            rb.AddForce(newDir * kickForce * player.kickupForce / 5);
                        }
                    }

                    

                }
                lastDribKickTime = Time.time;


            }
            



            if (playerKickState.Equals("kickup e") || playerKickState.Equals("kickup q"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
            }
            else if (playerKickState.Equals("kickup W"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                rb.AddForce(player.transform.forward * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup S"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -180f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup AW"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -45f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup DW"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 45f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup A"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup D"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup AS"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -135f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }
            else if (playerKickState.Equals("kickup DS"))
            {
                player.ResetDribState();
                moveRotation = player.transform.up;
                rb.velocity = Vector3.zero;
                rb.AddForce(moveRotation * kickForce * player.kickupForce / 10);
                moveRotation = cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 135f, 0) * moveRotation;
                rb.AddForce(newDir * kickForce * player.kickupForce / 30 * 10f / player.kickupForce * 1.35f);
            }

            if (playerDribState.Equals("roll stay"))
            {
                rb.velocity = rb.velocity * 0.5f;// * Time.deltaTime;
                rb.angularVelocity = rb.angularVelocity * 0.5f;// * Time.deltaTime;
            }
            else if (playerDribState.Equals("roll S"))
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -180f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
            else if(playerDribState.Equals("roll AS"))
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -135f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
            else if(playerDribState.Equals("roll DS"))
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -225f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
            else if(playerDribState.Equals("roll W"))
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 0, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
            else if(playerDribState.Equals("roll AW") && !player.isSpecialShooting)
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -45f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
            else if(playerDribState.Equals("roll DW") && !player.isSpecialShooting)
            {
                player.DisableRightFoot();
                rb.velocity = Vector3.zero;
                SimpleMouseOrbit cam2 = player.getCam();
                moveRotation = cam2.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 45f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                rb.AddForce(newDir * kickForce * 0.45f);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "leftfoot" || other.tag == "rightfoot" || other.tag == "player" || other.tag == "upLeg")
        {
            GuyController player = other.GetComponentInParent<GuyController>();
            if (player.playerDribState.Equals("slideR") || player.playerDribState.Equals("tackleR") && !tackleBusy)
            {               
                tackleBusy = true;
                Invoke("TackeNotBusy", 0.45f);
                rb.velocity = rb.velocity * 0.5f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward;
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.playerDribState.Equals("slideL") || player.playerDribState.Equals("slideR"))
                    rb.AddForce(newDir * kickForce * 1f);
                else
                    rb.AddForce(newDir * kickForce * 1.4f);

            }
            else if (player.playerDribState.Equals("slideL") || player.playerDribState.Equals("tackleL") && !tackleBusy)
            {
                tackleBusy = true;
                Invoke("TackeNotBusy", 0.45f);
                rb.velocity = rb.velocity * 0.5f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward;
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.playerDribState.Equals("slideL") || player.playerDribState.Equals("slideR"))
                    rb.AddForce(newDir * kickForce * 1f);
                else
                    rb.AddForce(newDir * kickForce * 1.4f);
            }

        }

        if (other.tag == "leftFoot")
        {

            GuyController player = other.GetComponentInParent<GuyController>();

            string playerDribState = player.getDribState();
            float speedBonus = (rb.velocity.magnitude / 200) + 1;
            string playerKickState = player.getKickState();

            if(playerKickState.Equals("volley D"))
            {
                player.DisableRightFootL();
                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();
                lobPower = 250f;
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;
                thePower = player.rotationForce;
                rotationForce = player.rotationForce;
                rotateState = player.RotationState;

                if (rotationForce < 11)
                    rotationForce = 0;

                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickPassForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }
                rb.velocity = rb.velocity * 0.1f;

                moveRotation = player.cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;

                rb.AddForce(newDir * kickJogaForce * kickPower * speedBonus * 0.9f);

                wasKicked = true;
                lastKickTime = Time.time;
                player.setKickState("");
            }
            else if (playerKickState.Equals("volley A"))
            {
                player.DisableRightFootL();
                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();
                lobPower = 250f;
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;
                thePower = player.rotationForce;
                rotateState = player.RotationState;                        
                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;

                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickPassForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }
                rb.velocity = rb.velocity * 0.1f;

                moveRotation = player.cam.transform.forward;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;

                rb.AddForce(newDir * kickJogaForce * kickPower * speedBonus * 0.9f);

                wasKicked = true;
                lastKickTime = Time.time;
                player.setKickState("");
            }
            else if (playerKickState.Equals("przewrotka") && player.WaitForAction)
            {
                isSpecialShoot = true;
                Invoke("UnlockSpecialShoot", 0.5f);

                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                dribCounter++;

                rotationForce = player.rotationForce;
                rotateState = player.RotationState;

                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }


                Vector3 mirroredVector = Vector3.Reflect(Vector3.Reflect(player.cam.transform.forward, Vector3.up), player.transform.forward * -1);


                rb.velocity = Vector3.zero;


                kickPower = player.getPower();
                //Debug.DrawRay(transform.position, mirroredVector * 10f, Color.red,5f);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;

                player.DisableRightFootL();
                rb.AddForce(mirroredVector * kickJogaForce / 1.2f * kickPower * speedBonus);

                
            }



        }

        if (other.tag == "playerHand" && other.isTrigger)
        {
            //Debug.Log("Catched! " + dribCounter++);
            GuyController player = other.GetComponentInParent<GuyController>();

            if (player.isGoalKeeper && player.isGkCatching)
            {
                hand = player.giveHand();
                //FixedJoint joint = hand.GetComponent<FixedJoint>();
                //joint.connectedBody = rb;
                isCatched = true;
            }
            
        }

        if (other.tag == "playerHead")
        {
            float speedBonus = 1 - (rb.velocity.magnitude / 200) ;

            float actualSpeed = rb.velocity.magnitude;
            

            GuyController player = other.GetComponentInParent<GuyController>();

            SimpleMouseOrbit cam = player.getCam();
            moveRotation = cam.transform.forward;


            if (player.GetV())
            {
                moveRotation = Vector3.Reflect(Vector3.Reflect(player.cam.transform.forward, Vector3.up), player.transform.forward * -1);
            }

            if (player.GetScroll())
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                if (player.IsMoving())
                {
                    rb.AddForce(moveRotation * actualSpeed * 36f * speedBonus);
                }
                else
                    rb.AddForce(moveRotation * actualSpeed * 25f * speedBonus);
                player.DisableHead();
            }
            

        }

        
       

        //Check to see if the tag on the collider is equal to Enemy
        if (other.tag == "rightFoot" && !isSpecialShoot)
        {
            GuyController player = other.GetComponentInParent<GuyController>();

            string playerDribState = player.getDribState();


            
            


            //rotationDecrease = rotationDecrease_safe;

            startKickTime = Time.time;
            thePower = rotationForce;
            theLowPower = rotationLowForce;
            theSecondPower = rotationForceRotate;

            float speedBonus = (rb.velocity.magnitude / 200) + 1;

            
            SimpleMouseOrbit cam = player.getCam();
            moveRotation = cam.transform.forward;

            string playerKickState = player.getKickState();
            rotateState = player.RotationState;

            kickPower = player.getPower();
            //Debug.Log(kickPower);

            if (playerKickState.Equals("scorpion"))
            {
                isSpecialShoot = true;
                Invoke("UnlockSpecialShoot", 0.5f);

                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                dribCounter++;

                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }




                rb.velocity = Vector3.zero;


                kickPower = player.getPower();
                //Debug.DrawRay(transform.position, mirroredVector * 10f, Color.red,5f);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;

                player.DisableRightFootL();
                rb.AddForce(player.cam.transform.forward * kickJogaForce / 1.6f * kickPower * speedBonus);


            }
            else if (playerKickState.Equals("heel kick D"))
            {
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                //isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                dribCounter++;

                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }

                rb.velocity = rb.velocity * 0.1f;
                rotationForce = rotationForce / 1.6f;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;
                rb.AddForce(newDir * kickJogaForce / 1.6f * kickPower * speedBonus);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;
            }
            else if (playerKickState.Equals("heel kick A"))
            {
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                //isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                dribCounter++;

                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }

                rb.velocity = rb.velocity * 0.1f;
                rotationForce = rotationForce / 1.6f;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;
                rb.AddForce(newDir * kickJogaForce / 1.6f * kickPower * speedBonus);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;
            }
            else if (playerKickState.Equals("rabona"))
            {
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                dribCounter++;

                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }

                rb.velocity = rb.velocity * 0.1f;
                rotationForce = rotationForce / 1.6f;
                rb.AddForce(moveRotation * kickJogaForce / 1.6f * kickPower * speedBonus);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;
            }
            if (playerKickState.Equals("jogaKick"))
            {
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;

                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;

                dribCounter++;

                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;
                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickJogaForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState;
                    wasFPressed = false;
                }

                rb.velocity = rb.velocity * 0.1f;              
                rb.AddForce(moveRotation * kickJogaForce * kickPower * speedBonus);

                player.setKickState("");
                wasKicked = true;
                lastKickTime = Time.time;
                
}
            if (playerKickState.Equals("idleKick"))
            {
                isLobbed = player.IsLobbing();
                IsKnuckleBall = player.IsKunckleBall();

                lobPower = 250f;
                lobVelocityDecreaseVal = lobVelocityDecreaseValv2;


                rotationForce = player.rotationForce;
                if (rotationForce < 11)
                    rotationForce = 0;


                if (wasFPressed)
                {
                    lastRotationForce = rotationForce;
                    positionBeforeShoot = transform.position;
                    LastMoveRotation = moveRotation; lastKickForce = kickPassForce; LastKickPower = kickPower; LastSpeedBonus = speedBonus; lastRotateState = rotateState; 
                    wasFPressed = false;
                }
                rb.velocity = rb.velocity * 0.1f;

                
                rb.AddForce(moveRotation * kickPassForce * kickPower * speedBonus);


                wasKicked = true;
                lastKickTime = Time.time;


                player.setKickState("");
            }

        }

    }
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.tag == "leftfoot" || collision.collider.tag == "rightfoot" || collision.collider.tag == "player" || collision.collider.tag == "upLeg")
        {
            GuyController player = collision.collider.GetComponentInParent<GuyController>();
            if (player.playerDribState.Equals("slideR") || player.playerDribState.Equals("tackleR") && !tackleBusy)
            {
                tackleBusy = true;
                Invoke("TackeNotBusy", 0.45f);
                rb.velocity = rb.velocity * 0.5f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward;
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if(player.playerDribState.Equals("slideL") || player.playerDribState.Equals("slideR"))
                    rb.AddForce(newDir * kickForce * 1f);
                else
                    rb.AddForce(newDir * kickForce * 1.4f);


            }
            else if (player.playerDribState.Equals("slideL") || player.playerDribState.Equals("tackleL") && !tackleBusy)
            {
                tackleBusy = true;
                Invoke("TackeNotBusy", 0.45f);
                rb.velocity = rb.velocity * 0.5f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward;
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.playerDribState.Equals("slideL") || player.playerDribState.Equals("slideR"))
                    rb.AddForce(newDir * kickForce * 1f);
                else
                    rb.AddForce(newDir * kickForce * 1.4f);
            }

        }

        if (collision.collider.tag == "player" || collision.collider.tag == "upLeg" && !isSpecialShoot)
        {
            GuyController player = collision.gameObject.GetComponentInParent<GuyController>();

            string playerDribState = player.getDribState();


            if((playerDribState.Equals("kickup e") || playerDribState.Equals("kickup q") )&& collision.collider.tag == "upLeg")
            {
                //player.DisableRightFootL();
                //moveRotation = player.transform.up; 
                //isDribLocked = true;
                //rb.AddForce(moveRotation * kickForce * 3.1f);               
            }
            else
            {
                float speedBonus = (rb.velocity.magnitude / 100);
                float minusik = 1 - speedBonus;
                rb.velocity = rb.velocity * minusik;
            }

            

        }
        if (collision.collider.tag == "rightFoot" || collision.collider.tag == "leftFoot" || collision.collider.tag == "playerHand" && !isSpecialShoot)
        {
            GuyController player = collision.gameObject.GetComponentInParent<GuyController>();

            if (player.isGoalKeeper)
            {
                rb.velocity = rb.velocity * 0.8f;

                SimpleMouseOrbit cam = player.getCam();
                float speedBonus = (rb.velocity.magnitude / 200) + 1;
                moveRotation = cam.transform.forward;
                rb.AddForce(moveRotation * speedBonus * gkDivingForce);

            }
        }

        if (collision.collider.tag == "rightFoot" && !isSpecialShoot)
        {

            GuyController player = collision.gameObject.GetComponentInParent<GuyController>();

            string playerKickState = player.getKickState();
            string playerDribState = player.getDribState();


            
            if (playerDribState.Equals("la croq") && !isDribLockedL)
            {
                player.DisableRightFoot();
                isDribLockedL = true;
                Invoke("UnlockDrib", 0.7f);
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                float dir = player.transform.localScale.x / 6;
                Vector3 newDir = Quaternion.Euler(0, -90f * dir, 0) * newMoveRotation;
                rb.AddForce(newDir * kickForce * rollBonus);

            }
            else if (playerDribState.Equals("spin R"))
            {
                player.DisableRightFoot();
                //CancelInvoke();
                isDribLockedL = true;
                player.moveSpeed = player.moveSpeed * 0.3f;
                player.sprintSpeed = player.sprintSpeed * 0.7f;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                Invoke("UnlockDrib", 1.5f);
            }
            else if (playerDribState.Equals("drib L"))
            {
                player.DisableRightFoot();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 3.4f);
                else
                    rb.AddForce(newDir * kickForce * 2.4f);
            }
            else if (playerDribState.Equals("drib R"))
            {
                player.DisableRightFoot();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 90f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 3.4f);
                else
                    rb.AddForce(newDir * kickForce * 2.4f);
            }
            else if (playerDribState.Equals("drib SD"))
            {

                player.DisableRightFoot();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 135f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 2.2f);
                else
                    rb.AddForce(newDir * kickForce * 1.8f);
            }
            else if (playerDribState.Equals("drib AS"))
            {


                player.DisableRightFoot();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -135f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 2.2f);
                else
                    rb.AddForce(newDir * kickForce * 1.8f);
            }
            else if (playerDribState.Equals("drib S"))
            {


                player.DisableRightFoot();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -180f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 2.2f);
                else
                    rb.AddForce(newDir * kickForce * 1.8f);
            }
        }


        if(collision.collider.tag == "leftFoot" && !isSpecialShoot)
        {
            GuyController player = collision.gameObject.GetComponentInParent<GuyController>();

            string playerKickState = player.getKickState();
            string playerDribState = player.getDribState();

            thePower = thePower / 4f;


            if (playerDribState.Equals("la croq") && !isDribLocked)
            {
                player.DisableRightFootL();
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                isDribLocked = true;
                rb.AddForce(moveRotation * kickForce * 2.5f);
            }
            if (playerDribState.Equals("spin R") && !isDribLocked)
            {
                player.DisableRightFootL();

                player.moveSpeed = 150f;
                player.sprintSpeed = 200f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                isDribLocked = true;
                rb.AddForce(moveRotation * kickForce * 2.5f);
                player.isSlowed = false;
                //Invoke("UnlockDrib", 0.5f);
                Debug.Log("LEFT FOOT SPIN " + dribCounter++);
            }
            if(playerDribState.Equals("drib f"))
            {
                player.DisableRightFootL();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if(player.IsSprinting())
                    rb.AddForce(moveRotation * kickForce * 3.1f);
                else
                    rb.AddForce(moveRotation * kickForce * 2.1f);

            }
            else if (playerDribState.Equals("drib aw"))
            {
                player.DisableRightFootL();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, -45f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 3.2f);
                else
                    rb.AddForce(newDir * kickForce * 2.2f);


            }
            else if (playerDribState.Equals("drib wd"))
            {
                player.DisableRightFootL();
                rb.velocity = rb.velocity * 0.05f;

                SimpleMouseOrbit cam = player.getCam();
                moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD
                Vector3 newMoveRotation = moveRotation;
                newMoveRotation.y = 0;
                Vector3 newDir = Quaternion.Euler(0, 45f, 0) * newMoveRotation;
                isDribLocked = true;
                Invoke("UnlockDrib", 0.5f);
                if (player.IsSprinting())
                    rb.AddForce(newDir * kickForce * 3.2f);
                else
                    rb.AddForce(newDir * kickForce * 2.2f);
            }
            
        }

        if (    (collision.collider.tag == "rightFoot" || collision.collider.tag == "leftFoot") && !isDribLocked && !isSpecialShoot)
        {

            GuyController player = collision.gameObject.GetComponentInParent<GuyController>();

            string playerKickState = player.getKickState();
            string playerDribState = player.getDribState();
            SimpleMouseOrbit cam = player.getCam();
            LastPlayerDribID = player.PlayerID;
            dribKickTime = Time.time;
            float speedBonus = (rb.velocity.magnitude / 200) + 1;


            moveRotation = cam.transform.forward; //trzeba obrocic moverotation zgodnie z kierunkiem WSAD


            
            if (dribKickTime - lastDribKickTime > 0.12f)
            {

                


                if (playerDribState.Equals("roll L"))
                {
                    Debug.Log("roll " + dribCounter++);
                    rb.velocity = Vector3.zero;
                    player.DisableRightFoot();

                    Vector3 newMoveRotation = moveRotation;
                    newMoveRotation.y = 0;
                    Vector3 newDir = Quaternion.Euler(0, -90f, 0) * newMoveRotation;
                    rb.AddForce(newDir * kickForce * rollBonus);
                }
                else if (playerDribState.Equals("roll R"))
                {
                    Debug.Log("roll " + dribCounter++);
                    rb.velocity = Vector3.zero;
                    player.DisableRightFoot();


                    Vector3 newMoveRotation = moveRotation;
                    newMoveRotation.y = 0;
                    Vector3 newDir = Quaternion.Euler(0, 90f, 0) * newMoveRotation;
                    rb.AddForce(newDir * kickForce * rollBonus);

                }                
                else if (playerDribState.Equals("ronaldo chop L"))
                {
                    Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;

                    rb.AddForce(newDir * kickForce * 1.5f * speedBonus);
                }
                else if(playerDribState.Equals("ronaldo chop R")){
                    Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;

                    rb.AddForce(newDir * kickForce * 1.5f * speedBonus);
                }
                else if (playerDribState.Equals("przekladanka"))
                {
                   

                }
                else if (playerDribState.Equals("jogaDrib"))
                {
                    if (player.IsW_PPressed() && !player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 0, 0) * moveRotation;
                        rb.AddForce(newDir * kickForce);
                    }
                    if (player.IsW_PPressed() && player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -75f, 0) * moveRotation;
                        rb.AddForce(newDir * kickForce);
                    }
                    if (player.IsW_PPressed() && !player.IsA_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 75f, 0) * moveRotation;
                        rb.AddForce(newDir * kickForce);
                    }






                    if (!player.IsW_PPressed() && player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;

                        rb.AddForce(newDir * kickForce);
                    }

                    if (!player.IsW_PPressed() && !player.IsA_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;

                        rb.AddForce(newDir * kickForce);
                    }

                    if (!player.IsW_PPressed() && !player.IsA_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -180f, 0) * moveRotation;

                        rb.AddForce(newDir * kickForce);
                    }

                }
                else if (playerDribState.Equals("sprintDrib"))
                {
                    if (player.IsW_PPressed() && !player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 0, 0) * moveRotation;
                        rb.AddForce(newDir * sprintKickForce);
                    }
                    if (player.IsW_PPressed() && player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -75f, 0) * moveRotation;
                        rb.AddForce(newDir * sprintKickForce);
                    }
                    if (player.IsW_PPressed() && !player.IsA_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 75f, 0) * moveRotation;
                        rb.AddForce(newDir * sprintKickForce);
                    }






                    if (!player.IsW_PPressed() && player.IsA_PPressed() && !player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -90f, 0) * moveRotation;

                        rb.AddForce(newDir * sprintKickForce);
                    }

                    if (!player.IsW_PPressed() && !player.IsA_PPressed() && player.IsD_PPressed() && !player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, 90f, 0) * moveRotation;

                        rb.AddForce(newDir * sprintKickForce);
                    }

                    if (!player.IsW_PPressed() && !player.IsA_PPressed() && !player.IsD_PPressed() && player.IsS_PPressed())
                    {
                        Vector3 newDir = Quaternion.Euler(0, -180f, 0) * moveRotation;

                        rb.AddForce(newDir * sprintKickForce);
                    }

                }
            }

            lastDribKickTime = Time.time;

           
        }

        if (collision.collider.tag == "net")
        {

            rb.velocity = rb.velocity * 0.2f;


        }
    }
}
