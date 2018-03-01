using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentCreation : MonoBehaviour
{

    public Mesh mountains;
    public Mesh plains;
    public List<Vector3> boundary;
    public float yMax;
    public float yMin;
    public float yVariance;
    public float layerWidth;
    public int peakOffset;
    public int extrudeTimes;
    public float groundYVariance;

    public MeshFilter plainFilter;

    public int minCliffSides;
    public int maxCliffSides;
    public float cliffY;

    private List<Vector3> mountainVerts;
    private List<int> mountainTris;
    private List<Vector2> mountainUVs;
    private Vector3 center;
    private int angles;
    private int tripleAngles;
    private int layers;

    //private Mesh plains;
    private Vector2[] boundaryFlat;

    public void CreateTerrain()
    {
        if (boundary != null)
        {
            layers = 1;
            angles = boundary.Count;
            tripleAngles = angles * 3;
            mountainVerts = new List<Vector3>(angles * 2);
            mountainUVs = new List<Vector2>(angles * 2);
            mountainTris = new List<int>(6 * angles);
            center = Vector3.zero;

            foreach (Vector3 vert in boundary)
            {
                center += vert;
                // Triple for hard edges
                mountainVerts.Add(vert);
                mountainVerts.Add(vert);
                mountainVerts.Add(vert);
                mountainUVs.Add(new Vector2(vert.x, vert.z));
                mountainUVs.Add(new Vector2(vert.x, vert.z));
                mountainUVs.Add(new Vector2(vert.x, vert.z));
            }

            center /= angles;

            Extrude(extrudeTimes);

            RandomizeHeights();
            CreateCliffs();

            mountains = new Mesh();

            mountains.SetVertices(mountainVerts);
            mountains.SetTriangles(mountainTris, 0);
            mountains.SetUVs(0, mountainUVs);

            mountains.RecalculateBounds();
            mountains.RecalculateNormals();
            mountains.RecalculateTangents();
            mountains.UploadMeshData(false);

            plains = new Mesh();
            boundaryFlat = new Vector2[boundary.Count];

            for (int v = 0; v < boundary.Count; v++)
            {
                Vector2 temp = new Vector2(boundary[v].x, boundary[v].z);
                boundaryFlat[v] = temp;
            }

            Triangulator triangulator = new Triangulator(boundaryFlat);
            plains.SetVertices(boundary);
            int[] indices = triangulator.Triangulate();
            plains.SetTriangles(indices, 0);
            
            plains.RecalculateBounds();
            plains.RecalculateNormals();
            plains.RecalculateTangents();
            plains.UploadMeshData(false);

            MeshHelper.Subdivide(plains, 9);

            List<Vector3> tempPlainVerts = new List<Vector3>();
            plains.GetVertices(tempPlainVerts);

            for (int i = 0; i < tempPlainVerts.Count; i++)
            {
                Vector3 temp = tempPlainVerts[i];
                temp.y += Random.Range(0, groundYVariance);
                tempPlainVerts[i] = temp;
            }

            plains.SetVertices(tempPlainVerts);
            plains.RecalculateBounds();
            plains.RecalculateNormals();
            plains.RecalculateTangents();
            plains.UploadMeshData(false);

            GetComponent<MeshFilter>().sharedMesh = mountains;
            plainFilter.sharedMesh = plains;
            GetComponent<MeshCollider>().sharedMesh = plains;

            Mesh boundaryMesh = new Mesh();
            List<Vector3> boundaryVerts = new List<Vector3>();
            List<int> boundaryindices = new List<int>();
            for (int i = 0; i < boundary.Count; i++)
            {
                int counter = boundaryVerts.Count;

                boundaryVerts.Add(boundary[i]);
                boundaryVerts.Add(boundary[i] + Vector3.up * 10f);

                boundaryindices.Add(counter);
                boundaryindices.Add(counter + 1);
                if (i == boundary.Count - 1)
                {
                    boundaryindices.Add(0);
                    boundaryindices.Add(0);
                }
                else
                {
                    boundaryindices.Add(counter + 2);
                    boundaryindices.Add(counter + 2);
                }

                boundaryindices.Add(counter + 1);
                if (i == boundary.Count - 1)
                    boundaryindices.Add(1);
                else
                    boundaryindices.Add(counter + 3);
            }

            for (int i = 0; i < boundaryVerts.Count; i++)
            {
                //boundaryVerts[i] = boundaryVerts[i] - transform.position; 
            }

            boundaryMesh.SetVertices(boundaryVerts);
            boundaryMesh.SetIndices(boundaryindices.ToArray(), MeshTopology.Triangles, 0);

            boundaryMesh.RecalculateBounds();
            boundaryMesh.RecalculateNormals();
            boundaryMesh.RecalculateTangents();
            boundaryMesh.UploadMeshData(false);

            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = boundaryMesh;
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
        int extrudeStart = curMountVerts - tripleAngles;

        for (int i = curMountVerts - tripleAngles; i < curMountVerts; i += 3)
        {
            Vector3 curMount = mountainVerts[i];
            Vector3 centerToMount = curMount - center;
            Vector3 temp = curMount + (centerToMount.normalized * layerWidth);

            // Triple for hard edges
            mountainVerts.Add(temp);
            mountainVerts.Add(temp);
            mountainVerts.Add(temp);
            mountainUVs.Add(new Vector2(temp.x, temp.z));
            mountainUVs.Add(new Vector2(temp.x, temp.z));
            mountainUVs.Add(new Vector2(temp.x, temp.z));

            if (i >= curMountVerts - 3)
            {
                mountainTris.Add(i);
                mountainTris.Add(i + tripleAngles);
                mountainTris.Add(extrudeStart + 2);

                mountainTris.Add(extrudeStart + 1);
                mountainTris.Add(i + 1 + tripleAngles);
                mountainTris.Add(extrudeStart + 2 + tripleAngles);
            }
            else
            {
                mountainTris.Add(i);
                mountainTris.Add(i + tripleAngles);
                mountainTris.Add(i + 5);

                mountainTris.Add(i + 4);
                mountainTris.Add(i + 1 + tripleAngles);
                mountainTris.Add(i + 5 + tripleAngles);
            }
        }

        Extrude(extrusions - 1);
    }

    private void RandomizeHeights()
    {
        int peakLayer = layers - peakOffset;
        float yRange = yMax - yMin;
        float yIncrement = yRange / peakLayer;
        int startIndex = tripleAngles;
        int endIndex = mountainVerts.Count;

        for (int i = startIndex; i < endIndex; i += 3)
        {
            Vector3 temp = mountainVerts[i];

            int curLayer = i / tripleAngles;

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
            mountainVerts[i + 1] = temp;
            mountainVerts[i + 2] = temp;
        }
    }

    private void CreateCliffs()
    {
        int cliffSides = Random.Range(minCliffSides, maxCliffSides + 1);
        int cliffStart = Random.Range(0, angles) * 3;
        int cliffEnd = cliffStart + (cliffSides * 3);

        for (int i = cliffStart; i < cliffEnd; i += 3)
        {
            for (int k = 1; k < layers; k++)
            {
                Vector3 temp = Vector3.zero;

                if (i >= tripleAngles)
                {
                    temp = mountainVerts[(i - tripleAngles) + (tripleAngles * k)];
                    temp.y = cliffY;
                    mountainVerts[(i - tripleAngles) + (tripleAngles * k)] = temp;
                    mountainVerts[(i - tripleAngles) + 1 + (tripleAngles * k)] = temp;
                    mountainVerts[(i - tripleAngles) + 2 + (tripleAngles * k)] = temp;
                }
                else
                {
                    temp = mountainVerts[i + (tripleAngles * k)];
                    temp.y = cliffY;
                    mountainVerts[i + (tripleAngles * k)] = temp;
                    mountainVerts[i + 1 + (tripleAngles * k)] = temp;
                    mountainVerts[i + 2 + (tripleAngles * k)] = temp;
                }
            }
        }
    }
}
