using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Helper class for updating local plane transforms and updating texture tiling
/// </summary>
/// 
public class LocalPlane : MonoBehaviour
{
    #region Fields

    private const float HEIGHT = 2.0f;          //height of tower

    private Vector2 scale = Vector2.one;        //x,z scale of tower

    //TEMP (prototype graphics)
    private Material m;                         //(for updating tiling)
                                                //END TEMP
    private MeshCollider meshCollider;
    private BoxCollider boxCollider;

    [SerializeField]
    private float topPlaneLayerSize = 0.01f;

    [SerializeField]
    private float updateRate = 0.1f;

    [SerializeField]
    private float textureScale = 0.25f;

    private Vector3 totalScale = Vector3.one;        //x,z scale of tower
    private Material matTop;
    private Material matX;
    private Material matY;

    private Vector2 topScale = Vector2.one;        //x,z scale of tower
    private Vector2 xScale = Vector2.one;        //x,z scale of tower
    private Vector2 yScale = Vector2.one;        //x,z scale of tower

    #endregion

    private void Awake()
    {
        meshCollider = GetComponent<MeshCollider>();
        boxCollider = GetComponent<BoxCollider>();
    }

    public void Start()
    {
#if UNITY_IOS
        GetComponent<Renderer>().enabled = false;
#else
        meshCollider.enabled = true;

        Vector3 newCenter = boxCollider.center;
        newCenter.y = -topPlaneLayerSize / 2;

        Vector3 newSize = boxCollider.size;
        newSize.y = topPlaneLayerSize;

        boxCollider.size = newSize;
        boxCollider.center = newCenter;

        boxCollider.enabled = false;
#endif



        Material[] mats = GetComponent<MeshRenderer>().materials;
        //Vector2 offset = new Vector2(Random.value, Random.value);
        //foreach (Material m in mats)
        //    m.SetTextureOffset("_MainTex", offset);

        matTop = mats[0];
        //matX = mats[1];
        //matY = mats[2];

        IEnumerator updateCoroutine = TextureUpdateCoroutine(updateRate);
        StartCoroutine(updateCoroutine);
    }

#if !UNITY_IOS
    private void Update()
    {
        //END TEMP
    }
#endif

    /// <summary>
    /// Updates the plane based off of the plane found in AR
    /// </summary>
    /// <param name="position">Position in space</param>
    /// <param name="rotation">Rotation on y-axis</param>
    /// <param name="scale">Scale of plane</param>
    public void UpdatePos(Vector3 position, float rotation, Vector3 scale)
    {
        //update position
        transform.position = position;

        //update scale (to height)
        //scale.y = HEIGHT;
        transform.localScale = scale;

        //moves tower down (pivot is in the center)
        //transform.Translate(HEIGHT / 2 * Vector3.down);

        //update rotation
        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = rotation;
        transform.rotation = Quaternion.Euler(euler);
    }

    IEnumerator TextureUpdateCoroutine(float waitTime)
    {
        while (true)
        {
            totalScale.x = transform.localScale.x / textureScale;
            totalScale.y = transform.localScale.y / textureScale;
            totalScale.z = transform.localScale.z / textureScale;

            topScale.x = totalScale.x;
            topScale.y = totalScale.z;
            matTop.SetTextureScale("_MainTex", topScale);
            matTop.SetTextureOffset("_MainTex", topScale);

            //xScale.y = totalScale.z;
            //xScale.x = totalScale.y;
            //matX.SetTextureScale("_MainTex", xScale);
            //matX.SetTextureOffset("_MainTex", xScale);
            //
            //yScale.y = totalScale.x;
            //yScale.x = totalScale.y;
            //matY.SetTextureScale("_MainTex", yScale);
            //matY.SetTextureOffset("_MainTex", yScale);

            yield return new WaitForSeconds(waitTime);
        }
    }
}
