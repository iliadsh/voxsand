using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using craftinggame.Graphics;

namespace craftinggame.Mechanics
{
    class Chunk
    {
        public Chunk((int x, int y) pos)
        {
            position = pos;
        }

        public (int x, int y) position;
        public ChunkMesh mesh = null;
        public bool needsRemesh = false;

        public byte[,,] blocks = new byte[16, 16, 16];
        public float[] verts;

        public static (int x, int y) PosToChunkPos(float x, float y)
        {
            int ox = x < 0 ? ((int)x + 1) / 16 - 1 : (int)x / 16;
            int oy = y < 0 ? ((int)y + 1) / 16 - 1 : (int)x / 16;
            return (ox, oy);
        }

        public static (int x, int y) PosToChunkOffset(float x, float y)
        {
            int ox = (int)x % 16;
            if (ox < 0) ox += 16;
            int oy = (int)y % 16;
            if (oy < 0) oy += 16;
            return (ox, oy);
        }

        public void GenVertsAsync()
        {
            Task.Run(GenVerts);
        }

        private async Task GenVerts()
        {
            float[] _vertices =
            {
            // Position         Texture coordinates
                 0.5f,  0.5f, 0.0f, 1.0f, 1.0f, // top right
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f,  // top left 
                 0.5f, -0.5f, 0.0f, 1.0f, 0.0f, // bottom right
                -0.5f, -0.5f, 0.0f, 0.0f, 0.0f, // bottom left
                -0.5f,  0.5f, 0.0f, 0.0f, 1.0f  // top left 
            };
            verts = _vertices;
            needsRemesh = true;
        }
    }
}
