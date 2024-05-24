using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block
{
    //
    // Define block types here
    //
    public enum Type : byte
    {
        Invalid = 0,
        Air,
        Dirt,
        Grass,
        Stone,
    }

    public class Face
    {
        //
        // Define texture IDs here (from texture map)
        //
        public enum Texture
        {
            Grass = 182,
            Stone = 227,
            Dirt = 289,
            GrassSides = 319,
        }

        public enum Pos : byte
        {
            Top = 0,
            Left,
            Right,
            Front,
            Back,
            Bottom
        }
    }

    private static Dictionary<Type, BlockTextures> blockTextures = new()
    {
        { Type.Dirt,  new BlockTextures(Face.Texture.Dirt) },
        { Type.Grass, new BlockTextures(Face.Texture.Grass, Face.Texture.GrassSides, Face.Texture.Dirt) },
        { Type.Stone, new BlockTextures(Face.Texture.Stone) },
    };

    private class BlockTextures
    {
        private readonly List<Face.Texture> faceTextures = new() { 0, 0, 0, 0, 0, 0 };

        public BlockTextures(Face.Texture tex)
        {
            faceTextures[(int)Face.Pos.Top] = tex;
            faceTextures[(int)Face.Pos.Left] = tex;
            faceTextures[(int)Face.Pos.Right] = tex;
            faceTextures[(int)Face.Pos.Front] = tex;
            faceTextures[(int)Face.Pos.Back] = tex;
            faceTextures[(int)Face.Pos.Bottom] = tex;
        }

        public BlockTextures(Face.Texture topTex, Face.Texture sidesTex, Face.Texture bottomTex)
        {
            faceTextures[(int)Face.Pos.Top] = topTex;
            faceTextures[(int)Face.Pos.Left] = sidesTex;
            faceTextures[(int)Face.Pos.Right] = sidesTex;
            faceTextures[(int)Face.Pos.Front] = sidesTex;
            faceTextures[(int)Face.Pos.Back] = sidesTex;
            faceTextures[(int)Face.Pos.Bottom] = bottomTex;
        }

        public Face.Texture GetTexture(Face.Pos face)
        {
            return faceTextures[(int)face];
        }
    }

    public static int GetTexUVIdx(Type type, Face.Pos face)
    {
        return (int)blockTextures[type].GetTexture(face);
    }
}
