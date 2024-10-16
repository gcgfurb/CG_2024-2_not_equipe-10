//Ricardo, Adam, Erick
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace gcgcg
{
    public static class Program
    {
        private static void Main()
        {
            var nativeWindowSettings = new NativeWindowSettings()
            {
                ClientSize = new Vector2i(800, 800),
                Title = "Círculo no centro do SRU",
                Flags = ContextFlags.ForwardCompatible,
            };

            using var window = new Mundo(GameWindowSettings.Default, nativeWindowSettings);
            window.Run();
        }
    }
}
