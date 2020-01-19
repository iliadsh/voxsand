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
        public Matrix4 model = Matrix4.Identity;

        public static void PreludeRender()
        {
            chunkTexture.Use();
            chunkShader.SetInt("texture0", 0);
            chunkShader.SetInt("texture1", 1);
        }

        public static void PreludeNormalRender()
        {
            GL.Disable(EnableCap.Blend);
            chunkShader.Use();
            chunkShader.SetMatrix4("lightProjection", ShadowMap.lightProjection);
            chunkShader.SetMatrix4("lightView", ShadowMap.lightView);
            chunkShader.SetMatrix4("view", Craft.theCraft.player.camera.view);
            chunkShader.SetMatrix4("projection", Craft.theCraft.player.camera.projection);
            //chunkShader.SetVector3("lightDir", ShadowMap.lightDir);
        }

        public static void PreludeWaterRender()
        {
            GL.Enable(EnableCap.Blend);
            chunkWaterShader.Use();
            chunkWaterShader.SetFloat("globalTime", Craft.globalTime);
            chunkWaterShader.SetMatrix4("view", Craft.theCraft.player.camera.view);
            chunkWaterShader.SetMatrix4("projection", Craft.theCraft.player.camera.projection);
        }

        int VAO;
        int VBO;
        int vertAmount;
        public ChunkMesh(float[] verts, (int x, int z) pos)
        {
            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, verts.Length * sizeof(float), verts, BufferUsageHint.StaticDraw);

            int vertexLocation = 0;//usedShader.GetAttribLocation("aPosition");
            GL.EnableVertexAttribArray(vertexLocation);
            GL.VertexAttribPointer(vertexLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 0);

            int texCoordLocation = 1;//usedShader.GetAttribLocation("aTexCoord");
            GL.EnableVertexAttribArray(texCoordLocation);
            GL.VertexAttribPointer(texCoordLocation, 2, VertexAttribPointerType.Float, false, 9 * sizeof(float), 3 * sizeof(float));

            int texLayerLocation = 2;//usedShader.GetAttribLocation("aLayer");
            GL.EnableVertexAttribArray(texLayerLocation);
            GL.VertexAttribPointer(texLayerLocation, 1, VertexAttribPointerType.Float, false, 9 * sizeof(float), 5 * sizeof(float));

            int normalLocation = 3;//usedShader.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 9 * sizeof(float), 6 * sizeof(float));

            vertAmount = verts.Length / 9;
            model = Matrix4.CreateTranslation(pos.x * 16, 0, pos.z * 16);
        }

        public void Render(Shader usedShader)
        {
            GL.BindVertexArray(VAO);
            usedShader.SetMatrix4("model", model);

            GL.DrawArrays(PrimitiveType.Triangles, 0, vertAmount);
        }

        public void Cleanup()
        {
            GL.DeleteVertexArray(VAO);
            GL.DeleteBuffer(VBO);
        }
    }
}
