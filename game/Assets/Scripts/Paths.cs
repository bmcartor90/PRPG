using System.IO;
using UnityEngine;

namespace Prpg.Game
{
    /// <summary>
    /// Where data and art are read from. In the editor: the repository's own data/ and assets/ folders.
    /// In a build: copies of them placed next to the .exe by the build script, so the tuning panel can write back to data/.
    /// </summary>
    public static class Paths
    {
        static string Root =>
            Application.isEditor
                ? Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."))
                : Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        public static string Data(params string[] parts) => Path.Combine(Path.Combine(Root, "data"), Path.Combine(parts));
        public static string Art(params string[] parts) => Path.Combine(Path.Combine(Root, "assets"), Path.Combine(parts));
        public static string ReadData(params string[] parts) => File.ReadAllText(Data(parts));
    }
}
