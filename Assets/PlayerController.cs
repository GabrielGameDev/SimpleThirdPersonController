using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Vector3 normalizeVector;

    public float speed = 1;


    public float turnSpeed = 15;


    public float jumpHeight = 5;
    public float gravity = 10;
    public float stepDown = 0.3f;
    public float airControl = 2;


    private Animator animator;
    private Vector2 input;

    private CharacterController cc;
    private Vector3 rootMotion;
    private Vector3 velocity;
    private bool isJumping;
    private float animVelocity;
    private Quaternion freeRotation;
    private Vector3 targetDirection;

    Camera mainCam;


    private void Awake()
    {
        animator = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

    }

    private void Start()
    {

        mainCam = Camera.main;
    }

    private void Update()
    {


        input.x = Input.GetAxis("Horizontal");
        input.y = Input.GetAxis("Vertical");

        float animSpeed = Mathf.Abs(input.x) + Mathf.Abs(input.y);

        animSpeed = Mathf.Clamp(animSpeed, 0f, 1f);
        animSpeed = Mathf.SmoothDamp(animator.GetFloat("Speed"), animSpeed, ref animVelocity, 0.1f);
        animator.SetFloat("Speed", animSpeed);



        animator.SetFloat("Direction", 0);

        if (Input.GetButtonDown("Jump"))
        {
            Jump(jumpHeight);
        }




    }

    private void FixedUpdate()
    {


        if (isJumping)
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
            Vector3 displacement = velocity * Time.fixedDeltaTime;
            displacement += CalculateAirControl();
            cc.Move(displacement);
            isJumping = !cc.isGrounded;
            rootMotion = Vector3.zero;
        }
        else
        {
            cc.Move((rootMotion * speed) + (Vector3.down * stepDown));
            rootMotion = Vector3.zero;

            if (!cc.isGrounded)
            {
                isJumping = true;
                velocity = animator.velocity;
                velocity.y = 0;
            }
        }

        UpdateTargetDirection();
        if (input != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Vector3 lookDirection = targetDirection.normalized;
            freeRotation = Quaternion.LookRotation(lookDirection, transform.up);
            var diferenceRotation = freeRotation.eulerAngles.y - transform.eulerAngles.y;
            var eulerY = transform.eulerAngles.y;

            if (diferenceRotation < 0 || diferenceRotation > 0) eulerY = freeRotation.eulerAngles.y;
            var euler = new Vector3(0, eulerY, 0);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), turnSpeed * Time.deltaTime);
        }

        animator.SetBool("isJumping", !cc.isGrounded);
    }

    public virtual void UpdateTargetDirection()
    {

        var forward = mainCam.transform.TransformDirection(Vector3.forward);
        forward.y = 0;

        //get the right-facing direction of the referenceTransform
        var right = mainCam.transform.TransformDirection(Vector3.right);

        // determine the direction the player will face based on input and the referenceTransform's right and forward directions
        targetDirection = input.x * right + input.y * forward;

    }

    private void OnAnimatorMove()
    {
        rootMotion += animator.deltaPosition;
    }

    public void Jump(float jumpHeightAmount)
    {
        if (!isJumping)
        {
            isJumping = true;
            velocity = animator.velocity;
            velocity.y = Mathf.Sqrt(2 * gravity * jumpHeightAmount);
        }
    }

    Vector3 CalculateAirControl()
    {

        //return ((transform.forward * input.y) + (transform.right * input.x)) * (airControl / 100);
        return targetDirection * (airControl / 100);
    }
}
