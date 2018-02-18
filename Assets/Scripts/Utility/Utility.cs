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
        Vector3 planeScale = plane.transform.localScale / 2f;
        List<Vector3> vertices = new List<Vector3>
        {
            //add the corners of the plane;
            new Vector3(-planeScale.x, 0f, planeScale.z),
            new Vector3(planeScale.x, 0f, planeScale.z),
            new Vector3(planeScale.x, 0f, -planeScale.z),
            new Vector3(-planeScale.x, 0f, -planeScale.z)
        };

        //loop through to rotate and translate accordingly
        Quaternion rot = Quaternion.AngleAxis(plane.transform.eulerAngles.y, Vector3.up);
        Vector3 translation = plane.transform.position;
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = (rot * vertices[i]) + translation;

        return vertices;
    }

    /// <summary>
    /// Creates a new list of vector3 that defines the edges of a rotatable plane
    /// </summary>
    /// <param name="plane">The plane</param>
    /// <returns>The list of vertices</returns>
    public static List<Vector3> CreateVerticesFromPlane(Vector3 translation, Vector2 scale, float rotY)
    {
        List<Vector3> vertices = new List<Vector3>
        {
            //add the corners of the plane;
            new Vector3(scale.x, 0f, -scale.y),
            new Vector3(-scale.x, 0f, -scale.y),
            new Vector3(-scale.x, 0f, scale.y),
            new Vector3(scale.x, 0f, scale.y)
        };

        //loop through to rotate and translate accordingly
        Quaternion rot = Quaternion.AngleAxis(rotY, Vector3.up);
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = (rot * vertices[i]) + translation;

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
    public static bool CheckIfTooCloseToEdge(List<Vector3> planeVertices, Vector3 point, float minDist)
    {
        int planeCount = planeVertices.Count;
        //this isn't a polygon
        if (planeCount < 2) return false;

        Vector3 vertex1 = planeVertices[0];
        Vector3 vertex2 = planeVertices[1];
        float minDistSqr = Mathf.Pow(minDist, 2f);

        //loops through every edge
        for (int i = 0; i < planeCount; i++)
        {
            vertex1 = planeVertices[i];
            if (i == planeCount - 1)
                vertex2 = planeVertices[0];
            else vertex2 = planeVertices[i + 1];

            //calc and checks distacnce to edge
            float distSqr = DistanceToLineSegmentSqr(point, vertex1, vertex2);
            if (distSqr < minDistSqr) return true;
        }
        return false;
    }

    public static float GetAreaSqr(List<Vector3> planeVertices)
    {
        return (planeVertices[0] - planeVertices[1]).sqrMagnitude *
            (planeVertices[2] - planeVertices[1]).sqrMagnitude;
    }

    public static List<Vector3> CombinePolygons(List<Vector3> polygon1, List<Vector3> polygon2, float epsilon)
    {
        //hoist some variables that are used a lot
        int polygon1Count = polygon1.Count;
        int polygon2Count = polygon2.Count;
        if (polygon1Count < polygon2Count)
        {
            List<Vector3> temp = polygon1;
            polygon1 = polygon2;
            polygon2 = temp;
        }
        else if (polygon1Count == polygon2Count)
        {
            float polyarea1 = (polygon1[0] - polygon1[1]).sqrMagnitude * (polygon1[0] - polygon1[polygon1Count - 1]).sqrMagnitude;
            float polyarea2 = (polygon2[0] - polygon2[1]).sqrMagnitude * (polygon2[0] - polygon2[polygon1Count - 1]).sqrMagnitude;

            if (polyarea1 < polyarea2)
            {
                List<Vector3> temp = polygon1;
                polygon1 = polygon2;
                polygon2 = temp;
            }
        }

        //check if p1 is contained in p2
        bool contained = true;
        for (int i = 0; i < polygon2Count; i++)
        {
            if (!CheckIfPointIsInPolygon(polygon2[i], polygon1))
            {
                contained = false;
                break;
            }
        }
        if (contained) return polygon1;

        //create the overall list and declare some variables
        List<Vector3> combined = new List<Vector3>();
        Vector3 p1segment1 = polygon1[0];
        Vector3 p1segment2 = polygon1[1];
        Vector3 p2segment1 = polygon2[0];
        Vector3 p2segment2 = polygon2[1];
        Vector3 intersectionPt = Vector3.zero;
        Vector3 closestIntersection = Vector3.one * float.MaxValue;
        int intersectionIndex = -1;

        //iterate through the segments of the first polygon
        int startingi = 0;
        while (CheckIfPointIsInPolygon(polygon1[startingi], polygon2))
        {
            startingi++;
        }

        for (int i = startingi; i - startingi <= polygon1Count; i++)
        {
            //determine the first line segment
            if (i < polygon1Count)
                p1segment1 = polygon1[i];
            else p1segment1 = polygon1[i % polygon1Count];
            if (i + 1 < polygon1Count)
                p1segment2 = polygon1[i + 1];
            else p1segment2 = polygon1[(i + 1) % polygon1Count];

            //track if there was intersection
            closestIntersection = Vector3.one * float.MaxValue;
            intersectionIndex = -1;
            //loop through all the other segments
            for (int j = 0; j < polygon2Count; j++)
            {
                //determine second line segment
                p2segment1 = polygon2[j];
                if (j == polygon2Count - 1)
                    p2segment2 = polygon2[0];
                else p2segment2 = polygon2[j + 1];

                //check if there was an intersection
                bool currDidIntersect = CheckIntersectionBetweenSegments(p1segment1, p1segment2, p2segment1, p2segment2, out intersectionPt);
                if (currDidIntersect)
                {
                    if ((intersectionPt - p1segment1).sqrMagnitude < (closestIntersection - p1segment1).sqrMagnitude)
                    {
                        closestIntersection = intersectionPt;
                        intersectionIndex = j;
                    }
                }
            }

            //if there were no intersections, just add the orig
            if (intersectionIndex == -1)
            {
                if (combined.Contains(p1segment2))
                    break;
                    //return combined;
                else combined.Add(p1segment2);
            }
            else
            {
                if (combined.Contains(closestIntersection))
                    break;
                    //return combined;
                else combined.Add(closestIntersection);

                int index = -1;
                if (intersectionIndex == polygon2Count - 1)
                    index = 0;
                else index = intersectionIndex + 1;

                combined.Add(polygon2[index]);

                List<Vector3> temp = polygon1;
                polygon1 = polygon2;
                polygon2 = temp;

                int tempCount = polygon1Count;
                polygon1Count = polygon2Count;
                polygon2Count = tempCount;

                i = intersectionIndex;
                startingi = i;
            }
        }

        for (int i = 0; i < combined.Count; i++)
        {
            Vector3 vert1 = combined[i];
            int index = i + 1;
            if (index >= combined.Count)
                index -= combined.Count;
            Vector3 vert2 = combined[index];

            if (Mathf.Abs(vert2.x - vert1.x) + Mathf.Abs(vert2.z - vert1.z) < epsilon)
            {
                combined.RemoveAt(index);
                i--;
            }
        }

        return combined;
    }

    /*
    int startingj = j;
    for (j = j + 1; j - startingj <= polygon2Count; j++)
    {
        if (j > polygon2Count - 1)
            p1segment1 = polygon2[j - polygon2Count];
        else p1segment1 = polygon2[j];
        if (j + 1 > polygon2Count - 1)
            p1segment2 = polygon2[j - polygon2Count + 1];
        else p1segment2 = polygon2[j + 1];
        for (int k = 0; k < polygon1Count; k++)
        {
            p2segment1 = polygon1[k];
            if (k == polygon1Count - 1)
                p2segment2 = polygon1[0];
            else p2segment2 = polygon1[k + 1];

            didIntersect = CheckIntersectionBetweenSegments(p1segment1, p1segment2, p2segment1, p2segment2, out intersectionPt);
            if (didIntersect)
            {
                combined.Add(intersectionPt);
                combined.Add(p2segment2);
                i = k + 1;
                break;
            }
        }

        if (!didIntersect)
            combined.Add(p1segment2);
        else break;
    }
    */
    public static bool CheckIntersectionBetweenSegments(Vector3 s1v1, Vector3 s1v2, Vector3 s2v1, Vector3 s2v2, out Vector3 intersectingPos)
    {
        //https://stackoverflow.com/questions/563198/whats-the-most-efficent-way-to-calculate-where-two-line-segments-intersect
        intersectingPos = Vector3.zero;
        Vector3 r = s1v2 - s1v1;
        Vector3 s = s2v2 - s2v1;
        Vector3 qp = s2v1 - s1v1;

        float rxs = (r.x * s.z) - (r.z * s.x);
        float qpxr = (qp.x * r.z) - (qp.z * r.x);

        if (Mathf.Abs(rxs) < 0.0001)
            return false;

        float u = qpxr / rxs;
        float qpxs = (qp.x * s.z) - (qp.z * s.x);
        float t = qpxs / rxs;

        if (u > 0f && u < 1f && t > 0f && t < 1f)
        {
            intersectingPos = s1v1 + (s1v2 - s1v1) * t;
            return true;
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