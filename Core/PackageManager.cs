using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Utils;

namespace Core
{
    public class PackageManager
    {
        private static string m_jsonfile;
        private string api;
        private string token;

        public string JsonFile
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

        public string homeDir { get; private set; }
        public string WorkDir { get; private set; }
        public string rcFile { get; private set; }
        public string npkgDir { get; private set; }
        public string AppDir { get; private set; }

        public PackageManager(string workdir)
        {
            WorkDir = workdir;
            JsonFile = Path.Combine(WorkDir, "npkg.json");
            homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            rcFile = Path.Combine(homeDir, ".npkgrc");
            npkgDir = Path.Combine(WorkDir, "npkgs");
            AppDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npkg");
            if (!Directory.Exists(AppDir)) Directory.CreateDirectory(AppDir);

            if (!File.Exists(rcFile)) File.WriteAllText(rcFile, "");

            api = GetOption("remote_api");
            if (api.EndsWith("/")) api = api.Remove(api.Length - 1);
            token = GetOption("token");
        }

        public int Init()
        {
           return Init(WorkDir, false);
        }

        public int Init(string path)
        {
            return Init(path, false);
        }

        public int Init(string path, bool confirm)
        {
            if (path != null) WorkDir = path;
            PackageInfo pkg = new PackageInfo();

            string dirName = Path.GetFileNameWithoutExtension(WorkDir);
            pkg.name = dirName;
            pkg.version = "0.0.1";

            if (!confirm)
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
            if (!File.Exists(JsonFile)) InitPackage(pkg);

            return 1;
        }

        public int Publish()
        {
            if (File.Exists(JsonFile))
            {
                PackageInfo pkg = JsonConvert.DeserializeObject<PackageInfo>(File.ReadAllText(JsonFile));
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

        private void log(object v)
        {
            Console.Write(v.ToString());
        }

        private void InitPackage(PackageInfo pkg)
        {
            Directory.CreateDirectory(Path.Combine(WorkDir, "src"));
            File.WriteAllText(JsonFile, JsonConvert.SerializeObject(pkg, Formatting.Indented));
            File.WriteAllText(Path.Combine(WorkDir, "README.md"), "# " + pkg.name + "\n\n" + pkg.description);
            string gitignore = "npkgs\n" + ".idea\n" + ".vscode\n" + ".DS_Store\n" + "thumbs.db\n";
            File.WriteAllText(Path.Combine(WorkDir, ".gitignore"), gitignore);
        }

        public int Set(string option, string value)
        {

            File.AppendAllText(rcFile, string.Format("{0}={1}\n", option, value));
            return 1;
        }

        public int Add(string pacakge, bool _global)
        {
            bool global = !Directory.GetFiles(WorkDir).ToList().Contains("npkg.json") || _global;
            if (global)
            {
                npkgDir = Path.Combine(AppDir, "npkgs");
                if (!_global) log(string.Format("当前文件夹下不存在npkg.json，将安装至{0}", npkgDir));
            }
            if (!Directory.Exists(npkgDir)) Directory.CreateDirectory(npkgDir);

            string[] packageInfo = pacakge.Split('@');

            if (packageInfo.Length > 2)
            {
                log("参数太多了");
                return 0;
            }

            RestClient client = null;
            RestRequest request = null;
            MakeRequest("/package/get/{name}/{version}", Method.POST, out client, out request);

            request.AddParameter("name", packageInfo[0], ParameterType.UrlSegment);
            if (packageInfo.Length == 2)
                request.AddParameter("version", packageInfo[1], ParameterType.UrlSegment);



            List<Parameter> headers = client.Execute(request).Headers.ToList();
            string orginalFilename = null;
            foreach (Parameter header in headers)
            {
                if (header.Name != "Filename") continue;
                orginalFilename = header.Value.ToString();
            }

            string hash = orginalFilename.Split('#')[1];
            string filename = orginalFilename.Split('#')[0];
            string pkgTar = Path.Combine(npkgDir, filename);
            string pkgRoot = Path.Combine(npkgDir, packageInfo[0]);

            string tmpFile = Path.GetTempFileName();
            client.DownloadData(request).SaveAs(tmpFile);

            if (string.Equals(hash, GetMD5HashFromFile(tmpFile)))
            {
                ExtractTGZ(tmpFile, pkgRoot);

                File.Delete(tmpFile);
            }
            else log("哈希验证未能通过");

            return 1;
        }

        public void MakeRequest(string route, Method method, out RestClient rec, out RestRequest req)
        {
            var client = new RestClient(api + route);
            var request = new RestRequest(method);

            request.AddHeader("Connection", "keep-alive");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Authorization", "Bearer " + token);
            request.AddHeader("Content-Type", "multipart/form-data");

            rec = client;
            req = request;
        }

        public string GetOption(string name)
        {
            if (!File.Exists(rcFile))
                throw new Exception("配置文件不存在");

            string rcFileContet = File.ReadAllText(rcFile);
            string[] configs = rcFileContet.Split(Environment.NewLine.ToCharArray());
            configs = configs.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            foreach (string config in configs)
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
