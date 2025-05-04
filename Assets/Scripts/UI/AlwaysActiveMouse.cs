using UnityEngine;

public class AlwaysActiveMouse : MonoBehaviour
{
    private void FixedUpdate()
    {
        Cursor.visible = true;
    }
}
