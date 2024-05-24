using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AbilityTools
{
    public static Vector3 AlignedVector(float size)
    {
        return new Vector3(size, size, size);
    }

    public static float VectorAverage(Vector3 vector)
    {
        return (vector.x + vector.y + vector.z) / 3;
    }

    public static Vector3 RandomRangeVector(Vector3 minRange, Vector3 maxRange)
    {
        return new Vector3(
        Random.Range(minRange.x, maxRange.x),
        Random.Range(minRange.y, maxRange.y),
        Random.Range(minRange.z, maxRange.z));
    }

    public static Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        return rotation * (point - pivot) + pivot;
    }


    public static Quaternion AnglesToRotation(Vector3 angles)
    {
        Quaternion rot = Quaternion.identity;

        rot *= Quaternion.AngleAxis(angles.z, Vector3.forward);
        rot *= Quaternion.AngleAxis(angles.y, Vector3.up);
        rot *= Quaternion.AngleAxis(angles.x, Vector3.right);

        return rot;
    }
}
