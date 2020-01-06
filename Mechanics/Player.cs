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
            entity = new Entity(new Vector3(0, 1, 0));
            camera = new Camera(entity, aspectRatio);
        }

        public void CheckChunks()
        {
            for (int x = -renderDistance; x < renderDistance; x++)
            {
                for (int y = -renderDistance; y < renderDistance; y++)
                {
                    var pos = Chunk.PosToChunkOffset(entity.Position.X, entity.Position.Y);
                    pos.x += x;
                    pos.y += y;
                    if(!Craft.theCraft.chunks.ContainsKey(pos))
                    {
                        Chunk chunk = new Chunk(pos);
                        chunk.GenVertsAsync();
                        Craft.theCraft.chunks[pos] = chunk;
                    }
                }
            }
        }
    }
}
