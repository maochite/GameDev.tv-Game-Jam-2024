using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using Color = UnityEngine.Color;
using System.Drawing;
using Unity.AI.Navigation;

[RequireComponent(typeof(Map))]
[ExecuteInEditMode]
public class MapEditor : MonoBehaviour, ISerializationCallbackReceiver
{
    public enum MapActionType
    {
        AddBlock,
        ReplaceBlock,
        RemoveBlock,
        SetBuildable,
        SetNotBuildable
    }

    public struct MapAction
    {
        public Vector2Int chunkIdx;
        public Vector3Int blockIdx;
        public MapActionType actionType;
        public Block.Type curBlockType;
        public Block.Type newBlockType;

        public MapAction(Vector2Int chunkIdx, Vector3Int blockIdx, MapActionType actionType)
        {
            this.actionType = actionType;
            this.chunkIdx = chunkIdx;
            this.blockIdx = blockIdx;
            this.newBlockType = Block.Type.Invalid;
            this.curBlockType = Block.Type.Invalid;
        }

        public MapAction(Vector2Int chunkIdx, Vector3Int blockIdx, MapActionType actionType, Block.Type curBlockType, Block.Type newBlockType)
        {
            this.actionType = actionType;
            this.chunkIdx = chunkIdx;
            this.blockIdx = blockIdx;
            this.curBlockType = curBlockType;
            this.newBlockType = newBlockType;
        }
    }

    public struct FacePos
    {
        public Map.BlockPos blockPos;
        public Block.Face.Pos face;
    }

    private enum Mode
    {
        MapEdit,
        TileMark
    }

    [SerializeField] private Color buildableOverlayColor = new(0.0f, 0.8f, 0.0f, 0.3f);
    [SerializeField] private Color selectedTileOverlayColor = new(0.0f, 0.8f, 0.8f, 0.7f);
    [SerializeField][ReadOnly] private float maxSelectDistance = 100.0f;
    [SerializeField][ReadOnly] private float tileOverlayOffset = 0.01f;
    [SerializeField] private Block.Type currentBlock = Block.Type.Dirt;
    [SerializeField] private Mode currentMode = Mode.MapEdit;


    [SerializeField][ReadOnly] private Material overlayMaterial;
    [SerializeField][ReadOnly] private bool isFaceSelected;
    [SerializeField][ReadOnly] private FacePos faceSelected;

    [SerializeReference] private Map map;
    [SerializeReference] private NavMeshSurface navmeshSurface;

    private Dictionary<KeyCode, Action> KeyMap;

    public static MapEditor Instance;
    [SerializeField][HideInInspector] private bool initialised = false;

    private void Reset()
    {
        if (!initialised)
        {
            map = GetComponent<Map>();
            navmeshSurface = GetComponent<NavMeshSurface>();
            isFaceSelected = false;
            faceSelected = new();

            initKeyMap();

            Shader shader = Shader.Find("Hidden/Internal-Colored");

            overlayMaterial = new(shader)
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            // Turn on alpha blending
            overlayMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            overlayMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            overlayMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            overlayMaterial.SetInt("_ZWrite", 0);

            map.ResetMap();
            navmeshSurface.BuildNavMesh();
        }
    }

    //[Button]
    //private void SaveMap()
    //{
    //}

    private void initKeyMap()
    {
        KeyMap = new()
        {
            { KeyCode.E, () => HandleChangeMode(Mode.MapEdit) },
            { KeyCode.Q, () => HandleChangeMode(Mode.TileMark) },
            { KeyCode.Alpha1, () => SetCurrentBlock(Block.Type.Dirt) },
            { KeyCode.Alpha2, () => SetCurrentBlock(Block.Type.Grass) },
            { KeyCode.Alpha3, () => SetCurrentBlock(Block.Type.Stone) },
        };
    }

    public void OnBeforeSerialize()
    {
        // Nothing to do
    }

    public void OnAfterDeserialize()
    {
        initKeyMap();
        Instance = this;
    }

    public void HandleClick(Ray ray, bool shiftHeld)
    {
        // Raycast in direction from editor camera to mouse click position
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxSelectDistance))
        {
            return;
        }

        MapActionType actionType = MapActionType.AddBlock;
        if (currentMode == Mode.MapEdit)
        {
            actionType = shiftHeld ? MapActionType.RemoveBlock: MapActionType.AddBlock;
        } else if (currentMode == Mode.TileMark)
        {
            actionType = shiftHeld ? MapActionType.SetNotBuildable : MapActionType.SetBuildable;
        }

        Vector3 targetPoint = hitInfo.point;
        Block.Type newBlockType = currentBlock;
        switch (actionType)
        {
            case MapActionType.AddBlock:
                // Move point away from the target block
                targetPoint -= ray.direction * 0.01f;
                break;
            case MapActionType.ReplaceBlock:
                // Move point inside the block to be replaced
                targetPoint += ray.direction * 0.01f;
                break;
            case MapActionType.RemoveBlock:
                // Move point inside the block to be removed
                targetPoint += ray.direction * 0.01f;
                break;
            case MapActionType.SetBuildable:
                // Move point inside the block
                targetPoint += ray.direction * 0.01f;
                break;
            case MapActionType.SetNotBuildable:
                // Move point inside the block
                targetPoint += ray.direction * 0.01f;
                break;
        }

        if (!map.GetBlock(targetPoint, out Chunk chunk, out Vector3Int blockIdx))
        {
            return;
        }
        Vector2Int chunkIdx = chunk.m_chunkIdx;

        switch (actionType)
        {
            case MapActionType.AddBlock:
                if (Map.Instance.GetBlockInDirection(chunk, blockIdx, Direction.Down, out Chunk chunkBelow, out Vector3Int blockIdxBelow))
                {
                    if (Map.Instance.IsBuildable(chunkBelow.m_chunkIdx, blockIdxBelow))
                    {
                        // Block below the one being added is marked as buildable so remove that first
                        HandleMapAction(new MapAction(chunkBelow.m_chunkIdx, blockIdxBelow, MapActionType.SetNotBuildable));
                    }
                }
                break;
            case MapActionType.ReplaceBlock:
                break;
            case MapActionType.RemoveBlock:
                if (Map.Instance.IsBuildable(chunkIdx, blockIdx))
                {
                    // Block to be removed is marked buildable so remove that first
                    HandleMapAction(new MapAction(chunkIdx, blockIdx, MapActionType.SetNotBuildable));
                }
                newBlockType = Block.Type.Air;
                break;
            case MapActionType.SetBuildable:
                break;
            case MapActionType.SetNotBuildable:
                break;
        }

        HandleMapAction(new MapAction(chunkIdx, blockIdx, actionType, currentBlock, newBlockType));
    }

    public void SetCurrentBlock(Block.Type block)
    {
        // TODO: Track for undo and stuff
        currentBlock = block;
    }

    // Returns true if the event is handled
    public bool HandleKeyDown(KeyCode key)
    {
        if (!KeyMap.TryGetValue(key, out Action keyCallback))
        {
            return false;
        }

        keyCallback();

        return true;
    }

    // Returns true if the event is handled
    public bool HandleKeyUp(KeyCode key)
    {
        return KeyMap.ContainsKey(key);
    }

    public Vector3 GetFaceCentrePos(Vector2Int chunkIdx, Vector3 blockIdx, Block.Face.Pos face)
    {
        Vector3 cubePos = new(
            (chunkIdx.x * map.chunkWidth) + blockIdx.x,
            (chunkIdx.y * map.chunkWidth) + blockIdx.y,
            blockIdx.z
        );

        return GetFaceCentrePos(cubePos, face);
    }

    public Vector3 GetFaceCentrePos(Vector3 cubePos, Block.Face.Pos face)
    {
        switch(face)
        {
            case Block.Face.Pos.Front:
                cubePos.x += 0.5f; cubePos.y += 0.5f;
                break;
            case Block.Face.Pos.Back:
                cubePos.x += 0.5f; cubePos.y += 0.5f; cubePos.z += 1.0f;
                break;
            case Block.Face.Pos.Left:
                cubePos.y += 0.5f; cubePos.z += 0.5f;
                break;
            case Block.Face.Pos.Right:
                cubePos.x += 1.0f; cubePos.y += 0.5f; cubePos.z += 0.5f;
                break;
            case Block.Face.Pos.Bottom:
                cubePos.x += 0.5f; cubePos.z += 0.5f;
                break;
            case Block.Face.Pos.Top:
                cubePos.x += 0.5f; cubePos.y += 1.0f; cubePos.z += 0.5f;
                break;
        }

        return cubePos;
    }

    public List<Vector3> GetFaceCorners(Vector2Int chunkIdx, Vector3 blockIdx, Block.Face.Pos face)
    {
        return GetFaceCorners(chunkIdx, blockIdx, face, 0.0f);
    }

    public List<Vector3> GetFaceCorners(Vector2Int chunkIdx, Vector3 blockIdx, Block.Face.Pos face, float offset)
    {
        Vector3 cubePos = new(
            (chunkIdx.x * map.chunkWidth) + blockIdx.x,
            (chunkIdx.y * map.chunkWidth) + blockIdx.y,
            blockIdx.z
        );

        return GetFaceCorners(cubePos, face, offset);
    }

    public List<Vector3> GetFaceCorners(Vector3 cubePos, Block.Face.Pos face)
    {
        return GetFaceCorners(cubePos, face, 0.0f);
    }

    public List<Vector3> GetFaceCorners(Vector3 cubePos, Block.Face.Pos face, float offset)
    {
        // [0] BotLeft
        // [1] TopLeft
        // [2] TopRight
        // [3] BotRight
        List<Vector3> corners = new(6);
        switch (face)
        {
            case Block.Face.Pos.Front:
                corners.Add(cubePos + new Vector3(0, 0, -offset));
                corners.Add(cubePos + new Vector3(0, 1, -offset));
                corners.Add(cubePos + new Vector3(1, 1, -offset));
                corners.Add(cubePos + new Vector3(1, 0, -offset));
                break;
            case Block.Face.Pos.Back:
                corners.Add(cubePos + new Vector3(1, 0, 1+offset));
                corners.Add(cubePos + new Vector3(1, 1, 1+offset));
                corners.Add(cubePos + new Vector3(0, 1, 1+offset));
                corners.Add(cubePos + new Vector3(0, 0, 1+offset));
                break;
            case Block.Face.Pos.Left:
                corners.Add(cubePos + new Vector3(-offset, 0, 1));
                corners.Add(cubePos + new Vector3(-offset, 1, 1));
                corners.Add(cubePos + new Vector3(-offset, 1, 0));
                corners.Add(cubePos + new Vector3(-offset, 0, 0));
                break;
            case Block.Face.Pos.Right:
                corners.Add(cubePos + new Vector3(1+offset, 0, 0));
                corners.Add(cubePos + new Vector3(1+offset, 1, 0));
                corners.Add(cubePos + new Vector3(1+offset, 1, 1));
                corners.Add(cubePos + new Vector3(1+offset, 0, 1));
                break;
            case Block.Face.Pos.Bottom:
                corners.Add(cubePos + new Vector3(0, -offset, 1));
                corners.Add(cubePos + new Vector3(0, -offset, 0));
                corners.Add(cubePos + new Vector3(1, -offset, 0));
                corners.Add(cubePos + new Vector3(1, -offset, 1));
                break;
            case Block.Face.Pos.Top:
                corners.Add(cubePos + new Vector3(0, 1+offset, 0));
                corners.Add(cubePos + new Vector3(0, 1+offset, 1));
                corners.Add(cubePos + new Vector3(1, 1+offset, 1));
                corners.Add(cubePos + new Vector3(1, 1+offset, 0));
                break;
        }

        return corners;
    }

    public void DrawOutline(FacePos facePos, Color fillColor)
    {
        DrawOutline(facePos.blockPos.chunkIdx, facePos.blockPos.blockIdx, facePos.face, fillColor);
    }

    public void DrawOutline(Map.BlockPos blockPos, Block.Face.Pos face, Color fillColor)
    {
        DrawOutline(blockPos.chunkIdx, blockPos.blockIdx, face, fillColor);
    }

    public void DrawOutline(Vector2Int chunkIdx, Vector3 blockIdx, Block.Face.Pos face, Color color)
    {
        // TODO: Replace this stuff with the getvertexnormal thing?
        List<Vector3> vertices = GetFaceCorners(chunkIdx, blockIdx, face, tileOverlayOffset);

        overlayMaterial.SetPass(0);

        // Set transformation matrix for drawing to
        // match our transform
        Matrix4x4 transformationMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

        // Apply the transformation matrix
        GL.PushMatrix();
        GL.MultMatrix(transformationMatrix);

        // Outline
        GL.Begin(GL.LINES);
        GL.Color(color);
        for (int i = 0; i < 4; i++)
        {
            int nextIndex = (i + 1) % 4;
            GL.Vertex(vertices[i]);
            GL.Vertex(vertices[nextIndex]);
        }
        GL.End();

        GL.PopMatrix();
    }

    public void DrawFace(FacePos facePos, Color color)
    {
        DrawFace(facePos.blockPos.chunkIdx, facePos.blockPos.blockIdx, facePos.face, color);
    }

    public void DrawFace(Map.BlockPos blockPos, Block.Face.Pos face, Color color)
    {
        DrawFace(blockPos.chunkIdx, blockPos.blockIdx, face, color);
    }

    public void DrawFace(Vector2Int chunkIdx, Vector3 blockIdx, Block.Face.Pos face, Color color)
    {
        // TODO: Replace this stuff with the getvertexnormal thing?
        List<Vector3> vertices = GetFaceCorners(chunkIdx, blockIdx, face, tileOverlayOffset);

        overlayMaterial.SetPass(0);

        // Set transformation matrix for drawing to
        // match our transform
        Matrix4x4 transformationMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, new Vector3(1, 1, 1));

        // Apply the transformation matrix
        GL.PushMatrix();
        GL.MultMatrix(transformationMatrix);

        // Fill
        GL.Begin(GL.QUADS);
        GL.Color(color);
        for (int i = 0; i < 4; i++)
        {
            GL.Vertex(vertices[i]);
        }
        GL.End();

        GL.PopMatrix();
    }

    public void UpdateSelection(Ray ray)
    {
        isFaceSelected = false; // Disable by default

        // Raycast in direction from editor camera to mouse click position
        if (!Physics.Raycast(ray, out RaycastHit hitInfo, maxSelectDistance))
        {
            return;
        }

        Vector3 targetPoint = hitInfo.point;

        // Move point inside the selected block
        if (!map.GetBlock(targetPoint + ray.direction * 0.01f, out Chunk chunk, out Vector3Int blockIdx))
        {
            return;
        }
        Vector2Int chunkIdx = chunk.m_chunkIdx;

        Vector3 cubePos = new(
            (chunkIdx.x * map.chunkWidth) + blockIdx.x,
            (chunkIdx.y * map.chunkWidth) + blockIdx.y,
            blockIdx.z
        );

        Block.Face.Pos closestFace = Block.Face.Pos.Front;
        float closestFaceDist = float.MaxValue;
        foreach (Block.Face.Pos face in Enum.GetValues(typeof(Block.Face.Pos)))
        {
            float dist = Vector3.Distance(targetPoint, GetFaceCentrePos(cubePos, face));
            if (dist < closestFaceDist)
            {
                closestFace = face;
                closestFaceDist = dist;
            }
        }

        faceSelected.blockPos.chunkIdx = chunkIdx;
        faceSelected.blockPos.blockIdx = blockIdx;
        faceSelected.face = closestFace;
        isFaceSelected = true;
    }

    public void DrawOverlay()
    {
        if (currentMode == Mode.MapEdit)
        {
            if (isFaceSelected)
            {
                DrawOutline(faceSelected, selectedTileOverlayColor);
            }
        }
        else if (currentMode == Mode.TileMark)
        {
            if (isFaceSelected)
            {
                DrawOutline(faceSelected.blockPos, Block.Face.Pos.Top, selectedTileOverlayColor);
            }

            foreach (Map.BlockPos blockPos in Map.Instance.GetBuildable())
            {
                DrawFace(blockPos, Block.Face.Pos.Top, buildableOverlayColor);
            }
        }
    }

    private void HandleChangeMode(Mode mode)
    {
        if (mode == currentMode)
        {
            return;
        }

        currentMode = mode;

        Debug.LogFormat("Changed mode to {0}", mode);
    }

    private void HandleMapAction(MapAction action)
    {
        Chunk chunk = map.GetChunk(action.chunkIdx);

        switch (action.actionType)
        {
            case MapActionType.AddBlock:
                Debug.LogFormat("Action: Add     chunk [{0,2},{1,2}] block [{2,2},{3,2},{4,2}]", action.chunkIdx.x, action.chunkIdx.y, action.blockIdx.x, action.blockIdx.y, action.blockIdx.z);
                chunk.SetBlock(action.blockIdx, action.newBlockType);
                chunk.BuildMesh();
                navmeshSurface.BuildNavMesh();
                break;
            case MapActionType.ReplaceBlock:
                Debug.LogFormat("Action: Replace chunk [{0,2},{1,2}] block [{2,2},{3,2},{4,2}]", action.chunkIdx.x, action.chunkIdx.y, action.blockIdx.x, action.blockIdx.y, action.blockIdx.z);
                chunk.SetBlock(action.blockIdx, action.newBlockType);
                chunk.BuildMesh();
                navmeshSurface.BuildNavMesh();
                break;
            case MapActionType.RemoveBlock:
                Debug.LogFormat("Action: Delete  chunk [{0,2},{1,2}] block [{2,2},{3,2},{4,2}]", action.chunkIdx.x, action.chunkIdx.y, action.blockIdx.x, action.blockIdx.y, action.blockIdx.z);
                chunk.SetBlock(action.blockIdx, action.newBlockType);
                chunk.BuildMesh();
                navmeshSurface.BuildNavMesh(); 
                break;
            case MapActionType.SetBuildable:
                Debug.LogFormat("Action: Build ON  chunk [{0,2},{1,2}] block [{2,2},{3,2},{4,2}]", action.chunkIdx.x, action.chunkIdx.y, action.blockIdx.x, action.blockIdx.y, action.blockIdx.z);
                Map.Instance.SetBuildable(action.chunkIdx, action.blockIdx, true);
                break;
            case MapActionType.SetNotBuildable:
                Debug.LogFormat("Action: Build OFF chunk [{0,2},{1,2}] block [{2,2},{3,2},{4,2}]", action.chunkIdx.x, action.chunkIdx.y, action.blockIdx.x, action.blockIdx.y, action.blockIdx.z);
                Map.Instance.SetBuildable(action.chunkIdx, action.blockIdx, false);
                break;
        }
    }
}
