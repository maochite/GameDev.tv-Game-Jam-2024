using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtils
{
    private static float defaultMaxDistance = 1f;
    private static int defaultNmAreaMask = NavMesh.GetAreaFromName("Everything");

    public static void SetDefaultMaxDistance(float maxDistance)
    {
        defaultMaxDistance = maxDistance;
    }

    public static void SetDefaultNavmeshAreaMask(int nmAreaMask)
    {
        defaultNmAreaMask = nmAreaMask;
    }

    public static bool NearestPointOnNavmesh(Vector3 startPos, out Vector3 navmeshPos)
    {
        return NearestPointOnNavmesh(startPos, defaultMaxDistance, out navmeshPos);
    }

    public static bool NearestPointOnNavmesh(Vector3 start, float maxDistance, out Vector3 navmeshPos)
    {
        return NearestPointOnNavmesh(start, maxDistance, defaultNmAreaMask, out navmeshPos);
    }

    public static bool NearestPointOnNavmesh(Vector3 start, int nmAreaMask, out Vector3 navmeshPos)
    {
        return NearestPointOnNavmesh(start, defaultMaxDistance, nmAreaMask, out navmeshPos);
    }

    public static bool NearestPointOnNavmesh(Vector3 start, float maxDistance, int nmAreaMask, out Vector3 navmeshPos)
    {
        if (NavMesh.SamplePosition(start, out NavMeshHit navHit, 1f, nmAreaMask))
        {
            navmeshPos = navHit.position;
            return true;
        } else
        {
            navmeshPos = default;
            return false;
        }
    }
}
