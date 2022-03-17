using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using TMPro;

public class GuyController : MonoBehaviour, IPunObservable, IOnEventCallback
{
    [Header("Components")]
    Animator animator;
    public SimpleMouseOrbit cam;
    public CapsuleCollider dribCollider;
    public CapsuleCollider dribColliderLegUpR;

    public CapsuleCollider dribColliderL;
    public TextMeshPro nickname;



    public CapsuleCollider dribColliderLeg;
    public CapsuleCollider dribColliderLegL;
    public CapsuleCollider headCollider;


    public Slider powerSlider;
    public Slider rotationSlider;
    public RawImage TipsImage;
    public RawImage gk;


    public GameObject trainingBall;
    public float PlayerID;
    public GameObject checker;
    public GameObject playerHead;
    public bool isInTrainingMode;
    public bool isGoalKeeper = false;
    public GameObject playerHand;
    public GameObject playerHandL;
    public float rotationForce;

    [Header("Photon")]
    public string PlayerNameText;
    PhotonView view;

    [Header("Movement values")]
    public float moveSpeed = 6f;
    public float sprintSpeed = 6f;
    public float sprintSpeedConst;
    public float moveSpeedConst;
    public float airMultiplier = 0.4f;
    public float jumpForce = 5f;
    public float isGroundedHeight = 6f;
    public float trainingBallPower = 6f;
    public float divingSpeed = 6f;
    float horizontalMovement;
    float verticalMovement;
    bool isGrounded = true;
    public string playerDribState = "jogaDrib";
    public string playerKickState = "";
    bool isSprinting = false;
    bool isDribbling = false;
    public float animatorSlow = 0f;
    public float finishSprintStrikeAt = 0f;
    public float finishIdleKickAt = 0f;
    public float fov = 50f;
    private bool isDiving = false;
    public float busyLatency = 0.2f;


    [Header("Controls")]
    private float holdDownStartTime = 0f;
    private float kickPower;
    float time = 0f;


    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;



    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 2f;

    Quaternion moveRotation;
    Vector3 gravity = new Vector3(0, -50f, 0);
    float powerMax = 3.06f;
    bool isHolding = false;

    public GameObject giveHand()
    {
        return playerHand;
    }

    public float getPower()
    {
        return kickPower;
    }
    public string getKickState() { return playerKickState; }
    public string getDribState() { return playerDribState; }
    public bool IsW_PPressed() { return Input.GetKey("w"); }
    public bool IsA_PPressed() { return Input.GetKey("a"); }
    public bool IsD_PPressed() { return Input.GetKey("d"); }
    public bool IsS_PPressed() { return Input.GetKey("s"); }
    public SimpleMouseOrbit getCam() { return cam; }


    public string currentAnimState;

    public void setKickState(string state) { playerKickState = state; }

    Vector3 moveDirection;
    Rigidbody rb;

    public bool startSync = false;


    public string RotationState = "noRotate";


    private void Start()
    {
       
        if (view.IsMine) {
            view.Owner.NickName = PlayerPrefs.GetString("username");
            powerSlider = GameObject.Find("Slider").GetComponent<Slider>();
            rotationSlider = GameObject.Find("RotationSlider").GetComponent<Slider>();
            TipsImage = GameObject.Find("TipsImage").GetComponent<RawImage>();
            gk = GameObject.Find("gk").GetComponent<RawImage>();
            nickname.enabled = false;
        }

        rb = GetComponent<Rigidbody>();
        cam.SetTarget(rb.transform);

        rb.freezeRotation = true;
        Physics.gravity = gravity;
        animator = GetComponent<Animator>();
        KickingComplete();
        SetAllCollidersStatus(false);

        cam.gameObject.SetActive(view.IsMine);



        startSync = true;
    }
    bool wasReleased = false;


    public float kickupForce = 0f;

    void CalculateKickUp()
    {
        kickupForce = (cam.transform.eulerAngles.x - 90f) * -1f;
        kickupForce *= 0.1f;

        if (kickupForce < -15f)
        {
            kickupForce *= 0.1f;
            kickupForce = ((26f / kickupForce) * 10f + 87f) * -1f;
        }
        kickupForce += 5f;

        if (kickupForce > 30f) kickupForce = 30f;

    }

    void stopReleased()
    { wasReleased = false; }

    public Player GetPlayer()
    {
        return view.Owner;
    }
    public void SetTrainingBall(GameObject newBall)
    {
        trainingBall = newBall;
    }
    private void Update()
    {
        
        ShowNicknames(); 
        
        if(view.IsMine && Input.GetKeyDown("h"))
        {
            TipsImage.enabled = !TipsImage.enabled;
        }
        if (view.IsMine && isGoalKeeper)      
            gk.enabled = true;     
        else if(view.IsMine && !isGoalKeeper)
            gk.enabled = false;

        if(view.IsMine && Input.GetKeyDown("1"))
        {
            RaiseMaterialEvent("red");
            Component[] bodyParts;
            bodyParts = GetComponentsInChildren<Renderer>();
            foreach (Renderer body in bodyParts)
            {
                body.material.SetColor("_Color", Color.red);
                Color theColorToAdjust = body.material.color;
                theColorToAdjust.a = 0.35f;
                body.material.color = theColorToAdjust;
            }
        }
        if (view.IsMine && Input.GetKeyDown("2"))
        {
            RaiseMaterialEvent("blue");
            Component[] bodyParts;
            bodyParts = GetComponentsInChildren<Renderer>();
            foreach (Renderer body in bodyParts)
            {
                body.material.SetColor("_Color", Color.blue);
                Color theColorToAdjust = body.material.color;
                theColorToAdjust.a = 0.35f;
                body.material.color = theColorToAdjust;
            }
        }
        CalculateKickUp();


        /*if (view.IsMine)
        {
            if (Input.GetKeyDown("h"))
            {
                view.RPC("ChatMessage", RpcTarget.All, view.GetInstanceID().ToString(), "and jump?");

            }
        }*/



        if (Input.GetMouseButtonUp(0) && view.IsMine)
            animator.enabled = true;

        if (Input.GetMouseButtonUp(0) && view.IsMine)
        {
            wasReleased = true;
            Invoke("stopReleased", 0.2f);
        }


        Vector3 relative;
        relative = cam.transform.forward;


        
        //Debug.DrawRay(transform.position, relative * 10f, Color.green);


        if (view.IsMine)
        {
            if (!isGoalKeeper)
                AnimationUpdate();
            else
            {
                GoalkeeperAnimationUpdate();
            }
        }

        Vector3 temp = transform.position;
        temp.y = temp.y + 2f;
        isGrounded = Physics.Raycast(temp, Vector3.down, isGroundedHeight / 2 + 0.1f);

        ControlDrag();
        if (view.IsMine)
            MyInput();
        //SliderControl();

        playerHead.transform.rotation = Quaternion.LookRotation(relative);


       
            if (Input.GetKeyDown(jumpKey) && isGrounded && view.IsMine)
            {
                Jump();
            }

            if (Input.GetButton("ball pull") && isInTrainingMode && view.IsMine)
            {
                if(!trainingBall.scene.IsValid())
                {
                    Vector3 ballPos = transform.position;
                    ballPos.x += 5f;
                    GameObject theBall = PhotonNetwork.Instantiate(trainingBall.name, ballPos, Quaternion.identity);
                    trainingBall = theBall;
                }
                Vector3 newSpot = transform.position + (relative.normalized * 5f);

                Rigidbody ball = trainingBall.GetComponent<Rigidbody>();
                ball.velocity = Vector3.zero;
                newSpot.y = 3;
                trainingBall.transform.position = newSpot;



                //Vector3 dir = (this.transform.position - trainingBall.transform.position).normalized;
                // trainingBall.gameObject.GetComponent<Rigidbody>().AddForce(dir * trainingBallPower);
            }
            if (Input.GetKeyDown("r") && isInTrainingMode && view.IsMine)
            {
                BallScript ballScript = trainingBall.GetComponent<BallScript>();
                ballScript.ReapeatLastKick();
            }
            if (Input.GetKeyDown("t") && isInTrainingMode && view.IsMine)
            {
                BallScript ballScript = trainingBall.GetComponent<BallScript>();
                ballScript.RandomShoot(this);
            }
            if (Input.GetKeyDown("g") && view.IsMine)
            {
                isGoalKeeper = !isGoalKeeper;
                SetAllCollidersStatus(isGoalKeeper);

            }
           


    }
    BallScript GetClosestBall(BallScript[] enemies)
    {
        BallScript tMin = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (BallScript t in enemies)
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

    /*public void zrobMiLoda(BallScript bScript, bool isLobbedX, bool isKnuckleballX, float rotationForceX, string rotateStateX)
    {
        bScript.setValues(isLobbedX, isKnuckleballX, rotationForceX, rotateStateX);
    }*/
    
    public void SetAllCollidersStatus(bool active)
    {
        foreach (Collider c in playerHand.GetComponents<Collider>())
        {
            c.enabled = active;
        }
        foreach (Collider c in playerHandL.GetComponents<Collider>())
        {
            c.enabled = active;
        }

    }
    GuyController FindMexD()
    {
        GuyController mexD = null;
        var foundPlayers = FindObjectsOfType<GuyController>();
        for (int i = 0; i < foundPlayers.Length; i++)
        {
            if (foundPlayers[i].view.IsMine)
                mexD = foundPlayers[i];
        }
        return mexD;
    }
    void ShowNicknames()
    {
        GuyController mexD = FindMexD();

        var foundPlayers = FindObjectsOfType<GuyController>();
        for (int i = 0; i < foundPlayers.Length; i++)
        {
            if (!foundPlayers[i].view.IsMine)
            {
                foundPlayers[i].nickname.enabled = true;
                if (foundPlayers[i].nickname.text.Equals("Sample text"))
                {
                    foundPlayers[i].nickname.text = foundPlayers[i].view.Owner.NickName;
                }
                
                foundPlayers[i].nickname.transform.LookAt(mexD.cam.transform);
                Rigidbody foundRb = foundPlayers[i].GetComponent<Rigidbody>();
                if(rb.transform.localScale.x == -6)
                    foundPlayers[i].nickname.transform.localScale = new Vector3(-0.07556732f, 0.07556732f, 0.07556732f);
                else
                    foundPlayers[i].nickname.transform.localScale = new Vector3(0.07556732f, 0.07556732f, 0.07556732f);

                foundPlayers[i].nickname.transform.Rotate(0, 180, 0);

            }
        }
    }


    void SyncOtherPlayersAnimation(string playerID, string newState, string animationMethod)
    {
        var foundPlayers = FindObjectsOfType<GuyController>();
        for (int i = 0; i < foundPlayers.Length; i++)
        {
            if (foundPlayers[i].view.ViewID.ToString().Equals(playerID) && foundPlayers[i].startSync)
            {

                if (animationMethod.Equals("ChangeAnimationState"))
                    foundPlayers[i].ChangeAnimationState_sync(newState);
                else if (animationMethod.Equals("ChangeAnimationState_Instant"))
                    foundPlayers[i].ChangeAnimationState_Instant_sync(newState);
                else if (animationMethod.Equals("ChangeAnimationState_instant_notransition_looping"))
                    foundPlayers[i].ChangeAnimationState_instant_notransition_looping_sync(newState);
                else if (animationMethod.Equals("ChangeAnimationState_instant_notransition"))
                    foundPlayers[i].ChangeAnimationState_instant_notransition_sync(newState);

            }

        }

    }
    void SyncOtherPlayersAnimation(string playerID, string newState, string animationMethod, float time)
    {
        var foundPlayers = FindObjectsOfType<GuyController>();
        for (int i = 0; i < foundPlayers.Length; i++)
        {
            if (foundPlayers[i].view.ViewID.ToString().Equals(playerID) && foundPlayers[i].startSync)
            {
                if (animationMethod.Equals("ChangeAnimationStateFromCurrentTime"))
                {
                    foundPlayers[i].ChangeAnimationStateFromCurrentTime_sync(newState, time);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (view.IsMine)
            MovePlayer();

        try
        {
            BallScript bScript = GetClosestBall(FindObjectsOfType<BallScript>());
            Rigidbody ballRb = bScript.GetComponent<Rigidbody>();

            Vector3 rotationDir = bScript.rotationDir;
            Vector3 torqueDir = bScript.torqueDir;
            float thePower = bScript.thePower;


            if (bScript.spinState.Equals("spinMeDaddy"))
            {
                ballRb.maxAngularVelocity = 20f;
                ballRb.AddForce(rotationDir * thePower);
                ballRb.AddTorque(torqueDir * thePower * 3 * (thePower / 30f) * (thePower / 30f) * (thePower / 30f));
            }
            else if (bScript.spinState.Equals("spinMeLowDaddy"))
            {
                ballRb.AddForce(rotationDir * thePower);
            }
        }
        catch(Exception e)
        {

        }

            
        

    }
    /* void SliderControl()
     {
         if (holdDownStartTime != 0 && isHolding)
             time = Time.time - holdDownStartTime;
         if (time > 0.5f) time = 0.5f;
         powerSlider.value = time / 0.5f;
     }*/
    void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    void ControlDrag()
    {
        if (isGrounded)
            rb.drag = groundDrag;
        else if (!isDiving)
            rb.drag = airDrag;
    }
    private void MovePlayer()
    {
        moveRotation = cam.rotation;
        moveRotation.x = 0;
        moveRotation.z = 0;
        transform.rotation = moveRotation;
        if (isSprinting && !isDiving && !isSlowed)
        {
            
            if (isGrounded)
                rb.AddForce(moveDirection.normalized * sprintSpeed, ForceMode.Acceleration);
            else
                rb.AddForce(moveDirection.normalized * sprintSpeed * airMultiplier, ForceMode.Acceleration);




            if (Input.GetKey(KeyCode.S))
            {
                if (isGrounded)
                {
                    if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                        rb.AddForce(transform.forward.normalized * sprintSpeed / 1.5f, ForceMode.Acceleration);
                    else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
                    {
                        rb.AddForce(transform.right.normalized * sprintSpeed, ForceMode.Acceleration);
                        Vector3 newDir = Quaternion.Euler(0, -45f, 0) * transform.forward.normalized;
                        rb.AddForce(newDir.normalized * sprintSpeed / 1.5f, ForceMode.Acceleration);

                    }
                    else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
                    {
                        rb.AddForce(transform.right.normalized * -1f * sprintSpeed, ForceMode.Acceleration);
                        Vector3 newDir = Quaternion.Euler(0, 45f, 0) * transform.forward.normalized;
                        rb.AddForce(newDir.normalized * sprintSpeed / 1.5f, ForceMode.Acceleration);
                    }

                }
            }

        }
        else if (isDiving)
        {
            rb.AddForce(moveDirection.normalized * divingSpeed, ForceMode.Acceleration);
        }
        else if (isSlowed)
        {
            rb.AddForce(moveDirection.normalized * 0.4f, ForceMode.Acceleration);

        }
        else if (isDribbling)
        {

        }
        else if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed, ForceMode.Acceleration);
            if (Input.GetKey(KeyCode.S))
            {

                if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
                    rb.AddForce(transform.forward.normalized * moveSpeed / 2, ForceMode.Acceleration);
                else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A))
                {
                    rb.AddForce(transform.right.normalized * moveSpeed, ForceMode.Acceleration);
                    Vector3 newDir = Quaternion.Euler(0, -45f, 0) * transform.forward.normalized;
                    rb.AddForce(newDir.normalized * moveSpeed / 2, ForceMode.Acceleration);

                }
                else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
                {
                    rb.AddForce(transform.right.normalized * -1f * moveSpeed, ForceMode.Acceleration);
                    Vector3 newDir = Quaternion.Euler(0, 45f, 0) * transform.forward.normalized;
                    rb.AddForce(newDir.normalized * moveSpeed / 2, ForceMode.Acceleration);
                }

            }
        }
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Acceleration);



    }
    bool canKickAfterDrib = false;
    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
    }
    public void TurnOnFoots()
    {
        dribCollider.enabled = true;
        dribColliderL.enabled = true;
        dribColliderLeg.enabled = true;
        dribColliderLegL.enabled = true;
        headCollider.enabled = true;
        dribColliderLegUpR.enabled = true;

    }

    public void DisableRightFoot()
    {
        dribCollider.enabled = false;
        dribColliderLeg.enabled = false;
        dribColliderLegUpR.enabled = false;
        Invoke("TurnOnFoots", 0.5f);
    }
    public void DisableRightFootL()
    {
        dribColliderL.enabled = false;
        dribColliderLegL.enabled = false;
        Invoke("TurnOnFoots", 0.5f);
    }
    public void DisableLeftFoot()
    {
        dribCollider.enabled = false;
        Invoke("TurnOnFoots", 0.5f);
    }
    public void DisableHead()
    {
        headCollider.enabled = false;
        Invoke("TurnOnFoots", 0.5f);
    }
    public void DisableDribTime(float time, string leg)
    {
        if (leg == "right")
        {
            dribCollider.enabled = false;
            dribColliderLeg.enabled = false;
        }
        else
        {
            dribColliderL.enabled = false;
            dribColliderLegL.enabled = false;
        }
        Invoke("TurnOnFoots", time);
    }
    public void ResetDribState()
    {
        playerDribState = "";
        playerKickState = "";
    }

    [PunRPC]
    void ChatMessage(string a, string b)
    {

        //Debug.Log(string.Format("ChatMessage {0} {1} {2}", playerID, newState, c));

    }
    bool wasKickedUp = false;

    void RaiseAnimationEvent(string animationState, string animationMethod)
    {
        object[] content = new object[] { view.ViewID, animationState, animationMethod };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }
    void RaiseAnimationEvent(string animationState, string animationMethod, float time)
    {
        object[] content = new object[] { view.ViewID, animationState, animationMethod, time };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(1, content, raiseEventOptions, SendOptions.SendReliable);
    }

    void RaiseMaterialEvent(string color)
    {
        object[] content = new object[] { view.ViewID, color, time };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(3, content, raiseEventOptions, SendOptions.SendReliable);
    }

    void ChangeAnimationState_sync(string newState)
    {
        animator.enabled = true;

        if (currentAnimState == newState)
            return;
        if (newState == "Breathing Idle")
            animator.CrossFade(newState, 0.1f, -1, 0, 0.5f);
        else
            animator.CrossFade(newState, 0.5f, -1, 0, 0.5f);
        currentAnimState = newState;
    }

    void ChangeAnimationState(string newState)
    {
        RaiseAnimationEvent(newState, "ChangeAnimationState");
        if (currentAnimState == newState)
            return;
        if (newState == "Breathing Idle")
            animator.CrossFade(newState, 0.1f, -1, 0, 0.5f);
        else
            animator.CrossFade(newState, 0.5f, -1, 0, 0.5f);
        currentAnimState = newState;
    }

    void ChangeAnimationState_Instant(string newState)
    {
        RaiseAnimationEvent(newState, "ChangeAnimationState_Instant");
        animator.CrossFade(newState, 0.5f, -1, 0, 0.5f);
        currentAnimState = newState;
    }
    void ChangeAnimationState_Instant_sync(string newState)
    {
        animator.enabled = true;

        animator.CrossFade(newState, 0.5f, -1, 0, 0.5f);
        currentAnimState = newState;
    }
    void ChangeAnimationStateFromCurrentTime(string newState, float time)
    {
        RaiseAnimationEvent(newState, "ChangeAnimationStateFromCurrentTime", time);
        if (kickingDirectionState.Equals("przewrotka") || kickingDirectionState.Equals("volley A") || kickingDirectionState.Equals("volley D") || kickingDirectionState.Equals("heel kick A")
            || kickingDirectionState.Equals("heel kick D") || kickingDirectionState.Equals("scorpion"))
            animator.CrossFade(newState, 0f, -1, time, 0f);
        else
            animator.CrossFade(newState, 0.5f, -1, time, 0.5f);
        currentAnimState = newState;
    }


    void AllowToStopAnimator()
    {
        canIstopAnimator = true;
    }

    void ChangeAnimationStateFromCurrentTime_sync(string newState, float time)
    {
        canIstopAnimator = false;
        animator.enabled = true;
        if (newState.Equals("Soccer Pass"))
            Invoke("AllowToStopAnimator", animator.GetCurrentAnimatorStateInfo(0).length - 0.5f);
        else
            Invoke("AllowToStopAnimator", animator.GetCurrentAnimatorStateInfo(0).length - 0.1f);

        if (kickingDirectionState.Equals("przewrotka") || kickingDirectionState.Equals("volley A") || kickingDirectionState.Equals("volley D") || kickingDirectionState.Equals("heel kick A")
            || kickingDirectionState.Equals("heel kick D") || kickingDirectionState.Equals("scorpion"))
            animator.CrossFade(newState, 0f, -1, time, 0f);
        else
            animator.CrossFade(newState, 0.5f, -1, time, 0.5f);
        currentAnimState = newState;
    }
    void ChangeAnimationStateFromCurrentTime_withReturn(string newState, float time)
    {
        if (currentAnimState == newState)
            return;

        animator.CrossFade(newState, 0.5f, -1, time, 0.5f);


        currentAnimState = newState;
    }

    bool isKicking = false;
    bool isBusy = false;
    private bool isBusyJump;
    bool isChanging = false;
    public bool isSlowed = false;

    public void LetsSlow()
    {
        isSlowed = true;
    }
    public void DontBeSlowed()
    {
        isSlowed = false;
    }

    bool canIstopAnimator = true;



    bool resetScale = false;

    void BusyComplete()
    {
        canKickAfterDrib = false;
        isGkCatching = false;
        WaitForAction = false;
        isKickingUp = false;
        isBusyJump = false;
        TurnOnFoots();
        animator.enabled = true;
        isBusy = false; isSlowed = false; isKicking = false;
        isParading = false; moveSpeed = moveSpeedConst; sprintSpeed = sprintSpeedConst; playerDribState = "jogaDrib"; transform.localScale = new Vector3(6, 6, 6);

    }
    void BusyComplete_no_scale() { isBusy = false; isSlowed = false; isKicking = false; isParading = false; }

    void DivingComplete() { isDiving = false; isSlowed = true; }
    void KickingComplete()
    {
        isBusyJump = false; isSpecialShooting = false;
        animator.enabled = true; isKicking = false; dribCollider.enabled = true; dribColliderLeg.enabled = true;
        powerSlider.gameObject.SetActive(false); holdDownStartTime = 0f; time = 0f;
         rotationSlider.gameObject.SetActive(false);
    }


    public void RaiseKickingHoldingComplete()
    {
        object[] content = new object[] { view.ViewID.ToString() };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; // You would have to set the Receivers to All in order to receive this event on the local client as well
        PhotonNetwork.RaiseEvent(2, content, raiseEventOptions, SendOptions.SendReliable);
    }

    void KickingHoldingComplete()
    {


        if (!isKicking && !isChanging)
        {
            animator.enabled = false; dribCollider.enabled = true; dribColliderLeg.enabled = true;
        }
        isChanging = false;
    }

    void KickingCollidingComplete() { setKickState(""); }
    void DeleteSlider()
    {
        powerSlider.gameObject.SetActive(false);
        rotationSlider.gameObject.SetActive(false);

    }

    bool isHoldingAnimation = false;
    bool isKickingUp = false;

    void ResetSpeed()
    {
        moveSpeed = moveSpeedConst;
        sprintSpeed = sprintSpeedConst;
    }

    public float startSprintKickTime;
    public float startStandKickTime;
    private string kickingDirectionState = "";
    float holdDownStartTime2 = 0f;
    bool shootReleasedFullPower = false;
    float lastReleasedTime = 0f;
    string rollDir = "";
    bool isRollBusy = false;

    public bool WaitForAction = false;

    void ActionTrue()
    {
        WaitForAction = true;
    }

    public bool IsMoving()
    {
        bool W_Pressed = Input.GetKey("w");
        bool D_Pressed = Input.GetKey("d");
        bool A_Pressed = Input.GetKey("a");
        bool S_PRessed = Input.GetKey("s");
        if (W_Pressed || D_Pressed || A_Pressed || S_PRessed)
        {
            return true;
        }
        else return false;
    }

    public bool GetScroll()
    {
        return Input.GetMouseButton(2);
    }
    public bool GetV()
    {
        return Input.GetKey(KeyCode.V);
    }

    public bool IsLobbing()
    {
        return Input.GetKey("s");
    }
    public bool IsSprinting()
    {
        return isSprinting;
    }
    public bool IsKunckleBall()
    {
        return (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D));
    }
    void KinematicTrue()
    {
        rb.isKinematic = true;
        rb.isKinematic = false;
        Invoke("KinematicTrue", 0.3f);
    }

    void LetsBeJumpBusy()
    {
        isBusyJump = true;
    }

    bool IsTrickPressed()
    {

        if (Input.GetKey("z") || Input.GetKey("x") || Input.GetKey("c") || Input.GetKey("v"))
        {
            return true;
        }
        else return false;
    }

    public bool isSpecialShooting = false;
    bool canIRabona = false;
    void AnimationUpdate()
    {



        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            ResetSpeed();
            canIRabona = true;
        }
        else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            canIRabona = false;

        rotationSlider.value = rotationForce;
        powerSlider.value = kickPower;

        bool W_Pressed = Input.GetKey("w");
        bool D_Pressed = Input.GetKey("d");
        bool S_Pressed = Input.GetKey("s");
        bool A_Pressed = Input.GetKey("a");
        bool SHIFT_Pressed = Input.GetKey(KeyCode.LeftShift);


        bool shootPressed = Input.GetMouseButtonDown(0);


        bool shootRightPressed = Input.GetMouseButtonDown(1);

        bool shootRightPressedGenerally = Input.GetMouseButton(1);

        bool shootPressed_generally = Input.GetMouseButton(0);
        bool controlPressed_generally = Input.GetKey(KeyCode.LeftControl);
        bool shootReleased = Input.GetMouseButtonUp(0);
        bool shootHolding = Input.GetButton("Fire1");

        if (A_Pressed)
        {
            rollDir = "left";
            RotationState = "rotateRight";

        }
        else if (D_Pressed)
        {
            rollDir = "right";
            RotationState = "rotateLeft";

        }





        if (shootHolding)
        {
            if (Input.GetMouseButton(1))
                rotationForce += 60 * Time.deltaTime;

            if (Input.GetMouseButton(0))
                kickPower += 3f * Time.deltaTime;

            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // forward
                rotationForce += 2;

            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // backwards
                rotationForce += 2;

            if (rotationForce > 30)
                rotationForce = 30;
            if (kickPower > 3)
                kickPower = 3;
        }

        

        if (Input.GetKeyUp(KeyCode.E) || Input.GetKeyUp(KeyCode.Q))
        {
            transform.localScale = new Vector3(6, 6, 6);
            TurnOnFoots();
            ResetDribState();
            ResetSpeed();
            isBusy = false;
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (!isKicking && !isHolding)
            {
                ResetSpeed();

                dribCollider.enabled = true;
                if (!isRollBusy)
                {
                    transform.localScale = new Vector3(6, 6, 6);
                    isBusy = false;

                }
                isRollBusy = false;
            }


            //rollDir = "";
        }
        if ((Input.GetKey(KeyCode.LeftControl) && (Input.GetKeyDown("a") || Input.GetKeyDown("d"))) || Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isKicking && !isHolding)
            {
                TurnOnFoots();
                //dribCollider.enabled = true;
                if (!isRollBusy)
                    animator.Play("Breathing Idle");
            }


        }

        if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(2))
        {
            if (!rollDir.Equals("left"))
            {
                transform.localScale = new Vector3(6, 6, 6);
                playerDribState = "slideR";

            }
            else
            {
                playerDribState = "slideL";
                transform.localScale = new Vector3(-6, 6, 6);
            }

            isBusy = true;
            ResetSpeed();
            TurnOnFoots();
            CancelInvoke();
            ChangeAnimationState_instant_notransition("slide");
            Invoke("BusyComplete", 1f);
        }
        else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(0))
        {
            playerDribState = "tackleL";
            transform.localScale = new Vector3(-6, 6, 6);
            isBusy = true;
            ResetSpeed();
            TurnOnFoots();
            CancelInvoke();
            ChangeAnimationState_instant_notransition("better tackle");
            Invoke("BusyComplete", 0.5f);
        }
        else if (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1))
        {
            playerDribState = "tackleR";
            transform.localScale = new Vector3(6, 6, 6);
            isBusy = true;
            ResetSpeed();
            TurnOnFoots();
            CancelInvoke();
            ChangeAnimationState_instant_notransition("better tackle");
            Invoke("BusyComplete", 0.5f);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && shootPressed_generally && !isSpecialShooting)
        {
            moveSpeed = 125f;
            sprintSpeed = 150f;
            isBusy = false;
            isSpecialShooting = true;
            transform.localScale = new Vector3(6, 6, 6);
            CancelInvoke();
            holdDownStartTime = Time.time;
            holdDownStartTime2 = Time.time;
            isHolding = true;
            isHoldingAnimation = true;


        }
        else if (Input.GetKey(KeyCode.LeftControl) && A_Pressed && shootRightPressedGenerally && !W_Pressed && !S_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;
            wasKickedUp = true;
            isBusyJump = true;
            isKickingUp = true;

            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState("kickup D");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.LeftControl) && D_Pressed && shootRightPressedGenerally && !W_Pressed && !S_Pressed && !isSpecialShooting && canIRabona)
        {
            wasKickedUp = true;

            moveSpeed = 100f;
            sprintSpeed = 150f;
            isBusyJump = true;
            isKickingUp = true;
            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("kickup D");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.LeftControl) && A_Pressed && shootRightPressedGenerally && !W_Pressed && S_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;
            wasKickedUp = true;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;

            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState("kickup SD");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.LeftControl) && D_Pressed && shootRightPressedGenerally && !W_Pressed && S_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;
            wasKickedUp = true;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;

            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("kickup SD");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !A_Pressed && !D_Pressed && shootRightPressedGenerally && !W_Pressed && S_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;
            wasKickedUp = true;

            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("kickup S");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
            isBusy = true;
        }
        else if (Input.GetKey(KeyCode.LeftControl) && W_Pressed && shootRightPressedGenerally && D_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;
            wasKickedUp = true;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;

            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("kickup WD");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.LeftControl) && W_Pressed && shootRightPressedGenerally && A_Pressed && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;
            wasKickedUp = true;

            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState("kickup WD");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
           
        }
        else if (Input.GetKey(KeyCode.LeftControl) && shootRightPressedGenerally && !isSpecialShooting && canIRabona)
        {
            moveSpeed = 100f;
            sprintSpeed = 150f;
            wasKickedUp = true;

            isBusyJump = true;
            isKickingUp = true;
            isRollBusy = true;

            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("kickup W");
            isBusy = true;
            Invoke("LetsBeBusy", 0.05f);
            Invoke("BusyComplete", 0.8f);
            playerDribState = "";
            playerKickState = "kickup";
        }
        else if (Input.GetKey(KeyCode.E))
        {
            CancelInvoke();
            transform.localScale = new Vector3(6, 6, 6);
            moveSpeed = 75f;
            sprintSpeed = 125f;
            ChangeAnimationState_instant_notransition_looping("kick up e");
            isBusy = true;

            if (!W_Pressed && !A_Pressed && !D_Pressed && !S_Pressed)
                playerKickState = "kickup e";
            else if (W_Pressed && A_Pressed)
                playerKickState = "kickup AW";
            else if (!W_Pressed && A_Pressed && !S_Pressed)
                playerKickState = "kickup A";
            else if (!W_Pressed && D_Pressed && !S_Pressed)
                playerKickState = "kickup D";
            else if (W_Pressed && D_Pressed)
                playerKickState = "kickup DW";
            else if (W_Pressed)
                playerKickState = "kickup W";
            else if (S_Pressed && A_Pressed)
                playerKickState = "kickup AS";
            else if (S_Pressed && D_Pressed)
                playerKickState = "kickup DS";
            else if (S_Pressed)
                playerKickState = "kickup S";


        }
        else if (Input.GetKey(KeyCode.Q))
        {
            CancelInvoke();
            //DisableRightFoot();
            transform.localScale = new Vector3(-6, 6, 6);
            moveSpeed = 75f;
            sprintSpeed = 125f;

            if (!W_Pressed && !A_Pressed && !D_Pressed && !S_Pressed)
                playerKickState = "kickup e";
            else if (W_Pressed && A_Pressed)
                playerKickState = "kickup AW";
            else if (!W_Pressed && A_Pressed && !S_Pressed)
                playerKickState = "kickup A";
            else if (!W_Pressed && D_Pressed && !S_Pressed)
                playerKickState = "kickup D";
            else if (W_Pressed && D_Pressed)
                playerKickState = "kickup DW";
            else if (W_Pressed)
                playerKickState = "kickup W";
            else if (S_Pressed && A_Pressed)
                playerKickState = "kickup AS";
            else if (S_Pressed && D_Pressed)
                playerKickState = "kickup DS";
            else if (S_Pressed)
                playerKickState = "kickup S";

            ChangeAnimationState_instant_notransition_looping("kick up e");
            isBusy = true;
            //Invoke("BusyComplete_no_scale", 0.85f);

        }
        else if (Input.GetKey(KeyCode.LeftControl) && A_Pressed && !isRollBusy && !shootPressed && !W_Pressed && !S_Pressed && !isHolding && !isKicking)
        {

            transform.localScale = new Vector3(6, 6, 6);
            moveSpeed = 125f;
            sprintSpeed = 150f;
            playerDribState = "roll L";
            ChangeAnimationState_instant_notransition_looping("roll L");
            isBusy = true;
            //Invoke("BusyComplete_no_scale", 0.85f);

        }
        else if (Input.GetKey(KeyCode.LeftControl) && D_Pressed && !isRollBusy && !W_Pressed && !S_Pressed && !isHolding && !isKicking)
        {

            transform.localScale = new Vector3(-6, 6, 6);
            moveSpeed = 125f;
            sprintSpeed = 150f;

            playerDribState = "roll R";
            ChangeAnimationState_instant_notransition_looping("roll L");
            isBusy = true;
            //Invoke("BusyComplete_no_scale", 0.85f);
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && !W_Pressed && !S_Pressed && !isHolding && !isKicking)
        {
            if (rollDir.Equals("left"))
                transform.localScale = new Vector3(-6, 6, 6);
            else
                transform.localScale = new Vector3(6, 6, 6);


            dribCollider.enabled = false;
            playerDribState = "roll stay";
            isBusy = true;
            ChangeAnimationState_instant_notransition("roll stay");

        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && W_Pressed && A_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;

            transform.localScale = new Vector3(6, 6, 6);

            playerDribState = "roll AW";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll W");
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && W_Pressed && D_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;

            transform.localScale = new Vector3(-6, 6, 6);

            playerDribState = "roll DW";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll W");
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && W_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;

            if (rollDir.Equals("left"))
                transform.localScale = new Vector3(-6, 6, 6);
            else
                transform.localScale = new Vector3(6, 6, 6);


            dribCollider.enabled = false;
            playerDribState = "roll W";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll W");
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && S_Pressed && A_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;

            transform.localScale = new Vector3(-6, 6, 6);

            dribCollider.enabled = false;
            playerDribState = "roll AS";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll S");
        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && S_Pressed && D_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;


            transform.localScale = new Vector3(6, 6, 6);

            //rb.isKinematic = true;
            //rb.isKinematic = false;

            dribCollider.enabled = false;
            playerDribState = "roll DS";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll S");

        }
        else if (Input.GetKey(KeyCode.LeftControl) && !isRollBusy && S_Pressed && !isHolding && !isKicking)
        {
            moveSpeed = 100f;
            sprintSpeed = 125f;

            if (rollDir.Equals("left"))
                transform.localScale = new Vector3(-6, 6, 6);
            else
                transform.localScale = new Vector3(6, 6, 6);

            //rb.isKinematic = true;
            //rb.isKinematic = false;

            dribCollider.enabled = false;
            playerDribState = "roll S";
            isBusy = true;
            ChangeAnimationState_instant_notransition_looping("roll S");

        }



        else if (Input.GetKey(KeyCode.V) && shootPressed)
        {
            transform.localScale = new Vector3(6, 6, 6);
            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            playerDribState = "la croq";
            ChangeAnimationState_instant_notransition("la croq L");
            isBusy = true;
            Invoke("BusyComplete", 0.7f);
        }
        else if (Input.GetKey(KeyCode.V) && shootRightPressed)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            playerDribState = "la croq";
            ChangeAnimationState_instant_notransition("la croq L");
            isBusy = true;
            Invoke("BusyComplete", 0.7f);
        }
        else if (Input.GetKey(KeyCode.C) && shootPressed)
        {
            transform.localScale = new Vector3(6, 6, 6);
            //dribCollider.enabled = false;

            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            dribColliderLeg.enabled = false;
            dribColliderLegL.enabled = false;

            playerDribState = "spin R";
            ChangeAnimationState_instant_notransition("spin");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("KickingComplete", 1.35f);
            Invoke("BusyComplete", 1.35f);
            Invoke("TurnOnFoots", 1.35f);

        }
        else if (Input.GetKey(KeyCode.C) && shootRightPressed)
        {
            dribColliderLeg.enabled = false;
            dribColliderLegL.enabled = false;
            transform.localScale = new Vector3(-6, 6, 6);
            //dribCollider.enabled = false;

            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            playerDribState = "spin R";
            ChangeAnimationState_instant_notransition("spin");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("TurnOnFoots", 1.35f);
            Invoke("KickingComplete", 1.35f);
            Invoke("BusyComplete", 1.35f);
        }
        else if (Input.GetKey(KeyCode.X) && shootPressed)
        {

            isBusyJump = false;
            transform.localScale = new Vector3(6, 6, 6);

            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);

            playerDribState = "ronaldo chop L";
            ChangeAnimationState_instant_notransition("ronaldo chop");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("BusyComplete", 0.8f);
        }
        else if (Input.GetKey(KeyCode.X) && shootRightPressed)
        {
            transform.localScale = new Vector3(-6, 6, 6);

            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);

            playerDribState = "ronaldo chop R";
            ChangeAnimationState_instant_notransition("ronaldo chop");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("BusyComplete", 0.8f);
        }
        else if (Input.GetKey(KeyCode.Z) && shootRightPressed)
        {
            transform.localScale = new Vector3(6, 6, 6);

            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            playerDribState = "przekladanka";
            ChangeAnimationState_Instant("przekladanka r");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("BusyComplete", 0.65f);
        }
        else if (Input.GetKey(KeyCode.Z) && shootPressed)
        {
            transform.localScale = new Vector3(-6, 6, 6);

            isBusyJump = false;
            CancelInvoke();
            Invoke("LetsBeJumpBusy", 0.1f);
            playerDribState = "przekladanka";
            ChangeAnimationState_Instant("przekladanka r");

            //ChangeAnimationState("ronaldo chop");
            isBusy = true;
            Invoke("BusyComplete", 0.65f);
        }
        else if (shootPressed)
        {
            transform.localScale = new Vector3(6, 6, 6);
            CancelInvoke();
            holdDownStartTime = Time.time;
            holdDownStartTime2 = Time.time;

            powerSlider.gameObject.SetActive(true);
            isHolding = true;
            isHoldingAnimation = true;

        }
        else if (shootHolding || (shootHolding && Input.GetKey(KeyCode.LeftControl)))
        {

            if ((kickingDirectionState.Equals("rabona AW") || kickingDirectionState.Equals("rabona DW")) && (Time.time - holdDownStartTime2) > 1f)
            {
                if (!isKicking)
                    KickingComplete();
                if(!wasKickedUp)
                    KickingCollidingComplete();
                animator.speed = 1f;
                kickingDirectionState = "";

                isHolding = false;
                isHoldingAnimation = false;
                DeleteSlider();
                animator.enabled = true;
            }

            if ((Time.time - holdDownStartTime2) > 1.1f)
            {

                if (!isBusy && !isKicking)
                    KickingComplete();
                if (!wasKickedUp)
                    KickingCollidingComplete();
                animator.speed = 1f;
                kickingDirectionState = "";

                isHolding = false;
                isHoldingAnimation = false;
                DeleteSlider();
                animator.enabled = true;
                // shootReleased = true;
            }
        }














        if (SHIFT_Pressed && !S_Pressed)
            isSprinting = true;
        else
            isSprinting = false;


        /*void assignKickPower()
        {
            kickPower = (Time.time - holdDownStartTime);
            float kickAddition = (kickPower / 0.5f) + 1f;
            if (kickPower > 0.5f) kickPower = 0.5f;
            kickPower = (kickPower + 1f) * kickAddition;
            if (kickPower > 3f) kickPower = 3f;

        }*/



        if (!W_Pressed && !D_Pressed && !S_Pressed && !A_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            ChangeAnimationState("Breathing Idle");
        }

        /*if (tacklePressed  )
        {
            ChangeAnimationState("Tackle");
            isBusy = true;
            Invoke("BusyComplete", animator.GetCurrentAnimatorStateInfo(0).length);
        }*/

        //ładowanie strzału____________________________________________
        if (shootRightPressedGenerally && (!isBusy || isBusyJump) && !isHolding && canIRabona && !IsTrickPressed() && !Input.GetKey(KeyCode.LeftControl))
        {
            if (isKicking)
            {
                KickingComplete();
                KickingCollidingComplete();
            }
            moveSpeed = moveSpeedConst;
            sprintSpeed = sprintSpeedConst;
            canKickAfterDrib = true;

            if (!S_Pressed && !D_Pressed && !A_Pressed)
            {
                ResetSpeed();
                if (rollDir.Equals("left"))
                    transform.localScale = new Vector3(6, 6, 6);
                else
                    transform.localScale = new Vector3(-6, 6, 6);
                moveSpeed = 150f;
                CancelInvoke();
                playerDribState = "drib f";
                ChangeAnimationState_instant_notransition("drib forward");
                Invoke("BusyComplete", 0.45f);
                isBusy = true;
         
            }
            else if (W_Pressed && D_Pressed && !A_Pressed && !S_Pressed)
            {
                ResetSpeed();
                TurnOnFoots();
                CancelInvoke();
                transform.localScale = new Vector3(6, 6, 6);
                playerDribState = "drib wd";
                ChangeAnimationState_instant_notransition("drib wd");
                Invoke("BusyComplete", 0.4f);
                isBusy = true;
            }
            else if (W_Pressed && !D_Pressed && A_Pressed && !S_Pressed)
            {
                ResetSpeed();

                TurnOnFoots();
                CancelInvoke();

                transform.localScale = new Vector3(-6, 6, 6);
                playerDribState = "drib aw";
                ChangeAnimationState_instant_notransition("drib wd");

                Invoke("BusyComplete", 0.4f);

                isBusy = true;
            }
            else if (!W_Pressed && !D_Pressed && A_Pressed && !S_Pressed)
            {
                ResetSpeed();
                TurnOnFoots();
                CancelInvoke();
                transform.localScale = new Vector3(6, 6, 6);
                playerDribState = "drib L";
                ChangeAnimationState_instant_notransition("drib L");
                Invoke("BusyComplete", 0.7f);
                isBusy = true;
            }
            else if (!W_Pressed && D_Pressed && !A_Pressed && !S_Pressed)
            {
                ResetSpeed();
                TurnOnFoots();
                CancelInvoke();
                transform.localScale = new Vector3(-6, 6, 6);
                playerDribState = "drib R";
                ChangeAnimationState_instant_notransition("drib L");
                Invoke("BusyComplete", 0.7f);
                isBusy = true;
            }
            else if (!W_Pressed && D_Pressed && !A_Pressed && S_Pressed)
            {
                ResetSpeed();
                TurnOnFoots();
                CancelInvoke();
                transform.localScale = new Vector3(-6, 6, 6);
                playerDribState = "drib SD";
                ChangeAnimationState_instant_notransition("drib AS");

                Invoke("BusyComplete", 0.5f);
                Invoke("ResetSpeed", 0.05f);


                isBusy = true;
            }
            else if (!W_Pressed && !D_Pressed && A_Pressed && S_Pressed)
            {
                ResetSpeed();
                TurnOnFoots();
                CancelInvoke();
                transform.localScale = new Vector3(6, 6, 6);
                playerDribState = "drib AS";
                ChangeAnimationState_instant_notransition("drib AS");

                Invoke("BusyComplete", 0.5f);
                Invoke("ResetSpeed", 0.05f);


                isBusy = true;
            }
            else if (S_Pressed)
            {
                ResetSpeed();
                if (rollDir.Equals("left"))
                    transform.localScale = new Vector3(6, 6, 6);
                else
                    transform.localScale = new Vector3(-6, 6, 6);
                moveSpeed = 150f;
                CancelInvoke();
                playerDribState = "drib f";
                ChangeAnimationState_instant_notransition("drib S");
                Invoke("BusyComplete", 0.45f);
                Invoke("LetsBeBusy", 0.05f);
                isBusy = true;
            }


        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && !W_Pressed && A_Pressed && !D_Pressed && S_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            moveSpeed = 60f;
            sprintSpeed = 100f;
            transform.localScale = new Vector3(6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.7f;
            kickingDirectionState = "heel kick A";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("heel kick");
            DisableRightFoot();
            Invoke("KickingHoldingComplete", 0.30f);
            Invoke("RaiseKickingHoldingComplete", 0.15f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && !W_Pressed && !A_Pressed && D_Pressed && S_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            moveSpeed = 60f;
            sprintSpeed = 100f;
            transform.localScale = new Vector3(-6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);

            animator.speed = 0.7f;
            kickingDirectionState = "heel kick D";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("heel kick");
            DisableRightFoot();
            Invoke("KickingHoldingComplete", 0.30f);
            Invoke("RaiseKickingHoldingComplete", 0.15f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && !W_Pressed && !A_Pressed && D_Pressed && !S_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            moveSpeed = 60f;
            sprintSpeed = 100f;
            transform.localScale = new Vector3(6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.7f;
            kickingDirectionState = "volley D";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("volley");
            dribColliderL.enabled = false; dribColliderLegL.enabled = false;
            Invoke("KickingHoldingComplete", 0.40f);
            Invoke("RaiseKickingHoldingComplete", 0.20f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && !W_Pressed && A_Pressed && !D_Pressed && !S_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            moveSpeed = 60f;
            sprintSpeed = 100f;
            transform.localScale = new Vector3(-6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.6f;
            kickingDirectionState = "volley A";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("volley");
            dribColliderL.enabled = false; dribColliderLegL.enabled = false;
            Invoke("KickingHoldingComplete", 0.40f);
            Invoke("RaiseKickingHoldingComplete", 0.20f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && W_Pressed && !A_Pressed && !D_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            transform.localScale = new Vector3(6, 6, 6);
            moveSpeed = 60f;
            sprintSpeed = 100f;

            /* if (rollDir.Equals("left"))
                 transform.localScale = new Vector3(-6, 6, 6);
             else
                 transform.localScale = new Vector3(6, 6, 6);*/

            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.5f;
            kickingDirectionState = "scorpion";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("scorpion");
            dribColliderL.enabled = false; dribColliderLegL.enabled = false;
            Invoke("KickingHoldingComplete", 0.40f);
            Invoke("RaiseKickingHoldingComplete", 0.20f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && !W_Pressed && !A_Pressed && !D_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.6f;
            kickingDirectionState = "przewrotka";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("przewrotka");
            dribColliderL.enabled = false; dribColliderLegL.enabled = false;
            Invoke("KickingHoldingComplete", 0.40f);
            Invoke("RaiseKickingHoldingComplete", 0.20f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && W_Pressed && A_Pressed && !S_Pressed && !D_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.6f;
            kickingDirectionState = "rabona AW";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("rabona");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", 0.45f);
            Invoke("RaiseKickingHoldingComplete", 0.22f);

        }
        else if (shootPressed_generally && Input.GetKey(KeyCode.LeftControl) && W_Pressed && D_Pressed && !S_Pressed && !A_Pressed && (!isBusy || isBusyJump || isSpecialShooting || canKickAfterDrib) && canIRabona)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            Invoke("LetsBeBusy", busyLatency);
            animator.speed = 0.6f;
            kickingDirectionState = "rabona DW";
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("rabona");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", 0.45f);
            Invoke("RaiseKickingHoldingComplete", 0.22f);

        }
        else if (shootPressed && W_Pressed && !A_Pressed && !D_Pressed && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            animator.speed = animatorSlow;
            kickingDirectionState = "Sprint Forward";
            //assignKickPower();
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("Strike Foward Jog");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", finishSprintStrikeAt);
            Invoke("RaiseKickingHoldingComplete", 0.25f);


        }
        else if (shootPressed && W_Pressed && A_Pressed && !D_Pressed && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            animator.speed = animatorSlow;

            kickingDirectionState = "Sprint Left";

            //assignKickPower();
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("Sprint Strike Left");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", finishSprintStrikeAt);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootPressed && W_Pressed && !A_Pressed && D_Pressed && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            animator.speed = animatorSlow;

            kickingDirectionState = "Sprint Right";

            //assignKickPower();
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("Sprint Strike Right");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", finishSprintStrikeAt);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootPressed && !W_Pressed && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            animator.speed = animatorSlow;

            kickingDirectionState = "Idle Kick";

            //assignKickPower();
            isHolding = true;
            isHoldingAnimation = true;
            ChangeAnimationState("Soccer Pass");
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("KickingHoldingComplete", finishIdleKickAt); //0.65
            Invoke("RaiseKickingHoldingComplete", 0.32f);

        }
        //strzal________________________________________________________________
        if (shootReleased && kickingDirectionState.Equals("heel kick A") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            shootReleasedFullPower = false;
            animator.speed = 1f;
            isHolding = false;
            isHoldingAnimation = false;
            isKicking = true;
            playerKickState = "heel kick A";
            ChangeAnimationStateFromCurrentTime("heel kick", 0.2f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.6f);
            Invoke("RaiseKickingHoldingComplete", 0.3f);

            kickingDirectionState = "";
        }
        else if (shootReleased && kickingDirectionState.Equals("heel kick D") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            shootReleasedFullPower = false;
            animator.speed = 1f;
            isHolding = false;
            isHoldingAnimation = false;
            isKicking = true;
            playerKickState = "heel kick D";
            ChangeAnimationStateFromCurrentTime("heel kick", 0.2f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.6f);
            Invoke("RaiseKickingHoldingComplete", 0.3f);

            kickingDirectionState = "";
        }
        else if (shootReleased && kickingDirectionState.Equals("volley D") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            shootReleasedFullPower = false;
            animator.speed = 1f;
            isHolding = false;
            isHoldingAnimation = false;
            isKicking = true;
            playerKickState = "volley D";
            ChangeAnimationStateFromCurrentTime("volley", 0.3f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.8f);
            Invoke("KickingComplete", 0.8f);
            Invoke("KickingCollidingComplete", 0.6f);
            Invoke("RaiseKickingHoldingComplete", 0.3f);

            kickingDirectionState = "";
        }
        else if (shootReleased && kickingDirectionState.Equals("volley A") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            shootReleasedFullPower = false;

            animator.speed = 1f;

            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "volley A";
            ChangeAnimationStateFromCurrentTime("volley", 0.3f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.8f);
            Invoke("KickingComplete", 0.8f);
            Invoke("KickingCollidingComplete", 0.6f);
            Invoke("RaiseKickingHoldingComplete", 0.3f);

            kickingDirectionState = "";

        }
        else if (shootReleased && kickingDirectionState.Equals("scorpion") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            shootReleasedFullPower = false;

            animator.speed = 1f;

            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "scorpion";
            ChangeAnimationStateFromCurrentTime("scorpion", 0.2f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.95f);
            Invoke("KickingComplete", 0.95f);
            Invoke("KickingCollidingComplete", 0.65f);
            Invoke("RaiseKickingHoldingComplete", 0.32f);

            kickingDirectionState = "";

        }
        else if (shootReleased && kickingDirectionState.Equals("przewrotka") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            CancelInvoke();
            Invoke("ActionTrue", 0.1f);

            shootReleasedFullPower = false;

            animator.speed = 1f;

            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "przewrotka";
            ChangeAnimationStateFromCurrentTime("przewrotka", 0.2f);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 2f);
            Invoke("KickingComplete", 2f);
            Invoke("KickingCollidingComplete", 1.4f);
            Invoke("RaiseKickingHoldingComplete", 0.7f);

            kickingDirectionState = "";

        }
        else if (shootReleased && kickingDirectionState.Equals("rabona AW") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";

            CancelInvoke();
            shootReleasedFullPower = false;

            animator.speed = 1f;
            kickingDirectionState = "";

            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "rabona";
            ChangeAnimationStateFromCurrentTime("rabona", startSprintKickTime);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.5f);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootReleased && kickingDirectionState.Equals("rabona DW") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";

            transform.localScale = new Vector3(-6, 6, 6);

            CancelInvoke();
            shootReleasedFullPower = false;

            animator.speed = 1f;
            kickingDirectionState = "";

            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "rabona";
            ChangeAnimationStateFromCurrentTime("rabona", startSprintKickTime);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.5f);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootReleased && kickingDirectionState.Equals("Sprint Forward") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            shootReleasedFullPower = false;

            animator.speed = 1f;
            kickingDirectionState = "";

            //assignKickPower();
            isHolding = false;
            isHoldingAnimation = false;

            isKicking = true;
            playerKickState = "jogaKick";
            ChangeAnimationStateFromCurrentTime("Strike Foward Jog", startSprintKickTime);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.5f);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootReleased && kickingDirectionState.Equals("Sprint Right") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";

            shootReleasedFullPower = false;

            animator.speed = 1f;

            kickingDirectionState = "";

            isHolding = false;
            isHoldingAnimation = false;
            //assignKickPower();
            isKicking = true;
            playerKickState = "jogaKick";
            ChangeAnimationStateFromCurrentTime("Sprint Strike Right", startSprintKickTime);
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.5f);
            Invoke("RaiseKickingHoldingComplete", 0.25f);

        }
        else if (shootReleased && kickingDirectionState.Equals("Sprint Left") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            shootReleasedFullPower = false;

            animator.speed = 1f;

            kickingDirectionState = "";

            isHolding = false;
            isHoldingAnimation = false;

            //assignKickPower();
            isKicking = true;
            playerKickState = "jogaKick";
            ChangeAnimationStateFromCurrentTime("Sprint Strike Left", startSprintKickTime);

            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.7f);
            Invoke("KickingComplete", 0.7f);
            Invoke("KickingCollidingComplete", 0.4f);
            Invoke("RaiseKickingHoldingComplete", 0.2f);

        }
        else if (shootReleased && kickingDirectionState.Equals("Idle Kick") && (!isBusy || isBusyJump || canKickAfterDrib))
        {
            playerDribState = "";
            shootReleasedFullPower = false;
            animator.speed = 1f;

            kickingDirectionState = "";
            isHolding = false;
            isHoldingAnimation = false;

            //assignKickPower();
            isKicking = true;
            ChangeAnimationStateFromCurrentTime("Soccer Pass", startStandKickTime);

            playerKickState = "idleKick";
            dribCollider.enabled = false; dribColliderLeg.enabled = false;
            Invoke("BusyComplete", 0.8f);
            Invoke("KickingComplete", 0.8f);
            Invoke("KickingCollidingComplete", 0.4f);
            Invoke("RaiseKickingHoldingComplete", 0.2f);

        }
        //movement_______________________________________
        else if (Input.GetKeyDown(KeyCode.Space) && !isHoldingAnimation && !isKicking && !isKickingUp)
        {
            isBusy = true;
            isBusyJump = true;
            playerDribState = "jogaDrib";
            ChangeAnimationState("jump");
            Invoke("BusyComplete", 1f);

        }
        if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && !D_Pressed && S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Forward");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && !D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Forward");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && A_Pressed && !D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Jog Sprint Left");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Jog Sprint Right");
        }
        else if (SHIFT_Pressed && !W_Pressed && !isKicking && !isBusy && !A_Pressed && D_Pressed && !S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Strafe Right");
        }
        else if (SHIFT_Pressed && !W_Pressed && !isKicking && !isBusy && A_Pressed && !D_Pressed && !S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Strafe Left");
        }


        else if (A_Pressed && W_Pressed && !isKicking && !isBusy && !isHoldingAnimation)/////////////////tu sie drybling zaczyna
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward Left");
        }
        else if (W_Pressed && !isKicking && !isBusy && !isHoldingAnimation && S_Pressed && !A_Pressed && !D_Pressed)
        {
            animator.SetBool("Jog Forward", true);
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward");
        }
        else if (D_Pressed && W_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward Right");
        }
        else if (A_Pressed && S_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward Left");
        }
        else if (D_Pressed && S_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward Right");
        }
        else if (D_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Strafe Right");
        }
        else if (A_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Strafe Left");
        }
        else if (S_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward");
        }
        else if (W_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {

            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward");

        }



        if (shootReleased)
        {
            checker.SetActive(false);
        }

        if (shootPressed && (!isBusy || isBusyJump))
        {
            rotationSlider.gameObject.SetActive(true);
            powerSlider.gameObject.SetActive(true);

            rotationForce = 4;
            kickPower = 1;
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (!isSpecialShooting)
            {
                CancelInvoke();
                BusyComplete();
                isHolding = false;
                isHoldingAnimation = false;
                if (wasKickedUp)
                    wasKickedUp = false;
                else 
                    KickingCollidingComplete();
                KickingComplete();
                animator.speed = 1f;
                kickingDirectionState = "";
                animator.enabled = true;
            }

        }


    }
    public string lastDribState;
    void ChangeAnimationState_instant_notransition_looping(string state)
    {
        RaiseAnimationEvent(state, "ChangeAnimationState_instant_notransition_looping");
        animator.Play(state);
        currentAnimState = state;
    }
    void ChangeAnimationState_instant_notransition_looping_sync(string state)
    {
        animator.enabled = true;


        animator.Play(state);
        currentAnimState = state;
    }

    void ChangeAnimationState_instant_notransition(string state)
    {
        RaiseAnimationEvent(state, "ChangeAnimationState_instant_notransition");
        if (currentAnimState == state)
            return;
        animator.Play(state);
        currentAnimState = state;
    }
    void ChangeAnimationState_instant_notransition_sync(string state)
    {
        animator.enabled = true;


        if (currentAnimState == state)
            return;
        animator.Play(state);
        currentAnimState = state;
    }

    bool isParading = false;
    void LetsBeBusy()
    {
        isParading = true;
        canIRabona = false;
    }

    void LetsBeBusyReally()
    {
        isBusy = true;
    }

    public bool isGkCatching = false;
    void GoalkeeperAnimationUpdate()
    {


        bool W_Pressed = Input.GetKey("w");
        bool D_Pressed = Input.GetKey("d");
        bool S_Pressed = Input.GetKey("s");
        bool A_Pressed = Input.GetKey("a");
        bool SHIFT_Pressed = Input.GetKey(KeyCode.LeftShift);
        bool SPACE_Pressed = Input.GetKey(KeyCode.Space);
        bool CTRL_Pressed = Input.GetKey(KeyCode.LeftControl);

        bool divePressed = Input.GetMouseButton(0);
        bool catchPressed = Input.GetMouseButton(1);

        if (SHIFT_Pressed && !S_Pressed)
            isSprinting = true;
        else
            isSprinting = false;

        if (!W_Pressed && !D_Pressed && !S_Pressed && !A_Pressed && !isKicking && !isBusy && !isHoldingAnimation)
        {
            ChangeAnimationState("idle gk");
        }


        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && !D_Pressed && S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Forward");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && !D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Forward");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && A_Pressed && !D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Jog Sprint Left");
        }
        else if (SHIFT_Pressed && W_Pressed && !isKicking && !isBusy && !A_Pressed && D_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Jog Sprint Right");
        }
        else if (SHIFT_Pressed && !W_Pressed && !isKicking && !isBusy && !A_Pressed && D_Pressed && !S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Strafe Right");
        }
        else if (SHIFT_Pressed && !W_Pressed && !isKicking && !isBusy && A_Pressed && !D_Pressed && !S_Pressed && !isHoldingAnimation)
        {
            playerDribState = "sprintDrib";
            ChangeAnimationState("Sprint Strafe Left");
        }


        if (D_Pressed && CTRL_Pressed && catchPressed && !isParading)//catching
        {
            isGkCatching = true;
            ChangeAnimationState("catch down r");
            Invoke("BusyComplete", 1.3f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (W_Pressed && !A_Pressed && !D_Pressed && CTRL_Pressed && catchPressed && !isParading)
        {
            isGkCatching = true;
            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("catch forward");
            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.70f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (A_Pressed && CTRL_Pressed && catchPressed && !isParading)
        {
            isGkCatching = true;
            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState("catch down r");
            Invoke("BusyComplete", 1.3f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (SPACE_Pressed && D_Pressed && catchPressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isGkCatching = true;

            ChangeAnimationState_instant_notransition("catch high r");
            Invoke("BusyComplete", 1.6f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            rb.drag = groundDrag;
            isBusy = true;


        }
        else if (SPACE_Pressed && A_Pressed && catchPressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isGkCatching = true;
            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState_instant_notransition("catch high r");
            Invoke("BusyComplete", 1.6f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            rb.drag = groundDrag;

        }
        else if (CTRL_Pressed && catchPressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isGkCatching = true;

            ChangeAnimationState_instant_notransition("catch down");
            Invoke("BusyComplete", 1.2f);
            Invoke("DivingComplete", 0.40f);
            isDiving = true;
        }
        else if (D_Pressed && catchPressed && !isParading)
        {
            ChangeAnimationState_instant_notransition("catch right");
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isGkCatching = true;

            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
        }
        else if (A_Pressed && catchPressed && !isParading)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState_instant_notransition("catch right");
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isGkCatching = true;

            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
        }
        else if (SPACE_Pressed && catchPressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isDiving = true;
            rb.drag = groundDrag;
            isGkCatching = true;

            ChangeAnimationState_instant_notransition("catch high");
            Invoke("DivingComplete", 0.20f);

            Invoke("BusyComplete", 1.15f);
        }
        else if (catchPressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isGkCatching = true;

            ChangeAnimationState_instant_notransition("catch stay");
            Invoke("BusyComplete", 1.2f);
            // Invoke("DivingComplete", 0.8f);
            // isDiving = true;
        }


        else if (D_Pressed && CTRL_Pressed && divePressed && !isParading) //diving
        {
            ChangeAnimationState("body block right");
            Invoke("BusyComplete", 1.3f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (W_Pressed && CTRL_Pressed && divePressed && !isParading)
        {
            transform.localScale = new Vector3(6, 6, 6);
            ChangeAnimationState("dive forward");
            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.70f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (A_Pressed && CTRL_Pressed && divePressed && !isParading)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState("body block right");
            Invoke("BusyComplete", 1.3f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
        }
        else if (SPACE_Pressed && D_Pressed && divePressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);

            ChangeAnimationState_instant_notransition("body dive up right");
            Invoke("BusyComplete", 1.6f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            rb.drag = groundDrag;
            isBusy = true;


        }
        else if (SPACE_Pressed && A_Pressed && divePressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;

            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState_instant_notransition("body dive up right");
            Invoke("BusyComplete", 1.6f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
            rb.drag = groundDrag;

        }
        else if (CTRL_Pressed && divePressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;

            ChangeAnimationState_instant_notransition("dive down");
            Invoke("BusyComplete", 0.8f);
            Invoke("DivingComplete", 0.40f);
            isDiving = true;
        }
        else if (D_Pressed && divePressed && !isParading)
        {
            ChangeAnimationState_instant_notransition("dive right");
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;

            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
        }
        else if (A_Pressed && divePressed && !isParading)
        {
            transform.localScale = new Vector3(-6, 6, 6);
            ChangeAnimationState_instant_notransition("dive right");
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;

            Invoke("BusyComplete", 1.4f);
            Invoke("DivingComplete", 0.75f);
            isDiving = true;
        }
        else if (SPACE_Pressed && divePressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;
            isDiving = true;
            rb.drag = groundDrag;

            ChangeAnimationState_instant_notransition("dive high");
            Invoke("DivingComplete", 0.20f);

            Invoke("BusyComplete", 1.15f);
        }
        else if (divePressed && !isParading)
        {
            Invoke("LetsBeBusy", busyLatency);
            isBusy = true;

            ChangeAnimationState_instant_notransition("dive");
            Invoke("BusyComplete", 1.2f);
            // Invoke("DivingComplete", 0.8f);
            // isDiving = true;
        }

        else if (A_Pressed && W_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)/////////////////tu sie drybling zaczyna
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward Left");
        }
        else if (D_Pressed && W_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward Right");
        }
        else if (A_Pressed && S_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward Left");
        }
        else if (D_Pressed && S_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward Right");
        }
        else if (D_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("sidestep gk right");
        }
        else if (A_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("sidestep gk left");
        }
        else if (S_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {
            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Backward");
        }
        else if (W_Pressed && !isKicking && !isBusy && !isHoldingAnimation && !SHIFT_Pressed)
        {

            playerDribState = "jogaDrib";
            ChangeAnimationState("Jog Forward");

        }






    }

    CursorLockMode lockMode;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        lockMode = CursorLockMode.Locked;
        Cursor.lockState = lockMode;


    }

    float counter = 0f;
    private void OnCollisionEnter(Collision collision)
    {
        // if(collision.collider.tag == "ball" && isGoalKeeper)
    }
    private void OnTriggerEnter(Collider other)
    {



        //Check to see if the tag on the collider is equal to Enemy
        if (other.tag == "ball")
        {


            bool D_Pressed = Input.GetKey("d");
            bool A_Pressed = Input.GetKey("a");
            bool shootReleased = Input.GetMouseButtonUp(0);



            /*if (A_Pressed && wasReleased)
                RotationState = "rotateRight";
            else if (D_Pressed && wasReleased)
                RotationState = "rotateLeft";
            else 
                RotationState = "noRotate";*/

            //Debug.Log(RotationState);


            //checker.SetActive(true);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {

        }
        else if (stream.IsReading)
        {

        }
    }
    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;

        if (eventCode == 1)
        {
            object[] data = (object[])photonEvent.CustomData;

            if (view.IsMine && !data[0].ToString().Equals(view.ViewID.ToString()))
            {
                if (data.Length == 4)
                {
                    SyncOtherPlayersAnimation(data[0].ToString(), data[1].ToString(), data[2].ToString(), (float)data[3]);
                }
                else
                    SyncOtherPlayersAnimation(data[0].ToString(), data[1].ToString(), data[2].ToString());
                //Debug.Log(data[0] + " " + data[1]);
            }
        }
        else if (eventCode == 2)
        {
            object[] data = (object[])photonEvent.CustomData;

            if (!view.IsMine && canIstopAnimator && view.ViewID.ToString().Equals(data[0].ToString()))
            {
                animator.enabled = false;
            }
        }
        else if (eventCode == 3)
        {
            object[] data = (object[])photonEvent.CustomData;

            if (!view.IsMine && view.ViewID.ToString().Equals(data[0].ToString()))
            {
                if (data[1].ToString().Equals("red"))
                {
                    Component[] bodyParts;
                    bodyParts = GetComponentsInChildren<Renderer>();
                    foreach (Renderer body in bodyParts)
                    {
                        body.material.SetColor("_Color", Color.red);
                        
                    }
                }
                else
                {
                    Component[] bodyParts;
                    bodyParts = GetComponentsInChildren<Renderer>();
                    foreach (Renderer body in bodyParts)
                    {
                        body.material.SetColor("_Color", Color.blue);
                        
                    }
                }
            }
        }
    }
}
