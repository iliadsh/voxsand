using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
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

        public ConcurrentDictionary<(int x, int z), Chunk> chunks;
        public Player player;

        Thread chunkMeshingThread;
        public ConcurrentQueue<Chunk> meshingQueue = new ConcurrentQueue<Chunk>();

        public Craft(int width, int height, string title)
            : base(width, height, GraphicsMode.Default, title) {}

        protected override void OnLoad(EventArgs e)
        {
            theCraft = this;
            VSync = VSyncMode.Off;
            GL.ClearColor(0.01176470588f, 0.59607843137f, 0.98823529411f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            player = new Player(10, Width / Height);
            chunks = new ConcurrentDictionary<(int x, int z), Chunk>();
            chunkMeshingThread = new Thread(MeshChunks);
            chunkMeshingThread.Start();

            player.CheckChunks();

            CursorVisible = false;

            base.OnLoad(e);
        }

        private void MeshChunks()
        {
            while (true)
            {
                if (meshingQueue.IsEmpty) continue;
                if(meshingQueue.TryDequeue(out Chunk chunk))
                {
                    chunk.GenVerts();
                }
            }
        }

        private bool _firstMove = true;
        private Vector2 _lastPos;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Title = "FPS: " + 1 / e.Time;

            if (!Focused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                Exit();
            }

            const float cameraSpeed = 10.5f;
            const float sensitivity = 0.002f;

            if (input.IsKeyDown(Key.W))
                player.entity.Position += player.camera.Front * cameraSpeed * (float)e.Time; // Forward 
            if (input.IsKeyDown(Key.S))
                player.entity.Position -= player.camera.Front * cameraSpeed * (float)e.Time; // Backwards
            if (input.IsKeyDown(Key.A))
                player.entity.Position -= player.camera.Right * cameraSpeed * (float)e.Time; // Left
            if (input.IsKeyDown(Key.D))
                player.entity.Position += player.camera.Right * cameraSpeed * (float)e.Time; // Right
            if (input.IsKeyDown(Key.Space))
                player.entity.Position += player.camera.Up * cameraSpeed * (float)e.Time; // Up 
            if (input.IsKeyDown(Key.LShift))
                player.entity.Position -= player.camera.Up * cameraSpeed * (float)e.Time; // Down

            if (input.IsKeyDown(Key.W) || input.IsKeyDown(Key.S) || input.IsKeyDown(Key.A) || input.IsKeyDown(Key.D))
                player.CheckChunks();

            // Get the mouse state
            var mouse = Mouse.GetState();

            if (_firstMove) // this bool variable is initially set to true
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                // Calculate the offset of the mouse position
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                // Apply the camera pitch and yaw (we clamp the pitch in the camera class)
                player.entity.Yaw += deltaX * sensitivity;
                player.entity.Pitch -= deltaY * sensitivity; // reversed since y-coordinates range from bottom to top
                player.camera.UpdateVectors();
            }

            base.OnUpdateFrame(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            ChunkMesh.PreludeRender();
            foreach(KeyValuePair<(int x, int z), Chunk> chunk in chunks)
            {
                if (chunk.Value.needsRemesh)
                {
                    if (chunk.Value.mesh != null)
                        chunk.Value.KillMesh();
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

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused) 
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, Width, Height);
            player.camera.AspectRatio = Width / (float)Height;
            base.OnResize(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            chunkMeshingThread.Abort();
            base.OnUnload(e);
        }
    }
}
    