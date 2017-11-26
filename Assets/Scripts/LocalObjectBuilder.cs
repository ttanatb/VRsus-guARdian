using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalObjectBuilder : SingletonMonoBehaviour<LocalObjectBuilder>
{
    [SerializeField]
    List<GameObject> localPlanes;
    int prevPlanesListCount = 0;
    public GameObject planePrefab;
    private PlaneManager planeManager;

    private float floorPos;

    [SerializeField]
    //List<GameObject> localBlocks;
    //int prevBlocksListCount = 0;
    //public Mesh[] blockMeshes;
    //public GameObject blockPrefab;
    //private BlockManager blockManager;

    public float FloorPos { get { return floorPos; } }

    public Vector3 GetAveragePos()
    {
        Vector3 avgPos = Vector3.zero;
        for (int i = 0; i < localPlanes.Count; i++)
        {
            avgPos += localPlanes[i].transform.position;
        }

        if (localPlanes.Count > 0)
            return avgPos / localPlanes.Count;
        else return avgPos;
    }

    // Use this for initialization
    void Start()
    {
        localPlanes = new List<GameObject>();
    }

    protected override void OnDestroy()
    {
        for (int i = 0; i < localPlanes.Count; i++)
        {
            Destroy(localPlanes[i]);
        }
        localPlanes.Clear();
        base.OnDestroy();
    }

    public void SetPlaneManager(PlaneManager planeManager)
    {
        this.planeManager = planeManager;

        if (localPlanes != null)
        {
            for (int i = 0; i < localPlanes.Count; i++)
            {
                Destroy(localPlanes[i]);
            }
            localPlanes.Clear();
        }

        localPlanes = new List<GameObject>();

        StartCoroutine("UpdateLocalPlanes");
    }

    /*
    public void SetBlockManager(BlockManager blockManager)
    {
        this.blockManager = blockManager;

        if (localBlocks != null)
        {
            for (int i = 0; i < localBlocks.Count; i++)
            {
                Destroy(localBlocks[i]);
            }
            localBlocks.Clear();
        }

        localBlocks = new List<GameObject>();

        StartCoroutine("UpdateLocalBlocks");
    }
    */

    IEnumerator UpdateLocalPlanes()
    {
        //endless loop
        for (; ; )
        {
            //add a plane
            if (prevPlanesListCount < planeManager.m_ARPlane.Count)
            {
                GameObject obj = Instantiate(planePrefab);
                localPlanes.Add(obj);
            }

            //destory a plane
            else if (prevPlanesListCount > planeManager.m_ARPlane.Count)
            {
                Destroy(localPlanes[prevPlanesListCount - 1]);
                localPlanes.RemoveAt(prevPlanesListCount - 1);
            }

            //update all the planes
            for (int i = 0; i < localPlanes.Count; i++)
            {
                //check to make sure plane exists
                if (i < planeManager.m_ARPlane.Count)
                {
                    localPlanes[i].GetComponent<LocalPlane>().UpdatePos(planeManager.m_ARPlane[i].position,
                        planeManager.m_ARPlane[i].rotation,
                        planeManager.m_ARPlane[i].scale);

                    float yPos = planeManager.m_ARPlane[i].position.y;

                    floorPos = yPos < floorPos ? yPos : floorPos;
                }
                else
                    break;
            }

            prevPlanesListCount = localPlanes.Count;
            yield return new WaitForSeconds(.1f);
        }
    }
    /*
    IEnumerator UpdateLocalBlocks()
    {
        //endless loop
        for (; ; )
        {
            //add a plane
            if (prevBlocksListCount < blockManager.blockList.Count)
            {
                GameObject obj = Instantiate(blockPrefab);
                obj.transform.position = blockManager.blockList[blockManager.blockList.Count - 1].position;
                obj.GetComponent<MeshFilter>().mesh = blockMeshes[blockManager.blockList[blockManager.blockList.Count - 1].type];
                localBlocks.Add(obj);
            }

            ////destory a plane
            //else if (prevBlocksListCount > blockManager.blockList.Count)
            //{
            //    Destroy(localPlanes[prevBlocksListCount - 1]);
            //    localPlanes.RemoveAt(prevBlocksListCount - 1);
            //}

            ////update all the planes
            //for (int i = 0; i < localPlanes.Count; i++)
            //{
            //    //check to make sure plane exists
            //    if (i < blockManager.blockList.Count)
            //    {
            //        localPlanes[i].GetComponent<LocalPlane>().UpdatePos(blockManager.blockList[i].position,
            //            blockManager.blockList[i].rotation,
            //            blockManager.blockList[i].scale);

            //        float yPos = blockManager.blockList[i].position.y;

            //        floorPos = yPos < floorPos ? yPos : floorPos;
            //    }
            //    else
            //        break;
            //}

            prevBlocksListCount = localBlocks.Count;
            yield return new WaitForSeconds(.1f);
        }
    }
    */
}
