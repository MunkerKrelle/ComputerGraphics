# ComputerGraphics

A small real-time 3D renderer written from scratch in C# with OpenTK (OpenGL 4). It draws procedurally generated shapes and a loaded `.obj` model in a scene you can fly around in. The project is about understanding the graphics pipeline and building a small engine around it: shaders, meshes, materials, a model loader and a camera.

## Highlights

| Topic | What is implemented | Where to look |
| --- | --- | --- |
| **Rendering pipeline** | GLSL vertex/fragment shaders, shader and material abstraction, model-view-projection matrix | `Shader.cs`, `Material.cs`, `Renderer.cs`, `Shaders/` |
| **Procedural meshes** | Triangle, quad, cube, sphere, cylinder and capsule generated in code | `Meshes/` |
| **Model loading** | Hand-written OBJ/MTL parser, one submesh per material, texture cache, white fallback texture for untextured materials | `ObjLoader.cs`, `MtlLoader.cs`, `ModelLoader.cs`, `ObjMesh.cs` |
| **Texturing** | Image loading and a fragment shader that alpha-blends two textures | `Texture.cs`, `Shaders/shader.frag` |
| **Scene structure** | Small component system: `GameObject`, `Transform` and `Behaviour` classes | `GameObject.cs`, `Transform.cs`, `Behaviour.cs` |
| **Camera** | Perspective camera with free-fly keyboard controls | `Camera.cs`, `CameraMoveBehaviour.cs` |

## Tech stack

- **.NET 10**, C#
- **OpenTK 4.9** (OpenGL bindings and windowing)
- **StbImageSharp** for image loading
- **GLSL** shaders

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A graphics card and driver with **OpenGL 4.0 or newer**

### Run

```bash
git clone https://github.com/MunkerKrelle/ComputerGraphics.git
cd ComputerGrafik1/OpenTkWindow11
dotnet run
```

Run it from the `OpenTkWindow11` folder, because shader, texture and model paths are relative. You can also open `OpenTkWindow11.sln` in Visual Studio and press F5.

## Controls

| Key | Action |
| --- | --- |
| `W` `A` `S` `D` | Move the camera |
| `Left` `Right` | Strafe |
| `Home` `End` `Delete` `Page Down` | Rotate the camera |
| `Esc` | Quit |

## Notes

- The scene shows a textured triangle, sphere, cylinder and capsule next to a model loaded from `Models/3d-model.obj`.
- `TextRenderer.cs` (FreeType text drawing) is included but switched off in `Game.cs`; it loads Arial from the Windows fonts folder.
- This is a learning project, not a finished engine.
