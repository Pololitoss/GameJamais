using UnityEngine;
using UnityEngine.InputSystem;

public class Deplacement : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    private Vector3 velocity = Vector3.zero;
    private Vector2 moveInput;

    public bool isJumping = false;
    public float jumpForce;
    public bool isGrounded;
    public int doubleJump = 0;

    public Transform groundCheckLeft;
    public Transform groundCheckRight;

    void OnEnable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var moveAction = playerInput.actions["Move"];
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
            
            var jumpAction = playerInput.actions["Jump"];
            jumpAction.performed += OnJump;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapArea(groundCheckLeft.position, groundCheckRight.position);

        float horizontalMvt = moveInput.x * moveSpeed * Time.deltaTime;

        MovePlayer(horizontalMvt);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && (isGrounded || doubleJump<2))
        {
            doubleJump ++;
            isJumping = true;
        }
        if(isGrounded){
            doubleJump = 0;
        }
    }

    void MovePlayer(float _horizontalMvt){
        Vector3 targetVelocity = new Vector2(_horizontalMvt, rb.linearVelocity.y);
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, .05f); //la on applique la vitesse au rb

        if(isJumping == true){
            rb.AddForce(new Vector2(0f, jumpForce));
            isJumping = false;
        }
    }
}
