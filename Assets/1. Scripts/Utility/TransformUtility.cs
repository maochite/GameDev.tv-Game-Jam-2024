using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class TransformUtility
{
    public static Transform FindClosestTransformSqr<T>(Vector3 referencePoint, IEnumerable<T> components) where T : Component
    {
        Transform closestTransform = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (T component in components)
        {
            Transform targetTransform = component.transform; 
            Vector3 displacement = targetTransform.position - referencePoint;
            float distanceSqr = displacement.sqrMagnitude;

            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestTransform = targetTransform;
            }
        }

        return closestTransform;
    }

    public static Transform FindClosestTransformApprox<T>(Vector3 referencePoint, IEnumerable<T> components) where T : Component
    {
        Transform closestTransform = null;
        float closestDistance = Mathf.Infinity;

        foreach (T component in components)
        {
            Transform targetTransform = component.transform;
            float distance = Vector3.Distance(referencePoint, targetTransform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTransform = targetTransform;
            }
        }

        return closestTransform;
    }

}
