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
        public static Shader chunkShader = new Shader("chunk_shader.vert", "chunk_shader.frag");
        public static Shader chunkWaterShader = new Shader("chunk_water_shader.vert", "chunk_water_shader.frag");
        public static ChunkTexture chunkTexture = new ChunkTexture("textures");

        public static void PreludeRender()
        {
            chunkTexture.Use();
            chunkShader.SetInt("texture0", 0);
        }

        public static void PreludeNormalRender()
        {
            chunkShader.Use();
        }

        public static void PreludeWaterRender()
        {
            chunkWaterShader.Use();
            chunkWaterShader.SetFloat("globalTime", Craft.globalTime);
        }

        int VAO;
        int VBO;
        int vertAmount;
        public ChunkMesh(float[] verts, byte shader)
        {
            Shader usedShader = shader == 0 ? chunkShader : chunkWaterShader;
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            int vertexLocation = usedShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);


            int texCoordLocation = usedShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));

            int texLayerLocation = usedShader.GetAttribLocation("aLayer");
            GL.EnableVertexAttribArray(texLayerLocation);
            GL.VertexAttribPointer(texLayerLocation, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));

            int normalLocation = usedShader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));

            vertAmount = verts.Length / 9;
        }

        public void Render((int x, int z) pos, byte shader)
        {
            Shader usedShader = shader == 0 ? chunkShader : chunkWaterShader;
            GL.BindVertexArray(VAO);
            var model = Matrix4.Identity * Matrix4.CreateTranslation(pos.x * 16, 0, pos.z * 16);
            usedShader.SetMatrix4("model", model);
            usedShader.SetMatrix4("view", Craft.theCraft.player.camera.GetViewMatrix());
            usedShader.SetMatrix4("projection", Craft.theCraft.player.camera.GetProjectionMatrix());

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertAmount);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
        }
    }
}
