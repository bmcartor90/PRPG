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
        static string root;

        /// <summary>
        /// The repository, when the game runs from it (the editor, or build/PRPG.exe inside it), so maps the editor saves
        /// land in the repository's data/maps and can be committed. Otherwise the folder the game was copied to.
        /// </summary>
        static string Root
        {
            get
            {
                if (root != null) return root;
                if (Application.isEditor) return root = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
                string exeDir = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                string repo = Path.GetFullPath(Path.Combine(exeDir, ".."));
                bool inRepo = Directory.Exists(Path.Combine(repo, "data")) && Directory.Exists(Path.Combine(repo, "sim")) && Directory.Exists(Path.Combine(repo, "game"));
                return root = inRepo ? repo : exeDir;
            }
        }

        public static string Data(params string[] parts) => Path.Combine(Path.Combine(Root, "data"), Path.Combine(parts));
        public static string Art(params string[] parts) => Path.Combine(Path.Combine(Root, "assets"), Path.Combine(parts));
        public static string ReadData(params string[] parts) => File.ReadAllText(Data(parts));
    }
}
