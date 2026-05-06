using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
namespace OpenTkWindow11
{
    internal class TriangleMesh : Mesh
    {
        protected override uint[] Indices => base.Indices;
        protected override float[] Vertices => new float[]
         {
            -0.5f, -0.5f, 0.0f, //Bottom-left vertex
            0.5f, -0.5f, 0.0f, //Bottom-right vertex
            0.0f,  0.5f, 0.0f  //Top vertex
        };
        static int vertexArrayObject;
        static int vertexBufferObject;
        private static bool buffersCreated;

        protected override void GenerateBuffers()
        {

            if (buffersCreated)
            {
                return;
            }
            vertexBufferObject = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, Vertices.Length * sizeof(float), Vertices, BufferUsageHint.StaticDraw);
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            // Enable variable 0 in the shader.
            GL.EnableVertexAttribArray(0);
            buffersCreated = true;
        }
        public override void Draw()
        {
            GL.BindVertexArray(vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }
    }
}
