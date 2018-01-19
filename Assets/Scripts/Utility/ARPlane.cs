using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Contains networked data for each plane
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public struct ARPlane
{
    //Fields
    public string identifier;
    public Vector3 position;
    public float rotation;
    public Vector3 scale;

    /// <summary>
    /// Default Constructor
    /// </summary>
    /// <param name="identifier">Identifier given by ARkit</param>
    /// <param name="position">Position in world</param>
    /// <param name="rotation">Rotation on y-axis</param>
    /// <param name="scale">Scale in world</param>
    public ARPlane(string identifier, Vector3 position, float rotation, Vector3 scale)
    {
        this.identifier = identifier;
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
    }
}

/// <summary>
/// A list of structs to be synced on network
/// </summary>
public class ARPlaneSync : SyncListStruct<ARPlane> { }