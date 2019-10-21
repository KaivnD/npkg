using CommandLine;
using Core;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Win32;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Extensions;
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

            [Option('g', "global", Required = false, Default = false)]
            public bool Global { set; get; }
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

        private static PackageManager pm;
        private static string WorkDir { set; get; }

        static int Main(string[] args)
        {
            bool fromUrl = false;
            if (args[0].Contains(':'))
            {
                fromUrl = true;
                string argsFromUrl = args[0].Replace("\\", "").Replace("/", "").Split(':')[1];
                argsFromUrl = argsFromUrl.Replace("%20", " ");
                args = argsFromUrl.Split(' ');
            }

            WorkDir = Directory.GetCurrentDirectory();

            pm = new PackageManager(WorkDir);

            var res = Parser.Default.ParseArguments<InitOptions, AddOptions, PublishOptions, RemoveOptions, InstallOptions, SetOptions>(args)
              .MapResult(
                (InitOptions opts) => InitPkgCommand(opts),
                (AddOptions opts) => AddPkgCommand(opts),
                (PublishOptions opts) => PublishCommand(opts),
                (RemoveOptions opts) => RemoveCommand(opts),
                (InstallOptions opts) => InstallCommand(opts),
                (SetOptions opts) => SetCommand(opts),
                errs => 1);
            Array.ForEach(args, arg => Console.WriteLine(arg));
            if (fromUrl) Console.ReadLine();
            return 1;
        }

        private static int InitPkgCommand(InitOptions opts)
        {
            return pm.Init(opts.path, opts.confirm);
        }

        private static int AddPkgCommand(AddOptions opts)
        {
            return pm.Add(opts.Package, opts.Global);
        }

        private static int PublishCommand(PublishOptions opts)
        {

            return pm.Publish();
        }

        private static int RemoveCommand(RemoveOptions opts)
        {
            return 1;
        }

        private static int InstallCommand(InstallOptions opts)
        {
            return 1;
        }

        private static int SetCommand(SetOptions opts)
        {
            return pm.Set(opts.option, opts.value);
        }

        static void RegisterMyProtocol(string myAppPath)  //myAppPath = full path to your application
        {
            RegistryKey key = Registry.ClassesRoot.OpenSubKey("npkg");  //open myApp protocol's subkey

            if (key == null)  //if the protocol is not registered yet...we register it
            {
                key = Registry.ClassesRoot.CreateSubKey("npkg");
                key.SetValue(string.Empty, "URL: npkg Protocol");
                key.SetValue("URL Protocol", string.Empty);

                key = key.CreateSubKey(@"shell\open\command");
                key.SetValue(string.Empty, myAppPath + " " + "%1");
                //%1 represents the argument - this tells windows to open this program with an argument / parameter
            }

            key.Close();
        }

        static void log(object str)
        {
            Console.WriteLine(str.ToString());
        }
    }
}
