using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPKG
{
    public static class Utils
    {
        /// <summary>
        /// Find a dir contains a dir named npkg.json
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string getWorkDir(string path)
        {
            if (File.Exists(path) || Directory.Exists(path))
            {
                if (!string.IsNullOrEmpty(Path.GetDirectoryName(path)))
                {
                    if (File.Exists(path)) path = Path.GetDirectoryName(path);

                    string[] files = Directory.GetFiles(path);
                    string jsonFile = Path.Combine(path, "npkg.json");
                    if (!files.Contains(jsonFile))
                    {
                        DirectoryInfo info = new DirectoryInfo(path);

                        return getWorkDir(info.Parent.FullName);
                    }
                    else return path;
                }
                else throw new Exception(string.Format("{0} This path is not in npkg", path));
            }
            else throw new Exception(string.Format("Path '{0}' doesn't exists !", path));
        }
    }
}
