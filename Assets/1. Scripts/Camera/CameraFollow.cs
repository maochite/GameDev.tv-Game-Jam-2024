using Entities;
using NaughtyAttributes.Test;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraFollow : StaticInstance<CameraFollow>
{
    [SerializeField] private LayerMask groundMask;
    public Player target;
    public Camera mainCamera;

    [Header("Camera Controls")] public float MaxCameraDistance = 7.0f;
    public float deadZoneDistance = 5.0f;
    public float minSpeed = 0.3f;
    public float maxSpeed = 1.0f;

    [field: SerializeField, Header("Aim Transform")] public Transform AimLook { get; private set; }
    Vector3 mousePos;
    private void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var (success, position) = GetMousePosition();

        mousePos = success ? position : mousePos;
        //AimLook.position = mousePos;

        Vector3 cursorPosition = mousePos;
        Vector3 directionToCursor = cursorPosition - target.transform.position;
        Vector3 targetPosition = target.transform.position + Vector3.ClampMagnitude(directionToCursor, MaxCameraDistance);

        float targetDistanceFromPlayer = Vector3.Distance(targetPosition, target.transform.position);
        float currentDistanceFromPlayer = Vector3.Distance(transform.position, targetPosition);
        if (targetDistanceFromPlayer > deadZoneDistance || currentDistanceFromPlayer > deadZoneDistance)
        {
            float distanceFromTarget = Mathf.Min(Vector3.Distance(transform.position, targetPosition), MaxCameraDistance);
            if (distanceFromTarget > 0)
            {
                float moveSpeed = minSpeed + (maxSpeed - minSpeed) * (distanceFromTarget / MaxCameraDistance);
                transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            }
        }
        mousePos = cursorPosition;
        AimLook.position = cursorPosition;
    }

    #region aiming
    // credit: https://github.com/BarthaSzabolcs/Tutorial-IsometricAiming/blob/main/Assets/Scripts/Simple%20-%20CopyThis/IsometricAiming.cs
    public Vector3 Aim()
    {
        var (success, position) = GetMousePosition();
        if (success)
        {
            // Calculate the direction
            var direction = position - transform.position;

            // You might want to delete this line.
            // Ignore the height difference.
            direction.y = 0;
            return direction;
        }
        return Vector3.zero;
    }

    public (bool success, Vector3 position) GetMousePosition()
    {
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, groundMask))
        {
            // The Raycast hit something, return with the position.
            return (success: true, position: hitInfo.point);
        }
        else
        {
            // The Raycast did not hit anything.
            return (success: false, position: Vector3.zero);
        }
    }
    #endregion
}

