using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sencha
{
    public class CmdParams
    {
        public const string WorkerRunnerSwitch = "worker";
        public const string LoopSwitch = "loop"; 

        public bool IsWorkerRunner { get; }

        public bool Loop { get; }

        public string TargetCsprojPath { get; }

        public CmdParams(string[] cmd)
        {
            TargetCsprojPath = cmd.FirstOrDefault();

            bool HasSwitch(string s) { return cmd.Any(c => c.Equals(s, StringComparison.InvariantCultureIgnoreCase)); }

            IsWorkerRunner = HasSwitch(WorkerRunnerSwitch);
            Loop = HasSwitch(LoopSwitch);
        }

        public string GetSlnDirectory()
        {
            var curdir = Path.GetDirectoryName(TargetCsprojPath);

            for(; !string.IsNullOrWhiteSpace(curdir); curdir = Directory.GetParent(curdir)?.FullName)
            {
                var slnFiles = Directory.GetFiles(curdir, "*.sln");
                if (slnFiles.Any())
                {
                    return curdir;
                }
            }

            throw new FileNotFoundException($"Didn't find any *.sln file searching UP from '{TargetCsprojPath}'");
        }
    }
}
