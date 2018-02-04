using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Utility class filled with utility-related stuff
/// 
/// Author: Tanat Boozayaangool
/// </summary>
public class Utility
{

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

    /// <summary>
    /// Creates a new list of vector3 that defines the edges of a rotatable plane
    /// </summary>
    /// <param name="plane">The plane</param>
    /// <returns>The list of vertices</returns>
    public static List<Vector3> CreateVerticesFromPlane(GameObject plane)
    {
        List<Vector3> vertices = new List<Vector3>();

        //add the corners of the plane;
        Vector3 planeScale = plane.transform.localScale / 2f;
        vertices.Add(new Vector3(planeScale.x, 0f, planeScale.z));
        vertices.Add(new Vector3(-planeScale.x, 0f, planeScale.z));
        vertices.Add(new Vector3(-planeScale.x, 0f, -planeScale.z));
        vertices.Add(new Vector3(planeScale.x, 0f, -planeScale.z));

        //loop through to rotate and translate accordingly
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = (Quaternion.AngleAxis(plane.transform.eulerAngles.y, Vector3.up) * vertices[i]) + plane.transform.position;

        return vertices;
    }

    /// <summary>
    /// Gets a random point on a plane (it uses square root so it might be a bit slow)
    /// </summary>
    /// <param name="vertices">List of vertices</param>
    /// <returns></returns>
    static public Vector3 GetRandomPointInPlane(List<Vector3> vertices)
    {
        //create a triangle from the quad
        Vector3[] triangleVertices = new Vector3[3];

        //randomize between two possible triangle
        if (Random.value < 0.5f)
        {
            triangleVertices[0] = vertices[0];
            triangleVertices[1] = vertices[1];
            triangleVertices[2] = vertices[2];
        }
        else
        {
            triangleVertices[0] = vertices[0];
            triangleVertices[1] = vertices[2];
            triangleVertices[2] = vertices[3];
        }

        //calculate a random point within that triangle
        float r1 = Mathf.Sqrt(Random.value);
        float r2 = Random.value;
        return ((1.0f - r1) * triangleVertices[0]) +
            (r1 * (1 - r2) * triangleVertices[1]) +
            (r1 * r2 * triangleVertices[2]);
    }

    /// <summary>
    /// Gets a random point on a plane (doesn't use square root, so it's weighted)
    /// </summary>
    /// <param name="vertices">List of vertices</param>
    /// <returns></returns>
    static public Vector3 GetFastRandomPointInPlane(List<Vector3> vertices)
    {
        //create one of two triangles
        Vector3[] triangleVertices = new Vector3[3];
        if (Random.value < 0.5f)
        {
            triangleVertices[0] = vertices[0];
            triangleVertices[1] = vertices[1];
            triangleVertices[2] = vertices[2];
        }
        else
        {
            triangleVertices[0] = vertices[0];
            triangleVertices[1] = vertices[2];
            triangleVertices[2] = vertices[3];
        }

        //gets a weighted random pos in a triangle
        float r1 = Random.value;
        float r2 = Random.value;
        return ((1.0f - r1) * triangleVertices[0]) +
            (r1 * (1 - r2) * triangleVertices[1]) +
            (r1 * r2 * triangleVertices[2]);
    }

    /// <summary>
    /// Checks if a point is inside of a polygon by checking how many edges it
    /// intersects if you draw a line from the point to the right of the world.
    /// </summary>
    /// <param name="point">Point to check</param>
    /// <param name="vertices">List of vertices that define a polygon</param>
    /// <returns>If the point is in the polygon</returns>
    public static bool CheckIfPointIsInPolygon(Vector3 point, List<Vector3> vertices)
    {
        //Check if it's actually a polygon
        if (vertices.Count < 3) return false;

        int counter = 0;
        Vector3 vertex1 = vertices[0];
        Vector3 vertex2 = vertices[1];

        //loop through the edges
        for (int i = 0; i < vertices.Count; i++)
        {
            //defining edge
            vertex1 = vertices[i];
            if (i == vertices.Count - 1)
                vertex2 = vertices[0];
            else vertex2 = vertices[i + 1];

            //check if the point is within the min and max z-indices
            //checks if the point is not to the right of both vertices
            if ((point.z > Mathf.Min(vertex1.z, vertex2.z)) &&
                (point.z < Mathf.Max(vertex1.z, vertex2.z)) &&
                (point.x < Mathf.Max(vertex1.x, vertex2.x)))
            {
                //this is used to calculate intersecting pt
                Vector3 lowerPoint = vertex1;
                Vector3 higherPoint = vertex2;
                if (lowerPoint.z > higherPoint.z)
                {
                    lowerPoint = vertex2;
                    higherPoint = vertex1;
                }
                
                //calculates intersecting point
                Vector3 intersectingPt = lowerPoint + ((higherPoint - lowerPoint) * ((point.z - lowerPoint.z) / (higherPoint.z - lowerPoint.z)));

                //it intersects if intersecting point is to the right of the point
                if (point.x < intersectingPt.x)
                    counter++;
            }
        }
        
        //return true if odd number of itnersections
        return (counter % 2 != 0);
    }

    /// <summary>
    /// Calculates the distance from a point to a line segment
    /// </summary>
    /// <param name="point">Point</param>
    /// <param name="startingVec">Starting vertex</param>
    /// <param name="endingVec">Ending vertex</param>
    /// <returns></returns>
    public static float DistanceToLineSegment(Vector3 point, Vector3 startingVec, Vector3 endingVec)
    {
        return Mathf.Sqrt(DistanceToLineSegmentSqr(point, startingVec, endingVec));
    }

    /// <summary>
    /// Calculates the square of the distance from a point to a line segment
    /// </summary>
    /// <param name="point">Point</param>
    /// <param name="startingVec">Starting Vertex</param>
    /// <param name="endingVec">Ending Vertex</param>
    /// <returns></returns>
    public static float DistanceToLineSegmentSqr(Vector3 point, Vector3 startingVec, Vector3 endingVec)
    {
        //checks if the distance of the segment is significant enough for the calculation
        float lengthSqr = (startingVec - endingVec).sqrMagnitude;
        if (lengthSqr < 0.0001f)
            return (point - startingVec).sqrMagnitude;

        //project and return distanceSqr
        Vector3 projection = Vector3.Project(point - startingVec, endingVec - startingVec) + startingVec;
        return (point - projection).sqrMagnitude;
    }

    /// <summary>
    /// Checks if a given point is too close to the edge
    /// </summary>
    /// <param name="planeVertices">Plane defined by a list of vertices</param>
    /// <param name="point">The given point to check</param>
    /// <param name="minDistSqr">The point has to be at least further than the square root of this from every edge</param>
    /// <returns></returns>
    public static bool CheckIfTooCloseToEdge(List<Vector3> planeVertices, Vector3 point, float minDistSqr)
    {
        //this isn't a polygon
        if (planeVertices.Count < 2) return false;

        Vector3 vertex1 = planeVertices[0];
        Vector3 vertex2 = planeVertices[1];

        //loops through every edge
        for (int i = 0; i < planeVertices.Count; i++)
        {
            vertex1 = planeVertices[i];
            if (i == planeVertices.Count - 1)
                vertex2 = planeVertices[0];
            else vertex2 = planeVertices[i + 1];

            //calc and checks distacnce to edge
            float distSqr = DistanceToLineSegmentSqr(point, vertex1, vertex2);
            if (distSqr < minDistSqr) return true;
        }
        return false;
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