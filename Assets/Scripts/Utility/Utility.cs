using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Utility class filled with utility-related stuff
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class Utility {

    /// <summary>
    /// Checks if a pointer (touch/cursor) collides with a UI object
    /// </summary>
    /// <returns>If the pointer is over a UI object</returns>
    static public bool IsPointerOverUIObject()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }
}

/// <summary>
/// The type of player
/// </summary>
public enum PlayerType
{
    AR = 0,
    VR = 1
}

/// <summary>
/// The phase of the game
/// </summary>
public enum GamePhase
{
    Scanning = 0,
    Placing = 1,
    Playing = 2,
    Over = 3
}