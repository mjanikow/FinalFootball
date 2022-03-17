using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegController : MonoBehaviour
{
    public float speed;
    public float rotSpeed;
    public float gravity = 4;
    public float rot = 0;
    public float jumpSpeed = 8.0f;



    public float Speed = 5f;
    public float JumpHeight = 2f;
    public float GroundDistance = 0.2f;
    public float DashDistance = 5f;
    public LayerMask Ground;
    Vector3 moveDir = Vector3.zero;

    CharacterController controller;
    Animator anim;
    Quaternion moveRotation;
    public SimpleMouseOrbit cam;

    Quaternion direction;

    private Rigidbody _body;
    private Vector3 _inputs = Vector3.zero;
    private bool _isGrounded = true;
    private Transform _groundChecker;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        //anim = GetComponent<Animator>();
       // transform.position = Quaternion.Euler(0, -90, 0) * transform.position;
        // _body = GetComponent<Rigidbody>();
        _groundChecker = transform.GetChild(0);
    }
    bool jump=false;

    void FixedUpdate()
    {   
        
        movement();
        rotate();



       

        //animations();
        // _body.MovePosition(_body.position + _inputs * Speed * Time.fixedDeltaTime);

    }
    void Update()
    {   
        //RigidBodyMovement();
        fastMovement();
        

    }

    void rotate2(){
        moveRotation = cam.rotation;
        moveRotation.x = 0;
        moveRotation.z = 0;

        if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)){
            moveRotation *= Quaternion.Euler(Vector3.up * -45);
        }
        else if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)){
            moveRotation *= Quaternion.Euler(Vector3.up * 45);
        }
        else if (Input.GetKey(KeyCode.W))
        {
            moveRotation = cam.rotation;
            moveRotation.x = 0;
            moveRotation.z = 0;
        }
        else if (Input.GetKey(KeyCode.A) )
        {
            moveRotation *= Quaternion.Euler(Vector3.up * -90);
        }
        else if (Input.GetKey(KeyCode.D) )
        {
            moveRotation *= Quaternion.Euler(Vector3.up * 90);

        }
        else{
            moveRotation = cam.rotation;
            moveRotation.x = 0;
            moveRotation.z = 0;
        }
        transform.rotation = moveRotation;   
    }

    void RigidBodyMovement(){
        _isGrounded = Physics.CheckSphere(_groundChecker.position, GroundDistance, Ground, QueryTriggerInteraction.Ignore);


        _inputs = Vector3.zero;
        _inputs.x = Input.GetAxis("Horizontal");
        _inputs.z = Input.GetAxis("Vertical");
        if (_inputs != Vector3.zero)
            transform.forward = _inputs;

        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _body.AddForce(Vector3.up * Mathf.Sqrt(JumpHeight * -2f * Physics.gravity.y), ForceMode.VelocityChange);
        }
        if (Input.GetButtonDown("Dash"))
        {
            Vector3 dashVelocity = Vector3.Scale(transform.forward, DashDistance * new Vector3((Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime), 0, (Mathf.Log(1f / (Time.deltaTime * _body.drag + 1)) / -Time.deltaTime)));
            _body.AddForce(dashVelocity, ForceMode.VelocityChange);
        }
    }

    void setFalse(){
            anim.SetBool("IsRunning", false);
            anim.SetBool("isRunLeftLight", false);
            anim.SetBool("isRunLeft", false);
            anim.SetBool("isRunLeftBack", false);
            anim.SetBool("isRunRightLight", false);
            anim.SetBool("isRunRight", false);
            anim.SetBool("isRunRightBack", false);
            anim.SetBool("isRunBack", false);
            anim.SetBool("IsIdle", false);
            anim.SetBool("isJumpRun", false);
    }

    bool isRunning = false;
    void animations2(){
        if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)){
            anim.SetBool("IsRunning", true);
            isRunning=true;
        }
        else if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)){
           anim.SetBool("IsRunning", true);
            isRunning=true;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("IsRunning", true);
            isRunning=true;

        }
        else if (Input.GetKey(KeyCode.A) )
        {
            anim.SetBool("IsRunning", true);
            isRunning=true;

        }
        else if (Input.GetKey(KeyCode.D) )
        {
            anim.SetBool("IsRunning", true);
            isRunning=true;
        }
        else{
            anim.SetBool("IsIdle", true);
            isRunning=false;
            
        }
    }
    void animations()
    {
        if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)){
            anim.SetBool("isRunLeftLight", true);
            setFalseExcept("isRunLeftLight");
        }
        else if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D)){
            anim.SetBool("isRunRightLight", true);
            setFalseExcept("isRunRightLight");
        }
        else if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("IsRunning", true);
            setFalseExcept("IsRunning");

        }
        else if (Input.GetKey(KeyCode.A) )
        {
            anim.SetBool("isRunLeft", true);
            setFalseExcept("isRunLeft");

        }
        else if (Input.GetKey(KeyCode.D) )
        {
            anim.SetBool("isRunRight", true);
            setFalseExcept("isRunRight");
        }
        else{
            anim.SetBool("IsIdle", true);
            setFalseExcept("IsIdle");
            
        }

        /*if(Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.A)){
            setFalse();
            anim.SetBool("isRunLeftLight", true);
        }else if (Input.GetKey(KeyCode.W))
        {
            setFalse();
            anim.SetBool("IsRunning", true);
        }
        else{
            setFalse();
        }*/
    }
    void movement()
    {
        if (controller.isGrounded)
        {
            moveDir = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDir = transform.TransformDirection(moveDir);
            moveDir *= speed;
            moveDir.y = 0;

            if(jump){
                moveDir.y = jumpSpeed;
                jump=false;
            }
        }

        moveDir.y -= gravity * Time.deltaTime;
        controller.Move(moveDir * Time.deltaTime);
    }
    void rotate()
    {
        moveRotation = cam.rotation;
        moveRotation.x = 0;
        moveRotation.z = 0;
        transform.rotation = moveRotation;   
    }
    void fastMovement(){
      if (Input.GetKeyDown(KeyCode.Space))
            {
              jump=true;
            }
    }




    void setFalseExcept(string x){
        if(!x.Equals("IsRunning"))
            anim.SetBool("IsRunning", false);
        if(!x.Equals("isRunLeftLight"))
            anim.SetBool("isRunLeftLight", false);
        if(!x.Equals("isRunLeft"))
            anim.SetBool("isRunLeft", false);
        if(!x.Equals("isRunLeftBack"))
            anim.SetBool("isRunLeftBack", false);
        if(!x.Equals("isRunRightLight"))
            anim.SetBool("isRunRightLight", false);
        if(!x.Equals("isRunRight"))
            anim.SetBool("isRunRight", false);
        if(!x.Equals("isRunRightBack"))
            anim.SetBool("isRunRightBack", false);
        if(!x.Equals("isRunBack"))
            anim.SetBool("isRunBack", false);
        if(!x.Equals("IsIdle"))
            anim.SetBool("IsIdle", false);
        if(!x.Equals("isJumpRun"))
            anim.SetBool("isJumpRun", false);
    }

}
