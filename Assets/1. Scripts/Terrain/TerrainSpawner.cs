using NaughtyAttributes;
using UnityEngine;
using System.Collections.Generic;
using System;


#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TerrainSpawner : MonoBehaviour
{
    [Serializable]
    public class TerrainSpawn
    {
        public GameObject ObjectToSpawn;
        public int NumberOfObjects = 100;
    }

    public Terrain Terrain;
    public LayerMask AvoidanceLayers;
    public float AvoidanceDistance = 5.0f;
    public int MaxAttempts = 100;

    [Header("Terrain Masks")]
    public List<TerrainSpawn> ForestTerrain;
    public List<TerrainSpawn> JungleTerrain;
    public List<TerrainSpawn> SnowTerrain;
    public List<TerrainSpawn> VolcanicTerrain;

    private TerrainData terrainData;
    private Collider[] terrainColliders;
    private List<List<TerrainSpawn>> terrainSpawnList;

    private Dictionary<List<TerrainSpawn>, string> terrainSpawnDict;

    private void Initialize()
    {
        InitializeTerrainSpawnDict();
        InitializeTerrainSpawnList();
        terrainColliders = new Collider[1000];
    }

    private void InitializeTerrainSpawnDict()
    {
        terrainSpawnDict = new Dictionary<List<TerrainSpawn>, string>
        {
            { ForestTerrain, nameof(ForestTerrain) },
            { JungleTerrain, nameof(JungleTerrain) },
            { SnowTerrain, nameof(SnowTerrain) },
            { VolcanicTerrain, nameof(VolcanicTerrain) }
        };
    }

    private void InitializeTerrainSpawnList()
    {
        terrainSpawnList = new List<List<TerrainSpawn>>
        {
            ForestTerrain,
            JungleTerrain,
            SnowTerrain,
            VolcanicTerrain
        };
    }

    private void SpawnObjects()
    {
        ClearObjects();
        Initialize();

        terrainData = Terrain.terrainData;

        float[,,] alphamaps = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        int width = terrainData.alphamapWidth;
        int height = terrainData.alphamapHeight;

        Vector3 terrainPosition = Terrain.transform.position;

        foreach (List<TerrainSpawn> spawnList in terrainSpawnList)
        {
            int currentLayer = terrainSpawnList.IndexOf(spawnList);

            foreach (TerrainSpawn spawn in spawnList)
            {
                if (currentLayer < 0 || currentLayer >= terrainData.alphamapLayers)
                {
                    Debug.LogError("Invalid terrainLayerIndex. It must be within the range of available terrain layers.");
                    return;
                }

                if (!SpawnObjectsInTerrainLayer(spawn, ref alphamaps, ref width, ref height, ref terrainPosition, ref currentLayer))
                {
                    Debug.LogWarning("TerrainSpawner: Max Attempts Reached in terrain: " + terrainSpawnDict[spawnList].ToString());
                    break;
                }
            }
        }
    }

    private bool SpawnObjectsInTerrainLayer(
        TerrainSpawn spawn, ref float[,,] alphamaps, ref int width, ref int height, ref Vector3 terrainPosition, ref int currentLayer)
    {
        int attempts = 0;
        int spawnedObjects = 0;

        for (int i = 0; i < spawn.NumberOfObjects; i++)
        {
            bool positionFound = false;

            while (!positionFound && attempts < MaxAttempts)
            {
                attempts++;

                int x = UnityEngine.Random.Range(0, width);
                int y = UnityEngine.Random.Range(0, height);

                float alpha = alphamaps[x, y, currentLayer];

                if (UnityEngine.Random.value <= alpha)
                {
                    float worldX = terrainPosition.x + ((float)y / width * terrainData.size.x);
                    float worldZ = terrainPosition.z + ((float)x / height * terrainData.size.z);
                    float worldY = Terrain.SampleHeight(new Vector3(worldX, 0, worldZ)) + terrainPosition.y;

                    Vector3 potentialPosition = new Vector3(worldX, worldY, worldZ);

                    int numColliders = Physics.OverlapSphereNonAlloc(potentialPosition, AvoidanceDistance, terrainColliders, AvoidanceLayers);

                    if (numColliders == 0)
                    {
                        GameObject spawnedObject = Instantiate(spawn.ObjectToSpawn, potentialPosition, Quaternion.identity, transform);
                        positionFound = true;
                        spawnedObjects++;
                    }
                }
            }

            if (attempts == MaxAttempts)
            {
                return false;
            }

            attempts = 0;
        }

        return true;
    }

    private void ClearObjects()
    {
        List<GameObject> childObjects = new List<GameObject>();

        for (int i = 0; i < Terrain.transform.childCount; i++)
        {
            Transform childTransform = Terrain.transform.GetChild(i);
            childObjects.Add(childTransform.gameObject);
        }

        foreach (GameObject childObject in childObjects)
        {
            DestroyImmediate(childObject);
        }

        childObjects.Clear();
        foreach (Transform childTransform in Terrain.transform)
        {
            childTransform.gameObject.SetActive(false);
        }
    }

    [Button(enabledMode: EButtonEnableMode.Always)]
    private void SpawnObjectsButton()
    {
        SpawnObjects();
    }

    [Button(enabledMode: EButtonEnableMode.Always)]
    private void ClearObjectsButton()
    {
        ClearObjects();
    }
}