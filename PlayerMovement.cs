using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float Speed;
    public float rotationSpeed;
    private Vector2 movementValue;
    private float lookValue;
    private Rigidbody rb;

    //  Sprint variables
    public float sprintMultiplier = 1.8f; // speed multiplier while shift is pressed
    private bool isSprinting; // flag for if shift is being pressed

    // Jump variables
    public float force = 6f; // strength for each jump
    public float checkDistance = 1.1f; // length from the player's center down to the floor
    private int jumps; // amount of jumps being used
    private float lastJump; // gives time between frames to jump before reset

    // Dodge variables
    public float dodgeSpeed = 20f; // speed during dodge burst
    public float dodgeTime = 0.2f; // how long will the dodge burst last 
    public float dodgeCooldown = 1f;
    private bool isDodging;
    private float dodgeEndTime;
    private float nextDodgeTime;
    private Vector3 dodgeDirection; // direction of the dodge, using vector3 to consider all directions

    private void Awake()
    {
       Cursor.visible = false;
       Cursor.lockState = CursorLockMode.Locked;

       rb = GetComponent<Rigidbody>();
       rb.freezeRotation = true;  // should stop physics from knocking the player over
    }

    public void OnMove(InputValue value)
    {
        movementValue = value.Get<Vector2>()*Speed;
    }

    public void OnLook(InputValue value)
    {
        lookValue = value.Get<Vector2>().x*rotationSpeed;
    }

    // sprint, will check if sprint key is pressed every frame and set flag
    public void OnSprint(InputValue value)
    {
        if (value.isPressed)
        {
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
        }
    }

    // jump, one press = jump, two presses = double jump
    public void OnJump(InputValue value)
    {
        // check for pressed button
        if (!value.isPressed) return;
        // check for possible jumps left
        if (jumps >= 2) return;

        // erase downward speed so the double jump feels as strong as the first
        Vector3 v = rb.linearVelocity;
        v.y = 0f;
        rb.linearVelocity = v;

        // jump by adding upwards force to our rigidbody
        rb.AddForce(Vector3.up * force, ForceMode.Impulse);
        jumps++; // update amount of jumps used
        lastJump = Time.time; // set a point of reference for last jump to apply cooldown
    }

    // dodge, quick burst of speed in determined direction (backwards not moving)
    public void OnDodge(InputValue value)
    {
        // check if dodge has been pressed
        if (!value.isPressed) return;
        if (isDodging || Time.time < nextDodgeTime) return; // check if player is currently dodging or on cooldown

        // set up which direction the dodge will go towards depending on what direction the player is going
        dodgeDirection = new Vector3(movementValue.x, 0, movementValue.y).normalized;
        if (dodgeDirection == Vector3.zero) dodgeDirection = Vector3.back; // if player is still step back

        // set flag as true and update times for when dodge will end and when it can be used again
        isDodging = true;
        dodgeEndTime = Time.time + dodgeTime;
        nextDodgeTime = Time.time + dodgeCooldown;
    }

    // flag that true if the floor is under the player
    private bool IsGrounded()
    {
        // way to check if the character is the correct distance from the ground, returns true or false
        return Physics.Raycast(transform.position, Vector3.down, checkDistance);
    }

    
    void Start()
    {
        //start off with 0 jumps
        jumps = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //check for dodge
        if (isDodging)
        {
            // during the dodge we move in the stored direction and ignore normal movement
            transform.Translate(dodgeDirection*dodgeSpeed*Time.deltaTime);
            // give room for dodge to run between frames by checking 
            // it's end time before resetting isDodging
            if (Time.time >= dodgeEndTime) isDodging = false;
        }
    
        float multiplier = 1f; //leaving multiplier out of the if so it automatically resets every update
        // check for sprinting
        if (isSprinting)
        {
            multiplier = sprintMultiplier; // use faster multiplier while sprinting
        }
        // controls player movement using multiplier
        transform.Translate(movementValue.x*multiplier*Time.deltaTime,
        0, movementValue.y*multiplier*Time.deltaTime);

        //the normal rotate transform
        transform.Rotate(0, lookValue*Time.deltaTime, 0);

        // reset amount of jumps when the feet touch the ground
        if (Time.time > lastJump && IsGrounded())
        {
            jumps = 0;
        }
    }
}