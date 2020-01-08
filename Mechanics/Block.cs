using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using craftinggame.Graphics;

namespace craftinggame.Mechanics
{
    public class Block
    {
        public enum Face
        {
            Front,
            Back,
            Top,
            Bottom,
            Right,
            Left
        }

        public enum Type
        {
            Solid,
            Transparent,
            Grass,
            Cactus
        }

        public enum Opacity
        {
            Solid,
            Transparent
        }

        public static Type GetBlockType(byte id)
        {
            switch(id)
            {
                case 0:
                    return Type.Transparent;
                case 1:
                case 2:
                case 3:
                case 4:
                    return Type.Solid;
                case 6:
                    return Type.Cactus;
                case 5:
                case 7:
                    return Type.Grass;
                default:
                    return Type.Transparent;
            }
        }

        public static Opacity GetBlockOpacity(byte id)
        {
            switch(id)
            {
                case 0:
                    return Opacity.Transparent;
                case 1:
                case 2:
                case 3:
                case 4:
                    return Opacity.Solid;
                case 5:
                case 6:
                case 7:
                    return Opacity.Transparent;
                default:
                    return Opacity.Transparent;
            }
        }

        public static float FaceToTexcoord(byte id, Face face)
        {
            switch (id)
            {
                case 1: 
                    switch (face)
                    {
                        case Face.Top:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass_top.png");
                        default:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass_side.png");
                    }
                case 2:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\sand.png");
                case 3:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\dirt.png");
                case 4:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\stone.png");
                case 5:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass.png");
                case 6:
                    switch (face)
                    {
                        case Face.Top:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\cactus_top.png");
                        default:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\cactus_side.png");
                    }
                case 7:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\dead_bush.png");
                default:
                    return 0;
            }
        }
    }
}
