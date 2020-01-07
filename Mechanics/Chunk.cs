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
        public Chunk((int x, int z) pos)
        {
            position = pos;
            Task.Run(GenChunk);
        }

        private async Task GenChunk()
        {
            Array.Clear(blocks, 0, blocks.Length);
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    for (int y = 0; y < 40 + 100; y++)
                    {
                        blocks[x, y, z] = 1;
                    }
                }
            }
            await GenVerts();
        }

        public (int x, int z) position;
        public ChunkMesh mesh = null;
        public bool needsRemesh = false;

        public byte[,,] blocks = new byte[16, 256, 16];
        public float[] verts;

        public static (int x, int z) PosToChunkPos(float x, float z)
        {
            int ox = x < 0 ? ((int)x + 1) / 16 - 1 : (int)x / 16;
            int oz = z < 0 ? ((int)z + 1) / 16 - 1 : (int)z / 16;
            return (ox, oz);
        }

        public static (int x, int z) PosToChunkOffset(float x, float z)
        {
            int ox = (int)x % 16;
            if (ox < 0) ox += 16;
            int oz = (int)z % 16;
            if (oz < 0) oz += 16;
            return (ox, oz);
        }

        public void KillMesh()
        {
            if (mesh == null) return;
            mesh.Cleanup();
            mesh = null;
        }

        public void GenVertsAsync()
        {
            Task.Run(GenVerts);
        }

        private async Task GenVerts()
        {
            List<float> outVerts = new List<float>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (blocks[x, y, z] != 0)
                        {
                            if (z == 0 || blocks[x, y, z - 1] == 0)
                            {
                                float[] _front =
                                {
                                    0f + x,  1f + y, 0f + z, 0f, 1f, // top left 
                                    1f + x,  1f + y, 0f + z, 1f, 1f, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 0f + z, 0f, 1f, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  0f + y, 0f + z, 0f, 0f, // bottom left
                                };
                                outVerts.AddRange(_front);
                            }
                            if (z == 15 || blocks[x, y, z + 1] == 0)
                            {
                                float[] _back =
                                {
                                    1f + x,  0f + y, 1f + z, 1f, 0f, // bottom right
                                    1f + x,  1f + y, 1f + z, 1f, 1f, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                    0f + x,  0f + y, 1f + z, 0f, 0f, // bottom left
                                    1f + x,  0f + y, 1f + z, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                };
                                outVerts.AddRange(_back);
                            }
                            if (x == 0 || blocks[x - 1, y, z] == 0)
                            {
                                float[] _left =
                                {
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left
                                    0f + x,  1f + y, 0f + z, 1f, 1f, // top right
                                    0f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  0f + y, 1f + z, 0f, 0f, // bottom left
                                };
                                outVerts.AddRange(_left);
                            }
                            if (x == 15 || blocks[x + 1, y, z] == 0)
                            {
                                float[] _right =
                                {
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    1f + x,  1f + y, 0f + z, 1f, 1f, // top right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                    1f + x,  0f + y, 1f + z, 0f, 0f, // bottom left
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                };
                                outVerts.AddRange(_right);
                            }
                            if (y == 255 || blocks[x, y + 1, z] == 0)
                            {
                                float[] _top =
                                {
                                    1f + x,  1f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 0f + z, 1f, 1f, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                    1f + x,  1f + y, 1f + z, 0f, 0f, // bottom left
                                    1f + x,  1f + y, 0f + z, 1f, 0f, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, // top left 
                                };
                                outVerts.AddRange(_top);
                            }
                            if (y == 0 || blocks[x, y - 1, z] == 0)
                            {
                                float[] _bottom =
                                {
                                    0f + x,  0f + y, 1f + z, 0f, 1f, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 1f, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom 
                                    0f + x,  0f + y, 1f + z, 0f, 1f, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, // bottom right
                                    1f + x,  0f + y, 1f + z, 0f, 0f, // bottom left
                                };
                                outVerts.AddRange(_bottom);
                            }
                        }
                    }
                }
            }
            
            verts = outVerts.ToArray();
            needsRemesh = true;
        }
    }
}
