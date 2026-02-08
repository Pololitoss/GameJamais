using UnityEngine;
using UnityEngine.InputSystem;

public class Deplacement : MonoBehaviour
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

    [Header("Combat (hitbox)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private LayerMask targetLayers = ~0; // by default, hit everything (filtered by component type)

    [Header("Damage")]
    [SerializeField] private int attackHit1Damage = 10;
    [SerializeField] private int attackHit2Damage = 25;
    [SerializeField] private bool oneHitPerAttack = true;

    [Header("AttackPoint auto facing (optional)")]
    [SerializeField] private bool autoFaceAttackPoint = true;
    [SerializeField] private float attackPointOffsetX = 0.6f;

    private bool hasHitThisAttack;
    private bool hasHitThisAttack2;

    [Header("Health")]
    [SerializeField] private HealthBleu health;

    [Header("Facing")]
    [Tooltip("Minimum absolute input X to update facing direction.")]
    [SerializeField] private float facingDeadzone = 0.05f;
    private int facingSign = 1; // 1 => right, -1 => left

    private void Awake()
    {
        if (health == null)
            health = GetComponent<HealthBleu>();
    }

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
            
            var attackBiteAction = playerInput.actions["AttackBite"];
            attackBiteAction.performed += OnAttackBite;
            
            var attackTeteAction = playerInput.actions["AttackTete"];
            attackTeteAction.performed += OnAttackTete;
        }
    }

    void OnDisable()
    {
        var playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            var moveAction = playerInput.actions["Move"];
            moveAction.performed -= OnMove;
            moveAction.canceled -= OnMove;
            
            var jumpAction = playerInput.actions["Jump"];
            jumpAction.performed -= OnJump;
            
            var attackBiteAction = playerInput.actions["AttackBite"];
            attackBiteAction.performed -= OnAttackBite;
            
            var attackTeteAction = playerInput.actions["AttackTete"];
            attackTeteAction.performed -= OnAttackTete;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (health != null && health.IsDead)
        {
            // Stop horizontal movement when dead.
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            animator.SetFloat("Speed", 0f);
            return;
        }

        isGrounded = Physics2D.OverlapArea(groundCheckLeft.position, groundCheckRight.position);

        float horizontalMvt = moveInput.x * moveSpeed * Time.deltaTime;

        MovePlayer(horizontalMvt);

        float characterVelocity = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", characterVelocity);

        Flip(moveInput.x);
    }

    void LateUpdate()
    {
        if (!autoFaceAttackPoint || attackPoint == null || spriteRenderer == null)
            return;

        // Place attackPoint in front of the sprite, depending on facing.
        Vector3 lp = attackPoint.localPosition;
        float x = Mathf.Abs(attackPointOffsetX);
        lp.x = spriteRenderer.flipX ? -x : x;
        attackPoint.localPosition = lp;
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        if (health != null && health.IsDead)
        {
            moveInput = Vector2.zero;
            return;
        }
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (health != null && health.IsDead)
            return;
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
        if (health != null && health.IsDead)
            return;
        if (context.performed)
        {
            // Important when attacks are chained/spammed: re-arm hit for the new attack window.
            hasHitThisAttack = false;
            hasHitThisAttack2 = false;
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackBite");
        }
    }

    private void OnAttackTete(InputAction.CallbackContext context)
    {
        if (health != null && health.IsDead)
            return;
        if (context.performed)
        {
            Debug.Log("AttackTete Bleu déclenchée!");

            // Important when attacks are chained/spammed: re-arm hit for the new attack window.
            hasHitThisAttack = false;
            hasHitThisAttack2 = false;
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackTete");
        }
    }

    // Méthode appelée par Animation Event à la fin de l'attaque
    public void OnAttackFinished()
    {
        Debug.Log("OnAttackFinished Bleu appelé!");
        animator.SetBool("IsAttacking", false);

        hasHitThisAttack = false;
        hasHitThisAttack2 = false;
    }

    // Animation Event (impact frame): weak
    public void AttackHit1()
    {
        DoAttackHit(attackHit1Damage, 1);
    }

    // Animation Event (impact frame): strong
    public void AttackHit2()
    {
        DoAttackHit(attackHit2Damage, 2);
    }

    // Compatibility: if your old animation event calls AttackHit()
    public void AttackHit()
    {
        AttackHit1();
    }

    private void DoAttackHit(int damage, int hitIndex)
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("[Bleu] attackPoint is not set.");
            return;
        }

        if (oneHitPerAttack)
        {
            if (hitIndex == 1 && hasHitThisAttack) return;
            if (hitIndex == 2 && hasHitThisAttack2) return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, 0f, targetLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (hits[i].attachedRigidbody != null && hits[i].attachedRigidbody.gameObject == gameObject) continue;
            if (hits[i].gameObject == gameObject) continue;

            // Bleu can only damage Orange
            if (hits[i].TryGetComponent<HealthOrange>(out var healthOrange))
            {
                healthOrange.TakeDamage(damage);
                if (hitIndex == 1) hasHitThisAttack = true;
                else if (hitIndex == 2) hasHitThisAttack2 = true;

                if (oneHitPerAttack)
                    break;
            }
        }
    }

    void MovePlayer(float _horizontalMvt){
        // Ne pas bouger pendant l'attaque
        if(animator.GetBool("IsAttacking"))
        {
            _horizontalMvt = 0;
        }
        
        Vector3 targetVelocity = new Vector2(_horizontalMvt, rb.linearVelocity.y);
        rb.linearVelocity = Vector3.SmoothDamp(rb.linearVelocity, targetVelocity, ref velocity, .05f); //la on applique la vitesse au rb

        if(isJumping == true){
            rb.AddForce(new Vector2(0f, jumpForce));
            isJumping = false;
        }
    }

    void Flip(float inputX){
        if (spriteRenderer == null) return;

        if (inputX > facingDeadzone)
            facingSign = 1;
        else if (inputX < -facingDeadzone)
            facingSign = -1;

        // In this project: flipX=true means facing left.
        spriteRenderer.flipX = (facingSign < 0);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
}
