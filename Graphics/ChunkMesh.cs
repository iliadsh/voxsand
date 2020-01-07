using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace craftinggame.Graphics
{
    class ChunkMesh
    {
        static Shader chunkShader = new Shader("chunk_shader.vert", "chunk_shader.frag");
        static Texture chunkTexture = new Texture("atlas.png");

        public static void PreludeRender()
        {
            chunkShader.Use();
            chunkTexture.Use();
            chunkShader.SetInt("texture0", 0);
        }

        int VAO;
        int VBO;
        int vertAmount;
        public ChunkMesh(float[] verts)
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            int vertexLocation = chunkShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 5 * sizeof(float), 0);


            int texCoordLocation = chunkShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 5 * sizeof(float), 3 * sizeof(float));

            vertAmount = verts.Length / 5;
        }

        public void Render((int x, int z) pos)
        {
            GL.BindVertexArray(VAO);
            var model = Matrix4.Identity * Matrix4.CreateTranslation(pos.x * 16, 0, pos.z * 16);
            chunkShader.SetMatrix4("model", model);
            chunkShader.SetMatrix4("view", Craft.theCraft.player.camera.GetViewMatrix());
            chunkShader.SetMatrix4("projection", Craft.theCraft.player.camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertAmount);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
        }
    }
}
