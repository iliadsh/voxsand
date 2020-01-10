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
        public const byte AIR = 0;
        public const byte GRASS_BLOCK = 1;
        public const byte SAND = 2;
        public const byte DIRT = 3;
        public const byte STONE = 4;
        public const byte GRASS = 5;
        public const byte CACTUS = 6;
        public const byte DEAD_BUSH = 7;
        public const byte WATER = 8;
        public const byte OAK_LOG = 9;
        public const byte BIRCH_LEAVES = 10;

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
            Leaves,
            Liquid,
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
                case AIR:
                    return Type.Transparent;
                case GRASS_BLOCK:
                case SAND:
                case DIRT:
                case STONE:
                case CACTUS:
                case OAK_LOG:
                    return Type.Solid;
                case WATER:
                    return Type.Liquid;
                case GRASS:
                case DEAD_BUSH:       
                    return Type.Grass;
                case BIRCH_LEAVES:
                    return Type.Leaves;
                default:
                    return Type.Transparent;
            }
        }

        public static Opacity GetBlockOpacity(byte id)
        {
            switch(id)
            {
                case AIR:
                    return Opacity.Transparent;
                case GRASS_BLOCK:
                case SAND:
                case DIRT:
                case STONE:
                case CACTUS:
                case OAK_LOG:
                    return Opacity.Solid;
                case GRASS:
                case DEAD_BUSH:
                case WATER:
                case BIRCH_LEAVES:
                    return Opacity.Transparent;
                default:
                    return Opacity.Transparent;
            }
        }

        public static float FaceToTexcoord(byte id, Face face)
        {
            switch (id)
            {
                case GRASS_BLOCK: 
                    switch (face)
                    {
                        case Face.Top:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass_top.png");
                        default:
                            return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass_side.png");
                    }
                case SAND:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\sand.png");
                case DIRT:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\dirt.png");
                case STONE:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\stone.png");
                case GRASS:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\grass.png");
                case CACTUS:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\cactus.png");
                case DEAD_BUSH:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\dead_bush.png");
                case WATER:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\water_still.png");
                case OAK_LOG:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\oak_log.png");
                case BIRCH_LEAVES:
                    return ChunkMesh.chunkTexture.GetImageIndex("Resources/textures\\birch_leaves.png");
                default:
                    return 0;
            }
        }
    }
}
