using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Gatherables
{

    public class GatherableManager : MonoBehaviour
    {

        [field: SerializeField] public Gatherable GatherablePrefab { get; private set; }

        [Header("Gatherable Pooling")]
        [SerializeField] int initalPoolSize = 100;
        [SerializeField] int poolExtension = 25;

        [SerializeField, ReadOnly] int currentActive = 0;

        private HashSet<Gatherable> activeGatherables = new();
        private Queue<Gatherable> gatherableSystemPool = new();

        private void Start()
        {
            ExtendGatherableObjectPool(initalPoolSize);
        }

        private void ExtendGatherableObjectPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                Gatherable gatherable = Instantiate(GatherablePrefab, Vector3.zero, Quaternion.identity, transform);
                gatherable.gameObject.SetActive(false);
                gatherableSystemPool.Enqueue(gatherable);
            }
        }

        public Gatherable RequestGatherable(GatherableSO gatherableSO, Vector3 pos, Quaternion rot)
        {
            if (!gatherableSystemPool.TryDequeue(out Gatherable gatherable))
            {
                ExtendGatherableObjectPool(poolExtension);
                gatherable = gatherableSystemPool.Dequeue();
            }


            gatherable.transform.SetPositionAndRotation(pos, rot);
            gatherable.AssignUnit(gatherableSO);
            gatherable.gameObject.SetActive(true);
            activeGatherables.Add(gatherable);

            currentActive = activeGatherables.Count;


            return gatherable;
        }

        public void ReturnGatherableToPool(Gatherable gatherable)
        {
            if (!activeGatherables.Contains(gatherable))
            {
                Debug.LogWarning("Invalid Gatherable returned to Gatherable Pool");
                return;
            }

            activeGatherables.Remove(gatherable);

            gatherable.StopAllCoroutines();
            gatherable.gameObject.SetActive(false);
            gatherableSystemPool.Enqueue(gatherable);

            currentActive = activeGatherables.Count;
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void SpawnItemTest()
        {

        }
    }
}

