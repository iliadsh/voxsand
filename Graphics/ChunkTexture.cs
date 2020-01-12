using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace craftinggame.Graphics
{
    public class ChunkTexture
    {
        private Dictionary<string, int> imageNums = new Dictionary<string, int>();

        public float GetImageIndex(string file)
        {
            return imageNums[file];
        }

        public readonly int Handle;

        public ChunkTexture(string path)
        {
            path = "Resources/" + path;

            string[] files = Directory.GetFiles(path);

            Handle = GL.GenTexture();

            Use();

            GL.TexStorage3D(TextureTarget3d.Texture2DArray,
                5,
                SizedInternalFormat.Rgba8,
                32,32,
                256);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            for (int i = 0; i < files.Length; i++)
            {
                using(var image = new Bitmap(files[i]))
                {

                    image.RotateFlip(RotateFlipType.Rotate180FlipX);

                    var data = image.LockBits(
                        new Rectangle(0, 0, image.Width, image.Height),
                        ImageLockMode.ReadOnly,
                        System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexSubImage3D(TextureTarget.Texture2DArray,
                        0,
                        0, 0, i,
                        32, 32, 1,
                        PixelFormat.Bgra,
                        PixelType.UnsignedByte,
                        data.Scan0);

                    imageNums[files[i]] = i;
                }
            }

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
            //GL.GetFloat((GetPName)All.MaxTextureMaxAnisotropy, out float maxAniso);
            //GL.TexParameter(TextureTarget.Texture2D, (TextureParameterName)All.TextureMaxAnisotropy, maxAniso);
        }

        public void Use(TextureUnit unit = TextureUnit.Texture0)
        {
            GL.ActiveTexture(unit);
            GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        }
    }
}
