using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference dashAction;
    [SerializeField] private InputActionReference jumpAction;

    [Header("Sensors")]
    [SerializeField] private GroundSensor groundSensor;
    [SerializeField] private GroundSensor topSlideSensor;
    [SerializeField] private GroundSensor bottomSlideSensor;
    [SerializeField] private GroundSensor rightWallSensor;
    [SerializeField] private GroundSensor climbingSensor;
    [SerializeField] private GroundSensor notClimbingSensor;

    [Header("Effects")]
    [SerializeField] private GameObject dashParticles;
    private Particles currentDashParticles;

    [Header("Movement Settings")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool isNormalXPositive = true;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 40;
    [SerializeField] private float dashTime = 0.07f;

    private float currentDashCooldown = 0.5f;
    private float currentDashingTime = 0f;
    private bool isDashing = false;
    private Vector2 currentDashDirection = Vector2.zero;
    private bool canUseDash = true;

    private float normalX;
    private bool isGrounded;
    private bool isClimbing;
    private bool isSliding;

    private Rigidbody2D rb;
    private Animator animator;

    private void Start()
    {
        GetComponents();
        SetStartAmountsForVariables();

        moveAction.action.Enable();
        dashAction.action.Enable();
        jumpAction.action.Enable();
    }

    private void GetComponents()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void SetStartAmountsForVariables()
    {
        normalX = transform.localScale.x;
        if (!isNormalXPositive)
            normalX *= -1;
    }

    private void Update()
    {
        CheckIsGrounded();

        if (isGrounded)
            canUseDash = true;

        if (isDashing)
        {
            currentDashingTime -= Time.deltaTime;
            if (currentDashingTime > 0)
                SetDashVelocity();
            else
                StopDashIfIsDashing();

            animator.SetBool("isDashing", isDashing);
            return;
        }

        TryClimb();

        if (isClimbing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        TryPush();
        TryDash();
        TrySlide();

        if (isSliding)
            canUseDash = true;

        if (jumpAction.action.WasPressedThisFrame())
            TryJump();

        TryWalk();
    }

    private void CheckIsGrounded()
    {
        isGrounded = groundSensor.State();
        animator.SetBool("isGrounded", isGrounded);
    }

    private void TryDash()
    {
        currentDashCooldown -= Time.deltaTime;

        if (dashAction.action.triggered && currentDashCooldown <= 0 && canUseDash)
        {
            Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
            Vector2 dashDirection = moveInput.x != 0 ? new Vector2(moveInput.x, 0).normalized : Vector2.right * Mathf.Sign(normalX) * Mathf.Sign(transform.localScale.x) * (-1);

            if (isSliding)
                dashDirection.x *= -1;

            currentDashCooldown = 0.5f;
            currentDashingTime = dashTime;
            isDashing = true;
            canUseDash = false;
            currentDashDirection = dashDirection;

            var newObj = Instantiate(dashParticles, transform);
            currentDashParticles = newObj.GetComponent<Particles>();
            Destroy(newObj, 5f);
        }
    }

    private void SetDashVelocity()
        => rb.linearVelocity = currentDashDirection * dashForce;

    private void StopDashIfIsDashing()
    {
        if (isDashing)
        {
            isDashing = false;
            currentDashParticles?.Stop();
            animator.SetBool("isDashing", isDashing);
        }
    }

    private void TryClimb()
    {
        if (notClimbingSensor.State() || !climbingSensor.State())
            return;

        notClimbingSensor.Disable(0.2f);
        climbingSensor.Disable(0.2f);

        isClimbing = true;
        animator.SetBool("isClimbing", true);
        StartCoroutine(ResetClimbFlag());
    }

    private void TryPush()
    {
        if (!isGrounded || !(topSlideSensor.State() && bottomSlideSensor.State()))
        {
            animator.SetBool("isPushing", false);
            return;
        }

        canUseDash = false;
        animator.SetBool("isPushing", true);
    }

    private void TrySlide()
    {
        if (isGrounded)
        {
            isSliding = false;
            animator.SetBool("isSliding", false);
            return;
        }

        isSliding = topSlideSensor.State() && bottomSlideSensor.State() && rightWallSensor.State();
        if (isSliding)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -0.1f);

        animator.SetBool("isSliding", isSliding);
    }

    private IEnumerator ResetClimbFlag()
    {
        Vector3 currentClimbingSensorPos = notClimbingSensor.transform.position;
        RaycastHit2D newPos = Physics2D.Raycast(currentClimbingSensorPos, Vector2.down, 2f, Global.groundLayer);

        float characterHeight = GetComponent<Collider2D>().bounds.size.y / 2;
        Vector3 correctedPosition = newPos.point + new Vector2(0, characterHeight);

        yield return ClimbLerp(correctedPosition, 0.3f);
        yield return new WaitForSeconds(0.3f);

        isClimbing = false;
        animator.SetBool("isClimbing", false);
    }

    private IEnumerator ClimbLerp(Vector3 targetPosition, float duration)
    {
        float time = 0;
        Vector3 startPosition = transform.position;

        while (time < duration)
        {
            float t = Mathf.SmoothStep(0, 1, time / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    private void TryWalk()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        float inputX = moveInput.x;

        if (!isGrounded && inputX != 0 && (topSlideSensor.State() && bottomSlideSensor.State() || rightWallSensor.State()))
        {
            return;
        }

        if (inputX != 0)
        {
            Vector3 newLocalScale = transform.localScale;
            newLocalScale.x = inputX > 0 ? -normalX : normalX;
            transform.localScale = newLocalScale;

            animator.SetFloat("currentSpeed", moveSpeed);
        }
        else
        {
            animator.SetFloat("currentSpeed", 0);
        }

        rb.linearVelocity = new Vector2(inputX * moveSpeed, rb.linearVelocity.y);
    }

    private void TryJump()
    {
        if (!isGrounded && !isSliding)
            return;

        if (isSliding)
        {
            float jumpDirection = Mathf.Sign(-transform.localScale.x);
            rb.linearVelocity = new Vector2(jumpDirection * jumpForce, jumpForce);

            topSlideSensor.Disable(0.2f);
            bottomSlideSensor.Disable(0.2f);
            climbingSensor.Disable(0.2f);

            isSliding = false;
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}
