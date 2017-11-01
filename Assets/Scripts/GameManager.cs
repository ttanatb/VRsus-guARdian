using System.Collections;
using System.Collections.Generic;
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
        if (Input.GetKeyDown(KeyCode.S) &&
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
        foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
        {
            float planeArea = UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.x *
                UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.z;

            area += planeArea;
        }

		area *= 100f;

        Debug.Log("Total plane area: " + area);

        if (area > minPlayArea)
            return true;
        else return false;
    }

    public void SetPhaseTo(GamePhase newPhase)
    {
        currGamePhase = (int)newPhase;
        CanvasManager.Instance.SetUI(this);

        switch (newPhase)
        {
            case GamePhase.Placing:
                CmdSpawnRelics();
                break;

            case GamePhase.Playing:
                CmdSpawnEntrances();
                break;
        }
    }

    public void SetCurrTrapSelection(int toSelect)
    {
        currTrapSelection = toSelect;
    }

    [Command]
    private void CmdSpawnRelics()
    {
        //know the floor plane
        float x = 0f;
        float z = 0f;
        float totalXScale = 0f;
        float totalZScale = 0f;

        foreach (string key in UnityARAnchorManager.Instance.planeAnchorMap.Keys)
        {
            x += UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.x *
                UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).position.x;
            z += UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.z *
                UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).position.z;

            totalXScale += UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.x;
            totalZScale += UnityARAnchorManager.Instance.planeAnchorMap[key].gameObject.transform.GetChild(0).localScale.z;
        }

        if (totalXScale < 0.01f)
            totalXScale = 1;

        if (totalZScale < 0.01f)
            totalZScale = 1;

        x /= totalXScale;
        z /= totalZScale;

        GameObject obj = Instantiate(relicPrefab, new Vector3(x, 0f, z), Quaternion.identity);
        NetworkServer.Spawn(obj);
    }


    [Command]
    private void CmdSpawnEntrances()
    {
        GameObject obj = Instantiate(entrancePrefab);
        NetworkServer.Spawn(obj);
    }
}
