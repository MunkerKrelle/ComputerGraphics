using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace OpenTkWindow11
{
    public class Material
    {
        Shader shader;
        Dictionary<string, object> uniforms = new Dictionary<string, object>();
        Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

        public Material(string vertPath, string fragPath, Dictionary<string, object> uniforms)
        {
            shader = Shader.GetOrLoad(vertPath, fragPath);
            foreach (KeyValuePair<string, object> uniform in uniforms)
            {
                SetUniform(uniform.Key, uniform.Value);
            }
        }

        // Records a uniform value for this material. The actual GPU upload
        // happens later in UseShader(), once per frame, so that materials
        // sharing a cached shader program don't bleed state into each other.
        public void SetUniform(string name, object value)
        {
            uniforms[name] = value;

            // Textures need an assigned sampler unit. Insertion order into
            // the textures dict picks the unit (first texture -> unit 0).
            if (value is Texture tex && !textures.ContainsValue(tex))
            {
                textures[textures.Count] = tex;
            }
        }

        public void UseShader()
        {
            shader.Use();

            // Re-apply every uniform this material owns. Uniforms are
            // program-wide GPU state, not per-instance — with shader caching,
            // multiple Materials may share one program, so each must re-assert
            // its own uniform values each frame.
            foreach (var kv in uniforms)
            {
                Upload(kv.Key, kv.Value);
            }

            // Bind textures to their assigned units. Upload() already told
            // each sampler uniform which unit to read from; this is the
            // matching act of actually putting our textures there.
            foreach (var t in textures)
            {
                t.Value.Use(TextureUnit.Texture0 + t.Key);
            }
        }

        private void Upload(string name, object value)
        {
            switch (value)
            {
                case int i:      shader.SetInt(name, i); break;
                case float f:    shader.SetFloat(name, f); break;
                case Matrix4 m:  shader.SetMatrix(name, m); break;
                case Vector3 v:  shader.SetVector3(name, v); break;
                case Texture:
                    // Find this texture's assigned unit and point the sampler at it.
                    int unit = textures.First(p => p.Value == value).Key;
                    shader.SetInt(name, unit);
                    break;
                default:
                    Console.WriteLine($"Unsupported shader uniform type for '{name}'");
                    break;
            }
        }
    }
}