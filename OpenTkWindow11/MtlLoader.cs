using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace OpenTkWindow11
{
    /// <summary>
    /// Parses a Wavefront .mtl file and returns a mapping from material name
    /// to its definition.
    ///
    /// Scope: only the diffuse texture (map_Kd) and diffuse color (Kd) are
    /// extracted. All other MTL directives (Ka, Ks, Ns, Ni, d, illum,
    /// map_Bump, etc.) are ignored because the engine has no lighting and
    /// can't use them.
    ///
    /// Texture paths written by the exporter are deliberately ignored —
    /// absolute paths from someone's Downloads folder aren't portable, and
    /// relative paths assume a folder layout we don't control. We take only
    /// the filename and assume textures live in the project's shared
    /// "Textures" folder.
    /// </summary>
    public static class MtlLoader
    {
        public static Dictionary<string, MaterialDefinition> Load(string mtlPath)
        {
            var materials = new Dictionary<string, MaterialDefinition>();
            MaterialDefinition? current = null;

            foreach (string rawLine in File.ReadAllLines(mtlPath))
            {
                string line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;

                string[] parts = line.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string directive = parts[0];

                if (directive == "newmtl")
                {
                    string name = string.Join(' ', parts, 1, parts.Length - 1);
                    current = new MaterialDefinition(name);
                    materials[name] = current;
                }
                else if (current == null)
                {
                    // Property line before any newmtl — malformed file, skip.
                    continue;
                }
                else if (directive == "map_Kd")
                {
                    string rel = string.Join(' ', parts, 1, parts.Length - 1);
                    string filename = Path.GetFileName(rel);
                    current.DiffuseTexturePath = Path.Combine("Textures", filename);
                }
                else if (directive == "Kd")
                {
                    // "Kd r g b" — three floats in [0, 1].
                    // CRITICAL: use InvariantCulture. On a Danish system the default
                    // culture parses "0.8" as failing because it expects "0,8".
                    if (parts.Length >= 4
                        && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float r)
                        && float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float g)
                        && float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float b))
                    {
                        current.DiffuseColor = new Vector3(r, g, b);
                    }
                }
                // Everything else (Ka, Ks, Ns, Ni, d, illum, map_*, etc.) is ignored.
            }

            return materials;
        }
    }
}