using System.Collections.Generic;

namespace OpenTkWindow11
{
    /// <summary>
    /// The plain-data result of parsing a .obj file. Contains no OpenGL objects —
    /// parsing is deliberately decoupled from GPU upload, so this is testable
    /// without a GL context. ModelLoader consumes this to build ObjMesh + Material submeshes.
    /// </summary>
    public class ObjData
    {
        /// <summary>Shared interleaved vertex pool. Stride 5: x, y, z, u, v.</summary>
        public float[] Vertices { get; }

        /// <summary>
        /// Material name -> index buffer into the shared vertex pool.
        /// Each entry becomes one submesh. Faces with no usemtl land under
        /// ObjLoader.DefaultMaterialName.
        /// </summary>
        public Dictionary<string, uint[]> IndicesByMaterial { get; }

        /// <summary>Path to the .mtl referenced by mtllib, or null if none.</summary>
        public string? MtlLibPath { get; }

        public ObjData(float[] vertices,
            Dictionary<string, uint[]> indicesByMaterial,
            string? mtlLibPath)
        {
            Vertices = vertices;
            IndicesByMaterial = indicesByMaterial;
            MtlLibPath = mtlLibPath;
        }
    }
}