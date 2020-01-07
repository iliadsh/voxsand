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

        public static (float u, float v) FaceToTexcoord(byte id, Face face)
        {
            switch (id)
            {
                case 1: 
                    switch (face)
                    {
                        case Face.Top:
                            return Texture.TexPosToAtlasCoord((0, 15));
                        default:
                            return Texture.TexPosToAtlasCoord((3, 15));
                    }
                case 2:
                    return Texture.TexPosToAtlasCoord((2, 14));
                default:
                    return (0, 0);
            }
        }
    }
}
