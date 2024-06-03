using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

[RequireComponent(typeof(RotationConstraint))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class HealthBar : MonoBehaviour
{
    [field: SerializeField] public MeshRenderer MeshRender { private set; get; }
    [field: SerializeField] public Vector3 HealthBarOffset { private set; get; } = Vector3.zero;
    [field: SerializeField] public RotationConstraint RotationConstraint { private set; get; }

    private void OnValidate()
    {
        MeshRender = GetComponent<MeshRenderer>();
    }

    public void ToggleHealthBar(bool toggle)
    {
        gameObject.SetActive(toggle);
    }

    public void SetHealthBarValue(float currentValue, float maxValue)
    {
        MeshRender.material.SetFloat("_Fill", currentValue / maxValue);
    }

    //public void UpdateHealthBarPosition(Vector3 position)
    //{
    //    transform.SetPositionAndRotation(position + HealthBarOffset, Quaternion.identity);
    //}
}
