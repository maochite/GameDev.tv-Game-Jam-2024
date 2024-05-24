using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField, Header("Player Components")] private Rigidbody rigidBody;

    [Header("Camera Components")]
    //[SerializeField] private CameraFollow camFollow;
    [SerializeField] private Vector3 dirToMouse;

    [Header("Controller Components")]
    [SerializeField, Range(1, 20)] private float inputSmoothing = 8f;

    [Header("Invincibility Component")]
    [SerializeField, Range(0, 20)] float invincibilityDuration = 1.5f;
    float invincibilityTimer = 0f;
    bool isInvincible = false;

    private float movementSpeed = 5;
    private Vector3 raw_input;
    private Vector3 calculated_input;
    private Vector3 world_input;

    private Camera mainCamera;

    private void GatherInput()
    {
        raw_input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        raw_input.Normalize();

        calculated_input = Vector3.Lerp(raw_input, calculated_input, inputSmoothing * Time.deltaTime);
    }

    private void Move()
    {
        rigidBody.MovePosition(rigidBody.position + movementSpeed * Time.deltaTime * calculated_input);
    }

    private void Update()
    {
        GatherInput();
    }

    private void FixedUpdate()
    {
        Move();
    }

}

public static class Helpers
{
    //private static Matrix4x4 _isoMatrix = Matrix4x4.Rotate(Quaternion.Euler(0, 45, 0));
    //public static Vector3 ToIso(this Vector3 input) => _isoMatrix.MultiplyPoint3x4(input);
}