using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.Common;

namespace OpenTkWindow11
{
    public class GameObject
    {
        public Transform transform;
        public Renderer renderer;
        protected GameWindow gameWindow;

        List<Behaviour> behaviours = new List<Behaviour>();
        public T GetComponent<T>() where T : Behaviour
        {
            foreach (Behaviour component in behaviours)
            {
                T componentAsT = component as T;
                if (componentAsT != null) return componentAsT;
            }
            return null;
        }
        public void AddComponent<T>(params object?[]? args) where T : Behaviour
        {
            if (args == null)
            {
                behaviours.Add(Activator.CreateInstance(typeof(T), this, gameWindow) as T);
            }
            else
            {
                int initialParameters = 2;
                int totalParams = args.Length + initialParameters;
                object?[]? objects = new object[totalParams];
                objects[0] = this;
                objects[1] = gameWindow;
                for (int i = initialParameters; i < totalParams; i++)
                {
                    objects[i] = args[i-2];
                }
                
                behaviours.Add(Activator.CreateInstance(typeof(T), objects) as T);
            }
        }

        public GameObject(Renderer renderer, Game gameWindow)
        {
            this.renderer = renderer;
            this.gameWindow = gameWindow;
            transform = new Transform();
        }

        public void Update(FrameEventArgs args)
        {
            foreach (Behaviour behaviour in behaviours)
            {
                behaviour.Update(args);
            }
        }

        public void Draw(Matrix4 vp)
        {
            if (renderer != null)
            {
                renderer.Draw(transform.CalculateModel() * vp);

            }
        }
    }
}
