using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DebugPlaneTiler : MonoBehaviour
{
    private Vector3 totalScale = Vector3.one;        //x,z scale of tower

    private Material matTop;
    private Material matX;
    private Material matY;

    private Vector2 topScale = Vector2.one;        //x,z scale of tower
    private Vector2 xScale = Vector2.one;        //x,z scale of tower
    private Vector2 yScale = Vector2.one;        //x,z scale of tower

    [SerializeField]
    private float updateRate = 0.1f;

    [SerializeField]
    private float renderUpdateRate = 0.1f;

    [SerializeField]
    static List<DebugPlaneTiler> planeList;

    private List<Vector3> vertices;

    //private static bool isFirst = true;

    private void Awake()
    {
        if (planeList == null)
        {
            planeList = new List<DebugPlaneTiler>();
        }

        //if (isFirst)
        //{
        //    isFirst = false;
        //
        //    IEnumerator renderOrderCoroutine = RenderOrderCoroutine(renderUpdateRate);
        //    StartCoroutine(renderOrderCoroutine);
        //}
    }

    // Use this for initialization
    void Start()
    {
        Material[] mats = GetComponent<MeshRenderer>().materials;
        Color c = Random.ColorHSV();
        foreach (Material m in mats)
            m.SetColor("_TintColor", c);

        matTop = mats[0];
        matX = mats[1];
        matY = mats[2];

        IEnumerator updateCoroutine = TextureUpdateCoroutine(updateRate);
        StartCoroutine(updateCoroutine);
    }

    private void OnDestroy()
    {
        if (planeList != null && planeList.Count == 0)
        {
            planeList = null;
        }
    }

    IEnumerator TextureUpdateCoroutine(float waitTime)
    {
        while (true)
        {
            totalScale.x = transform.localScale.x * transform.parent.localScale.x;
            totalScale.y = transform.localScale.y * transform.parent.localScale.y;
            totalScale.z = transform.localScale.z * transform.parent.localScale.z;
            totalScale = totalScale / 0.02f;

            topScale.x = totalScale.x;
            topScale.y = totalScale.z;
            matTop.SetTextureScale("_MainTex", topScale);
            matTop.SetTextureOffset("_MainTex", topScale);

            xScale.x = totalScale.x;
            xScale.y = totalScale.y;
            matX.SetTextureScale("_MainTex", xScale);
            matX.SetTextureOffset("_MainTex", xScale);

            yScale.x = totalScale.z;
            yScale.y = totalScale.y;
            matY.SetTextureScale("_MainTex", yScale);
            matY.SetTextureOffset("_MainTex", yScale);

            yield return new WaitForSeconds(waitTime);
        }
    }

    //IEnumerator RenderOrderCoroutine(float waitTime)
    //{
    //    while(true)
    //    {
    //
    //        yield return new WaitForSeconds(waitTime);
    //    }
    //}


    public void AddToList()
    {
        if (planeList.Contains(this))
            return;

        planeList.Add(this);
        planeList = planeList.OrderBy(o => o.transform.position.y).ToList();
        vertices = Utility.CreateVerticesFromPlane(transform);
    }

    public void RemoveFromList()
    {
        if (planeList.Contains(this))
            planeList.Remove(this);
    }

    public void UpdateWithinList()
    {
        planeList = planeList.OrderBy(o => o.transform.position.y).ToList();

        //for (int i = 0; i < planeList.Count; i++)
        //    planeList[i].name = "Plane " + i;

        vertices = Utility.CreateVerticesFromPlane(transform);

        //Debug.Log(gameObject.name + " vertices");
        //foreach(Vector3 v in vertices)
        //{
        //    Debug.Log(v);
        //}

        float yPosBelow = planeList[0].transform.position.y ;
        for (int i = 0; i < planeList.Count; i++)
        {
            //Debug.Log(i);
            if (planeList[i] == this)
            {
                break;
            }
            else if (Utility.CheckIfContains(planeList[i].vertices, vertices))
            {
                yPosBelow = planeList[i].transform.position.y;
                //Debug.Log("New yPosBelow: " + yPosBelow);
            }
        }

        Vector3 newScale = transform.localScale;
        newScale.y = (transform.position.y - yPosBelow) / 10f;
        if (newScale.y < 0.001f)
            newScale.y = 0.001f;

        transform.localScale = newScale;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            RemoveFromList();
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            UpdateWithinList();

        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            AddToList();
        }
    }
}