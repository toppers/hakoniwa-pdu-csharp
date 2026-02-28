#if !NO_USE_UNITY
using UnityEngine;
using System.IO;
using hakoniwa.environment.interfaces;

namespace hakoniwa.environment.impl.unity
{
    public class ResourcesFileLoader: IFileLoader
    {
        public ResourcesFileLoader()
        {
        }

        public string LoadText(string filePath, string extension = null)
        {
            string fullPath = filePath.Replace(Path.DirectorySeparatorChar, '/')
                .Replace(Path.AltDirectorySeparatorChar, '/');

            if (fullPath.StartsWith("./"))
            {
                fullPath = fullPath.Substring(2);
            }
            else
            {
                fullPath = fullPath.TrimStart('/');
            }

            if (!string.IsNullOrEmpty(extension) && fullPath.EndsWith(extension))
            {
                fullPath = fullPath.Substring(0, fullPath.Length - extension.Length);
            }

            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);
            if (textAsset == null)
            {
                throw new FileNotFoundException($"File '{fullPath}' not found in Resources.");
            }
            return textAsset.text;
        }
    }
}
#endif
