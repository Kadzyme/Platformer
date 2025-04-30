using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GroundSensor groundSensor;
    [SerializeField] private GroundSensor topSlideSensor;
    [SerializeField] private GroundSensor bottomSlideSensor;
    [SerializeField] private GroundSensor rightWallSensor;

    [SerializeField] private GroundSensor climbingSensor;
    [SerializeField] private GroundSensor notClimbingSensor;

    [SerializeField] private GameObject dashParticles;
    private Particles currentDashParticles;

    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool isNormalXPositive = true;

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

        if (isClimbing)
        {
            rb.linearVelocity = Vector2.zero;

            if (isDashing)
            {
                isDashing = false;
                currentDashParticles.Stop();
                animator.SetBool("isDashing", isDashing);
            }

            return;
        }

        TryClimb();

        if (isDashing)
        {
            currentDashingTime -= Time.deltaTime;

            if (currentDashingTime > 0)
                SetDashVelocity();
            else
            {
                isDashing = false;
                currentDashParticles.Stop();
            }

            animator.SetBool("isDashing", isDashing);

            return;
        }

        TryDash();

        TryPush();

        TrySlide();

        if (isSliding)
            canUseDash = true;

        if (Input.GetKeyDown(KeyCode.Space))
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

        if (!Input.GetKeyDown(KeyCode.E) || currentDashCooldown > 0 || !canUseDash)
            return;

        Vector2 dashDirection = Vector2.right;

        if (Input.GetAxisRaw("Horizontal") != 0)
            dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), 0).normalized;
        else
            dashDirection *= Mathf.Sign(normalX) * Mathf.Sign(transform.localScale.x) * (-1);

        const float dashCooldown = 0.5f;
        currentDashCooldown = dashCooldown;

        currentDashingTime = dashTime;

        isDashing = true;

        canUseDash = false;

        currentDashDirection = dashDirection;

        var newObj = Instantiate(dashParticles, transform);
        currentDashParticles = newObj.GetComponent<Particles>();
        Destroy(newObj, 5f);
    }

    private void SetDashVelocity()
        => rb.linearVelocity = currentDashDirection * dashForce;

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

        animator.SetBool("isPushing", true);
    }

    private void TrySlide()
    {
        if (isGrounded)
        {
            isSliding = false;
            animator.SetBool("isSliding", isSliding);
            return;
        }

        isSliding = topSlideSensor.State() && bottomSlideSensor.State() && rightWallSensor.State();

        if (isSliding)
        {
            rb.linearVelocityY = -0.1f;
        }

        animator.SetBool("isSliding", isSliding);
    }

    private IEnumerator ResetClimbFlag()
    {
        Vector3 currentClimbingSensorPos = notClimbingSensor.transform.position;
        RaycastHit2D newPos = Physics2D.Raycast(currentClimbingSensorPos, Vector2.down, 2f, Global.groundLayer);

        float characterHeight = GetComponent<Collider2D>().bounds.size.y / 2;
        Vector3 correctedPosition = newPos.point + new Vector2(0, characterHeight);

        StartCoroutine(ClimbLerp(correctedPosition, 0.3f));

        yield return new WaitForSeconds(0.6f);

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
        float inputX = Input.GetAxis("Horizontal");

        if (!isGrounded && inputX != 0 && (topSlideSensor.State() && bottomSlideSensor.State() || rightWallSensor.State()))
        {
            return;
        }

        if (inputX != 0)
        {
            Vector3 newLocalScale = transform.localScale;

            if (inputX > 0)
                newLocalScale.x = -normalX;
            else
                newLocalScale.x = normalX;

            transform.localScale = newLocalScale;

            animator.SetFloat("currentSpeed", moveSpeed);
        }
        else
            animator.SetFloat("currentSpeed", 0);

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
