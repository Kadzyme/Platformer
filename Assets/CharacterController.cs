using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController : MonoBehaviour
{
    [SerializeField] private GroundSensor groundSensor;
    [SerializeField] private GroundSensor rightSensor;

    [SerializeField] private GroundSensor climbingSensor;
    [SerializeField] private GroundSensor notClimbingSensor;

    [SerializeField] private float jumpForce;
    [SerializeField] private float moveSpeed;
    [SerializeField] private bool isNormalXPositive = true;
    private float normalX;
    private bool isGrounded;
    private bool isClimbing;

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

        if (isClimbing)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        TryClimb();

        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        
        TryWalk();
    }

    private void CheckIsGrounded()
    {
        isGrounded = groundSensor.State();

        animator.SetBool("isGrounded", isGrounded);
    }

    private void TryClimb()
    {
        if (notClimbingSensor.State() || !climbingSensor.State())
            return;

        notClimbingSensor.Disable(0.2f);
        climbingSensor.Disable(0.2f);

        isClimbing = true;

        animator.SetTrigger("Climb");
        animator.SetBool("isClimbing", true);

        StartCoroutine(ResetClimbFlag());

    }

    private IEnumerator ResetClimbFlag()
    {
        Vector3 currentClimbingSensorPos = notClimbingSensor.transform.position;
        RaycastHit2D newPos = Physics2D.Raycast(currentClimbingSensorPos, Vector2.down, 2f, Global.groundLayer);

        float characterHeight = GetComponent<Collider2D>().bounds.extents.y;
        Vector3 correctedPosition = newPos.point - new Vector2(0, characterHeight);

        StartCoroutine(ClimbLerp(correctedPosition, 0.3f));

        yield return new WaitForSeconds(0.6f);

        Climb();
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

    private void Climb()
    {
        isClimbing = false;
        animator.SetBool("isClimbing", false);
    }

    private void TryWalk()
    {
        float inputX = Input.GetAxis("Horizontal");

        if (!isGrounded && inputX != 0 && rightSensor.State())
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
        if (!isGrounded)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}
