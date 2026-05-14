using System;
using System.IO;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;


    public class Shader
    {
        public int Handle;

        // Cache of compiled shader programs, keyed by (vertPath, fragPath).
        // Multiple Materials referencing the same shader pair share one
        // program — shader compilation is slow, so this is a real saving
        // when loading a multi-material model where every submesh wants
        // the same albedo shader. Mirrors the texture cache in ModelLoader.
        private static readonly Dictionary<(string, string), Shader> cache = new();

        /// <summary>
        /// Returns a Shader for the given vertex/fragment file pair,
        /// compiling and caching it on first use, or returning the cached
        /// one on subsequent calls. Prefer this over the constructor when
        /// the same shader pair may be requested multiple times.
        /// </summary>
        public static Shader GetOrLoad(string vertexPath, string fragmentPath)
        {
            var key = (vertexPath, fragmentPath);
            if (cache.TryGetValue(key, out Shader? cached))
                return cached;

            var fresh = new Shader(vertexPath, fragmentPath);
            cache[key] = fresh;
            return fresh;
        }

        public Shader(string vertexPath, string fragmentPath)
        {

            string vertexSource = File.ReadAllText(vertexPath);
            string fragmentSource = File.ReadAllText(fragmentPath);
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexSource);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentSource);
            CompileShader(vertexShader);
            CompileShader(fragmentShader);

            // These two shaders must then be merged into a shader program, which can then be used by OpenGL.
            // To do this, create a program...
            Handle = GL.CreateProgram();

            // Attach both shaders...
            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            // And then link them together.
            LinkProgram(Handle);

            // When the shader program is linked, it no longer needs the individual shaders attached to it; the compiled code is copied into the shader program.
            // Detach them, and then delete them.
            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);
            GL.DeleteShader(fragmentShader);
            GL.DeleteShader(vertexShader);

        }
        private void CompileShader(int shader)
        {
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out var code);
            if (code != (int)All.True)
            {
                var infoLog = GL.GetShaderInfoLog(shader);
                throw new Exception($"Error occurred whilst compiling Shader({shader}).\n\n{infoLog}");
            }
        }

        private void LinkProgram(int program)
        {
            GL.LinkProgram(program);
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var code);
            if (code != (int)All.True)
            {
                throw new Exception($"Error occurred whilst linking Program({program})");
            }
        }
        public void Use()
        {
            GL.UseProgram(Handle);
        }
    public int GetAttribLocation(string attribName)
    {
        return GL.GetAttribLocation(Handle, attribName);
    }
    public void SetInt(string name, int value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, value);
    }
    public void SetFloat(string name, float value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform1(location, (float)value);
    }
    
    public void SetVector3(string name, Vector3 value)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.Uniform3(location, value);
    }

    public void SetMatrix(string name, Matrix4 transform)
    {
        int location = GL.GetUniformLocation(Handle, name);
        GL.UniformMatrix4(location,true,ref transform);
    }
}