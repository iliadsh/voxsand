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
                default:
                    return 0;
            }
        }
    }
}
