using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace craftinggame.Graphics
{
    public class ShadowMap
    {
        public static Shader shadowShader = new Shader("shadow_shader.vert", "shadow_shader.frag");
        public static int FBO;
        public static int depthMap;
        public const int SHADOW_WIDTH = 1024, SHADOW_HEIGHT = 1024;
        public const float NEAR_PLANE = 1.0f, FAR_PLANE = 700f;
        public static Matrix4 lightProjection;
        public static Matrix4 lightView;

        public static void Setup()
        {
            FBO = GL.GenFramebuffer();

            depthMap = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, depthMap);
            GL.TexImage2D(TextureTarget.Texture2D,
                0,
                PixelInternalFormat.DepthComponent,
                SHADOW_WIDTH,
                SHADOW_HEIGHT,
                0,
                PixelFormat.DepthComponent,
                PixelType.Float,
                IntPtr.Zero);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);


            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
            GL.DrawBuffer(DrawBufferMode.None);
            GL.ReadBuffer(ReadBufferMode.None);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            lightProjection = Matrix4.CreateOrthographic(100f, 100f, NEAR_PLANE, FAR_PLANE);
        }

        public static void PreludeRender()
        {
            var playerPos = Craft.theCraft.player.entity.Position;
            lightView = Matrix4.LookAt(new Vector3(4f, 10, 0), new Vector3(), new Vector3(0, 1, 0));
            shadowShader.Use();
            shadowShader.SetMatrix4("lightProjection", lightProjection);
            shadowShader.SetMatrix4("lightView", lightView);
            GL.Viewport(0, 0, SHADOW_WIDTH, SHADOW_HEIGHT);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FBO);
            GL.Clear(ClearBufferMask.DepthBufferBit);
        }
    }
}
