using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTkWindow11
{
    public class Camera : Behaviour
    {
        private float FOV;
        private float aspectX;
        private float aspectY;
        private float near;
        private float far;

        public Camera(GameObject gameObject, Game window, float FOV, float aspectX, float aspectY, float near, float far) : base(gameObject, window)
        {
            gameObject.transform.Position = new Vector3(0.0f, 0.0f, 3.0f);
            this.FOV = FOV;
            this.aspectX = aspectX;
            this.aspectY = aspectY;
            this.near = near;
            this.far = far;
        }
        public Matrix4 GetViewProjection()
        {
            gameObject.transform.CalculateModel();
            Matrix4 view = Matrix4.LookAt(gameObject.transform.Position, gameObject.transform.Position - gameObject.transform.Front, gameObject.transform.Up);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FOV), aspectX / aspectY, near, far);
            return view * projection;
        }

        public override void Update(FrameEventArgs args)
        {
        }
    }
}
