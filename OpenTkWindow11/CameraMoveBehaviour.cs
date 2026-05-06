using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenTkWindow11
{
    public class CameraMoveBehaviour : Behaviour
    {
        float movementSpeed = 1;
        Vector2 lastPos;
        float Sensitivity = 0.3f;
        public CameraMoveBehaviour(GameObject gameObject, Game window) : base(gameObject, window)
        {
            Console.WriteLine(gameObject.transform); //not null!
        }
        public override void Update(FrameEventArgs args)
        {
            KeyboardState input = window.KeyboardState;

            if (input.IsKeyDown(Keys.Left))
            {
                gameObject.transform.Position.X -= gameObject.transform.Right.X * movementSpeed * (float)args.Time;

            }
            if (input.IsKeyDown(Keys.Right))
            {
                gameObject.transform.Position.X += gameObject.transform.Right.X * movementSpeed * (float)args.Time;

            }
            if (input.IsKeyDown(Keys.W))
            {
                gameObject.transform.Position -= gameObject.transform.Front * movementSpeed * (float)args.Time;               
            }
            if (input.IsKeyDown(Keys.S))
            {
                gameObject.transform.Position += gameObject.transform.Front * movementSpeed * (float)args.Time;
            }
             if (input.IsKeyDown(Keys.A))
            {
                gameObject.transform.Position -= gameObject.transform.Right * movementSpeed * (float)args.Time;               
            }
            if (input.IsKeyDown(Keys.D))
            {
                gameObject.transform.Position += gameObject.transform.Right * movementSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.Home))
            {
                gameObject.transform.Rotation.X += movementSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.End))
            {
                gameObject.transform.Rotation.X -= movementSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.Delete))
            {
                gameObject.transform.Rotation.Y += movementSpeed * (float)args.Time;
            }
            if (input.IsKeyDown(Keys.PageDown))
            {
                gameObject.transform.Rotation.Y -= movementSpeed * (float)args.Time;
            }
            MouseState mouse = window.MouseState;
            if (mouse.IsButtonDown(MouseButton.Right))
            {
                //if (FirstMove)
                //{
                //    lastPos = new Vector2(mouse.X, mouse.Y);
                //    FirstMove = false;
                //}
                //else
                //{
                float deltaX = mouse.X - lastPos.X;
                float deltaY = mouse.Y - lastPos.Y;
                lastPos = new Vector2(mouse.X, mouse.Y);

                gameObject.transform.Rotation.Y += deltaX * Sensitivity * (float)args.Time;
                if (gameObject.transform.Rotation.X > 89.0f)
                {
                    gameObject.transform.Rotation.X = 89.0f;
                }
                else if (gameObject.transform.Rotation.X < -89.0f)
                {
                    gameObject.transform.Rotation.X = -89.0f;
                }
                else
                {
                    gameObject.transform.Rotation.X += deltaY * Sensitivity * (float)args.Time;
                }
                //}
            }
            else
            {
                lastPos = new Vector2(mouse.X, mouse.Y);
            }

        }
    }
}
