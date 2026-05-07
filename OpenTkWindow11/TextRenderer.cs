using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using SharpFont;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace OpenTkWindow11
{
    public class TextRenderer : IDisposable
    {
        private struct Character
        {
            public int TextureID;
            public Vector2 Size;
            public Vector2 Bearing;
            public float Advance;
        }

        private readonly Dictionary<char, Character> characters = new Dictionary<char, Character>();
        private readonly int shaderProgram;
        private readonly int vao;
        private readonly int vbo;
        private readonly int projectionLocation;
        private readonly int colorLocation;

        public TextRenderer(string fontPath, uint pixelSize, int viewportWidth, int viewportHeight)
        {
            shaderProgram = CreateShaderProgram();
            projectionLocation = GL.GetUniformLocation(shaderProgram, "projection");
            colorLocation = GL.GetUniformLocation(shaderProgram, "textColor");

            vao = GL.GenVertexArray();
            vbo = GL.GenBuffer();

            GL.BindVertexArray(vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, 6 * 4 * sizeof(float), IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);

            FontFace fontFace;
            using (FileStream stream = File.OpenRead(fontPath))
            {
                fontFace = new FontFace(stream);
            }

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            for (char c = (char)0; c < 128; c++)
            {
                Glyph? glyph = fontFace.GetGlyph(c, pixelSize);
                if (glyph == null || glyph.RenderWidth <= 0 || glyph.RenderHeight <= 0)
                {
                    continue;
                }

                int width = glyph.RenderWidth;
                int height = glyph.RenderHeight;
                IntPtr buffer = Marshal.AllocHGlobal(width * height);
                for (int i = 0; i < width * height; i++)
                {
                    Marshal.WriteByte(buffer, i, 0);
                }

                glyph.RenderTo(new Surface
                {
                    Bits = buffer,
                    Width = width,
                    Height = height,
                    Pitch = width
                });

                int texture = GL.GenTexture();
                GL.BindTexture(TextureTarget.Texture2D, texture);
                GL.TexImage2D(
                    TextureTarget.Texture2D,
                    0,
                    PixelInternalFormat.R8,
                    width,
                    height,
                    0,
                    PixelFormat.Red,
                    PixelType.UnsignedByte,
                    buffer);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                Marshal.FreeHGlobal(buffer);

                characters[c] = new Character
                {
                    TextureID = texture,
                    Size = new Vector2(width, height),
                    Bearing = new Vector2(glyph.HorizontalMetrics.Bearing.X, glyph.HorizontalMetrics.Bearing.Y),
                    Advance = glyph.HorizontalMetrics.Advance
                };
            }
            GL.BindTexture(TextureTarget.Texture2D, 0);

            SetProjection(viewportWidth, viewportHeight);
        }

        public void SetProjection(int width, int height)
        {
            Matrix4 projection = Matrix4.CreateOrthographicOffCenter(0.0f, width, height, 0.0f, -1.0f, 1.0f);
            GL.UseProgram(shaderProgram);
            GL.UniformMatrix4(projectionLocation, false, ref projection);
            GL.Uniform1(GL.GetUniformLocation(shaderProgram, "text"), 0);
        }

        public void DrawText(string text, Vector2 position, float scale, Vector3 color)
        {
            GL.Disable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            GL.UseProgram(shaderProgram);
            GL.Uniform3(colorLocation, color);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindVertexArray(vao);

            float x = position.X;
            float y = position.Y;

            foreach (char c in text)
            {
                if (!characters.TryGetValue(c, out Character ch))
                {
                    continue;
                }

                float xpos = x + ch.Bearing.X * scale;
                float ypos = y - ch.Bearing.Y * scale;
                float w = ch.Size.X * scale;
                float h = ch.Size.Y * scale;

                float[] vertices =
                {
                    xpos,     ypos + h, 0.0f, 1.0f,
                    xpos,     ypos,     0.0f, 0.0f,
                    xpos + w, ypos,     1.0f, 0.0f,

                    xpos,     ypos + h, 0.0f, 1.0f,
                    xpos + w, ypos,     1.0f, 0.0f,
                    xpos + w, ypos + h, 1.0f, 1.0f
                };

                GL.BindTexture(TextureTarget.Texture2D, ch.TextureID);
                GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, vertices.Length * sizeof(float), vertices);
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.DrawArrays(PrimitiveType.Triangles, 0, 6);

                x += ch.Advance * scale;
            }

            GL.BindVertexArray(0);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
        }

        private static int CreateShaderProgram()
        {
            string vertexShaderSource = File.ReadAllText("Shaders/TextShader.vert");
            string fragmentShaderSource = File.ReadAllText("Shaders/TextShader.frag");

            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.CompileShader(vertexShader);

            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(fragmentShader);

            int program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        public void Dispose()
        {
            foreach (Character character in characters.Values)
            {
                GL.DeleteTexture(character.TextureID);
            }

            GL.DeleteBuffer(vbo);
            GL.DeleteVertexArray(vao);
            GL.DeleteProgram(shaderProgram);
        }
    }
}
