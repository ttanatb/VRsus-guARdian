using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCreation : MonoBehaviour {

    public Mesh mountains;
    public List<Vector3> boundary;
    public float yMax;
    public float yMin;
    public float yVariance;
    public float layerWidth;
    public int peakOffset;
    public int extrudeTimes;

    private List<Vector3> mountainVerts;
    private List<int> mountainTris;
    private List<Vector2> mountainUVs;
    private Vector3 center;
    private int angles;
    private int layers = 1;

    void Start()
    {
        CreateTerrain();
    }

    public void CreateTerrain ()
    {
		if (boundary != null)
        {
            angles = boundary.Count;
            mountainVerts = new List<Vector3>(angles * 2);
            mountainUVs = new List<Vector2>(angles * 2);
            mountainTris = new List<int>(6 * angles);
            center = Vector3.zero;
            foreach (Vector3 vert in boundary)
            {
                center += vert;
                mountainVerts.Add(vert);
                mountainUVs.Add(new Vector2(vert.x, vert.z));
            }
        
            center /= angles;

            Extrude(extrudeTimes);

            RandomizeHeights();

            mountains = new Mesh();

            mountains.SetVertices(mountainVerts);
            mountains.SetTriangles(mountainTris, 0);
            mountains.SetUVs(0, mountainUVs);

            mountains.RecalculateBounds();
            mountains.RecalculateNormals();
            mountains.RecalculateTangents();
            mountains.UploadMeshData(false);

            GetComponent<MeshFilter>().sharedMesh = mountains;
        }
	}

    private void Extrude(int extrusions)
    {
        if (extrusions == 0)
        {
            return;
        }

        layers++;

        int curMountVerts = mountainVerts.Count;
        int extrudeStart = curMountVerts - angles;

        for (int i = curMountVerts - angles; i < curMountVerts; i++)
        {
            Vector3 curMount = mountainVerts[i];
            Vector3 centerToMount = curMount - center;
            Vector3 temp = curMount + (centerToMount.normalized * layerWidth);

            mountainVerts.Add(temp);
            mountainUVs.Add(new Vector2(temp.x, temp.z));

            if (i == curMountVerts - 1)
            {
                mountainTris.Add(i);
                mountainTris.Add(i + angles);
                mountainTris.Add(extrudeStart);

                mountainTris.Add(extrudeStart);
                mountainTris.Add(i + angles);
                mountainTris.Add(extrudeStart + angles);
            }
            else
            {
                mountainTris.Add(i);
                mountainTris.Add(i + angles);
                mountainTris.Add(i + 1);

                mountainTris.Add(i + 1);
                mountainTris.Add(i + angles);
                mountainTris.Add(i + 1 + angles);
            }
        }

        Extrude(extrusions - 1);
    }

    private void RandomizeHeights()
    {
        int peakLayer = layers - peakOffset;
        float yRange = yMax - yMin;
        float yIncrement = yRange / peakLayer;
        int startIndex = angles;
        int endIndex = mountainVerts.Count;

        for (int i = startIndex; i < endIndex; i++)
        {
            Vector3 temp = mountainVerts[i];

            int curLayer = i / angles;

            float standardHeight = 0;

            if (curLayer > peakLayer)
            {
                standardHeight = (peakLayer - Mathf.Abs(peakLayer - curLayer)) * yIncrement;
            }
            else
            {
                standardHeight = curLayer * yIncrement;
            }

            temp.y = standardHeight + Random.Range(-yVariance, yVariance);
            mountainVerts[i] = temp;
        }
    }
}
