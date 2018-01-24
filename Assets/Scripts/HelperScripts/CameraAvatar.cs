using UnityEngine;

/// <summary>
/// Keeps a reference to the combat script of the base class
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class CameraAvatar : MonoBehaviour
{
    public Combat RootPlayer
    {
        get
        {
            Transform curr = transform;
            while(curr.parent != null)
            {
                curr = curr.parent;
            }

            return curr.GetComponent<Combat>();
        }
    }
}
