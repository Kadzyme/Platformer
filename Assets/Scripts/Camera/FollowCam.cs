using UnityEngine;
using UnityEngine.InputSystem;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private float maxDistanceX;
    [SerializeField] private float maxDistanceY;
    [SerializeField] private float additionalY;
    [SerializeField] private float additionalX;
    [SerializeField] private float changingY;
    [SerializeField] private float changingX;

    [SerializeField] private InputActionReference lookInputAction;

    private float currentTime;
    private bool isOldPlayerXPositive;

    private void OnEnable()
    {
        lookInputAction.action.Enable();
    }

    private void OnDisable()
    {
        lookInputAction.action.Disable();
    }

    private void Update()
    {
        if (Global.currentPlayer == null)
            return;

        float currentAdditionalY = additionalY;

        Vector2 input = lookInputAction.action.ReadValue<Vector2>();
        currentAdditionalY += input.y * changingY;

        Vector2 currentPos = transform.position;
        Vector2 playerCurrentPos = Global.currentPlayer.position;

        float currentAdditionalX = additionalX;
        Transform player = Global.currentPlayer;

        bool isCurrentPlayerXPositive = Global.currentPlayer.localScale.x > 0;

        if (isOldPlayerXPositive == isCurrentPlayerXPositive)
        {
            currentTime += Time.deltaTime;

            if (currentTime > 3f)
                currentAdditionalX += changingX;
        }
        else
        {
            isOldPlayerXPositive = isCurrentPlayerXPositive;
            currentTime = 0f;
        }

        if (player.localScale.x > 0)
            currentAdditionalX *= -1;

        playerCurrentPos.x += currentAdditionalX;
        playerCurrentPos.y += currentAdditionalY;

        if (Mathf.Abs(currentPos.x - playerCurrentPos.x) > maxDistanceX
            || Mathf.Abs(currentPos.y - playerCurrentPos.y) > maxDistanceY)
        {
            Vector2 temp =
                Vector2.Lerp(transform.position, playerCurrentPos, Time.deltaTime * Vector2.Distance(currentPos, playerCurrentPos));
            transform.position = new Vector3(temp.x, temp.y, transform.position.z);
        }
    }
}
