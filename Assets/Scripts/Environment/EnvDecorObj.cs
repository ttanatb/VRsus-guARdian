using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvDecorObj : MonoBehaviour
{
    public MeshFilter meshFilter;
    public MeshRenderer mrenderer;
    private float radius;

    public float Radius
    {
        get { return radius; }
        set { radius = value; }
    }

    public void Initialize(DecorEnvObj decorEnvObj)
    {
        meshFilter.mesh = decorEnvObj.mesh;
        mrenderer.material = decorEnvObj.mat;
        radius = decorEnvObj.radius;
    }
}
