using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


[ExecuteInEditMode]
[Serializable]
public class Chunk : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] public Vector2Int m_chunkIdx;
    [SerializeField] private int m_chunkWidth;
    [SerializeField] private int m_chunkHeight;
    [HideInInspector] public Block.Type[,,] m_blocks;

    [SerializeField][HideInInspector] private Block.Type[] m_blocks_serializable;

    public void OnBeforeSerialize()
    {
        m_blocks_serializable = new Block.Type[m_chunkWidth * m_chunkHeight * m_chunkWidth];
        for (int z = 0, i=0; z < m_chunkWidth; ++z)
        {
            for (int y = 0; y < m_chunkHeight; ++y)
            {
                for (int x = 0; x < m_chunkWidth; ++x, ++i)
                {
                    m_blocks_serializable[i] = m_blocks[z, y, x];
                }
            }
        }
    }

    public void OnAfterDeserialize()
    {
        m_blocks = new Block.Type[m_chunkWidth, m_chunkHeight, m_chunkWidth];
        for (int i=0; i<m_blocks_serializable.Length; ++i)
        {
            int x = i % m_chunkWidth;
            int y = (i / m_chunkWidth) % m_chunkHeight;
            int z = i / (m_chunkHeight * m_chunkWidth);
            m_blocks[z, y, x] = m_blocks_serializable[i];
        }
    }

    public void Init(Vector2Int chunkIdx, int chunkWidth, int chunkHeight)
    {
        m_chunkIdx = chunkIdx;
        m_chunkWidth = chunkWidth;
        m_chunkHeight = chunkHeight;
        m_blocks = new Block.Type[m_chunkWidth, m_chunkHeight, m_chunkWidth];
        ClearBlocks();
    }

    public Block.Type GetBlock(Vector3Int blockIdx)
    {
        return m_blocks[blockIdx.z, blockIdx.y, blockIdx.x];
    }

    public void SetBlock(Vector3Int blockIdx, Block.Type blockType)
    {
        m_blocks[blockIdx.z, blockIdx.y, blockIdx.x] = blockType;
    }

    private void ClearBlocks() 
    {
        for (int z = 0; z < m_chunkWidth + 0; ++z)
        {
            for (int y = 0; y < m_chunkHeight; ++y)
            {
                for (int x = 0; x < m_chunkWidth; ++x)
                {
                    if (y == 0)
                    {
                        m_blocks[z, y, x] = Block.Type.Dirt;
                    } else if (y == 1)
                    {
                        m_blocks[z, y, x] = Block.Type.Grass;
                    } else
                    {
                        m_blocks[z, y, x] = Block.Type.Air;
                    }
                }
            }
        }
    }

    public void BuildMesh()
    {
        Mesh mesh = new();

        List<Vector3> verts = new();
        List<int> tris = new();
        List<Vector3> uvs = new();

        Block.Type adjacentType;

        for (int z = 0; z < m_chunkWidth; ++z)
        {
            for (int y = 0; y < m_chunkHeight; ++y)
            {
                for (int x = 0; x < m_chunkWidth; ++x)
                {
                    Block.Type blockType = m_blocks[z, y, x];

                    if (blockType == Block.Type.Air)
                    {
                        continue;
                    }

                    Vector3 blockPos = new(x, y, z);
                    int numFaces = 0;
                    //no land above, build top face
                    if (y == m_chunkHeight - 1 || m_blocks[z, y + 1, x] == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Top);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }

                    // Bottom
                    if (y == 0 || m_blocks[z, y - 1, x] == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Bottom);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }

                    // Front
                    adjacentType = Map.Instance.GetBlockTypeInDirection(this, new Vector3Int(x, y, z), Direction.Forward);
                    if (adjacentType == Block.Type.Invalid || adjacentType == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Front);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }

                    // Right
                    adjacentType = Map.Instance.GetBlockTypeInDirection(this, new Vector3Int(x, y, z), Direction.Right);
                    if (adjacentType == Block.Type.Invalid || adjacentType == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 0));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Right);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }

                    // Back
                    adjacentType = Map.Instance.GetBlockTypeInDirection(this, new Vector3Int(x, y, z), Direction.Backward);
                    if (adjacentType == Block.Type.Invalid || adjacentType == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(1, 0, 1));
                        verts.Add(blockPos + new Vector3(1, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Back);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }

                    // Left
                    adjacentType = Map.Instance.GetBlockTypeInDirection(this, new Vector3Int(x, y, z), Direction.Left);
                    if (adjacentType == Block.Type.Invalid || adjacentType == Block.Type.Air)
                    {
                        verts.Add(blockPos + new Vector3(0, 0, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 1));
                        verts.Add(blockPos + new Vector3(0, 1, 0));
                        verts.Add(blockPos + new Vector3(0, 0, 0));
                        int texIdx = Block.GetTexUVIdx(blockType, Block.Face.Pos.Left);
                        uvs.Add(new Vector3(0, 0, texIdx));
                        uvs.Add(new Vector3(0, 1, texIdx));
                        uvs.Add(new Vector3(1, 1, texIdx));
                        uvs.Add(new Vector3(1, 0, texIdx));
                        numFaces++;
                    }


                    int tl = verts.Count - 4 * numFaces;
                    for (int i = 0; i < numFaces; i++)
                    {
                        tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                    }
                }
            }
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
