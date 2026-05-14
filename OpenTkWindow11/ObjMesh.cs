using OpenTK.Graphics.OpenGL4;
using OpenTkWindow11.Meshes;

namespace OpenTkWindow11
{
    /// <summary>
    /// A Mesh whose geometry is supplied at construction time, rather than
    /// hardcoded like CubeMesh / QuadMesh / TriangleMesh. Used for loaded models.
    ///
    /// Buffers are INSTANCE fields (not static): every loaded mesh has unique
    /// geometry, so unlike the built-in primitives we cannot share one VBO/VAO
    /// across all instances of the type.
    ///
    /// Vertex format: interleaved, stride 5 floats — x, y, z, u, v.
    /// This matches the albedo shader's inputs (location 0 = position,
    /// location 1 = texcoord).
    /// </summary>
    public class ObjMesh : Mesh
    {
        private int vertexArrayObject;
        private int vertexBufferObject;
        private int elementBufferObject;
        private int indexCount;

        public ObjMesh(float[] vertices, uint[] indices)
        {
            // The base Mesh() constructor has ALREADY run by the time we get
            // here, and it called the virtual GenerateBuffers(). We override
            // GenerateBuffers() to do nothing (see below) because when the base
            // constructor runs, this constructor's parameters don't exist yet.
            // The real GPU upload happens here, where 'vertices' and 'indices'
            // are available.
            UploadToGpu(vertices, indices);
        }

        /// <summary>
        /// Intentionally empty. The base Mesh constructor calls GenerateBuffers()
        /// before ObjMesh's own constructor body runs — at that moment there is
        /// no geometry to upload. ObjMesh uploads from its constructor instead.
        /// </summary>
        protected override void GenerateBuffers()
        {
            // deliberately empty — see the constructor comment
        }

        private void UploadToGpu(float[] vertices, uint[] indices)
        {
            indexCount = indices.Length;

            // --- VBO: copy the interleaved vertex data CPU -> GPU ---
            vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer,
                          vertices.Length * sizeof(float),
                          vertices,
                          BufferUsageHint.StaticDraw);

            // --- VAO: record how to interpret that buffer ---
            vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(vertexArrayObject);

            // attribute 0 = position: 3 floats, stride 20 bytes, offset 0
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false,
                                   5 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);

            // attribute 1 = texcoord: 2 floats, stride 20 bytes, offset 12 bytes
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false,
                                   5 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);

            // --- EBO: this submesh's index buffer ---
            // Bound while the VAO is bound, so the VAO records it.
            elementBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          indices.Length * sizeof(uint),
                          indices,
                          BufferUsageHint.StaticDraw);
        }

        public override void Draw()
        {
            GL.BindVertexArray(vertexArrayObject);
            GL.DrawElements(PrimitiveType.Triangles, indexCount,
                            DrawElementsType.UnsignedInt, 0);
        }
    }
}