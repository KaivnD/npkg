using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

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

        [Verb("remove", HelpText = "Record changes to the repository.")]
        class RemoveOptions
        {
        }

        [Verb("install", HelpText = "Record changes to the repository.")]
        class InstallOptions
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
            var res = Parser.Default.ParseArguments<InitOptions, AddOptions, PublishOptions, RemoveOptions, InstallOptions>(args)
              .MapResult(
                (InitOptions opts) => InitPkgCommand(opts),
                (AddOptions opts) => AddPkgCommand(opts),
                (PublishOptions opts) => PublishCommand(opts),
                (RemoveOptions opts) => RemoveCommand(opts),
                (InstallOptions opts) => InstallCommand(opts),
                errs => 1);
            return res;
        }

        private static int InstallCommand(InstallOptions opts)
        {
            throw new NotImplementedException();
        }

        private static int RemoveCommand(RemoveOptions opts)
        {
            throw new NotImplementedException();
        }

        private static int PublishCommand(PublishOptions opts)
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

        private static int InitPkgCommand(InitOptions opts)
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

        private static int AddPkgCommand(AddOptions opts)
        {
            throw new NotImplementedException();
        }

        static void log(object str)
        {
            Console.WriteLine(str.ToString());
        }
    }
}
