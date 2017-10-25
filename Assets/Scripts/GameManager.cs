using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

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

    private int currTrapSelection = -1;

    public override void OnStartServer()
    {
#if UNITY_IOS
        Instantiate(PlaneGeneratorPrefab);
        CanvasManager.Instance.SetUI(this);
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_IOS
#endif

        switch (currGamePhase)
        {
            //Placing
            case 1:
                if (Utility.IsPointerOverUIObject()) return;
                CheckTapOnARPlane();
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

                    GameObject go = Instantiate(trapList[currTrapSelection].trap, hit.point, Quaternion.identity);
                    NetworkServer.Spawn(go);

                    CanvasManager.Instance.ClearSelection(this);
                    return;
                }
            }

        }
        return;
    }

    public void SetPhaseTo(GamePhase newPhase)
    {
        currGamePhase = (int)newPhase;
        CanvasManager.Instance.SetUI(this);
    }

    public void SetCurrTrapSelection(int toSelect)
    {
        currTrapSelection = toSelect;
    }
}
