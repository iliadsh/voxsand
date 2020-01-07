using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using craftinggame.Graphics;

namespace craftinggame.Mechanics
{
    public class Player
    {
        public Camera camera;
        public Entity entity;
        public int renderDistance;

        public Player(int renderDistance, float aspectRatio)
        {
            this.renderDistance = renderDistance;
            entity = new Entity(new Vector3(0, 200, 1));
            camera = new Camera(entity, aspectRatio);
        }

        public void CheckChunks()
        {
            var pos = Chunk.PosToChunkPos(entity.Position.X, entity.Position.Z);
            foreach ((int x, int z) chunk in Craft.theCraft.chunks.Keys)
            {
                if (chunk.x > Craft.theCraft.player.renderDistance + pos.x
                    || chunk.x < -Craft.theCraft.player.renderDistance + pos.x
                    || chunk.z > Craft.theCraft.player.renderDistance + pos.z
                    || chunk.z < -Craft.theCraft.player.renderDistance + pos.z)
                {
                    Craft.theCraft.chunks[chunk].KillMesh();
                    if (!Craft.theCraft.chunks.TryRemove(chunk, out Chunk value)) Console.WriteLine("Failed to remove chunk.");
                }
            }
            List<Chunk> needsMeshing = new List<Chunk>();
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                for (int z = -renderDistance; z < renderDistance; z++)
                {
                    (int x, int z) newpos = (pos.x + x, pos.z + z);
                    if(!Craft.theCraft.chunks.ContainsKey(newpos))
                    {
                        Chunk chunk = new Chunk(newpos);

                        var pospx = (newpos.x + 1, newpos.z);
                        Chunk chunkpx = Craft.theCraft.chunks.ContainsKey(pospx) ? Craft.theCraft.chunks[pospx] : null;
                        var posnx = (newpos.x - 1, newpos.z);
                        Chunk chunknx = Craft.theCraft.chunks.ContainsKey(posnx) ? Craft.theCraft.chunks[posnx] : null;
                        var pospz = (newpos.x, newpos.z + 1);
                        Chunk chunkpz = Craft.theCraft.chunks.ContainsKey(pospz) ? Craft.theCraft.chunks[pospz] : null;
                        var posnz = (newpos.x, newpos.z - 1);
                        Chunk chunknz = Craft.theCraft.chunks.ContainsKey(posnz) ? Craft.theCraft.chunks[posnz] : null;

                        Craft.theCraft.chunks[newpos] = chunk;
                        needsMeshing.Add(chunk);
                        if(chunkpx != null && chunkpx.mesh != null)
                        {
                            needsMeshing.Add(chunkpx);
                        }
                        if (chunknx != null && chunknx.mesh != null)
                        {
                            needsMeshing.Add(chunknx);
                        }
                        if (chunkpz != null && chunkpz.mesh != null)
                        {
                            needsMeshing.Add(chunkpz);
                        }
                        if (chunknz != null && chunknz.mesh != null)
                        {
                            needsMeshing.Add(chunknz);
                        }
                    }
                }
            }
            foreach(Chunk chunk in needsMeshing)
            {
                Craft.theCraft.meshingQueue.Enqueue(chunk);
            }
        }
    }
}
