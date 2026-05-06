using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;

namespace OpenTkWindow11
{
    public class Material
    {
        Shader shader;
        Dictionary<string, object> uniforms = new Dictionary<string, object>();
        Dictionary<int, Texture> textures = new Dictionary<int, Texture>();

        public Material(string vertPath,string fragPath, Dictionary<string,object> uniforms)
        {
            shader = new Shader(vertPath, fragPath);
            foreach (KeyValuePair<string,object> uniform in uniforms)
            {
                SetUniform(uniform.Key, uniform.Value);
            }
        }

        public void SetUniform(string name, object uniform)
        {
            //Need to activate the shader before setting uniforms :) 
            shader.Use();
            if (uniform is int uniformInt)
            {
                shader.SetInt(name, uniformInt);
            }
            else if (uniform is float uniformFloat)
            {
                shader.SetFloat(name, uniformFloat);
            }
            else if(uniform is Matrix4 uniformMatrix)
            {
                shader.SetMatrix(name, uniformMatrix);
            }
            else if (uniform is Texture tex)
            {
                int addedTextures = textures.Count;
               
                shader.SetInt(name, addedTextures);
                textures.Add(addedTextures, tex);
            }
            else
            {
                Console.WriteLine($"Unsupported shader uniform type: ");
                return;
            }

            uniforms[name] = uniform;
        }
        public void UseShader()
        {
            foreach (KeyValuePair<int,Texture> TexWithIndex in textures)
            {
                TexWithIndex.Value.Use(TextureUnit.Texture0 + TexWithIndex.Key);
            }
            shader.Use();
            
        }
    }
}
