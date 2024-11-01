#if UNITY
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
            string fullPath = extension == null ? filePath : filePath + extension;
            TextAsset textAsset = Resources.Load<TextAsset>(fullPath);
            if (textAsset == null)
            {
                throw new FileNotFoundException($"File '{filePath}' not found in Resources.");
            }
            return textAsset.text;
        }
    }
}
#endif
