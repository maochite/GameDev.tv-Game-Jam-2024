using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor;
using UnityEngine;

public enum Direction
{
    Up, Down, Left, Right, Forward, Backward
}

[ExecuteInEditMode]
[Serializable]
public class Map : MonoBehaviour
{
    [Serializable]
    public struct BlockPos
    {
        [SerializeField] public Vector2Int chunkIdx;
        [SerializeField] public Vector3Int blockIdx;
        public BlockPos(Vector2Int chunkIdx, Vector3Int blockIdx)
        {
            this.chunkIdx = chunkIdx;
            this.blockIdx = blockIdx;
        }
    }

    [SerializeField] private int mapWidth = 1;
    [SerializeField] public readonly int chunkWidth = 16; // TODO: This should be private with an accessor
    [SerializeField] public readonly int chunkHeight = 2; // TODO: This should be private with an accessor
    [SerializeField] private Chunk ChunkPrefab;

    //private List<Tower> towers;
    //private HashSet<(int, int)> occupiedSquares;

    public static Map Instance;

    [SerializeField] private HashSet<BlockPos> buildableBlockSet;
    [SerializeField] private List<BlockPos> buildableBlockList;

    [SerializeField] private Chunk[,] m_chunks;

    // TODO: Perhaps the map should handle the overlay itself
    // make a new component and use it from here
    // then this doesn't need to be accessible
    public List<BlockPos> GetBuildable()
    {
        return buildableBlockList;
    }

    public bool IsBuildable(Vector2Int chunkIdx, Vector3Int blockIdx)
    {
        return IsBuildable(new BlockPos(chunkIdx, blockIdx));
    }

    public bool IsBuildable(BlockPos blockPos)
    {
        return buildableBlockSet.Contains(blockPos);
    }

    public void SetBuildable(Vector2Int chunkIdx, Vector3Int blockIdx, bool shouldBeBuildable)
    {
        SetBuildable(new BlockPos(chunkIdx, blockIdx), shouldBeBuildable);
    }

    public void SetBuildable(BlockPos blockPos, bool shouldBeBuildable)
    {
        if (shouldBeBuildable)
        {
            if (buildableBlockSet.Contains(blockPos))
            {
                return; // Nothing to do
            }

            Chunk chunk = GetChunk(blockPos.chunkIdx);
            Block.Type blockType = GetBlockTypeInDirection(chunk, blockPos.blockIdx, Direction.Up);
            if (blockType != Block.Type.Invalid && blockType != Block.Type.Air)
            {
                // There's something above the selected block
                // Don't mark it as buildable
                return;
            }
            buildableBlockSet.Add(blockPos);
            buildableBlockList.Add(blockPos);
        } else
        {
            if (!buildableBlockSet.Contains(blockPos))
            {
                return; // Nothing to do
            }
            buildableBlockSet.Remove(blockPos);
            buildableBlockList.Remove(blockPos);
        }
    }


    public void OnAfterDeserialize()
    {
        Debug.Log("AA"); 
        Instance = this; 
    }

    private void Awake()
    {
        Instance = this;
        Reset();
        //towers = new();
        //occupiedSquares = new();
    }

    private void Reset()
    {
        buildableBlockSet = new();
        buildableBlockList = new();
        BlockPos blockPos = new()
        {
            chunkIdx = new Vector2Int(0, 0),
            blockIdx = new Vector3Int(1, 2, 4)
        };
        buildableBlockSet.Add(blockPos);
        buildableBlockList.Add(blockPos);

        for (int i = transform.childCount; i > 0; --i)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        m_chunks = new Chunk[mapWidth, mapWidth];

        for (int z = 0; z < mapWidth; ++z)
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                Chunk chunk = Instantiate(ChunkPrefab, new Vector3(x*chunkWidth, 0, z*chunkWidth), Quaternion.identity);
                chunk.Init(new Vector2Int(x, z), chunkWidth, chunkHeight);
                chunk.transform.parent = transform;
                m_chunks[z, x] = chunk;
            }
        }

        for (int z = 0; z < mapWidth; ++z)
        {
            for (int x = 0; x < mapWidth; ++x)
            {
                m_chunks[z, x].BuildMesh();
            }
        }
    }

    public Block.Type GetBlockTypeInDirection(Chunk startChunk, Vector3Int startBlockIdx, Direction direction)
    {
        if (GetBlockInDirection(startChunk, startBlockIdx, direction, out Chunk destChunk, out Vector3Int destBlockIdx))
        {
            return destChunk.m_blocks[destBlockIdx.z, destBlockIdx.y, destBlockIdx.x];
        } else
        {
            return Block.Type.Invalid;
        }
    }

    public bool GetBlockInDirection(Chunk startChunk, Vector3Int startBlockIdx, Direction direction, [MaybeNullWhen(false)] out Chunk destChunk, out Vector3Int destBlockIdx)
    {
        if (!BlockIdxInBounds(startBlockIdx))
        {
            Debug.LogErrorFormat("Invalid starting block index: {0}", startBlockIdx);
            destChunk = null;
            destBlockIdx = default;
            return false;
        }

        switch (direction)
        {
            case Direction.Up:
                destBlockIdx = startBlockIdx + new Vector3Int(0, 1, 0);
                break;
            case Direction.Down:
                destBlockIdx = startBlockIdx + new Vector3Int(0, -1, 0);
                break;
            case Direction.Left:
                destBlockIdx = startBlockIdx + new Vector3Int(-1, 0, 0);
                break;
            case Direction.Right:
                destBlockIdx = startBlockIdx + new Vector3Int(1, 0, 0);
                break;
            case Direction.Forward:
                destBlockIdx = startBlockIdx + new Vector3Int(0, 0, -1);
                break;
            case Direction.Backward:
                destBlockIdx = startBlockIdx + new Vector3Int(0, 0, 1);
                break;
            default:
                Debug.LogErrorFormat("Invalid direction: {0}", direction);
                destChunk = null;
                destBlockIdx = default;
                return false;
        }

        if (BlockIdxInBounds(destBlockIdx))
        {
            destChunk = startChunk;
            return true;
        }

        Vector2Int destChunkIdx;
        switch (direction)
        {
            case Direction.Up:
            case Direction.Down:
                // There is no chunk above or below, so just return false
                destChunk = null;
                destBlockIdx = default;
                return false;
            case Direction.Left:
                destChunkIdx = startChunk.m_chunkIdx + new Vector2Int(-1, 0);
                destBlockIdx.x = chunkWidth-1;
                break;
            case Direction.Right:
                destChunkIdx = startChunk.m_chunkIdx + new Vector2Int(1, 0);
                destBlockIdx.x = 0;
                break;
            case Direction.Forward:
                destChunkIdx = startChunk.m_chunkIdx + new Vector2Int(0, -1);
                destBlockIdx.z = 0;
                break;
            case Direction.Backward:
                destChunkIdx = startChunk.m_chunkIdx + new Vector2Int(0, 1);
                destBlockIdx.z = chunkWidth-1;
                break;
            default:
                Debug.LogErrorFormat("Invalid direction: {0}", direction);
                destChunk = null;
                destBlockIdx = default;
                return false;
        }

        if (ChunkIdxInBounds(destChunkIdx))
        {
            destChunk = m_chunks[destChunkIdx.y, destChunkIdx.x];
            return true;
        } else
        {
            destChunk = null;
            return false;
        }
    }

    public Chunk GetChunk(Vector2Int chunkIdx)
    {
        return m_chunks[chunkIdx.y, chunkIdx.x];
    }

    public bool GetBlock(Vector3 point, [MaybeNullWhen(false)] out Chunk chunk, out Vector3Int blockIdx)
    {
        Vector2Int chunkIdx = new(
            Mathf.FloorToInt(point.x / chunkWidth),
            Mathf.FloorToInt(point.z / chunkWidth)
        );

        //Debug.LogFormat("GetBlock: point={0} chunk={1}", point, chunkIdx);

        if (!ChunkIdxInBounds(chunkIdx))
        {
            chunk = null;
            blockIdx = default;
            return false;
        }

        chunk = m_chunks[chunkIdx.y, chunkIdx.x];

        blockIdx = new(
            Mathf.FloorToInt(point.x) - Mathf.FloorToInt(chunk.transform.position.x),
            Mathf.FloorToInt(point.y),
            Mathf.FloorToInt(point.z) - Mathf.FloorToInt(chunk.transform.position.z)
        );

        return BlockIdxInBounds(blockIdx);
    }

    public bool GetBlock(Vector3 point, [MaybeNullWhen(false)] out BlockPos blockpos)
    {
        blockpos = default;
        if (GetBlock(point, out Chunk chunk, out Vector3Int blockIdx))
        {
            blockpos.chunkIdx = chunk.m_chunkIdx;
            blockpos.blockIdx = blockIdx;
            return true;
        }
        return false;
    }

    private bool ChunkIdxInBounds(Vector2Int chunkIdx)
    {
        return chunkIdx.x >= 0 && chunkIdx.y >= 0 && chunkIdx.x <= mapWidth-1 && chunkIdx.y <= mapWidth-1;
    }

    private bool BlockIdxInBounds(Vector3Int blockIdx)
    {
        return blockIdx.x >= 0 && blockIdx.y >= 0 && blockIdx.z >= 0 && blockIdx.x <= chunkWidth-1 && blockIdx.y <= chunkHeight-1 && blockIdx.z <= chunkWidth-1;
    }
}
