using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Constructs;
using UnityEngine;

public class ConstructManager : StaticInstance<ConstructManager>
{
    public const float TOWER_SQUARE_SIZE = 1f;

    public enum PreviewState
    {
        NonBuildable,
        Buildable,
        Obstructed,
    }

    readonly Queue<Construct> constructPool = new();
    [SerializeField] private Construct constructPrefab;

    [Header("Pool")]
    [SerializeField] private int initalPoolSize = 25;
    [SerializeField] private int poolExtension = 25;
    [SerializeField, ReadOnly] int poolSize = 0;
    [SerializeField, ReadOnly] int currentActive = 0;

    void Awake()
    {

    }

    void Start()
    {
        //ExtendConstructPool(initalPoolSize);
    }

    public void PlaceConstruct(ConstructSO constructSO, Vector3 pos)
    {
        Construct constructable = RequestConstruct(
            constructSO,
            pos,
            Quaternion.identity);
    }

    private Construct RequestConstruct(ConstructSO constructSO, Vector3 pos, Quaternion rot)
    {
        if (constructPool.Count == 0)
        {
            ExtendConstructPool(poolExtension);
        }

        Construct construct = constructPool.Dequeue();
        construct.name = constructSO.name;

        currentActive++;
        construct.AssignConstruct(constructSO, pos, rot);

        return construct;
    }

    public void ReturnConstructToPool(Construct construct)
    {
        construct.gameObject.SetActive(false);
        constructPool.Enqueue(construct);
        construct.name = constructPrefab.name;
        currentActive--;
    }


    private void ExtendConstructPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Construct construct = Instantiate(constructPrefab, transform);
            construct.gameObject.SetActive(false);
            constructPool.Enqueue(construct);
            poolSize++;
        }
    }
}
