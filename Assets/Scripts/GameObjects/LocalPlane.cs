using UnityEngine;

/// <summary>
/// Helper class for updating local plane transforms and updating texture tiling
/// </summary>
/// 
public class LocalPlane : MonoBehaviour
{
    #region Fields

	private const float HEIGHT = 50.15f;          //height of tower
    
    private Vector2 scale = Vector2.one;        //x,z scale of tower

    //TEMP (prototype graphics)
    private Material m;                         //(for updating tiling)
    //END TEMP
    #endregion

    public void Start()
    {
#if UNITY_IOS
        //shouldn't be seen on AR
        GetComponent<Renderer>().enabled = false;  
#endif
    	m = GetComponent<Renderer>().material;
        m.SetTextureOffset("_MainTex", new Vector2(Random.value, Random.value));
        GetComponent<Renderer>().material = m;
    }

#if !UNITY_IOS
    private void Update()
    {
        //TEMP (prototype graphics)
        scale.x = transform.localScale.x;
        scale.y = transform.localScale.z;

        if (!m)
            m = GetComponent<Renderer>().material;
        m.SetTextureScale("_MainTex", scale / 2.5f);
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
        scale.y = HEIGHT;
        transform.localScale = scale;

        //moves tower down (pivot is in the center)
        transform.Translate(HEIGHT / 2 * Vector3.down);

        //update rotation
        Vector3 euler = transform.rotation.eulerAngles;
        euler.y = rotation;
        transform.rotation = Quaternion.Euler(euler);
    }
}
