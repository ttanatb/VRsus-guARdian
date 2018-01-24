using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.XR.iOS;

/// <summary>
/// This for the AR player to use to keep track of the traps
/// </summary>
[System.Serializable]
public class TrapCounter
{
    public GameObject trap;     //the trap itself
    public int count;           //how mnay traps can till be placed
    public int maxCount;        //the maximum amount of traps
}

/// <summary>
/// This class takes care of the game phase, setting up the world,
/// and setting up traps for AR
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class ARSetUp : PlayerComponent
{
    #region Fields
    [Tooltip("The prefab of the AR plane generator")]
    public GameObject planeGeneratorPrefab;

    [Tooltip("The list of traps to be placed")]
    public TrapCounter[] trapList;

    [Tooltip("Prefab for the relic")]
    public GameObject relicPrefab;

    [Tooltip("Prefab for the entrance")]
    public GameObject entrancePrefab;

    [Tooltip("Prefab for the wall")]
    public GameObject wallPrefab; //currently not used

    [Tooltip("The minimum area to count as a play area")]
    public float minPlayArea = 1f;

    private GameObject planeGeneratorObj;       //reference to the AR plane generator

    private int currTrapSelection = -1;                 //index of the trap that is chosen
    private TrapDefense previouslySelectedTrap = null;
    private TrapDefense currentlySelectedTrap = null;

    [SyncVar]
    private int currGamePhase = 0;

    /// <summary>
    /// Gets the current phase of the game
    /// </summary>
    public GamePhase CurrGamePhase { get { return (GamePhase)currGamePhase; } }
    #endregion

    #region Init Logic
    public override void OnStartLocalPlayer()
    {
        if (isServer)
        {
            InitObj();
            CanvasManager.Instance.SetUpUI(this);
        }
    }

    public void Awake()
    {
        //psuedo-singleton (fix later)
        if (XInput.Instance == null)
            new XInput();
    }

    public void Start()
    {
        foreach (TrapCounter t in trapList)
            t.maxCount = t.count;
    }

    protected override void InitObj()
    {
        if (!planeGeneratorObj)
            planeGeneratorObj = Instantiate(planeGeneratorPrefab);
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !isLocalPlayer)
            return;

        switch (currGamePhase)
        {
            case 1: //placing phase

                //doesn't do anything if pointer is on UI obj
                if (Utility.IsPointerOverUIObject()) return;

                //if something is selected
                if (currTrapSelection != -1)
                {
                    //i think this should be commented out 
                    if (currentlySelectedTrap != null)
                    {
                        currentlySelectedTrap = null;
                        TogglePreviouslySelectedTrap();
                    }

                    //check for a tap on the plane to place trap
                    CheckTapOnARPlane();
                }

                //if no trap is currently selected
                else
                {
                    CheckTapOnSecurityScreen();
                    CheckTapOnTraps();
                    MoveTrap();
                }
                break;

            case 2: //playing phase
                CheckTapOnSecurityScreen();
                break;

            default:
                break;
        }

        //if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
        //    CmdSpawnRelics();
    }

    /// <summary>
    /// Checks if a tap hits a trap
    /// 
    /// To-do: Extend an input class to do this
    /// </summary>
    void CheckTapOnTraps()
    {
        LayerMask layer = LayerMask.NameToLayer("Trap");
        InputResult resultInfo = XInput.Instance.CheckTap(1 << layer, TouchPhase.Began);
        if (resultInfo.result == ResultType.Success)
        {
            currentlySelectedTrap = resultInfo.hit.transform.GetComponent<TrapDefense>();
            currentlySelectedTrap.ToggleSelected();
            TogglePreviouslySelectedTrap();
        }
        else if (resultInfo.result == ResultType.MissTap)
        {
            currentlySelectedTrap = null;
            TogglePreviouslySelectedTrap();
        }
    }

    /// <summary>
    /// Handles moving traps around
    /// 
    /// To-do: extend the input class
    /// </summary>
    void MoveTrap()
    {
        if (currentlySelectedTrap == null) return;

        LayerMask layer = LayerMask.NameToLayer("Tower");
        LayerMask layer2 = LayerMask.NameToLayer("Trap");
        InputResult resultInfo = XInput.Instance.CheckTap((1 << layer), TouchPhase.Moved, TouchPhase.Stationary);
        if (resultInfo.result == ResultType.Success)
        {
            currentlySelectedTrap.transform.position = resultInfo.hit.point + Vector3.up * currentlySelectedTrap.transform.localScale.y / 2;
        }
        else if (resultInfo.result == ResultType.NoTap)
        {
        }
    }

    /// <summary>
    /// Check if there was a tap on the security screen
    /// 
    /// Extend input class
    /// </summary>
    void CheckTapOnSecurityScreen()
    {
        LayerMask layer = LayerMask.NameToLayer("UI");
        InputResult resultInfo = XInput.Instance.CheckTap(1 << layer, TouchPhase.Began);
        if (resultInfo.result == ResultType.Success)
        {
            currentlySelectedTrap = resultInfo.hit.collider.GetComponent<SecurityScreen>().associatedCamera;
            currentlySelectedTrap.ToggleSelected();
            TogglePreviouslySelectedTrap();
        }
        else if (resultInfo.result == ResultType.MissTap)
        {
            currentlySelectedTrap = null;
            TogglePreviouslySelectedTrap();
        }
    }

    /// <summary>
    /// Toggles the trap that was previousl selected
    /// </summary>
    void TogglePreviouslySelectedTrap()
    {
        if (previouslySelectedTrap)
            previouslySelectedTrap.ToggleSelected();

        previouslySelectedTrap = currentlySelectedTrap;
    }

    /// <summary>
    /// Check if there was a tap on the plane
    /// 
    /// To-do: extend input
    /// </summary>
    /// <returns></returns>
    private void CheckTapOnARPlane()
    {
        LayerMask layer = LayerMask.NameToLayer("Tower");
        InputResult resultInfo = XInput.Instance.CheckTap(1 << layer, TouchPhase.Began);
        if (resultInfo.result == ResultType.Success)
            PlaceTrap(resultInfo.hit.point);
    }

    /// <summary>
    /// Places a trap and updates selection & UI
    /// </summary>
    private void PlaceTrap(Vector3 position)
    {
        trapList[currTrapSelection].count -= 1;

        TogglePreviouslySelectedTrap();
        CmdSpawnTrap(currTrapSelection, position);

        CanvasManager.Instance.ClearSelection(this);
        CanvasManager.Instance.UpdateTrapCount(this);

        currTrapSelection = -1;
    }

    /// <summary>
    /// Spawns a trap on the network
    /// </summary>
    /// <param name="index"></param>
    /// <param name="pos"></param>
    [Command]
    private void CmdSpawnTrap(int index, Vector3 pos)
    {
        GameObject go = Instantiate(trapList[index].trap, pos, Quaternion.identity);
        currentlySelectedTrap = go.GetComponent<TrapDefense>();
        currentlySelectedTrap.ToggleSelected();
        //TogglePreviouslySelectedTrap();
        NetworkServer.Spawn(go);
    }

    /// <summary>
    /// Returns how larg the area currently is
    /// 
    /// To-do: This function should be done elsewhere
    /// </summary>
    /// <returns></returns>
    public bool CheckAggregrateArea()
    {
        return true;
        float area = 0;
        if (UnityARAnchorManager.Instance == null)
        {
            foreach (LocalPlane obj in FindObjectsOfType<LocalPlane>())
            {
                float planeArea = obj.transform.localScale.x * obj.transform.localScale.z;
                area += planeArea;
            }
        }
        else
        {
            if (UnityARAnchorManager.Instance.planeAnchorMap.Count < 0)
                return false;

            foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                float planeArea = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.x *
                    UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.z;

                area += planeArea;
            }
        }

        area *= 100f;

        //Debug.Log("Total plane area: " + area);

        if (area > minPlayArea)
            return true;
        else return false;
    }

    /// <summary>
    /// Resets the game after the game is 'done'
    /// </summary>
    public void ResetGame()
    {
        if (!isServer) return;

        currGamePhase = (int)GamePhase.Placing;

        foreach (TrapDefense trap in FindObjectsOfType<TrapDefense>())
            NetworkServer.Destroy(trap.gameObject);

        foreach (Relic relic in FindObjectsOfType<Relic>())
            NetworkServer.Destroy(relic.gameObject);

        foreach (Entrance entrance in FindObjectsOfType<Entrance>())
            NetworkServer.Destroy(entrance.gameObject);

        foreach (TrapCounter t in trapList)
        {
            t.count = t.maxCount;
        }

        FindObjectOfType<VRCombat>().RpcRespawn();

        SetPhaseTo(GamePhase.Placing);
        CanvasManager.Instance.SetUpUI(this);
        UnityARAnchorManager.Instance.TogglePlaneMaterial();
    }

    /// <summary>
    /// Changes the phase
    /// </summary>
    /// <param name="newPhase"></param>
    public void SetPhaseTo(GamePhase newPhase)
    {

        switch (newPhase)
        {
            case GamePhase.Placing:
#if UNITY_IOS
			UnityARCameraManager.Instance.StopTracking ();
#endif
                CmdSpawnRelics();
                break;

            case GamePhase.Playing:
                RpcSpawnEntrances();

                VRTransition blockManager = FindObjectOfType<VRTransition>();
                if (blockManager)
                    blockManager.RpcSwitchToTopViewCam();

                ARCombat combat = GetComponent<ARCombat>();
                if (combat != null)
                    combat.CanShoot = true;

#if UNITY_IOS
                UnityARAnchorManager.Instance.TogglePlaneMaterial();
#endif
                break;
            case GamePhase.Over:
                if (!isServer) break;
                ARCombat combatt = GetComponent<ARCombat>();
                if (combatt != null)
                    combatt.CanShoot = false;
                CanvasManager.Instance.ShowGameOverBtn(this);
                break;
        }

        currGamePhase = (int)newPhase;
        CanvasManager.Instance.SetUpUI(this);
    }

    public void SetCurrTrapSelection(int toSelect)
    {
        currTrapSelection = toSelect;
    }

    [Command]
    private void CmdSpawnRelic(GameObject plane, GameObject[] walls)
    {
        GameObject obj = Instantiate(relicPrefab,
            new Vector3(Random.Range(plane.transform.position.x - plane.transform.localScale.x / 4, plane.transform.position.x + plane.transform.localScale.x / 4),
            plane.transform.localScale.y / 2 + plane.transform.position.y,
            Random.Range(plane.transform.position.z - plane.transform.localScale.z / 4, plane.transform.position.z + plane.transform.localScale.z / 4)),
            Quaternion.identity);
        //obj.GetComponent<Relic>().Init(walls, plane);
        NetworkServer.Spawn(obj);
    }

    [Command]
    private void CmdSpawnWall(GameObject plane)
    {
        GameObject wall = Instantiate(wallPrefab,
            new Vector3(Random.Range(plane.transform.position.x - plane.transform.localScale.x / 3, plane.transform.position.x + plane.transform.localScale.x / 3),
            plane.transform.position.y + plane.transform.localScale.y / 2 - wallPrefab.transform.localScale.y / 1.9f,
            Random.Range(plane.transform.position.z - plane.transform.localScale.z / 3, plane.transform.position.z + plane.transform.localScale.z / 3)),
            Quaternion.Euler(0, Random.Range(0f, 360f), 0f));
        NetworkServer.Spawn(wall);
        walls.Add(wall);
    }

    List<GameObject> walls = new List<GameObject>();

    [Command]
    private void CmdSpawnRelics()
    {
        //know the floor plane
        Vector2 center = new Vector2(0, 0);

        float totalXScale = 0f;
        float totalZScale = 0f;

        if (UnityARAnchorManager.Instance == null)
        {
            foreach (LocalPlane plane in FindObjectsOfType<LocalPlane>())
            {
                center.x += plane.transform.localScale.x * plane.transform.position.x;
                center.y += plane.transform.localScale.z * plane.transform.position.z;

                totalXScale += plane.transform.localScale.x;
                totalZScale += plane.transform.localScale.z;
            }
        }
        else
        {
            foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                Transform plane = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0);

                center.x += plane.localScale.x * plane.position.x;
                center.y += plane.localScale.z * plane.position.z;

                totalXScale += plane.localScale.x;
                totalZScale += plane.localScale.z;
            }
        }


        if (totalXScale < 0.01f)
            totalXScale = 1;

        if (totalZScale < 0.01f)
            totalZScale = 1;

        center.x /= totalXScale;
        center.y /= totalZScale;

        Transform centerPlane = null;
        Transform closestPlane = null;

        if (UnityARAnchorManager.Instance == null)
        {
            LocalPlane[] objects = FindObjectsOfType<LocalPlane>();
            centerPlane = objects[0].transform;
            closestPlane = objects[0].transform;

            for (int i = 1; i < objects.Length; i++)
            {
                Vector2 pos = new Vector2(objects[i].transform.position.x, objects[i].transform.position.z);
                if ((new Vector2(centerPlane.position.x, centerPlane.position.z) - center).sqrMagnitude > (pos - center).sqrMagnitude)
                {
                    closestPlane = centerPlane;
                    centerPlane = objects[i].transform;
                }
                else if ((new Vector2(closestPlane.position.x, closestPlane.position.z) - center).sqrMagnitude > (pos - center).sqrMagnitude)
                {
                    closestPlane = objects[i].transform;
                }
            }
        }
        else
        {
            //Re do this part later
            LocalPlane[] objects = FindObjectsOfType<LocalPlane>();
            centerPlane = objects[0].transform;
            closestPlane = objects[0].transform;

            for (int i = 1; i < objects.Length; i++)
            {
                Vector2 pos = new Vector2(objects[i].transform.position.x, objects[i].transform.position.z);
                if ((new Vector2(centerPlane.position.x, centerPlane.position.z) - center).sqrMagnitude > (pos - center).sqrMagnitude)
                {
                    closestPlane = centerPlane;
                    centerPlane = objects[i].transform;
                }
                else if ((new Vector2(closestPlane.position.x, closestPlane.position.z) - center).sqrMagnitude > (pos - center).sqrMagnitude)
                {
                    closestPlane = objects[i].transform;
                }
            }
        }

        closestPlane.name = "Closest Plane";
        centerPlane.name = "Center Plane";

        if (closestPlane.localScale.x * closestPlane.localScale.z > centerPlane.localScale.x * centerPlane.localScale.z)
        {
            Transform temp = closestPlane;
            closestPlane = centerPlane;
            centerPlane = temp;
        }

        walls.Clear();
        for (int i = 0; i < 4; i++)
        {
            CmdSpawnWall(centerPlane.gameObject);
        }
        CmdSpawnRelic(centerPlane.gameObject, walls.ToArray());

        walls.Clear();
        for (int i = 0; i < 4; i++)
        {
            CmdSpawnWall(closestPlane.gameObject);
        }
        CmdSpawnRelic(closestPlane.gameObject, walls.ToArray());
    }

    [ClientRpc]
    private void RpcSpawnEntrances()
    {
        if (!isServer) return;

        LocalPlane[] objects = FindObjectsOfType<LocalPlane>();

        for (int i = 0; i < objects.Length; i++)
        {
            //Debug.Log(objects[i].name);
            if (objects[i].name == "Closest Plane" || objects[i].name == "Center Plane")
                continue;

            GameObject obj = Instantiate(entrancePrefab,
                new Vector3(Random.Range(objects[i].transform.position.x - objects[i].transform.localScale.x / 4, objects[i].transform.position.x + objects[i].transform.localScale.x / 4),
                objects[i].transform.position.y + objects[i].transform.localScale.y / 2 + entrancePrefab.transform.localScale.y / 2,
                Random.Range(objects[i].transform.position.z - objects[i].transform.localScale.z / 4, objects[i].transform.position.z + objects[i].transform.localScale.z / 4)),
                Quaternion.identity);
            NetworkServer.Spawn(obj);
        }

        /*
        //know the floor plane
        float minX = float.MinValue;
        float maxX = float.MaxValue;

        float minZ = float.MinValue;
        float maxZ = float.MaxValue;

        if (UnityARAnchorManager.Instance == null)
        {
            foreach (LocalPlane plane in FindObjectsOfType<LocalPlane>())
            {
                float x = plane.transform.position.x;
                float z = plane.transform.position.z;


                if (x - plane.transform.localScale.x < minX)
                    minX = x - plane.transform.localScale.x;

                if (x + plane.transform.localScale.x > maxX)
                    maxX = x + plane.transform.localScale.x;


                if (z - plane.transform.localScale.z < minZ)
                    minZ = z - plane.transform.localScale.z;

                if (z + plane.transform.localScale.z > maxZ)
                    maxZ = z + plane.transform.localScale.z;
            }
        }
        else
        {
            foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
            {
                Transform plane = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0);

                float x = plane.position.x;
                float z = plane.position.z;


                if (x - plane.localScale.x < minX)
                    minX = x - plane.localScale.x;

                if (x + plane.localScale.x > maxX)
                    maxX = x + plane.localScale.x;


                if (z - plane.localScale.z < minZ)
                    minZ = z - plane.localScale.z;

                if (z + plane.localScale.z > maxZ)
                    maxZ = z + plane.localScale.z;
            }
        }

        Vector3 centerPos = new Vector3((minX + maxX) / 2f, 0, (minZ + maxZ) / 2f);

        //loop through every plane and create one that's away from the center

        GameObject obj = Instantiate(entrancePrefab);
        NetworkServer.Spawn(obj);
        */
    }
}
