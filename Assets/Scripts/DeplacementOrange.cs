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

    [Header("Attack")]
    [Tooltip("Point devant l'orange d'où part la zone de hit (child Transform conseillé).")]
    [SerializeField] private Transform attackPoint;

    [Tooltip("Distance de l'attackPoint par rapport au pivot du joueur (en local X). Utilisé si attackPoint est un child.")]
    [SerializeField] private float attackPointOffsetX = 0.8f;

    [Tooltip("Taille de la hitbox (unités monde).")]
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);

    [Tooltip("Dégâts de l'attaque 1 (faible) vers le Bleu.")]
    [SerializeField] private int damageHit1ToBleu = 1;

    [Tooltip("Dégâts de l'attaque 2 (forte) vers le Bleu.")]
    [SerializeField] private int damageHit2ToBleu = 2;

    [Tooltip("Empêche plusieurs hits dans la même animation.")]
    [SerializeField] private bool oneHitPerAttack = true;

    private bool hasHitThisAttack;

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
            hasHitThisAttack = false;
        }
    }

    private void OnAttackTete(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackTete");
            hasHitThisAttack = false;
        }
    }

    public void AttackHit() => AttackHit1();

    public void AttackHit1()
    {
        DoAttackHit(damageHit1ToBleu);
    }

    public void AttackHit2()
    {
        DoAttackHit(damageHit2ToBleu);
    }

    private void DoAttackHit(int damage)
    {
        if (oneHitPerAttack && hasHitThisAttack)
            return;

        if (attackPoint == null)
        {
            Debug.LogWarning("[Deplacement/Orange] attackPoint n'est pas assigné.");
            return;
        }

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, 0f);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D c = hits[i];
            if (c == null) continue;
            if (c.gameObject == gameObject) continue;

            if (c.TryGetComponent<HealthBleu>(out var healthBleu))
            {
                healthBleu.TakeDamage(damage);
                hasHitThisAttack = true;
            }
        }
    }

    // Méthode appelée par Animation Event à la fin de l'attaque
    public void OnAttackFinished()
    {
        animator.SetBool("IsAttacking", false);
        hasHitThisAttack = false;
    }

    private void LateUpdate()
    {
        if (attackPoint != null)
        {
            float dir = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;
            Vector3 lp = attackPoint.localPosition;
            lp.x = Mathf.Abs(attackPointOffsetX) * dir;
            attackPoint.localPosition = lp;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
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
