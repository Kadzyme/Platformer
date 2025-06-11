using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private Vector2 followOffset;
    [SerializeField] private Vector2 followThreshold = new(1f, 1f);
    [SerializeField] private float lookOffsetYMultiplier = 1f;
    [SerializeField] private float verticalLookSmoothSpeed = 3f;
    [SerializeField] private InputActionReference lookInputAction;

    private Transform target;

    private float targetOffsetY = 0f;
    private float currentOffsetY = 0f;

    private void OnEnable()
    {
        lookInputAction.action.Enable();
    }

    private void OnDisable()
    {
        lookInputAction.action.Disable();
    }

    private void LateUpdate()
    {
        target = Global.currentPlayer;

        if (target == null)
            return;

        Vector2 lookInput = lookInputAction.action.ReadValue<Vector2>();

        targetOffsetY = lookInput.y * lookOffsetYMultiplier;

        currentOffsetY = Mathf.Lerp(currentOffsetY, targetOffsetY, verticalLookSmoothSpeed * Time.deltaTime);

        float flipX = target.localScale.x > 0 ? -1 : 1;
        Vector2 dynamicOffset = followOffset;
        dynamicOffset.x *= flipX;
        dynamicOffset.y += currentOffsetY;

        Vector2 desiredPosition = (Vector2)target.position + dynamicOffset;
        Vector2 currentPosition = transform.position;
        Vector2 delta = desiredPosition - currentPosition;

        if (Mathf.Abs(delta.x) > followThreshold.x || Mathf.Abs(delta.y) > followThreshold.y)
        {
            Vector2 newPos = Vector2.Lerp(currentPosition, desiredPosition, followSpeed * Time.deltaTime);
            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }
}
