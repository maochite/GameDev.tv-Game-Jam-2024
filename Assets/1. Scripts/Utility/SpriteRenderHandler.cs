using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteRenderHandler : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField] private Vector3 BoundsCenter;
    [SerializeField] private Vector3 BoundsSize;

    private void OnValidate()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        ChangeBounds();
    }

    [Button(enabledMode: EButtonEnableMode.Playmode)]
    private void ChangeBounds()
    {
        spriteRenderer.localBounds = new(BoundsCenter, BoundsSize);
    }
}
