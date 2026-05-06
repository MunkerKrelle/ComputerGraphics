using OpenTK.Mathematics;
using static System.Formats.Asn1.AsnWriter;

namespace OpenTkWindow11
{
    public class Transform
    {
        public Vector3 Position;

        public Vector3 Rotation;

        public Vector3 Scale = new Vector3(1, 1, 1);
        public Vector3 Front { get; private set; } = new Vector3(0, 0, 1); // Negativ Z-akse (standard front)
        public Vector3 Right { get; private set; } = new Vector3(1, 0, 0);  // X-akse
        public Vector3 Up { get; private set; } = new Vector3(0, 1, 0);     // Y-akse

        public Matrix4 CalculateModel()
        {
            Matrix4 t = Matrix4.CreateTranslation(Position);
            Matrix4 rX = Matrix4.CreateRotationX(Rotation.X);
            Matrix4 rY = Matrix4.CreateRotationY(Rotation.Y);
            Matrix4 rZ = Matrix4.CreateRotationZ(Rotation.Z);
            Matrix4 s = Matrix4.CreateScale(Scale);

            Matrix4 r = rX*rY*rZ;
            UpdateDirections(r);

            return s * r * t;
        }

        private void UpdateDirections(Matrix4 rotationMatrix)
        {
            Front = Vector3.Normalize(new Vector3(rotationMatrix.Row2));
            Right = Vector3.Normalize(new Vector3(rotationMatrix.Row0));
            Up = Vector3.Normalize(new Vector3(rotationMatrix.Row1));     
        }
    }

}