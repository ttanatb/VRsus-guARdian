using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMode : SingletonMonoBehaviour<DebugMode>
{
    public bool IsDebugging
    {
        get { return true; }
    }
}
