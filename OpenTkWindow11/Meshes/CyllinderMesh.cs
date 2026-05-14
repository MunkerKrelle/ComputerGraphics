using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace OpenTkWindow11.Meshes
{
    internal class CylinderMesh : Mesh
    {
        static bool buffersCreated;
        static int vertexArrayObject;
        static int elementBufferObject;
        static int vertexBufferObject;

        const int Segments = 32;
        const float Height = 2f;
        const float Radius = 0.5f;

        static float[] vertices;
        static uint[] indices;

        public CylinderMesh()
        {
            if (!buffersCreated)
            {
                GenerateCylinder();
                GenerateBuffers();
                buffersCreated = true;
            }
        }

        private static void GenerateCylinder()
        {
            List<float> verts = new();
            List<uint> inds = new();

            float halfH = Height * 0.5f;

            // Side walls
            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float angle = t * MathF.PI * 2f;

                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;

                verts.Add(x);
                verts.Add(halfH);
                verts.Add(z);
                verts.Add(t);
                verts.Add(1f);

                verts.Add(x);
                verts.Add(-halfH);
                verts.Add(z);
                verts.Add(t);
                verts.Add(0f);
            }

            for (uint i = 0; i < Segments * 2; i += 2)
            {
                inds.Add(i);
                inds.Add(i + 1);
                inds.Add(i + 2);

                inds.Add(i + 1);
                inds.Add(i + 3);
                inds.Add(i + 2);
            }

            uint baseIndex = (uint)(verts.Count / 5);

            // Top center
            verts.Add(0);
            verts.Add(halfH);
            verts.Add(0);
            verts.Add(0.5f);
            verts.Add(0.5f);

            uint topCenter = baseIndex++;

            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float angle = t * MathF.PI * 2f;

                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;

                verts.Add(x);
                verts.Add(halfH);
                verts.Add(z);

                verts.Add((x / Radius + 1f) * 0.5f);
                verts.Add((z / Radius + 1f) * 0.5f);

                if (i > 0)
                {
                    inds.Add(topCenter);
                    inds.Add(baseIndex + (uint)i - 1);
                    inds.Add(baseIndex + (uint)i);
                }
            }

            baseIndex = (uint)(verts.Count / 5);

            // Bottom center
            verts.Add(0);
            verts.Add(-halfH);
            verts.Add(0);
            verts.Add(0.5f);
            verts.Add(0.5f);

            uint bottomCenter = baseIndex++;

            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float angle = t * MathF.PI * 2f;

                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;

                verts.Add(x);
                verts.Add(-halfH);
                verts.Add(z);

                verts.Add((x / Radius + 1f) * 0.5f);
                verts.Add((z / Radius + 1f) * 0.5f);

                if (i > 0)
                {
                    inds.Add(bottomCenter);
                    inds.Add(baseIndex + (uint)i);
                    inds.Add(baseIndex + (uint)i - 1);
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
