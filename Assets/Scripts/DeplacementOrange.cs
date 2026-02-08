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

    [Header("Combat (hitbox)")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Vector2 attackBoxSize = new Vector2(1.2f, 0.8f);
    [SerializeField] private LayerMask targetLayers = ~0;

    [Header("Damage")]
    [SerializeField] private int attackHit1Damage = 10;
    [SerializeField] private int attackHit2Damage = 25;
    [SerializeField] private bool oneHitPerAttack = true;

    [Header("AttackPoint auto facing (optional)")]
    [SerializeField] private bool autoFaceAttackPoint = true;
    [SerializeField] private float attackPointOffsetX = 0.6f;

    private bool hasHitThisAttack;

    [Header("Combat safety")]
    [Tooltip("Fail-safe: if the animation event OnAttackFinished isn't called, unlock movement after this many seconds. Set to 0 to disable.")]
    [SerializeField] private float attackLockTimeout = 0.9f;

    private float attackLockStartedAt = -999f;

    [Header("Facing")]
    [Tooltip("Minimum absolute input X to update facing direction.")]
    [SerializeField] private float facingDeadzone = 0.05f;
    private int facingSign = 1;

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
        // Fail-safe unlock in case the animation event doesn't fire.
        if (attackLockTimeout > 0f && animator != null && animator.GetBool("IsAttacking"))
        {
            if (Time.time >= attackLockStartedAt + attackLockTimeout)
            {
                Debug.LogWarning("[Orange] Attack lock timeout reached -> forcing IsAttacking = false. (Check OnAttackFinished animation event)");
                animator.SetBool("IsAttacking", false);
                hasHitThisAttack = false;
            }
        }

        isGrounded = Physics2D.OverlapArea(groundCheckLeft.position, groundCheckRight.position);

        float horizontalMvt = moveInput.x * moveSpeed * Time.deltaTime;

        MovePlayer(horizontalMvt);

        float characterVelocity = Mathf.Abs(rb.linearVelocity.x);
        animator.SetFloat("Speed", characterVelocity);

    Flip(moveInput.x);
        
        // Debug pour vérifier l'état de l'animation
        if(animator.GetBool("IsAttacking"))
        {
            AnimatorClipInfo[] clipInfo = animator.GetCurrentAnimatorClipInfo(0);
            if (clipInfo.Length > 0)
            {
                Debug.Log($"Clip en cours: {clipInfo[0].clip.name} | NormalizedTime: {animator.GetCurrentAnimatorStateInfo(0).normalizedTime}");
            }
        }
    }

    void LateUpdate()
    {
        if (!autoFaceAttackPoint || attackPoint == null || spriteRenderer == null)
            return;

        Vector3 lp = attackPoint.localPosition;
        float x = Mathf.Abs(attackPointOffsetX);
        lp.x = spriteRenderer.flipX ? -x : x;
        attackPoint.localPosition = lp;
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
            hasHitThisAttack = false;
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackBite");

            attackLockStartedAt = Time.time;
        }
    }

    private void OnAttackTete(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"AttackTete déclenchée! État actuel: {currentState.shortNameHash}");
            Debug.Log($"IsAttacking avant: {animator.GetBool("IsAttacking")}");

            hasHitThisAttack = false;
            
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("AttackTete");

            attackLockStartedAt = Time.time;
            
            // Vérifier si le trigger existe
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "AttackTete")
                {
                    Debug.Log($"Trigger 'AttackTete' trouvé, type: {param.type}");
                }
            }
        }
    }

    // Méthode appelée par Animation Event à la fin de l'attaque
    public void OnAttackFinished()
    {
        Debug.Log("OnAttackFinished Orange appelé!");
        animator.SetBool("IsAttacking", false);

        hasHitThisAttack = false;
    }

    // Animation Event (impact frame): weak
    public void AttackHit1()
    {
        DoAttackHit(attackHit1Damage);
    }

    // Animation Event (impact frame): strong
    public void AttackHit2()
    {
        DoAttackHit(attackHit2Damage);
    }

    // Compatibility: if your old animation event calls AttackHit()
    public void AttackHit()
    {
        AttackHit1();
    }

    private void DoAttackHit(int damage)
    {
        if (attackPoint == null)
        {
            Debug.LogWarning("[Orange] attackPoint is not set.");
            return;
        }

        if (oneHitPerAttack && hasHitThisAttack)
            return;

        Collider2D[] hits = Physics2D.OverlapBoxAll(attackPoint.position, attackBoxSize, 0f, targetLayers);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (hits[i].attachedRigidbody != null && hits[i].attachedRigidbody.gameObject == gameObject) continue;
            if (hits[i].gameObject == gameObject) continue;

            // Orange can only damage Bleu
            if (hits[i].TryGetComponent<HealthBleu>(out var healthBleu))
            {
                healthBleu.TakeDamage(damage);
                hasHitThisAttack = true;

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

        spriteRenderer.flipX = (facingSign < 0);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(attackPoint.position, attackBoxSize);
    }
}
