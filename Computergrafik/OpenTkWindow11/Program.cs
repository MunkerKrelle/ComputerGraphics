using OpenTK.Windowing.Desktop;
using OpenTkWindow11;
using OpenTK.Mathematics;

GameWindowSettings settings = new GameWindowSettings()
{
    UpdateFrequency = 60.0
};

NativeWindowSettings nativeWindowSettings = new NativeWindowSettings()
{
    ClientSize = new Vector2i(800, 600),
    Title = "Cool Window"
};
Game game = new Game(settings, nativeWindowSettings);
game.Run();
