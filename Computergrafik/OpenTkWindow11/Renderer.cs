using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OpenTkWindow11
{
    public class Renderer
    {
        public Material material;
        Mesh mesh;
        public Renderer(Material material, Mesh mesh)
        {
            this.material = material;
            this.mesh = mesh;
        }

        public void Draw(Matrix4 mvp)
        {
            material.UseShader();
            material.SetUniform("mvp", mvp);
            mesh.Draw();
        }
    }
}