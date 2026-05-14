using OpenTK.Graphics.OpenGL4;

namespace OpenTkWindow11.Meshes
{
    internal class CapsuleMesh : Mesh
    {
        static bool buffersCreated;
        static int vertexArrayObject;
        static int elementBufferObject;
        static int vertexBufferObject;

        const int Segments = 32;
        const int Rings = 16;
        const float Radius = 0.5f;
        const float CylinderHeight = 1f;

        static float[] vertices;
        static uint[] indices;

        public CapsuleMesh()
        {
            if (!buffersCreated)
            {
                GenerateCapsule();
                GenerateBuffers();
                buffersCreated = true;
            }
        }

        private static void GenerateCapsule()
        {
            List<float> verts = new();
            List<uint> inds = new();

            float halfCyl = CylinderHeight * 0.5f;

            // Cylinder walls
            for (int i = 0; i <= Segments; i++)
            {
                float t = (float)i / Segments;
                float angle = t * MathF.PI * 2f;

                float x = MathF.Cos(angle) * Radius;
                float z = MathF.Sin(angle) * Radius;

                verts.Add(x);
                verts.Add(halfCyl);
                verts.Add(z);
                verts.Add(t);
                verts.Add(1f);

                verts.Add(x);
                verts.Add(-halfCyl);
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

            // Top 
            for (int y = 0; y <= Rings; y++)
            {
                float v = (float)y / Rings;
                float theta = v * MathF.PI * 0.5f;

                float sinT = MathF.Sin(theta);
                float cosT = MathF.Cos(theta);

                for (int x = 0; x <= Segments; x++)
                {
                    float u = (float)x / Segments;
                    float phi = u * MathF.PI * 2f;

                    float sinP = MathF.Sin(phi);
                    float cosP = MathF.Cos(phi);

                    float px = cosP * sinT * Radius;
                    float py = cosT * Radius + halfCyl;
                    float pz = sinP * sinT * Radius;

                    verts.Add(px);
                    verts.Add(py);
                    verts.Add(pz);

                    verts.Add(u);
                    verts.Add(1f - v * 0.5f);
                }
            }

            for (int y = 0; y < Rings; y++)
            {
                for (int x = 0; x < Segments; x++)
                {
                    uint i0 = baseIndex + (uint)(y * (Segments + 1) + x);
                    uint i1 = i0 + 1;
                    uint i2 = i0 + (uint)(Segments + 1);
                    uint i3 = i2 + 1;

                    inds.Add(i0);
                    inds.Add(i2);
                    inds.Add(i1);

                    inds.Add(i1);
                    inds.Add(i2);
                    inds.Add(i3);
                }
            }

            baseIndex = (uint)(verts.Count / 5);

            // Bottom 
            for (int y = 0; y <= Rings; y++)
            {
                float v = (float)y / Rings;
                float theta = v * MathF.PI * 0.5f;

                float sinT = MathF.Sin(theta);
                float cosT = MathF.Cos(theta);

                for (int x = 0; x <= Segments; x++)
                {
                    float u = (float)x / Segments;
                    float phi = u * MathF.PI * 2f;

                    float sinP = MathF.Sin(phi);
                    float cosP = MathF.Cos(phi);

                    float px = cosP * sinT * Radius;
                    float py = -cosT * Radius - halfCyl;
                    float pz = sinP * sinT * Radius;

                    verts.Add(px);
                    verts.Add(py);
                    verts.Add(pz);

                    verts.Add(u);
                    verts.Add(v * 0.5f);
                }
            }

            for (int y = 0; y < Rings; y++)
            {
                for (int x = 0; x < Segments; x++)
                {
                    uint i0 = baseIndex + (uint)(y * (Segments + 1) + x);
                    uint i1 = i0 + 1;
                    uint i2 = i0 + (uint)(Segments + 1);
                    uint i3 = i2 + 1;

                    inds.Add(i0);
                    inds.Add(i1);
                    inds.Add(i2);

                    inds.Add(i1);
                    inds.Add(i3);
                    inds.Add(i2);
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
