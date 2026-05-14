using OpenTK.Mathematics;

namespace OpenTkWindow11
{
    /// <summary>
    /// A parsed material definition from a .mtl file.
    /// Both texture and color are independent: the renderer multiplies them
    /// (texture * color), so a material can have either, both, or neither.
    /// </summary>
    public class MaterialDefinition
    {
        public string Name;

        /// <summary>Absolute path to the diffuse texture, or null if none.</summary>
        public string? DiffuseTexturePath;

        /// <summary>Diffuse color (Kd). Defaults to white so a texture-only material renders as the texture.</summary>
        public Vector3 DiffuseColor = new Vector3(1f, 1f, 1f);

        public MaterialDefinition(string name) { Name = name; }
    }
}