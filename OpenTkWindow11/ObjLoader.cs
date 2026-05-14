using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OpenTkWindow11
{
    /// <summary>
    /// Parses a Wavefront .obj file into an ObjData (plain data, no GL).
    ///
    /// Scope and assumptions:
    ///  - Input must be pre-triangulated (every face has exactly 3 vertices).
    ///  - Only positive 1-based indices are supported; negative (relative)
    ///    indices throw.
    ///  - Normals (vn) are parsed-skipped; the vertex format is position + uv.
    ///  - Faces are grouped by usemtl into one index buffer per material.
    ///
    /// Instance-per-parse: the loader's working state (the index spaces, the
    /// dedup map, the per-material index buffers) is held as private fields
    /// rather than threaded through method parameters. Callers use the static
    /// Load() entry point; the instance is short-lived and discarded once the
    /// ObjData is built.
    /// </summary>
    public class ObjLoader
    {
        /// <summary>Faces appearing before any usemtl directive land here.</summary>
        public const string DefaultMaterialName = "__default__";

        // OBJ's three independent index spaces, straight from the file.
        private readonly List<Vector3> positions = new();
        private readonly List<Vector2> texCoords = new();

        // The flattened OpenGL vertex pool we're building. Interleaved x y z u v.
        private readonly List<float> vertices = new();

        // Global dedup map: (0-based positionIndex, 0-based uvIndex) -> pool index.
        // uvIndex == -1 means the face vertex had no texcoord.
        private readonly Dictionary<(int pos, int uv), uint> remap = new();

        // One index buffer per material. Each becomes a submesh.
        private readonly Dictionary<string, List<uint>> indicesByMaterial = new();

        private string currentMaterial = DefaultMaterialName;
        private string? mtlLibPath;

        public static ObjData Load(string objPath) => new ObjLoader().Parse(objPath);

        private ObjData Parse(string objPath)
        {
            string objDirectory = Path.GetDirectoryName(objPath) ?? "";

            foreach (string rawLine in File.ReadAllLines(objPath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                string[] parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        // A position may have extra components (w, or r g b vertex
                        // colors); we take the first three.
                        if (parts.Length < 4)
                            throw new Exception($"Malformed vertex line: \"{line}\"");
                        positions.Add(new Vector3(
                            ParseFloat(parts[1]), ParseFloat(parts[2]), ParseFloat(parts[3])));
                        break;

                    case "vt":
                        if (parts.Length < 3)
                            throw new Exception($"Malformed texcoord line: \"{line}\"");
                        texCoords.Add(new Vector2(
                            ParseFloat(parts[1]), ParseFloat(parts[2])));
                        break;

                    case "vn":
                        // Parsed-skipped: no lighting, so normals are unused.
                        break;

                    case "mtllib":
                        {
                            string rel = string.Join(' ', parts, 1, parts.Length - 1);
                            mtlLibPath = Path.Combine(objDirectory, rel);
                        }
                        break;

                    case "usemtl":
                        currentMaterial = string.Join(' ', parts, 1, parts.Length - 1);
                        break;

                    case "f":
                        {
                            // Strict triangulation: 3 vertices, nothing else.
                            if (parts.Length != 4)
                                throw new Exception(
                                    $"Face is not a triangle ({parts.Length - 1} vertices). " +
                                    $"Re-export with 'Triangulate Faces' enabled. Line: \"{line}\"");

                            uint i0 = ResolveVertex(parts[1]);
                            uint i1 = ResolveVertex(parts[2]);
                            uint i2 = ResolveVertex(parts[3]);

                            List<uint> indexList = GetOrCreate(indicesByMaterial, currentMaterial);
                            indexList.Add(i0);
                            indexList.Add(i1);
                            indexList.Add(i2);
                        }
                        break;

                    // o, g, s, and anything else: ignored.
                }
            }

            // Freeze the per-material lists into arrays for the result.
            var indexArrays = new Dictionary<string, uint[]>();
            foreach (var kv in indicesByMaterial)
                indexArrays[kv.Key] = kv.Value.ToArray();

            return new ObjData(vertices.ToArray(), indexArrays, mtlLibPath);
        }

        /// <summary>
        /// Resolves one face-vertex token ("v", "v/vt", "v//vn", "v/vt/vn") to an
        /// index into the shared vertex pool. If this exact (position, uv) pair
        /// hasn't been seen, a new interleaved vertex is appended to the pool.
        /// </summary>
        private uint ResolveVertex(string token)
        {
            string[] comps = token.Split('/');

            int posIdx = ParseIndex(comps[0], positions.Count);

            int uvIdx = -1;
            if (comps.Length >= 2 && comps[1].Length > 0)
                uvIdx = ParseIndex(comps[1], texCoords.Count);
            // comps[2] — the normal index — is intentionally ignored.

            var key = (posIdx, uvIdx);
            if (remap.TryGetValue(key, out uint existing))
                return existing;   // already in the pool — reuse it

            // New unique combination: assemble an interleaved vertex.
            Vector3 p = positions[posIdx];
            Vector2 uv = uvIdx >= 0 ? texCoords[uvIdx] : Vector2.Zero;

            uint newIndex = (uint)(vertices.Count / 5);   // 5 floats per vertex
            vertices.Add(p.X); vertices.Add(p.Y); vertices.Add(p.Z);
            vertices.Add(uv.X); vertices.Add(uv.Y);

            remap[key] = newIndex;
            return newIndex;
        }

        /// <summary>
        /// Converts a 1-based OBJ index string to a 0-based array index.
        /// Throws on negative (relative) indices and on out-of-range values.
        /// </summary>
        private static int ParseIndex(string s, int listCount)
        {
            int raw = int.Parse(s, CultureInfo.InvariantCulture);
            if (raw < 0)
                throw new Exception(
                    $"Negative (relative) OBJ index '{raw}' is not supported. " +
                    "Re-export with absolute indices.");

            int zeroBased = raw - 1;   // OBJ indices are 1-based
            if (zeroBased < 0 || zeroBased >= listCount)
                throw new Exception($"OBJ index {raw} out of range (have {listCount} entries).");

            return zeroBased;
        }

        private static float ParseFloat(string s)
            => float.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);

        private static List<uint> GetOrCreate(Dictionary<string, List<uint>> dict, string key)
        {
            if (!dict.TryGetValue(key, out List<uint>? list))
            {
                list = new List<uint>();
                dict[key] = list;
            }
            return list;
        }
    }
}