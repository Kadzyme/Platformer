using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Global : MonoBehaviour
{
    public static UnityEvent OnReplaceEvent = new();
    public static UnityEvent OnGameContinue = new();
    public static List<GameObject> objectsToRevive = new();

    public static InputActionReference interactAction;
    [SerializeField] private InputActionReference startInteractAction;

    private static Vector2 checkpointPos = Vector2.zero;
    [SerializeField] private Transform startCheckpointPos;

    public static int groundLayer;
    [SerializeField] private int groundLayerNum;

    public static int unitsLayer;
    [SerializeField] private int unitsLayerNum;

    public static Transform currentPlayer;

    private static Transform playerPrefab;
    [SerializeField] private Transform startPlayerPrefab;

    [SerializeField] private Menu menu;

    void Awake()
    {
        Cursor.visible = false;

        groundLayer = 1 << groundLayerNum;
        unitsLayer = 1 << unitsLayerNum;

        playerPrefab = startPlayerPrefab;

        interactAction = startInteractAction;
        interactAction.action.Enable();

        CreateNewHero();

        SetNewCheckPointPos(startCheckpointPos.position);

        OnReplaceEvent.AddListener(MovePlayerToCheckPoint);
    }

    public static void SetNewCheckPointPos(Vector2 newPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(newPos, Vector2.down, 10f, groundLayer);

        Vector2 correctedPos = new(hit.point.x, hit.point.y + currentPlayer.GetComponent<Collider2D>().bounds.size.y / 2);

        checkpointPos = hit ? correctedPos : newPos;
    }

    private void MovePlayerToCheckPoint()
       => currentPlayer.position = checkpointPos;

    private void FixedUpdate()
    {
        if (currentPlayer == null)
            CreateNewHero();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (menu.isActive)
                menu.ContinueGame();
            else
                menu.StopGame();
        }
    }


    public static void CreateNewHero()
    {
        currentPlayer = Instantiate(playerPrefab);
        ReviveObjects();
        OnReplaceEvent.Invoke();
    }

    public static void ReviveObjects()
    {
        foreach (GameObject objectToRevive in objectsToRevive)
        {
            objectToRevive?.SetActive(true);
        }
    }
}
