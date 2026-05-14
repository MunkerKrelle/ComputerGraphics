using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.IO;
using OpenTkWindow11.Meshes;

namespace OpenTkWindow11
{
    public class Game : GameWindow
    {
        NativeWindowSettings settings;
        Color4 clearColor = new Color4(0.2f, 0.3f, 0.3f, 1.0f);
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
        {
            GL.ClearColor(clearColor);
            settings = nativeWindowSettings;
        }
        Stopwatch watch = new Stopwatch();

        Texture texture0;
        Texture texture1;
        TextRenderer? textRenderer;

        List<GameObject> gameObjects = new List<GameObject>();
        Camera camera;
        protected override void OnLoad()
        {
            base.OnLoad();
            texture0 = new Texture("Textures/wall.jpg");
            texture1 = new Texture("Textures/AragonTexUdenBaggrund.png");
            Dictionary<string, object> uniforms = new Dictionary<string, object>();
            uniforms.Add("texture0", texture0);
            uniforms.Add("texture1", texture1);
            Material mat = new Material("Shaders/shader.vert", "Shaders/shader.frag", uniforms);
            Renderer rend = new Renderer(mat, new TriangleMesh());
            Renderer rend2 = new Renderer(mat, new CubeMesh());
            GameObject triangle = new GameObject(rend, this);
            gameObjects.Add(triangle);
            GameObject cube = new GameObject(rend2, this);
            //gameObjects.Add(cube);

            //Add sphere
            Renderer rend3 = new Renderer(mat, new SphereMesh());
            GameObject sphere = new GameObject(rend3, this);
            gameObjects.Add(sphere);

            //Add Cylinder
            Renderer rend4 = new Renderer(mat, new CylinderMesh());
            GameObject cylinder = new GameObject(rend4, this);
            gameObjects.Add(cylinder);

            //Add Capsule
            Renderer rend5 = new Renderer(mat, new CapsuleMesh());
            GameObject capsule = new GameObject(rend5, this);
            gameObjects.Add(capsule);

            sphere.transform.Position = new Vector3(-2, 0, 0);
            cylinder.transform.Position = new Vector3(0, 0, 0);
            capsule.transform.Position = new Vector3(2, 0, 0);
            
            Renderer loadedRenderer = ModelLoader.Load("Models/3d-model.obj");
            GameObject loadedModel = new GameObject(loadedRenderer, this);
            loadedModel.transform.Position = new Vector3(1.0f, -1.0f, 1.0f);
            gameObjects.Add(loadedModel);

            GameObject cam = new GameObject(null, this);
            cam.AddComponent<CameraMoveBehaviour>();
            cam.AddComponent<Camera>(60.0f, (float)Size.X, (float)Size.Y, 0.3f, 1000.0f);
            gameObjects.Add(cam);
            camera = cam.GetComponent<Camera>();
            GL.Enable(EnableCap.DepthTest);
            string fontPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
                "arial.ttf");
            textRenderer = new TextRenderer(fontPath, 32, Size.X, Size.Y);
            watch.Start();
        }

        protected override void OnUnload()
        {
            base.OnUnload();
            textRenderer?.Dispose();
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        }
        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            gameObjects.ForEach(x => x.Draw(camera.GetViewProjection()));
            //textRenderer?.DrawText("Dette er en sample text", new Vector2(200.0f, 350.0f), 1.0f, new Vector3(0.5f, 0.8f, 0.2f));
            SwapBuffers();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            textRenderer?.SetProjection(Size.X, Size.Y);
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            base.OnUpdateFrame(args);
            gameObjects.ForEach(x => x.Update(args));
            KeyControls();
        }

        private void KeyControls()
        {
            if (KeyboardState.IsKeyReleased(Keys.Escape))
            {
                Close();
            }
        }
    }
}
