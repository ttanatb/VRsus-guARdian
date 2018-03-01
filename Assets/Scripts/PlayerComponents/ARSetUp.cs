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

    [Tooltip("The asset file with a look-up array filled with environment object data")]
    public EnvironmentData environmentData;

    public GameObject envObjPrefab;

    [Tooltip("The minimum area to count as a play area")]
    public float minPlayArea = 1f;

    private GameObject planeGeneratorObj;       //reference to the AR plane generator

    private int currTrapSelection = -1;                 //index of the trap that is chosen
    private TrapDefense previouslySelectedTrap = null;
    private TrapDefense currentlySelectedTrap = null;

    [SyncVar]
    private int currGamePhase = 0;

    private List<List<Vector3>> sortedPlanes;
    private List<float> usedIndecies;// = new List<float>();
    private List<TrapDefense> trapObjList;// = new List<TrapDefense>();
    private List<Relic> relicObjList;// = new List<Relic>();
    private List<Entrance> entranceObjList;// = new List<Entrance>();
    private List<GameObject> envObjList;

    private float scale;
    /// <summary>
    /// Gets the current phase of the game
    /// </summary>
    public GamePhase CurrGamePhase { get { return (GamePhase)currGamePhase; } }
    #endregion

    #region Init Logic
    public override void OnStartServer()
    {
        usedIndecies = new List<float>();
        trapObjList = new List<TrapDefense>();
        relicObjList = new List<Relic>();
        entranceObjList = new List<Entrance>();
        envObjList = new List<GameObject>();
    }
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
            currentlySelectedTrap.transform.position = resultInfo.hit.point;// + Vector3.up * currentlySelectedTrap.transform.localScale.y / 2;
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
        trapObjList.Add(currentlySelectedTrap);
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

        foreach (TrapDefense trap in trapObjList)
            NetworkServer.Destroy(trap.gameObject);
        trapObjList.Clear();
        foreach (Relic relic in relicObjList)
            NetworkServer.Destroy(relic.gameObject);
        relicObjList.Clear();
        foreach (Entrance entrance in entranceObjList)
            NetworkServer.Destroy(entrance.gameObject);
        entranceObjList.Clear();
        foreach (GameObject prop in envObjList)
            NetworkServer.Destroy(prop);
        entranceObjList.Clear();
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
                scale = FindObjectOfType<LocalPlane>().transform.localScale.y / 2;
                SpawnRelics();
                SpawnEntrances();
                StartCoroutine("SpawnEnvObjs");
                RpcBuildTerrain();
                break;

            case GamePhase.Playing:
                currentlySelectedTrap = null;
                TogglePreviouslySelectedTrap();
                foreach (TrapDefense trap in trapObjList) trap.TransitionToPlayPhase();

                VRTransition vrTransition = FindObjectOfType<VRTransition>();
                if (vrTransition)
                    vrTransition.RpcSwitchToTopViewCam();
                RpcSetMessage();
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
        if (isServer)
            relicObjList.Add(obj.GetComponent<Relic>());
    }

    [Command]
    private void CmdSpawnEnvDecor(Vector3 position, int index, float rotY)
    {
        GameObject obj = Instantiate(envObjPrefab, position, Quaternion.Euler(0f, rotY, 0f));
        NetworkServer.Spawn(obj);
        obj.GetComponent<EnvironmentObject>().RpcInit(0, index);// environmentData.decorDataList[index]);
        if (isServer)
        {
            envObjList.Add(obj);
        }
    }

    [Command]
    private void CmdSpawnEnvStructure(Vector3 position, int index, float rotY)
    {
        GameObject obj = Instantiate(envObjPrefab, position, Quaternion.Euler(0f, rotY, 0f));
        NetworkServer.Spawn(obj);
        obj.GetComponent<EnvironmentObject>().RpcInit(1, index);// environmentData.structureDataList[index]);
        envObjList.Add(obj);
    }

    [Command]
    private void CmdSpawnEnvLandMark(Vector3 position, int index, float rotY)
    {
        GameObject obj = Instantiate(envObjPrefab, position, Quaternion.Euler(0f, rotY, 0f));
        NetworkServer.Spawn(obj);
        obj.GetComponent<EnvironmentObject>().RpcInit(2, index);// environmentData.landMarkDataList[index]);
        if (isServer)
            envObjList.Add(obj);
    }

    [Command]
    private void CmdSpawnEntrance(Vector3 position)
    {
        GameObject obj = Instantiate(entrancePrefab, position, Quaternion.identity);
        NetworkServer.Spawn(obj);

        if (isServer)
        {
            Entrance e = obj.GetComponent<Entrance>();
            relicObjList[Random.Range(0, relicObjList.Count)].AddEntrance(e);
            entranceObjList.Add(e);
        }
    }
    private Vector3 GetRandPosNotUnderAnyOtherPlanes(int index, float radius, int maxTries)
    {
        Vector3 spawnPos = Utility.GetRandPosInPlaneAndFarFromEdge(sortedPlanes[index], radius, maxTries);
        if (spawnPos.x > float.MaxValue / 2f) return spawnPos;

        int planeCount = sortedPlanes.Count;
        int tries = 0;
        if (index != planeCount - 1)
        {
            for (int i = index + 1; i < planeCount; i++)
            {
                if (Utility.CheckIfPointIsInPolygon(spawnPos, sortedPlanes[i]) || Utility.CheckIfTooCloseToEdge(sortedPlanes[i], spawnPos, radius))
                {
                    spawnPos = Utility.GetRandPosInPlaneAndFarFromEdge(sortedPlanes[index], radius, maxTries);

                    tries++;
                    if (tries > maxTries || spawnPos.x > float.MaxValue / 2f) return spawnPos;

                    i = index;
                }
            }
        }
        return spawnPos;
    }

    private void SpawnRelics()
    {
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

        Vector3 position = GetRandPosNotUnderAnyOtherPlanes(0, 0.1f, 10);
        if (position.x > float.MaxValue / 2f)
            position = Utility.GetRandomPointInPlane(sortedPlanes[0]);

        CmdSpawnRelic(position); // + Vector3.up * scale);

        position = GetRandPosNotUnderAnyOtherPlanes(largestPlaneIndex, 0.1f, 10);
        if (position.x > float.MaxValue / 2f)
            position = Utility.GetRandomPointInPlane(sortedPlanes[largestPlaneIndex]);

        CmdSpawnRelic(position); // + Vector3.up * scale);
    }

    /// <summary>
    /// Coroutine for spawning environment objects
    /// </summary>
    private System.Collections.IEnumerator SpawnEnvObjs()
    {
        int planeCount = sortedPlanes.Count;
        int relicCount = relicObjList.Count;
        List<Vector3> spawnedList = new List<Vector3>();
        for (int i = 0; i < planeCount; i++)
        {
            spawnedList.Clear();

            /*
            float threshold = 3f;
            if (Utility.GetAreaSqr(sortedPlanes[i]) > threshold)
            {
                int rndmIndex = Random.Range(0, environmentData.landMarkDataList.Length);
                float radius = environmentData.landMarkDataList[rndmIndex].radius;

                Vector3 spawnPos = GetEnvObjSpawnPos(radius, i, relicCount, spawnedList);

                CmdSpawnEnvLandMark(spawnPos + Vector3.up * scale, rndmIndex, Random.Range(0f, 360f));
                spawnPos.y = radius;
                spawnedList.Add(spawnPos);

                yield return new WaitForSeconds(0.001f);
            }
            */

            int structureSpawnCount = (int)(Utility.GetAreaSqr(sortedPlanes[i]) * 2.0f);
            structureSpawnCount = Mathf.Clamp(structureSpawnCount, 2, 20);
            for (int j = 0; j < structureSpawnCount; j++)
            {
                int rndmIndex = Random.Range(0, environmentData.structureDataList.Length);
                float radius = environmentData.structureDataList[rndmIndex].radius;

                Vector3 spawnPos = GetEnvObjSpawnPos(radius, i, relicCount, spawnedList, 10);
                if (spawnPos.x > float.MaxValue / 2f) continue;
                else
                {
                    CmdSpawnEnvStructure(spawnPos, rndmIndex, Random.Range(0f, 360f));
                    spawnPos.y = radius;
                    spawnedList.Add(spawnPos);
                }

                yield return new WaitForSeconds(0.01f);
            }

            int decorSpawnCount = (int)(Utility.GetAreaSqr(sortedPlanes[i]) * 1.0f);
            decorSpawnCount = Mathf.Clamp(decorSpawnCount, 5, 10);
            for (int j = 0; j < decorSpawnCount; j++)
            {

                int rndmIndex = Random.Range(0, environmentData.decorDataList.Length);
                float radius = environmentData.decorDataList[rndmIndex].radius;

                Vector3 spawnPos = GetEnvObjSpawnPos(radius, i, relicCount, spawnedList, 5);
                if (spawnPos.x > float.MaxValue / 2f) continue;
                else
                {
                    CmdSpawnEnvDecor(spawnPos, rndmIndex, Random.Range(0f, 360f));
                    spawnPos.y = radius;
                    spawnedList.Add(spawnPos);
                }

                yield return new WaitForSeconds(0.01f);
            }

            yield return new WaitForSeconds(0.01f);

        }

        yield return null;
    }

    private Vector3 GetEnvObjSpawnPos(float radius, int planeIndex, int relicCount, List<Vector3> spawnedList, int maxTries)
    {
        Vector3 spawnPos = GetRandPosNotUnderAnyOtherPlanes(planeIndex, radius, maxTries);
        int countOfObjToCheckFor = relicCount + spawnedList.Count;
        int tries = 0;
        for (int k = 0; k < countOfObjToCheckFor; k++)
        {
            if (k < relicCount)
            {
                if (Mathf.Pow(spawnPos.x - relicObjList[k].transform.position.x, 2f) +
                    Mathf.Pow(spawnPos.z - relicObjList[k].transform.position.z, 2f) <
                    Mathf.Pow(radius + 0.2f, 2f))
                {
                    spawnPos = GetRandPosNotUnderAnyOtherPlanes(planeIndex, radius, maxTries);
                    if (spawnPos.x > float.MaxValue / 2f) return spawnPos;
                    k = 0;
                }
            }
            else
            {
                int index = k - relicCount;
                if (Mathf.Pow(spawnPos.x - spawnedList[index].x, 2f) +
                    Mathf.Pow(spawnPos.z - spawnedList[index].z, 2f) <
                    Mathf.Pow(spawnedList[index].y + radius, 2f))
                {
                    spawnPos = GetRandPosNotUnderAnyOtherPlanes(planeIndex, radius, maxTries);
                    if (spawnPos.x > float.MaxValue / 2f) return spawnPos;

                    k = 0;
                    tries++;

                    if (tries > maxTries) return Vector3.one * float.MaxValue;
                }
            }
        }

        return spawnPos;
    }

    private void SpawnEntrances()
    {
        for (int i = 1; i < sortedPlanes.Count; i++)
        {
            if (usedIndecies.Contains(i)) continue;

            Vector3 position = GetRandPosNotUnderAnyOtherPlanes(i, 0.2f, 10);
            if (position.x > float.MaxValue / 2f)
                position = Utility.GetRandomPointInPlane(sortedPlanes[i]);

            CmdSpawnEntrance(position);
        }
    }

    [ClientRpc]
    private void RpcSetMessage()
    {
        if (isServer) return;
        CanvasManager.Instance.SetMessage("Click one of the white boxes to choose where to spawn from");
    }

    [ClientRpc]
    private void RpcBuildTerrain()
    {
        if (isServer) return;
        sortedPlanes = LocalObjectBuilder.Instance.GetSortedPlanes();
        float floor = LocalObjectBuilder.Instance.FloorPos;// + FindObjectOfType<LocalPlane>().transform.localScale.y / 2f;

        List<Vector3> vertices = new List<Vector3>();
        vertices = Utility.CombinePolygons(sortedPlanes[0], sortedPlanes[1], 0.2f);
        for (int i = 2; i < sortedPlanes.Count; i++)
        {
            vertices = Utility.CombinePolygons(vertices, sortedPlanes[i], 0.2f);
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            Vector3 vert = vertices[i];
            vert.y = floor;
            vertices[i] = vert;
        }

        EnvironmentCreation terrainBuilder = LocalObjectBuilder.Instance.GetComponent<EnvironmentCreation>();
        terrainBuilder.boundary = vertices;
        terrainBuilder.CreateTerrain();
    }
}
