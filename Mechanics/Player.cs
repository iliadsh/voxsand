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
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                for (int z = -renderDistance; z < renderDistance; z++)
                {
                    var newpos = (pos.x + x, pos.z + z);
                    if(!Craft.theCraft.chunks.ContainsKey(newpos))
                    {
                        Chunk chunk = new Chunk(newpos);
                        Craft.theCraft.chunks[newpos] = chunk;
                    }
                }
            }
        }
    }
}
