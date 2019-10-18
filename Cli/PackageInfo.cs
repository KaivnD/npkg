using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cli
{
    public class PackageInfo
    {
        public PackageInfo()
        {
        }

        public string name { set; get; }

        public string version { set; get; }

        public string description { set; get; }

        public List<string> modules { set; get; }

        public List<string> files { set; get; }

        public string author { set; get; }

        public string repository { get; set; }

        public List<Dependency> dependencies { get; set; }
    }

    public class Dependency
    {
        public Dependency()
        {
        }

        public string name { get; set; }

        public string version { get; set; }
    }
}
