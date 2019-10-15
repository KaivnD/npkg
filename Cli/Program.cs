using CommandLine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Cli
{
    class Program
    {
        [Verb("init", HelpText = "Add file contents to the index.")]
        class InitOptions
        {
            [Option('p', "path", Required = false, Default = null)]
            public string path { set; get; }

            [Option('y', "confirm", Required = false, Default = false)]
            public bool confirm { set; get; }
        }

        [Verb("add", HelpText = "Record changes to the repository.")]
        class AddOptions
        {
        }

        [Verb("publish", HelpText = "Publish package to npkg.net")]
        class PublishOptions
        {
        }

        private static string WorkDir { set; get; }

        private static string m_jsonfile;

        public static string JsonFile
        {
            set
            {
                m_jsonfile = value;
            }
            get
            {
                if (WorkDir != null)
                {
                    return Path.Combine(WorkDir, "npkg.json");
                }
                else return m_jsonfile;
            }
        }

        static int Main(string[] args)
        {
            WorkDir = Directory.GetCurrentDirectory();
            JsonFile = Path.Combine(WorkDir, "npkg.json");
            return Parser.Default.ParseArguments<InitOptions, AddOptions, PublishOptions>(args)
              .MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (AddOptions opts) => RunAddAndReturnExitCode(opts),
                (PublishOptions opts) => RunPublishAndReturnExitCode(opts),
                errs => 1);
        }

        private static int RunPublishAndReturnExitCode(PublishOptions opts)
        {
            if (File.Exists(JsonFile))
            {
                Package pkg = JsonConvert.DeserializeObject<Package>(File.ReadAllText(JsonFile));
                log(pkg.name);
                log(pkg.dependencies[0].name + pkg.dependencies[0].version);
            }
            else log("npkg.json is not exist");

            return 1;
        }

        private static int RunInitAndReturnExitCode(InitOptions opts)
        {
            if (opts.path != null) WorkDir = opts.path;
            Package pkg = new Package();

            string dirName = Path.GetFileNameWithoutExtension(WorkDir);
            pkg.name = dirName;
            pkg.version = "0.0.1";
            pkg.main = "main.gh";

            if (!opts.confirm)
            {
                log(string.Format("name ({0})", dirName));
                string name = Console.ReadLine();
                if (name != "") pkg.name = name;

                log("version ?");
                pkg.version = Console.ReadLine();
                log("description ?");
                pkg.description = Console.ReadLine();
                log("main ?");
                pkg.main = Console.ReadLine();
                log("repository url ?");
                pkg.repository = Console.ReadLine();
                log("author ?");
                pkg.author = Console.ReadLine();
            }

            pkg.dependencies = new List<Dependency>();
            pkg.files = new List<string>();

            InitPackage(pkg);
            return 1;
        }

        private static void InitPackage(Package pkg)
        {
            File.WriteAllText(JsonFile, JsonConvert.SerializeObject(pkg, Formatting.Indented));
        }

        private static int RunAddAndReturnExitCode(AddOptions opts)
        {
            throw new NotImplementedException();
        }

        static void log(object str)
        {
            Console.WriteLine(str.ToString());
        }
    }
}
