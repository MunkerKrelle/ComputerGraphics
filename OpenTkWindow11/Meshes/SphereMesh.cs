using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTkWindow11.Meshes
{
    internal class SphereMesh : Mesh
    {
        static bool buffersCreated;
        static int vertexArrayObject;
        static int elementBufferObject;
        static int vertexBufferObject;

        const int LatitudeSegments = 32;
        const int LongitudeSegments = 32;

        static float[] vertices;
        static uint[] indices;

        public SphereMesh()
        {
            if (!buffersCreated)
            {
                GenerateSphere();
                GenerateBuffers();
                buffersCreated = true;
            }
        }

        private static void GenerateSphere()
        {
            List<float> verts = new();
            List<uint> inds = new();

            for (int lat = 0; lat <= LatitudeSegments; lat++)
            {
                float theta = (float)lat / LatitudeSegments * MathF.PI;

                float sinTheta = MathF.Sin(theta);
                float cosTheta = MathF.Cos(theta);

                for (int lon = 0; lon <= LongitudeSegments; lon++)
                {
                    float phi = (float)lon / LongitudeSegments * MathF.PI * 2f;

                    float sinPhi = MathF.Sin(phi);
                    float cosPhi = MathF.Cos(phi);

                    float x = cosPhi * sinTheta;
                    float y = cosTheta;
                    float z = sinPhi * sinTheta;

                    float u = (float)lon / LongitudeSegments;
                    float v = (float)lat / LatitudeSegments;

                    verts.Add(x);
                    verts.Add(y);
                    verts.Add(z);

                    verts.Add(u);
                    verts.Add(v);
                }
            }

            for (int lat = 0; lat < LatitudeSegments; lat++)
            {
                for (int lon = 0; lon < LongitudeSegments; lon++)
                {
                    uint first = (uint)(lat * (LongitudeSegments + 1) + lon);
                    uint second = first + (uint)(LongitudeSegments + 1);

                    inds.Add(first);
                    inds.Add(second);
                    inds.Add(first + 1);

                    inds.Add(second);
                    inds.Add(second + 1);
                    inds.Add(first + 1);
                }
            }

            vertices = verts.ToArray();
            indices = inds.ToArray();
        }

        private static void GenerateBuffers()
        {
            vertexArrayObject = GL.GenVertexArray();
            vertexBufferObject = GL.GenBuffer();
            elementBufferObject = GL.GenBuffer();

            GL.BindVertexArray(vertexArrayObject);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(
                BufferTarget.ArrayBuffer,
                vertices.Length * sizeof(float),
                vertices,
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(
                BufferTarget.ElementArrayBuffer,
                indices.Length * sizeof(uint),
                indices,
                BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(
                0,
                3,
                VertexAttribPointerType.Float,
                false,
                5 * sizeof(float),
                0);

            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(
                1,
                2,
                VertexAttribPointerType.Float,
                false,
                5 * sizeof(float),
                3 * sizeof(float));

            GL.EnableVertexAttribArray(1);

            GL.BindVertexArray(0);
        }

        public override void Draw()
        {
            GL.BindVertexArray(vertexArrayObject);

            GL.DrawElements(
                PrimitiveType.Triangles,
                indices.Length,
                DrawElementsType.UnsignedInt,
                0);
        }
    }
}

