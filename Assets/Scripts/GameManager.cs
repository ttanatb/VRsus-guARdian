using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.iOS;

public enum GamePhase
{
    Scanning = 0,
    Placing = 1,
    Playing = 2,
}

[System.Serializable]
public class Trap
{
    public GameObject trap;
    public int count;
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

    public override void OnStartLocalPlayer()
    {
#if UNITY_IOS
        Instantiate(planeGeneratorPrefab);
        CanvasManager.Instance.SetUI(this);
#endif
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
                    CheckTapOnARPlane();
                else
                {
                    //move traps around??
                }
                break;
            default:
                break;
        }

        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.S))
            CmdSpawnRelics();
    }


    void CheckTapOnARPlane()
    {
        RaycastHit hit;
        int layer = LayerMask.NameToLayer("Tower");

        if (Input.touchCount > 0)
        {
            foreach (Touch t in Input.touches)
            {
                if (t.phase == TouchPhase.Began &&
                    (currTrapSelection >= 0 && currTrapSelection < trapList.Length && trapList[currTrapSelection].count > 0) &&
                    Physics.Raycast(Camera.main.ScreenPointToRay(t.position), out hit, layer))
                {
                    trapList[currTrapSelection].count -= 1;

                    CmdSpawnTrap(currTrapSelection, hit.point);

                    CanvasManager.Instance.ClearSelection(this);
                    CanvasManager.Instance.UpdateTrapCount(this);

                    currTrapSelection = -1;
                    return;
                }
            }
        }

        //Testing for PC
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.B) &&
         (currTrapSelection >= 0 && currTrapSelection < trapList.Length && trapList[currTrapSelection].count > 0))
        {
            trapList[currTrapSelection].count -= 1;

            CmdSpawnTrap(currTrapSelection, Vector3.zero);

            CanvasManager.Instance.ClearSelection(this);
            CanvasManager.Instance.UpdateTrapCount(this);

            currTrapSelection = -1;
            return;
        }

        return;
    }

    [Command]
    private void CmdSpawnTrap(int index, Vector3 pos)
    {
        GameObject go = Instantiate(trapList[index].trap, pos, Quaternion.identity);
        NetworkServer.Spawn(go);
    }

    public bool CheckAggregrateArea()
    {
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
            if (UnityARAnchorManager.Instance.planeAnchorMap.Count < 3)
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

    public void SetPhaseTo(GamePhase newPhase)
    {

        switch (newPhase)
        {
            case GamePhase.Placing:
                CmdSpawnRelics();
                break;

            case GamePhase.Playing:
                CmdSpawnEntrances();
                Combat combat = GetComponent<Combat>();
                if (combat != null)
                    combat.canShoot = true;
                CanvasManager.Instance.ToggleCrossHairUI();
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

        GameObject obj = Instantiate(relicPrefab,
            new Vector3(Random.Range(centerPlane.position.x - centerPlane.localScale.x / 2, 0),
            centerPlane.localScale.y / 2 + centerPlane.position.y + relicPrefab.transform.localScale.y / 2,
            Random.Range(centerPlane.position.z - centerPlane.localScale.z / 2, 0)),
            Quaternion.identity);

        GameObject[] walls = new GameObject[4];
        for (int i = 0; i < walls.Length; i++)
        {
            walls[i] = Instantiate(wallPrefab,
                    new Vector3(Random.Range(centerPlane.position.x - centerPlane.localScale.x / 4, centerPlane.position.x + centerPlane.localScale.x / 4),
                    centerPlane.position.y + centerPlane.localScale.y / 2 - wallPrefab.transform.localScale.y / 2,
                    Random.Range(centerPlane.position.x - centerPlane.localScale.x / 4, centerPlane.position.x + centerPlane.localScale.x / 4)),
                    Quaternion.identity);
        }

        obj.GetComponent<Relic>().Init(walls, centerPlane.gameObject);
        NetworkServer.Spawn(obj);

        //obj = Instantiate(relicPrefab,
        //    new Vector3(Random.Range(centerPlane.position.x + centerPlane.localScale.x / 2, 0),
        //    centerPlane.localScale.y / 2 + centerPlane.position.y + relicPrefab.transform.localScale.y / 2,
        //    Random.Range(centerPlane.position.z + centerPlane.localScale.z / 2, 0)),
        //    Quaternion.identity);

        //NetworkServer.Spawn(obj);

        obj = Instantiate(relicPrefab,
            new Vector3(Random.Range(closestPlane.position.x - closestPlane.localScale.x / 2, closestPlane.position.x + closestPlane.localScale.x / 2),
            closestPlane.localScale.y / 2 + closestPlane.position.y + relicPrefab.transform.localScale.y / 2,
            Random.Range(closestPlane.position.z - closestPlane.localScale.z / 2, closestPlane.position.z + closestPlane.localScale.z / 2)),
            Quaternion.identity);

        walls = new GameObject[4];
        for (int i = 0; i < walls.Length; i++)
        {
            walls[i] = Instantiate(wallPrefab,
                    new Vector3(Random.Range(closestPlane.position.x - closestPlane.localScale.x / 4, closestPlane.position.x + closestPlane.localScale.x / 4),
                    closestPlane.position.y + closestPlane.localScale.y / 2 - wallPrefab.transform.localScale.y / 2,
                    Random.Range(closestPlane.position.x - closestPlane.localScale.x / 4, closestPlane.position.x + closestPlane.localScale.x / 4)),
                    Quaternion.identity);
        }
        obj.GetComponent<Relic>().Init(walls, closestPlane.gameObject);
        NetworkServer.Spawn(obj);
    }


    [Command]
    private void CmdSpawnEntrances()
    {
        LocalPlane[] objects = FindObjectsOfType<LocalPlane>();

        for (int i = 0; i < objects.Length; i++)
        {
            Debug.Log(objects[i].name);
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
