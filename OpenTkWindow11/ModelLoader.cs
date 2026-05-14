using OpenTK.Mathematics;
using System.Collections.Generic;
using System.IO;

namespace OpenTkWindow11
{
    /// <summary>
    /// Top-level entry point for loading an .obj (with optional .mtl) into a
    /// ready-to-render Renderer. Ties together the four lower-level pieces:
    /// ObjLoader (geometry), MtlLoader (materials), ObjMesh (GPU upload),
    /// and the albedo shader pair.
    ///
    /// Owns two small bits of shared infrastructure:
    ///   - a texture cache, so the same image file isn't loaded twice
    ///   - a 1x1 white fallback texture, so the albedo shader can always
    ///     sample SOMETHING even for color-only materials
    /// </summary>
    public static class ModelLoader
    {
        // Path -> Texture. Keyed by absolute path so "Models/brick.png" and
        // "Models/../Models/brick.png" don't end up as separate entries.
        private static readonly Dictionary<string, Texture> textureCache = new();

        // Lazily created on first use, then reused for every untextured material.
        private static Texture? whiteTexture;

        // Shader paths are constant — every loaded model uses the albedo pair.
        private const string AlbedoVertPath = "Shaders/albedo.vert";
        private const string AlbedoFragPath = "Shaders/albedo.frag";

        public static Renderer Load(string objPath)
        {
            // --- 1. Parse the geometry ---
            ObjData data = ObjLoader.Load(objPath);

            // --- 2. Parse the material library, if there is one ---
            // Some OBJs have no mtllib at all (raw geometry, no materials).
            // Others reference an MTL we can't find on disk. Either way, we
            // fall back to an empty dictionary — materials will then be
            // synthesized with defaults (white tint, white texture).
            Dictionary<string, MaterialDefinition> mtlDefs;
            if (data.MtlLibPath != null && File.Exists(data.MtlLibPath))
            {
                mtlDefs = MtlLoader.Load(data.MtlLibPath);
            }
            else
            {
                mtlDefs = new Dictionary<string, MaterialDefinition>();
            }

            // --- 3. Build one submesh per material group ---
            var submeshes = new List<Submesh>();
            foreach (var kv in data.IndicesByMaterial)
            {
                string materialName = kv.Key;
                uint[] indices = kv.Value;

                // Look up the MaterialDefinition. If the OBJ references a name
                // the MTL doesn't define (or there's no MTL at all), invent a
                // default — pure white, no texture. The model still renders;
                // it just looks plain.
                if (!mtlDefs.TryGetValue(materialName, out MaterialDefinition? def))
                {
                    def = new MaterialDefinition(materialName);
                }

                Material material = BuildMaterial(def);
                Mesh mesh = new ObjMesh(data.Vertices, indices);
                submeshes.Add(new Submesh(mesh, material));
            }

            return new Renderer(submeshes);
        }

        /// <summary>
        /// Converts a parsed MaterialDefinition into a live Material with the
        /// albedo shader, a bound texture (real or fallback), and a tint uniform.
        /// </summary>
        private static Material BuildMaterial(MaterialDefinition def)
        {
            Texture albedo = def.DiffuseTexturePath != null
                ? GetOrLoadTexture(def.DiffuseTexturePath)
                : GetWhiteTexture();

            var uniforms = new Dictionary<string, object>
            {
                { "albedo", albedo },
                { "tintColor", def.DiffuseColor }
            };

            return new Material(AlbedoVertPath, AlbedoFragPath, uniforms);
        }

        /// <summary>
        /// Texture cache: each unique absolute path becomes exactly one
        /// Texture object, no matter how many materials reference it.
        /// </summary>
        private static Texture GetOrLoadTexture(string absolutePath)
        {
            if (textureCache.TryGetValue(absolutePath, out Texture? cached))
                return cached;

            // If the file doesn't exist on disk (broken MTL reference), fall
            // back to white rather than crashing. The tint still works, so a
            // mis-pathed material renders as its Kd color instead of vanishing.
            if (!File.Exists(absolutePath))
            {
                System.Console.WriteLine(
                    $"[ModelLoader] Texture not found: {absolutePath} — using white fallback.");
                return GetWhiteTexture();
            }

            var tex = new Texture(absolutePath);
            textureCache[absolutePath] = tex;
            return tex;
        }

        /// <summary>
        /// The 1x1 RGBA-white texture used for untextured materials.
        /// Lazily created so we don't allocate it for projects that never load a model.
        /// </summary>
        private static Texture GetWhiteTexture()
        {
            if (whiteTexture == null)
            {
                byte[] whitePixel = { 255, 255, 255, 255 };
                whiteTexture = new Texture(whitePixel, 1, 1);
            }
            return whiteTexture;
        }
    }
}