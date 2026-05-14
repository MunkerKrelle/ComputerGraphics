using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using OpenTkWindow11.Meshes;

namespace OpenTkWindow11
{
    /// <summary>
    /// A single (Mesh, Material) pair. A Renderer is composed of one or more of these.
    /// Built-in primitives have one submesh. A loaded OBJ with multiple materials
    /// has one submesh per material group.
    /// </summary>
    public class Submesh
    {
        public Mesh Mesh;
        public Material Material;

        public Submesh(Mesh mesh, Material material)
        {
            Mesh = mesh;
            Material = material;
        }
    }

    public class Renderer
    {
        private readonly List<Submesh> submeshes = new();

        // Single-submesh constructor: same shape as the old Renderer constructor
        // (Material first, Mesh second). Existing call sites in Game.cs continue
        // to compile and behave identically.
        public Renderer(Material material, Mesh mesh)
        {
            submeshes.Add(new Submesh(mesh, material));
        }

        // Multi-submesh constructor: used by the OBJ loader, which already
        // has a list of submeshes assembled.
        public Renderer(IEnumerable<Submesh> initialSubmeshes)
        {
            submeshes.AddRange(initialSubmeshes);
        }

        // Convenience for incremental construction (e.g., the loader adding
        // submeshes one usemtl-group at a time).
        public void AddSubmesh(Mesh mesh, Material material)
        {
            submeshes.Add(new Submesh(mesh, material));
        }

        public void Draw(Matrix4 mvp)
        {
            // The MVP is computed once per Renderer.Draw call (in GameObject.Draw)
            // and is the same for every submesh — all submeshes belong to the same
            // GameObject and therefore share its Transform.
            //
            // What differs between submeshes is the material: each one has its
            // own shader program and its own textures, so we re-bind the material
            // before each submesh's draw.
            foreach (var sub in submeshes)
            {
                sub.Material.UseShader();
                sub.Material.SetUniform("mvp", mvp);
                sub.Mesh.Draw();
            }
        }
    }
}