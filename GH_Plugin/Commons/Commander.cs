using Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NPKG.Commons
{
    public class Commander
    {

        private PackageManager pm;

        public delegate void EchoHandler(object sender, string echo);
        public event EchoHandler EchoEvent;
        public event EchoHandler DoneEvent;
        public event EchoHandler ErrorEvent;

        private string[] args { set; get; }

        public Commander(string cmd, string WorkDir)
        {
            pm = new PackageManager(WorkDir);
            string argsFromStr = cmd.Replace("\\", "").Replace("/", "").Trim();
            args = argsFromStr.Split(' ');
        }

        public void Parese()
        {
            if (args.Length > 0)
            {
                if (!string.Equals(args[0], "npkg"))
                {
                    ErrorEvent(this, string.Format("{0} command not found", args[0]));
                    return;
                }

                Type t = typeof(Commander);
                MethodInfo mt = t.GetMethod(args[1]);

                List<string> aargs = new List<string>();

                foreach(string arg in args)
                {
                    if (string.Equals(arg, args[0]) || string.Equals(arg, args[0]))
                        continue;
                    aargs.Add(arg);
                }

                if (mt != null)
                {
                    mt.Invoke(this, new object[] { aargs.ToArray() });
                }
                else
                {
                    ErrorEvent(this, string.Format("{0} command is not implied", args[1]));
                    return;
                }
            } else
            {
                ErrorEvent(this, "Please type something...");
                return;
            }            
        }

        public void init(string[] args)
        {
            if (Path.Equals(pm.WorkDir, pm.homeDir))
            {
                ErrorEvent(this, "");
                return;
            }

            EchoEvent(this, string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(), "开始初始化"));
            pm.Init();
            DoneEvent(this, string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(), "初始化完成"));
        }

        public void workdir(string[] args)
        {
            EchoEvent(this, string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(), args.Length));
            //EchoEvent(this, string.Format("[{0}] {1}", DateTime.Now.ToLongTimeString(), pm.WorkDir));
        }
    }
}
