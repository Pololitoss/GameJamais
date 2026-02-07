using UnityEngine;
using UnityEngine.InputSystem;

public class DeplacementOrange : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
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
            var moveAction = playerInput.actions["MoveOrange"];
            moveAction.performed += OnMove;
            moveAction.canceled += OnMove;
            
            var jumpAction = playerInput.actions["JumpOrange"];
            jumpAction.performed += OnJump;
            
            var attackBiteAction = playerInput.actions["AttackBiteOrange"];
            attackBiteAction.performed += OnAttackBite;
            
            var attackTeteAction = playerInput.actions["AttackTeteOrange"];
            attackTeteAction.performed += OnAttackTete;
        }
    }

    void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var moveAction = playerInput.actions["MoveOrange"];
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
            
            var jumpAction = playerInput.actions["JumpOrange"];
            jumpAction.performed -= OnJump;
            
            var attackBiteAction = playerInput.actions["AttackBiteOrange"];
            attackBiteAction.performed -= OnAttackBite;
            
            var attackTeteAction = playerInput.actions["AttackTeteOrange"];
            attackTeteAction.performed -= OnAttackTete;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapArea(groundCheckLeft.position, groundCheckRight.position);

        float horizontalMvt = moveInput.x * moveSpeed * Time.deltaTime;

        MovePlayer(horizontalMvt);

        float characterVelocity = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", characterVelocity);

        Flip(rb.linearVelocity.x);
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed && (isGrounded || doubleJump<1))
        {
            doubleJump ++;
            isJumping = true;
        }
        if(isGrounded){
            doubleJump = 0;
        }
    }

    private void OnAttackBite(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackBite");
        }
    }

    private void OnAttackTete(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackTete");
        }
    }

    // Méthode appelée par Animation Event à la fin de l'attaque
    public void OnAttackFinished()
    {
        animator.SetBool("IsAttacking", false);
    }

    void MovePlayer(float _horizontalMvt){
        Vector3 targetVelocity = new Vector2(_horizontalMvt, rb.linearVelocity.y);
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, .05f); //la on applique la vitesse au rb

        if(isJumping == true){
            rb.AddForce(new Vector2(0f, jumpForce));
            isJumping = false;
        }
    }

    void Flip(float _velocity){
        if(_velocity > 0.1f){
            spriteRenderer.flipX = false;
        }else if(_velocity < -0.1f){
            spriteRenderer.flipX = true;
        }
    }
}
