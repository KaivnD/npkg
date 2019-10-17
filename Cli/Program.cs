using CommandLine;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

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
            [Value(0, HelpText = "Record changes to the repository.")]
            public string Package { set; get; }
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

        [Verb("set", HelpText = "Publish package to npkg.net")]
        class SetOptions
        {
            [Value(0, HelpText = "Record changes to the repository.")]
            public string option { set; get; }

            [Value(1, HelpText = "Record changes to the repository.")]
            public string value { set; get; }
        }

        private static string WorkDir { set; get; }

        private static string rcFile { set; get; }

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
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            rcFile = Path.Combine(homeDir, ".npkgrc");

            var res = Parser.Default.ParseArguments<InitOptions, AddOptions, PublishOptions, RemoveOptions, InstallOptions, SetOptions>(args)
              .MapResult(
                (InitOptions opts) => InitPkgCommand(opts),
                (AddOptions opts) => AddPkgCommand(opts),
                (PublishOptions opts) => PublishCommand(opts),
                (RemoveOptions opts) => RemoveCommand(opts),
                (InstallOptions opts) => InstallCommand(opts),
                (SetOptions opts) => SetCommand(opts),
                errs => 1);
            return res;
        }

        private static int SetCommand(SetOptions opts)
        {
            if (!File.Exists(rcFile)) File.WriteAllText(rcFile, "");

            File.AppendAllText(rcFile, string.Format("{0}={1}\n", opts.option, opts.value));
            return 1;
        }

        private static int InstallCommand(InstallOptions opts)
        {
            throw new NotImplementedException();
        }

        private static int RemoveCommand(RemoveOptions opts)
        {
            throw new NotImplementedException();
        }

        private static int InitPkgCommand(InitOptions opts)
        {
            if (opts.path != null) WorkDir = opts.path;
            Package pkg = new Package();

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

        private static void InitPackage(Package pkg)
        {
            Directory.CreateDirectory(Path.Combine(WorkDir, "src"));
            File.WriteAllText(JsonFile, JsonConvert.SerializeObject(pkg, Formatting.Indented));
            File.WriteAllText(Path.Combine(WorkDir, "README.md"), "# " + pkg.name + "\n\n" + pkg.description);
            string gitignore = "npkgs\n" + ".idea\n" + ".vscode\n" + ".DS_Store\n" + "thumbs.db\n";
            File.WriteAllText(Path.Combine(WorkDir, ".gitignore"), gitignore);
        }

        private static int AddPkgCommand(AddOptions opts)
        {
            string[] packageInfo = opts.Package.Split(':');

            if (packageInfo.Length > 2)
            {
                log("参数太多了");
                return 0;
            }

            RestClient client = null;
            RestRequest request = null;
            MakeRequest("/package/get", Method.POST, out client, out request);

            request.AddQueryParameter("package", packageInfo[0]);
            if (packageInfo.Length == 2)
                request.AddQueryParameter("version", packageInfo[1]);

            string tempFile = Path.GetTempFileName();
            using (var writer = File.OpenWrite(tempFile))
            {
                request.ResponseWriter = responseStream =>
                {
                    using (responseStream)
                    {
                        responseStream.CopyTo(writer);
                    }
                };
                var response = client.DownloadData(request);
            }

            return 1;
        }

        static void log(object str)
        {
            Console.WriteLine(str.ToString());
        }

        private static int PublishCommand(PublishOptions opts)
        {
            if (File.Exists(JsonFile))
            {
                Package pkg = JsonConvert.DeserializeObject<Package>(File.ReadAllText(JsonFile));
                string outputpath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()) + ".tar.gz");

                Stream outStream = File.Create(outputpath);
                Stream gzoStream = new GZipOutputStream(outStream);
                TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

                tarArchive.RootPath = WorkDir.Replace('\\', '/');
                if (tarArchive.RootPath.EndsWith("/"))
                    tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

                AddDirectoryFilesToTar(tarArchive, JsonFile);

                pkg.modules.ForEach(path => AddDirectoryFilesToTar(tarArchive, Path.Combine(WorkDir, path)));
                pkg.files.ForEach(path => AddDirectoryFilesToTar(tarArchive, Path.Combine(WorkDir, path)));

                tarArchive.Close();

                string hash = GetMD5HashFromFile(outputpath);

                try
                {
                    RestClient client = null;
                    RestRequest request = null;
                    MakeRequest("/package/push", Method.POST, out client, out request);

                    request.AddParameter("hash", hash);
                    request.AddParameter("jsonfile", File.ReadAllText(JsonFile));

                    string docFile = Path.Combine(WorkDir, "README.md");
                    if (File.Exists(docFile)) request.AddParameter("doc", File.ReadAllText(docFile));
                    else log("根目录下没有README.md， 跳过文档上传");

                    request.AddFile("package", outputpath);
                    IRestResponse response = client.Execute(request);

                    log(response.Content);
                }
                catch (Exception ex)
                {
                    log(ex.Message);
                }
            }
            else log("npkg.json is not exist");

            return 1;
        }

        private static void MakeRequest(string route, Method method, out RestClient rec, out RestRequest req)
        {
            string api = GetOption("remote_api");
            if (api.EndsWith("/")) api = api.Remove(api.Length - 1);
            string token = GetOption("token");

            var client = new RestClient(api + route);
            var request = new RestRequest(method);

            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "multipart/form-data");

            rec = client;
            req = request;
        }

        private static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        private static void AddDirectoryFilesToTar(TarArchive tarArchive, string path)
        {
            if (File.Exists(path))
            {
                TarEntry tarEntry = TarEntry.CreateEntryFromFile(path);
                tarArchive.WriteEntry(tarEntry, true);
            } else if (Directory.Exists(path))
            {
                Array.ForEach(Directory.GetFiles(path), file => AddDirectoryFilesToTar(tarArchive, file));
            } else
            {

            }
        }

        private static string GetOption(string name)
        {
            if (!File.Exists(rcFile))
                throw new Exception("配置文件不存在");

            string rcFileContet = File.ReadAllText(rcFile);
            string[] configs = rcFileContet.Split(Environment.NewLine.ToCharArray());
            configs = configs.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            foreach(string config in configs)
            {
                string[] args = config.Split('=');
                if (args.Length != 2)
                    continue;

                if (args[0] == name)
                    return args[1];
            }
            throw new Exception("Option不存在");
        }
    }
}
