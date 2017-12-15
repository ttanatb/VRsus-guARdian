using UnityEngine;
using System.Collections.Generic;

using UnityEngine.Networking;
using UnityEngine.XR.iOS;

public enum GamePhase
{
    Scanning = 0,
    Placing = 1,
    Playing = 2,
    Over = 3
}

[System.Serializable]
public class Trap
{
    public GameObject trap;
    public int count;
    public int maxCount;
}

public class GameManager : NetworkBehaviour
{
    [SyncVar]
    private int currGamePhase = 0;

    public GamePhase CurrGamePhase { get { return (GamePhase)currGamePhase; } }

    public GameObject planeGeneratorPrefab;

    public Trap[] trapList;

    public GameObject relicPrefab;
    public GameObject entrancePrefab;
    public GameObject wallPrefab;

    private int currTrapSelection = -1;

    public float minPlayArea = 1f;

    private TrapDefense previouslySelectedTrap = null;
    private TrapDefense currentlySelectedTrap = null;

    public override void OnStartLocalPlayer()
    {
#if UNITY_IOS
        Instantiate(planeGeneratorPrefab);
        CanvasManager.Instance.SetUI(this);
#else 
        if (DebugMode.Instance.IsDebugging)
        {
            Instantiate(planeGeneratorPrefab);
            CanvasManager.Instance.SetUI(this);
        }
#endif
    }

    public void Start()
    {
        foreach (Trap t in trapList)
        {
            t.maxCount = t.count;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isServer || !isLocalPlayer)
            return;

        switch (currGamePhase)
        {
            //Placing
            case 1:
                if (Utility.IsPointerOverUIObject()) return;
                if (currTrapSelection != -1)
                {
                    currentlySelectedTrap = null;
                    TogglePreviouslySelectedTrap();
                    CheckTapOnARPlane();
                }
                else
                {
                    CheckTapOnSecurityScreen();
                    CheckTapOnTraps();
                    MoveTrap();
                }
                break;
            case 2:
                CheckTapOnSecurityScreen();
                break;
            default:
                break;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            CmdSpawnRelics();
    }

    void CheckTapOnTraps()
    {
        RaycastHit hit;
        LayerMask layer = LayerMask.NameToLayer("Trap");
        if (Input.touchCount > 0)
        {
            //Debug.Log("Checking Tap");
            foreach (Touch t in Input.touches)
            {
                if (t.phase != TouchPhase.Began)
                {
                    continue;
                } else if (Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, float.MaxValue, 1 << layer))
                {
                    //Debug.Log("Raycast hit the trap");
                    currentlySelectedTrap = hit.transform.GetComponent<TrapDefense>();
                    Debug.Log(currentlySelectedTrap);
                    currentlySelectedTrap.ToggleSelected();
                    TogglePreviouslySelectedTrap();
                }
                else
                {
                    //Debug.Log("Raycast did not hit a trap");
                    currentlySelectedTrap = null;
                    TogglePreviouslySelectedTrap();
                }
                return;
            }
        }
    }

    void MoveTrap()
    {
        if (currentlySelectedTrap != null)
        {
            RaycastHit hit;
            LayerMask layer = LayerMask.NameToLayer("Tower");
            if (Input.touchCount > 0)
            {
                foreach (Touch t in Input.touches)
                {
                    if (t.phase == TouchPhase.Stationary || t.phase == TouchPhase.Began) continue;

                    if (Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, float.MaxValue, 1 << layer))
                    {
                        currentlySelectedTrap.transform.position = hit.point;
                        return;
                    }
                }
            }
        }
    }


    void CheckTapOnSecurityScreen()
    {
        RaycastHit hit;
        LayerMask layer = LayerMask.NameToLayer("UI");
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase != TouchPhase.Began) continue;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, float.MaxValue, 1 << layer))
                {
                    currentlySelectedTrap = hit.collider.GetComponent<SecurityScreen>().associatedCamera;
                    currentlySelectedTrap.ToggleSelected();
                    TogglePreviouslySelectedTrap();
                }
                else
                {
                    currentlySelectedTrap = null;
                    TogglePreviouslySelectedTrap();
                }
                return;
            }
        }
    }

    void TogglePreviouslySelectedTrap()
    {
        if (previouslySelectedTrap)
            previouslySelectedTrap.ToggleSelected();

        previouslySelectedTrap = currentlySelectedTrap;
    }

    void CheckTapOnARPlane()
    {
        RaycastHit hit;
        LayerMask layer = LayerMask.NameToLayer("Tower");
        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase != TouchPhase.Began || currTrapSelection < 0 || currTrapSelection > trapList.Length - 1 || trapList[currTrapSelection].count < 1)
                    continue;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, 10f, 1 << layer))
                {
                    trapList[currTrapSelection].count -= 1;

                    CmdSpawnTrap(currTrapSelection, hit.point);
                    TogglePreviouslySelectedTrap();

                    CanvasManager.Instance.ClearSelection(this);
                    CanvasManager.Instance.UpdateTrapCount(this);

                    currTrapSelection = -1;
                    return;
                }
            }
        }

#if !UNITY_IOS
        //Testing for PC
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B) &&
         (currTrapSelection >= 0 && currTrapSelection < trapList.Length && trapList[currTrapSelection].count > 0))
        {
            trapList[currTrapSelection].count -= 1;

            CmdSpawnTrap(currTrapSelection, Vector3.zero);
            TogglePreviouslySelectedTrap();

            CanvasManager.Instance.ClearSelection(this);
            CanvasManager.Instance.UpdateTrapCount(this);

            currTrapSelection = -1;
        }
#endif
    }

    [Command]
    private void CmdSpawnTrap(int index, Vector3 pos)
    {
        GameObject go = Instantiate(trapList[index].trap, pos, Quaternion.identity);
        NetworkServer.Spawn(go);
    }

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

    public void ResetGame()
    {
        //foreach (Wall w in FindObjectsOfType<Wall>())
        //{
        //    Network.Destroy(w.gameObject);
        //}
        currGamePhase = (int)GamePhase.Placing;


        foreach (TrapDefense t in FindObjectsOfType<TrapDefense>())
        {
            Network.Destroy(t.gameObject);
        }

        foreach (Trap t in trapList)
        {
            t.count = t.maxCount;
        }
    }

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

				BlockManager blockManager = FindObjectOfType<BlockManager> ();
				if (blockManager)
					blockManager.RpcStartPlacing();
			
                Combat combat = GetComponent<Combat>();
                if (combat != null)
                    combat.CanShoot = true;
			
                //CanvasManager.Instance.ToggleCrossHairUI();
                UnityARAnchorManager.Instance.TogglePlaneMaterial();
                break;
        }

        currGamePhase = (int)newPhase;
        CanvasManager.Instance.SetUI(this);
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
        obj.GetComponent<Relic>().Init(walls, plane);
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
