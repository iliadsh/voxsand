using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;
using craftinggame.Graphics;
using craftinggame.Mechanics;

namespace craftinggame
{
    class Craft : GameWindow
    {
        public static Craft theCraft;

        public Dictionary<(int x, int y), Chunk> chunks;
        public Player player;

        public Craft(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title) {}

        protected override void OnLoad(EventArgs e)
        {
            theCraft = this;
            GL.ClearColor(0.01176470588f, 0.59607843137f, 0.98823529411f, 1.0f);
            GL.Enable(EnableCap.DepthTest);

            player = new Player(5, Width / Height);
            chunks = new Dictionary<(int x, int y), Chunk>();

            player.CheckChunks();

            base.OnLoad(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ChunkMesh.PreludeRender();
            foreach(KeyValuePair<(int x, int y), Chunk> chunk in chunks)
            {
                if (chunk.Value.needsRemesh)
                {
                    chunk.Value.mesh = new ChunkMesh(chunk.Value.verts);
                    chunk.Value.needsRemesh = false;
                }
                if (chunk.Value.mesh != null)
                {
                    chunk.Value.mesh.Render(chunk.Key);
                }
            }

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
        }
    }
}
    