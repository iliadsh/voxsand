using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using craftinggame.Graphics;

namespace craftinggame.Mechanics
{
    class Chunk
    {
        public static Random rand = new Random();
        public static OpenSimplexNoise noise = new OpenSimplexNoise(rand.Next());
        public static OpenSimplexNoise biomenoise = new OpenSimplexNoise(rand.Next());

        public Chunk((int x, int z) pos)
        {
            position = pos;
        }

        public void GenChunk()
        {
            blocks = new byte[16, 256, 16];
            Array.Clear(blocks, 0, blocks.Length);
            for (int x = 0; x < 16; x++)
            {
                for (int z = 0; z < 16; z++)
                {
                    int high;
                    double noisenum = noise.Evaluate((x + position.x * 16) / 100f, (z + position.z * 16) / 100f);
                    if (noisenum < 0.4 && noisenum > -0.3)
                    {
                        high = CalculateForestNoise(x + position.x * 16, z + position.z * 16);
                        if (noisenum > 0.3)
                        {
                            double delta = (0.4 - noisenum) * 5;
                            high = (int)((0.5 + delta) * high +  (0.5 - delta) * CalculateDesertNoise(x + position.x * 16, z + position.z * 16));
                        }
                        if (noisenum < -0.2)
                        {
                            double delta = Math.Abs(-0.3 - noisenum) * 10;
                            high = (int)((delta) * high + (1 - delta) * CalculateWaterNoise(x + position.x * 16, z + position.z * 16)) + 1;
                        }
                        if (rand.Next(0, 40) > 38)
                        {
                            blocks[x, high, z] = 5;
                        }
                        for (int y = 0; y < high; y++)
                        {
                            byte value = 1;
                            if (y < high - 1)
                            {
                                value = 3;
                            }
                            blocks[x, y, z] = value;
                        }
                    }
                    else if (noisenum < -0.3 || noisenum > 0.8)
                    {
                        high = CalculateWaterNoise(x + position.x * 16, z + position.z * 16);
                        for (int y = 0; y < high; y++)
                        {
                            byte value = 8;
                            if (y < high - 10)
                            {
                                value = 3;
                            }
                            blocks[x, y, z] = value;
                        }
                    }
                    else
                    {
                        high = CalculateDesertNoise(x + position.x * 16, z + position.z * 16);
                        if (noisenum < 0.5)
                        {
                            double delta = (noisenum - 0.4) * 5;
                            high = (int)((0.5 + delta) * high + (0.5 - delta) * CalculateForestNoise(x + position.x * 16, z + position.z * 16));
                        }
                        if (rand.Next(0, 80) > 78)
                        {
                            blocks[x, high, z] = 7;
                        }
                        else if (rand.Next(0, 1000) > 998)
                        {
                            blocks[x, high, z] = 6;
                            blocks[x, high + 1, z] = 6;
                            blocks[x, high + 2, z] = 6;
                        }
                        for (int y = 0; y < high; y++)
                        {
                            byte value = 2;
                            blocks[x, y, z] = value;
                        }
                    }
                }
            }
        }

        public static int CalculateForestNoise(int x, int z)
        {
            return (int)
                (40 * Math.Pow(noise.Evaluate(x / 50f, z / 50f), 3) +
                20 * Math.Pow(noise.Evaluate(x / 25f, z / 25f), 3) +
                10 * Math.Pow(noise.Evaluate(x / 14f, z / 14f), 3) +
                100);
        }
        public static int CalculateDesertNoise(int x, int z)
        {
            return (int)
                (30 * Math.Pow(noise.Evaluate(x / 100f, z / 100f), 3) +
                100);
        }
        public static int CalculateWaterNoise(int x, int z)
        {
            return (int)
                (//30 * Math.Pow(noise.Evaluate(x / 150f, z / 150f), 3) +
                100);
        }

        public (int x, int z) position;
        public ChunkMesh mesh = null;
        public ChunkMesh waterMesh = null;
        public bool needsRemesh = false;

        public byte[,,] blocks = null;
        public float[] verts = null;
        public float[] waterVerts = null;

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
            if (waterMesh == null) return;
            waterMesh.Cleanup();
            waterMesh = null;
        }

        public void GenVerts()
        {
            var pospx = (position.x + 1, position.z);
            Chunk chunkpx = Craft.theCraft.chunks.ContainsKey(pospx) ? Craft.theCraft.chunks[pospx] : null;
            var posnx = (position.x - 1, position.z);
            Chunk chunknx = Craft.theCraft.chunks.ContainsKey(posnx) ? Craft.theCraft.chunks[posnx] : null;
            var pospz = (position.x, position.z + 1);
            Chunk chunkpz = Craft.theCraft.chunks.ContainsKey(pospz) ? Craft.theCraft.chunks[pospz] : null;
            var posnz = (position.x, position.z - 1);
            Chunk chunknz = Craft.theCraft.chunks.ContainsKey(posnz) ? Craft.theCraft.chunks[posnz] : null;

            List<float> outVerts = new List<float>();
            List<float> outWater = new List<float>();
            for (int x = 0; x < 16; x++)
            {
                for (int y = 0; y < 256; y++)
                {
                    for (int z = 0; z < 16; z++)
                    {
                        if (blocks[x, y, z] != 0)
                        {
                            if (Block.GetBlockType(blocks[x, y, z]) == Block.Type.Grass)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] left_front =
                                {
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                };
                                outVerts.AddRange(left_front);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] left_back =
                                {
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                };
                                outVerts.AddRange(left_back);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] right_front =
                                {
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                };
                                outVerts.AddRange(right_front);
                                uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] right_back =
                                {
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                };
                                outVerts.AddRange(right_back);
                                continue;
                            }
                            if (Block.GetBlockType(blocks[x, y, z]) == Block.Type.Liquid)
                            {
                                if ((z == 0 && chunknz != null && chunknz.blocks != null && Block.GetBlockOpacity(chunknz.blocks[x, y, 15]) == Block.Opacity.Transparent && Block.GetBlockType(chunknz.blocks[x, y, 15]) != Block.Type.Liquid) || (z != 0 && Block.GetBlockOpacity(blocks[x, y, z - 1]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y, z - 1]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                    float[] _front =
                                    {
                                        0f + x,  1f + y, 0f + z, 0f, 1f, uv, // top left 
                                        1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  1f + y, 0f + z, 0f, 1f, uv, // top left 
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  0f + y, 0f + z, 0f, 0f, uv, // bottom left
                                    };
                                    outWater.AddRange(_front);
                                }
                                if ((z == 15 && chunkpz != null && chunkpz.blocks != null && Block.GetBlockOpacity(chunkpz.blocks[x, y, 0]) == Block.Opacity.Transparent && Block.GetBlockType(chunkpz.blocks[x, y, 0]) != Block.Type.Liquid) || (z != 15 && Block.GetBlockOpacity(blocks[x, y, z + 1]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y, z + 1]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                    float[] _back =
                                    {
                                        1f + x,  0f + y, 1f + z, 1f, 0f, uv, // bottom right
                                        1f + x,  1f + y, 1f + z, 1f, 1f, uv, // top right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                        0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                        1f + x,  0f + y, 1f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    };
                                    outWater.AddRange(_back);
                                }
                                if ((x == 0 && chunknx != null && chunknx.blocks != null && Block.GetBlockOpacity(chunknx.blocks[15, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(chunknx.blocks[15, y, z]) != Block.Type.Liquid) || (x != 0 && Block.GetBlockOpacity(blocks[x - 1, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x - 1, y, z]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Left);
                                    float[] _left =
                                    {
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left
                                        0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                        0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                        0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    };
                                    outWater.AddRange(_left);
                                }
                                if ((x == 15 && chunkpx != null && chunkpx.blocks != null && Block.GetBlockOpacity(chunkpx.blocks[0, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(chunkpx.blocks[0, y, z]) != Block.Type.Liquid) || (x != 15 && Block.GetBlockOpacity(blocks[x + 1, y, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x + 1, y, z]) != Block.Type.Liquid))
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Right);
                                    float[] _right =
                                    {
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                        1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                        1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    };
                                    outWater.AddRange(_right);
                                }
                                if (y == 255 || Block.GetBlockOpacity(blocks[x, y + 1, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y + 1, z]) != Block.Type.Liquid)
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Top);
                                    float[] _top =
                                    {
                                        1f + x,  1f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                        1f + x,  1f + y, 1f + z, 0f, 0f, uv, // bottom left
                                        1f + x,  1f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    };
                                    outWater.AddRange(_top);
                                }
                                if (y == 0 || Block.GetBlockOpacity(blocks[x, y - 1, z]) == Block.Opacity.Transparent && Block.GetBlockType(blocks[x, y - 1, z]) != Block.Type.Liquid)
                                {
                                    var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Bottom);
                                    float[] _bottom =
                                    {
                                        0f + x,  0f + y, 1f + z, 0f, 1f, uv, // top left 
                                        0f + x,  0f + y, 0f + z, 1f, 1f, uv, // top right
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom 
                                        0f + x,  0f + y, 1f + z, 0f, 1f, uv, // top left 
                                        1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                        1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    };
                                    outWater.AddRange(_bottom);
                                }
                                continue;
                            }
                            if ((z == 0 && chunknz != null && chunknz.blocks != null && Block.GetBlockOpacity(chunknz.blocks[x, y, 15]) == Block.Opacity.Transparent) || (z != 0 && Block.GetBlockOpacity(blocks[x, y, z - 1]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Front);
                                float[] _front =
                                {
                                    0f + x,  1f + y, 0f + z, 0f, 1f, uv, // top left 
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 0f + z, 0f, 1f, uv, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  0f + y, 0f + z, 0f, 0f, uv, // bottom left
                                };
                                outVerts.AddRange(_front);
                            }
                            if ((z == 15 && chunkpz != null && chunkpz.blocks != null && Block.GetBlockOpacity(chunkpz.blocks[x, y, 0]) == Block.Opacity.Transparent) || (z != 15 && Block.GetBlockOpacity(blocks[x, y, z + 1]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Back);
                                float[] _back =
                                {
                                    1f + x,  0f + y, 1f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 1f + z, 1f, 1f, uv, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    1f + x,  0f + y, 1f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                };
                                outVerts.AddRange(_back);
                            }
                            if ((x == 0 && chunknx != null && chunknx.blocks != null && Block.GetBlockOpacity(chunknx.blocks[15, y, z]) == Block.Opacity.Transparent) || (x != 0 && Block.GetBlockOpacity(blocks[x - 1, y, z]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Left);
                                float[] _left =
                                {
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                };
                                outVerts.AddRange(_left);
                            }
                            if ((x == 15 && chunkpx != null && chunkpx.blocks != null && Block.GetBlockOpacity(chunkpx.blocks[0, y, z]) == Block.Opacity.Transparent) || (x != 15 && Block.GetBlockOpacity(blocks[x + 1, y, z]) == Block.Opacity.Transparent))
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Right);
                                float[] _right =
                                {
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                };
                                outVerts.AddRange(_right);
                            }
                            if (y == 255 || Block.GetBlockOpacity(blocks[x, y + 1, z]) == Block.Opacity.Transparent)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Top);
                                float[] _top =
                                {
                                    1f + x,  1f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 0f + z, 1f, 1f, uv, // top right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  1f + y, 1f + z, 0f, 0f, uv, // bottom left
                                    1f + x,  1f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    0f + x,  1f + y, 1f + z, 0f, 1f, uv, // top left 
                                };
                                outVerts.AddRange(_top);
                            }
                            if (y == 0 || Block.GetBlockOpacity(blocks[x, y - 1, z]) == Block.Opacity.Transparent)
                            {
                                var uv = Block.FaceToTexcoord(blocks[x, y, z], Block.Face.Bottom);
                                float[] _bottom =
                                {
                                    0f + x,  0f + y, 1f + z, 0f, 1f, uv, // top left 
                                    0f + x,  0f + y, 0f + z, 1f, 1f, uv, // top right
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom 
                                    0f + x,  0f + y, 1f + z, 0f, 1f, uv, // top left 
                                    1f + x,  0f + y, 0f + z, 1f, 0f, uv, // bottom right
                                    1f + x,  0f + y, 1f + z, 0f, 0f, uv, // bottom left
                                };
                                outVerts.AddRange(_bottom);
                            }
                        }
                    }
                }
            }
            
            verts = outVerts.ToArray();
            waterVerts = outWater.ToArray();
            needsRemesh = true;
        }
    }
}
