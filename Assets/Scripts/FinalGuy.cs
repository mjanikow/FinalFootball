using Animancer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalGuy : MonoBehaviour
{
    Rigidbody rb;
    Animator animator;
    public SimpleMouseOrbit cam;
    Camera camObject;
    string currentAnimState = "";
    public GameObject headAimTarget;

    [Header("Animation crossfade values")]
    public float TransitionDuration = 0.5f;
    public float TimeOffset = 0f;
    public float TransitionTime = 0.5f;
    public float newBreathingDuration = 0.02f;
    public float currentBreathingDuration = 0.02f;
    public float newBreathingTime = 0.02f;
    private int frames = 0;


    [Header("Movement values")]
    public float moveSpeed = 6f;
    float horizontalMovement;
    float verticalMovement;
    public float sprintSpeed = 6f;
    public float airMultiplier = 0.4f;
    public float jumpForce = 5f;
    public float isGroundedHeight = 6f;
    bool isGrounded = true;
    bool isSprinting = false;
    public string playerDribState = "jogaDrib";
    public string playerKickState = "";
    Vector3 gravity = new Vector3(0, -50f, 0);
    Vector3 moveDirection;

    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 2f;

    Quaternion moveRotation;
    private int currentHash;
    private float currentLength;
    private float normalizedTime;
    private bool inTransition;
    private float transitionDuration;
    private float transitionNormalizedTime;
    private int nextHash;
    private float nextNormalizedTime;
    private int layer = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        animator = GetComponent<Animator>();
        Physics.gravity = gravity;
        cam.SetTarget(rb.transform);
        Array.Clear(rotateArray, 0, rotateArray.Length);

    }
    bool isMoving = false;
    
    float HandleRotation()
    {
        //najpierw od 90 do 0 a potem od 360 do 270
        float basicRotation = cam.transform.eulerAngles.x;

        if (basicRotation < 90)
        {
            return 90 - basicRotation;
        }
        else
            return (basicRotation - 360) * -1 + 90; 
    }
    CursorLockMode lockMode;

    void Awake()
    {
      //  lockMode = CursorLockMode.Locked;
      //  Cursor.lockState = lockMode;
    }
    // Update is called once per frame
    void Update()
    {
        frames++;

        controlMovement();
        MyInput();
        Vector3 temp = transform.position;
        temp.y = temp.y + 2f;
        isGrounded = Physics.Raycast(temp, Vector3.down, isGroundedHeight / 2 + 0.1f);
        ControlDrag();

        AnimateMovement();

        headAimTarget.transform.localPosition = new Vector3(headAimTarget.transform.localPosition.x, HandleRotation() / 10 + 3, headAimTarget.transform.localPosition.z);

        if(calculateRotationFactor() > 2)
        Debug.Log(calculateRotationFactor());
        

    }
    private void FixedUpdate()
    {
        MovePlayer();
    }
    float[] rotateArray = new float[30];

    private float calculateRotationFactor()
    {
        if (frames > 29) frames = 0;
        rotateArray[frames] = transform.eulerAngles.y;

        float min = rotateArray.Min();
        float max = rotateArray.Max();

        return max / min;
    }
    private void controlMovement()
    {      
        if (Input.GetKey(KeyCode.LeftShift) && !Input.GetKey("s"))
            isSprinting = true;
        else
            isSprinting = false;

        if (isMoving)
        {
            
            if (isSprinting)
            {
                float val = rb.velocity.magnitude;
                if (val < 19)
                    val = 19;
                animator.speed = (val - 19) / 47f + 0.6f;

            }
            else
                animator.speed = rb.velocity.magnitude / 47f + 0.4f;
        }
    }

    private void MovePlayer()
    {
        moveRotation = cam.rotation;
        moveRotation.x = 0;
        moveRotation.z = 0;
        transform.rotation = moveRotation;
        if (isSprinting)
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
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier, ForceMode.Acceleration);

        }



    }

    void AnimateMovement()
    {

        

        bool W_Pressed = Input.GetKey("w");
        bool D_Pressed = Input.GetKey("d");
        bool A_Pressed = Input.GetKey("a");
        bool S_PRessed = Input.GetKey("s");

        if( (!W_Pressed && !A_Pressed && !D_Pressed && !S_PRessed))
        {
                ChangeAnimationState("idle"); isMoving = false;
        }
        else if (!isSprinting)
        {
            if (W_Pressed && !A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("jog w"); isMoving = true;
            }
            else if (W_Pressed && A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("jog wa"); isMoving = true; 
            }
            else if (W_Pressed && !A_Pressed && D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("jog wd"); isMoving = true; 
            }
            else if (!W_Pressed && !A_Pressed && D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("jog d"); isMoving = true;
            }
            else if (!W_Pressed && A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("jog a"); isMoving = true;
            }
            else if (!W_Pressed && A_Pressed && !D_Pressed && S_PRessed)
            {
                ChangeAnimationState("jog as"); isMoving = true; 
            }
            else if (!W_Pressed && !A_Pressed && D_Pressed && S_PRessed)
            {
                ChangeAnimationState("jog sd"); isMoving = true; 
            }
            else if (!W_Pressed && !A_Pressed && !D_Pressed && S_PRessed)
            {
                ChangeAnimationState("jog s"); isMoving = true;
            }
        }
        else
        {
            if (W_Pressed && !A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("sprint w"); isMoving = true;
            }
            else if (W_Pressed && A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("sprint wa"); isMoving = true;
            }
            else if (W_Pressed && !A_Pressed && D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("sprint wd"); isMoving = true;
            }
            else if (!W_Pressed && !A_Pressed && D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("sprint d"); isMoving = true;
            }
            else if (!W_Pressed && A_Pressed && !D_Pressed && !S_PRessed)
            {
                ChangeAnimationState("sprint a"); isMoving = true;
            }          
        }
        
        

    }

    void MyInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
    }

    void ControlDrag()
    {
        if (isGrounded)
            rb.drag = groundDrag;
        else
            rb.drag = airDrag;
    }

    
    void ChangeAnimationState(string newState)
    {
        


        if (currentAnimState == newState)
            return;
        if (currentAnimState == "idle")
            animator.CrossFade(newState, currentBreathingDuration, -1, 0, 0);
        else if (newState == "idle")
        {

            animator.CrossFade(newState, currentBreathingDuration, -1, 0, 0);

        }
        else
            animator.CrossFade(newState, TransitionDuration, -1, TimeOffset, TransitionTime);
        currentAnimState = newState;
    }
   
}

