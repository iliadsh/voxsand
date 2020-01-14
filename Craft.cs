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
        public static float globalTime = 0;

        public ConcurrentDictionary<(int x, int z), Chunk> chunks;
        public Player player;
        public Skybox skybox;

        Thread chunkMeshingThread;
        public ConcurrentQueue<Chunk> meshingQueue = new ConcurrentQueue<Chunk>();
        public Vector3 sun = new Vector3();

        bool escaped = false;

        public Craft(int width, int height, string title)
            : base(width, height, new GraphicsMode(new ColorFormat(8, 8, 8, 8), 24, 8, 4), title) {}

        protected override void OnLoad(EventArgs e)
        {
            theCraft = this;
            VSync = VSyncMode.Off;
            //WindowState = WindowState.Fullscreen;
            GL.ClearColor(0.5f, 0.5f, 0.7f, 1.0f);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //GL.Enable(EnableCap.FramebufferSrgb);

            player = new Player(10, Width / Height);
            chunks = new ConcurrentDictionary<(int x, int z), Chunk>();
            skybox = new Skybox();
            ShadowMap.Setup();
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
                    if (chunk.blocks == null)
                    {
                        chunk.GenChunk();
                        continue;
                    }
                    chunk.GenVerts();
                }
            }
        }

        private bool _firstMove = true;
        private Vector2 _lastPos;

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            Title = "FPS: " + (int)(1 / e.Time);

            if (!Focused) // check to see if the window is focused
            {
                return;
            }

            KeyboardState input = Keyboard.GetState();

            if (input.IsKeyDown(Key.Escape))
            {
                CursorVisible = true;
                escaped = true;
            }

            if (escaped)
            {
                return;
            }

            if (input.IsKeyDown(Key.G))
            {
                Console.WriteLine($"[DEBUG]: pX={player.entity.Position.X} pY={player.entity.Position.Y} pZ={player.entity.Position.Z} \n\t Pi={player.entity.Pitch} Ya={player.entity.Yaw} \n\t Dx={player.camera.Front.X} Dy={player.camera.Front.Y} Dz={player.camera.Front.Z}");
            }

            const float cameraSpeed = 25.5f;
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
            globalTime += (float)e.Time;
            player.camera.view = player.camera.GetViewMatrix();
            player.camera.projection = player.camera.GetProjectionMatrix();

            GL.Disable(EnableCap.CullFace);
            ShadowMap.PreludeRender();
            RenderChunks(ShadowMap.shadowShader);

            GL.Enable(EnableCap.CullFace);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, Width, Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, ShadowMap.depthMap);

            ChunkMesh.PreludeRender();
            ChunkMesh.PreludeNormalRender();
            RenderChunks(ChunkMesh.chunkShader);

            ChunkMesh.PreludeWaterRender();
            RenderWater();

            Skybox.PreludeRender();
            skybox.Render();

            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        void RenderChunks(Shader shader)
        {
            foreach (KeyValuePair<(int x, int z), Chunk> chunk in chunks)
            {
                if (chunk.Value.needsRemesh)
                {
                    if (chunk.Value.mesh != null)
                        chunk.Value.KillMesh();

                    chunk.Value.mesh = new ChunkMesh(chunk.Value.verts);
                    chunk.Value.verts = null;

                    chunk.Value.waterMesh = new ChunkMesh(chunk.Value.waterVerts);
                    chunk.Value.waterVerts = null;

                    chunk.Value.needsRemesh = false;
                }
                if (chunk.Value.mesh != null)
                {
                    var chunkPos = chunk.Value.position;

                    var p1 = new Vector3(chunkPos.x * 16, 0, chunkPos.z * 16);
                    var p1ss = new Vector4(p1, 1.0f) * player.camera.view * player.camera.projection;

                    var p2 = new Vector3(chunkPos.x * 16 + 16, 0, chunkPos.z * 16);
                    var p2ss = new Vector4(p2, 1.0f) * player.camera.view * player.camera.projection;

                    var p3 = new Vector3(chunkPos.x * 16, 0, chunkPos.z * 16 + 16);
                    var p3ss = new Vector4(p3, 1.0f) * player.camera.view * player.camera.projection;

                    var p4 = new Vector3(chunkPos.x * 16 + 16, 0, chunkPos.z * 16 + 16);
                    var p4ss = new Vector4(p4, 1.0f) * player.camera.view * player.camera.projection;

                    var p11 = new Vector3(chunkPos.x * 16, 256, chunkPos.z * 16);
                    var p11ss = new Vector4(p11, 1.0f) * player.camera.view * player.camera.projection;

                    var p12 = new Vector3(chunkPos.x * 16 + 16, 256, chunkPos.z * 16);
                    var p12ss = new Vector4(p12, 1.0f) * player.camera.view * player.camera.projection;

                    var p13 = new Vector3(chunkPos.x * 16, 256, chunkPos.z * 16 + 16);
                    var p13ss = new Vector4(p13, 1.0f) * player.camera.view * player.camera.projection;

                    var p14 = new Vector3(chunkPos.x * 16 + 16, 256, chunkPos.z * 16 + 16);
                    var p14ss = new Vector4(p14, 1.0f) * player.camera.view * player.camera.projection;

                    if (p1ss.Z < 0 && p2ss.Z < 0 && p3ss.Z < 0 && p3ss.Z < 0 && p11ss.Z < 0 && p12ss.Z < 0 && p13ss.Z < 0 && p13ss.Z < 0) continue;

                    chunk.Value.mesh.Render(chunk.Key, shader);
                }
            }
        }

        void RenderWater()
        {
            foreach (KeyValuePair<(int x, int z), Chunk> chunk in chunks)
            {
                if (chunk.Value.waterMesh != null)
                {
                    chunk.Value.waterMesh.Render(chunk.Key, ChunkMesh.chunkWaterShader);
                }
            }
        }

        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (Focused && !escaped) 
            {
                Mouse.SetPosition(X + Width / 2f, Y + Height / 2f);
            }

            base.OnMouseMove(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (escaped)
            {
                escaped = false;
                CursorVisible = false;
                return;
            }
            Physics.Raycast(player.entity.Position, player.camera.Front, 10f, (x, y, z, face) =>
            {
                if (y < 0 || y > 255) return false;
                var type = Block.GetBlockType(GetBlock(x, y, z));
                if (type == Block.Type.Transparent || type == Block.Type.Liquid) return false;
                if (e.Button == MouseButton.Left)
                {
                    SetBlock(x, y, z, Block.AIR);
                }
                else if (e.Button == MouseButton.Right)
                {
                    Vector3 vec = new Vector3(x, y, z);
                    vec += face;
                    if (vec.Y < 0 || vec.Y > 255) return true;
                    SetBlock((int)vec.X, (int)vec.Y, (int)vec.Z, Block.OAK_LOG);
                }
                else if (e.Button == MouseButton.Middle)
                {
                    Vector3 vec = new Vector3(x, y, z);
                    vec += face;
                    if (vec.Y < 0 || vec.Y > 255) return true;
                    SetBlock((int)vec.X, (int)vec.Y, (int)vec.Z, Block.BIRCH_LEAVES);
                }
                return true;
            });
            base.OnMouseDown(e);
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

        public byte GetBlock(int x, int y, int z)
        {
            if (y < 0 || y > 255)
                throw new Exception("Tried to get invalid block! (y in invalid range)");
            if (chunks.ContainsKey(Chunk.PosToChunkPos(x, z))) {
                Chunk chunk = chunks[Chunk.PosToChunkPos(x, z)];
                var pos = Chunk.PosToChunkOffset(x, z);
                return chunk.blocks[pos.x, y, pos.z];
            } else throw new Exception("Tried to get invalid block! (chunk not loaded)");
        }

        public void SetBlock(int x, int y, int z, byte id)
        {
            if (y < 0 || y > 255)
                throw new Exception("Tried to set invalid block! (y in invalid range)");
            if (chunks.ContainsKey(Chunk.PosToChunkPos(x, z)))
            {
                var newpos = Chunk.PosToChunkPos(x, z);
                Chunk chunk = chunks[newpos];
                var pos = Chunk.PosToChunkOffset(x, z);
                chunk.blocks[pos.x, y, pos.z] = id;

                var pospx = (newpos.x + 1, newpos.z);
                Chunk chunkpx = chunks.ContainsKey(pospx) ? chunks[pospx] : null;
                var posnx = (newpos.x - 1, newpos.z);
                Chunk chunknx = chunks.ContainsKey(posnx) ? chunks[posnx] : null;
                var pospz = (newpos.x, newpos.z + 1);
                Chunk chunkpz = chunks.ContainsKey(pospz) ? chunks[pospz] : null;
                var posnz = (newpos.x, newpos.z - 1);
                Chunk chunknz = chunks.ContainsKey(posnz) ? chunks[posnz] : null;

                if (Block.GetBlockOpacity(id) == Block.Opacity.Transparent)
                {
                    if (chunkpx != null && chunkpx.mesh != null) meshingQueue.Enqueue(chunkpx);
                    if (chunknx != null && chunknx.mesh != null) meshingQueue.Enqueue(chunknx);
                    if (chunkpz != null && chunkpz.mesh != null) meshingQueue.Enqueue(chunkpz);
                    if (chunknz != null && chunknz.mesh != null) meshingQueue.Enqueue(chunknz);

                    meshingQueue.Enqueue(chunk);
                }
                else
                {
                    meshingQueue.Enqueue(chunk);

                    if (chunkpx != null && chunkpx.mesh != null) meshingQueue.Enqueue(chunkpx);
                    if (chunknx != null && chunknx.mesh != null) meshingQueue.Enqueue(chunknx);
                    if (chunkpz != null && chunkpz.mesh != null) meshingQueue.Enqueue(chunkpz);
                    if (chunknz != null && chunknz.mesh != null) meshingQueue.Enqueue(chunknz);
                }
            }
            else throw new Exception("Tried to set invalid block! (chunk not loaded)");
        }
    }
}
    