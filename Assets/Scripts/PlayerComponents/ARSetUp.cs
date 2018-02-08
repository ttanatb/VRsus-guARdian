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

    private List<List<Vector3>> sortedPlanes;
    private List<float> usedIndecies = new List<float>();

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
        //LayerMask layer2 = LayerMask.NameToLayer("Trap");
        InputResult resultInfo = XInput.Instance.CheckTap((1 << layer), TouchPhase.Moved, TouchPhase.Stationary);
        if (resultInfo.result == ResultType.Success)
        {
            currentlySelectedTrap.transform.position = resultInfo.hit.point + Vector3.up * currentlySelectedTrap.transform.localScale.y / 2;
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
        TogglePreviouslySelectedTrap();
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
#if UNITY_IOS
        UnityARAnchorManager.Instance.TogglePlaneMaterial();
#endif
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
                sortedPlanes = LocalObjectBuilder.Instance.GetSortedPlanes();
                CmdSpawnRelics();
                RpcSpawnEntrances();
                break;

            case GamePhase.Playing:
                currentlySelectedTrap = null;
                TogglePreviouslySelectedTrap();


                VRTransition vrTransition = FindObjectOfType<VRTransition>();
                if (vrTransition)
                    vrTransition.RpcSwitchToTopViewCam();

                ARCombat combat = GetComponent<ARCombat>();
                if (combat != null)
                    combat.IsShootingEnabled = true;

#if UNITY_IOS
                UnityARAnchorManager.Instance.TogglePlaneMaterial();
#endif
                break;
            case GamePhase.Over:
                if (!isServer) break;
                ARCombat combatt = GetComponent<ARCombat>();
                if (combatt != null)
                    combatt.IsShootingEnabled = false;
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
    private void CmdSpawnRelic(Vector3 position)
    {
        GameObject obj = Instantiate(relicPrefab, position, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }

    [Command]
    private void CmdSpawnRelics()
    {
        if (!isServer) return;

        int largestPlaneIndex = 0;
        float largestArea = float.MinValue;
        for (int i = 1; i < sortedPlanes.Count; i++)
        {
            float area = Utility.GetAreaSqr(sortedPlanes[i]);
            if (area > largestArea)
            {
                largestArea = area;
                largestPlaneIndex = i;
            }
        }

        usedIndecies.Add(largestPlaneIndex);
        float scale = FindObjectOfType<LocalPlane>().transform.localScale.y / 2;

        CmdSpawnRelic(GetRandPosNotUnderAnyOtherPlanes(0) + Vector3.up * scale);
        CmdSpawnRelic(GetRandPosNotUnderAnyOtherPlanes(largestPlaneIndex) + Vector3.up * scale);
    }

    private Vector3 GetRandPosNotUnderAnyOtherPlanes(int index)
    {
        Vector3 spawnPos = Utility.GetRandomPointInPlane(sortedPlanes[index]);
        while (Utility.CheckIfTooCloseToEdge(sortedPlanes[index], spawnPos, 0.05f))
            spawnPos = Utility.GetRandomPointInPlane(sortedPlanes[index]);

        for (int i = index + 1; i < sortedPlanes.Count; i++)
        {
            if (Utility.CheckIfPointIsInPolygon(spawnPos, sortedPlanes[i]) || Utility.CheckIfTooCloseToEdge(sortedPlanes[i], spawnPos, 0.05f))
            {
                spawnPos = Utility.GetRandomPointInPlane(sortedPlanes[index]);
                while (Utility.CheckIfTooCloseToEdge(sortedPlanes[index], spawnPos, 0.05f))
                    spawnPos = Utility.GetRandomPointInPlane(sortedPlanes[index]);
                i = index;
            }
        }
        return spawnPos;
    }


    [ClientRpc]
    private void RpcSpawnEntrances()
    {
        if (!isServer) return;
        float scale = FindObjectOfType<LocalPlane>().transform.localScale.y / 2;
        for (int i = 1; i < sortedPlanes.Count; i++)
        {
            if (usedIndecies.Contains(i)) continue;
            else CmdSpawnEntrance(GetRandPosNotUnderAnyOtherPlanes(i) + Vector3.up * scale);
        }
    }

    [Command]
    private void CmdSpawnEntrance(Vector3 position)
    {
        GameObject obj = Instantiate(entrancePrefab, position + entrancePrefab.transform.localScale / 2f, Quaternion.identity);
        NetworkServer.Spawn(obj);
    }
}
