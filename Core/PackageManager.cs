using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class PackageManager
    {
        public PackageManager()
        {
        }

        public int Init(string path, bool confirm)
        {
            if (opts.path != null) WorkDir = opts.path;
            PackageInfo pkg = new PackageInfo();

            string dirName = Path.GetFileNameWithoutExtension(WorkDir);
            pkg.name = dirName;
            pkg.version = "0.0.1";

            if (!opts.confirm)
            {
                log(string.Format("name ({0})", dirName));
                string name = Console.ReadLine();
                if (name != "") pkg.name = name;

                log("version ?");
                pkg.version = Console.ReadLine();
                log("description ?");
                pkg.description = Console.ReadLine();

                log("repository url ?");
                pkg.repository = Console.ReadLine();
                log("author ?");
                pkg.author = Console.ReadLine();
            }

            pkg.dependencies = new List<Dependency>();
            pkg.files = new List<string>();
            pkg.modules = new List<string> { "src" };

            InitPackage(pkg);
            return 1;
        }

        private static void InitPackage(PackageInfo pkg)
        {
            Directory.CreateDirectory(Path.Combine(WorkDir, "src"));
            File.WriteAllText(JsonFile, JsonConvert.SerializeObject(pkg, Formatting.Indented));
            File.WriteAllText(Path.Combine(WorkDir, "README.md"), "# " + pkg.name + "\n\n" + pkg.description);
            string gitignore = "npkgs\n" + ".idea\n" + ".vscode\n" + ".DS_Store\n" + "thumbs.db\n";
            File.WriteAllText(Path.Combine(WorkDir, ".gitignore"), gitignore);
        }
    }
}
