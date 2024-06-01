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
    [Serializable]
    public enum Type : byte
    {
        Invalid = 0,
        Air,
        Dirt,
        Grass,
        Dirt2,
        Grass2,
        OrangeDirt,
        OrangeGrass,
        SnowDirt,
        SnowGrass,
        Log,
        Ice,
        Lava,
        DarkStone,
        MossyStone,
        CobbleStone
    }

    public class Face
    {
        //
        // Define texture IDs here (from texture map)
        //
        public enum Texture
        {
            Dirt = 504,
            GrassTop = 469,
            GrassSide = 490,
            GrassBottom = Dirt,

            Dirt2 = 522,
            Grass2Top = 471,
            Grass2Side = 486,
            Grass2Bottom = Dirt2,

            OrangeDirt = 519,
            OrangeGrassTop = 483,
            OrangeGrassSide = 493,
            OrangeGrassBottom = OrangeDirt,

            SnowDirt = 514,
            SnowGrassTop = 313,
            SnowGrassSide = 495,
            SnowGrassBottom = SnowDirt,

            LogTop = 194,
            LogSide = 193,
            LogBottom = LogTop,

            Ice = 165,
            Lava = 126,
            DarkStone = 42,
            MossyStone = 44,
            CobbleStone = 48,
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
        { Type.Grass,  new BlockTextures(Face.Texture.GrassTop, Face.Texture.GrassSide, Face.Texture.GrassBottom) },
        { Type.Dirt2,  new BlockTextures(Face.Texture.Dirt2) },
        { Type.Grass2,  new BlockTextures(Face.Texture.Grass2Top, Face.Texture.Grass2Side, Face.Texture.Grass2Bottom) },
        { Type.OrangeGrass,  new BlockTextures(Face.Texture.OrangeGrassTop, Face.Texture.OrangeGrassSide, Face.Texture.OrangeGrassBottom) },
        { Type.SnowGrass,  new BlockTextures(Face.Texture.SnowGrassTop, Face.Texture.SnowGrassSide, Face.Texture.SnowGrassBottom) },
        { Type.Log,  new BlockTextures(Face.Texture.LogTop, Face.Texture.LogSide, Face.Texture.LogBottom) },
        { Type.Ice,  new BlockTextures(Face.Texture.Ice) },
        { Type.Lava,  new BlockTextures(Face.Texture.Lava) },
        { Type.DarkStone,  new BlockTextures(Face.Texture.DarkStone) },
        { Type.MossyStone,  new BlockTextures(Face.Texture.MossyStone) },
        { Type.CobbleStone,  new BlockTextures(Face.Texture.CobbleStone) },
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

        public BlockTextures(Face.Texture topTex, Face.Texture leftTex, Face.Texture rightTex, Face.Texture frontTex, Face.Texture backTex, Face.Texture bottomTex)
        {
            faceTextures[(int)Face.Pos.Top] = topTex;
            faceTextures[(int)Face.Pos.Left] = leftTex;
            faceTextures[(int)Face.Pos.Right] = rightTex;
            faceTextures[(int)Face.Pos.Front] = frontTex;
            faceTextures[(int)Face.Pos.Back] = backTex;
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
