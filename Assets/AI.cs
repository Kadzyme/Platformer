using UnityEngine;
using System.Collections.Generic;

public enum AIState
{
    idle,
    chase
}

public class AI : MonoBehaviour
{
    [SerializeField] private float viewRange;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxChaseLength;
    private float currentChaseLength;

    [SerializeField] private GroundSensor wallSensor;
    [SerializeField] private GroundSensor floorSensor;

    [SerializeField] private Collider2D hitCollider;

    private AIState currentState = AIState.idle;
    private Rigidbody2D rb;
    private Animator animator;
    private float currentDelayToTurnAround;
    private Transform currentTarget;
    private bool isPositionFixed = false;

    public void TryKill()
    {
        if (IsTargetInAttackZone())
        {
            Destroy(currentTarget.gameObject);
            currentState = AIState.idle;
            animator.SetBool("isAttacking", false);
        }
    }

    public void FixPosition()
        => isPositionFixed = true;

    public void UnFixPosition()
        => isPositionFixed = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        switch (currentState)
        {
            case AIState.idle:
                Patrool();
                TryViewEnemy();
                break;
            case AIState.chase:
                Chase();
                break;
        }
        animator.SetFloat("currentSpeed", Mathf.Abs(rb.linearVelocity.x));

        if (isPositionFixed)
            rb.linearVelocity = Vector2.zero;
    }

    private void Patrool()
    {
        if (wallSensor.State() || !floorSensor.State())
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            currentDelayToTurnAround -= Time.deltaTime;

            if (currentDelayToTurnAround <= 0f)
                TurnAround();

            return;
        }

        TryMove(transform.localScale.x > 0);
    }

    private void Chase()
    {
        if (currentTarget == null)
        {
            currentState = AIState.idle;
            return;
        }

        if (TryViewEnemy())
        {
            currentChaseLength = 0;
        }
        else 
        {
            currentChaseLength += Time.deltaTime;

            if (currentChaseLength > maxChaseLength)
            {
                currentState = AIState.idle;
                currentTarget = null;
                currentChaseLength = 0;
                return;
            }
        }

        if (!IsTargetInAttackZone())
        {
            TryMove(transform.position.x < currentTarget.position.x);
            animator.SetBool("isAttacking", false);
        }
        else
            animator.SetBool("isAttacking", true);
    }

    private bool IsTargetInAttackZone()
    {
        ContactFilter2D filter = new();
        filter.SetLayerMask(Global.unitsLayer);
        filter.useTriggers = true;

        List<Collider2D> hittedColliders = new();
        try
        {
            hitCollider.Overlap(filter, hittedColliders);
        }
        catch 
        {
            Debug.Log("Как же я заебался с этими нуллрефами...");
        }

        if (hittedColliders.Count == 0)
        {
            return false;
        }

        foreach (Collider2D col in hittedColliders)
        {
            if (col.CompareTag("Player"))
                return true;
        }

        return false;
    }

    private void TryMove(bool isRightDirection)
    {
        float moveDir = isRightDirection ? 1f : -1f;

        if (Mathf.Sign(moveDir) != Mathf.Sign(transform.localScale.x))
            TurnAround();

        if (wallSensor.State() || !floorSensor.State())
            return;

        rb.linearVelocity = new Vector2(moveDir * moveSpeed, rb.linearVelocity.y);
    }

    private void TurnAround()
    {
        if (isPositionFixed)
            return;

        transform.localScale = new Vector2(-transform.localScale.x, transform.localScale.y);

        currentDelayToTurnAround = 3f;
    }

    private bool TryViewEnemy()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        RaycastHit2D[] hits = 
            Physics2D.RaycastAll(transform.position, direction, viewRange, Global.unitsLayer);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.CompareTag("Player"))
            {
                currentState = AIState.chase;
                currentTarget = hit.collider.transform;
                return true;
            }
        }
        return false;
    }

    private void OnDrawGizmos()
    {
        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Vector2 origin = transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + direction * viewRange);
    }
}
