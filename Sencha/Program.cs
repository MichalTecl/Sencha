using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Sencha
{
    class Program
    {
        private static Mutex s_mutex;

        static async Task Main(string[] args)
        {
            var cmd = new CmdParams(args);

            if (string.IsNullOrWhiteSpace(cmd.TargetCsprojPath) || !File.Exists(cmd.TargetCsprojPath))
            {
                throw new ArgumentException($"Unexpected arguments. Expected form is: sencha <path_to_existing_csproj> [worker]");
            }

            var mutname = $"sencha_{cmd.TargetCsprojPath.ToLowerInvariant().Replace('\\','-')}";
            s_mutex = new Mutex(true, mutname, out var thisIsTheSingleInstance);
            if (!thisIsTheSingleInstance)
            {
                Console.WriteLine("Other instanceof Sencha is already running for this directory");
                return;
            }

            if (cmd.IsWorkerRunner)
            {
                LaunchLooper(cmd.TargetCsprojPath);
                return;
            }

            var solutionDir = cmd.GetSlnDirectory();

            Func<string, string, bool> decider = (a, b) => true;

            var prevEntriesCount = -1;
            while (true)
            {
                var entries = await SenchaEntry.FindEntries(solutionDir, Path.GetDirectoryName(cmd.TargetCsprojPath));
                
                if (entries.Count != prevEntriesCount)
                {
                    prevEntriesCount = entries.Count;
                    Console.WriteLine($"Sencha watches {prevEntriesCount} file(s)");
                }

                foreach(var e in entries)
                {
                    await e.Process(decider);
                }

                if (!cmd.Loop)
                {
                    return;
                }

                decider = WriteTimeBasedDecider;

                await Task.Delay(100);
             }
        }

        private static bool WriteTimeBasedDecider(string fileA, string fileB)
        {
            return File.GetLastWriteTimeUtc(fileA) > File.GetLastWriteTimeUtc(fileB);
        }

        private static void LaunchLooper(string targetCsprojPath)
        {
            var me = Assembly.GetExecutingAssembly().Location;

            if (Path.GetExtension(me).Equals(".dll"))
            {
                var dir = Path.GetDirectoryName(me);
                var mod = Path.GetFileNameWithoutExtension(me);

                var exe = $"{Path.Combine(dir, mod)}.exe";

                if (File.Exists(exe))
                {
                    me = exe;
                }
            }

            Console.WriteLine(me);

            var pi = new ProcessStartInfo();
            pi.FileName = me;
            pi.UseShellExecute = true;
            pi.CreateNoWindow = false;
            pi.Arguments = $"\"{targetCsprojPath}\" {CmdParams.LoopSwitch}";

            Process.Start(pi);
           
        }
    }
}
