using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sencha
{
    public class SenchaEntry
    {
        public string SourceFilePath { get; private set; }
        public string DestinationFilePath { get; private set; }

        public static async Task<List<SenchaEntry>> FindEntries(string rootFolder, string destinationRoot)
        {
            var result = new List<SenchaEntry>();

            foreach(var root in await SenchaFile.FindAllSenchaRoots(rootFolder))
            {
                var files = Directory.GetFiles(root.Item2, "*.*", SearchOption.AllDirectories);

                var normalizedRoot = root.Item2.TrimEnd('\\', '/', ' ');
                var rootLen = normalizedRoot.Length;

                foreach(var file in files)
                {
                    var unrootedFile = file.Substring(rootLen).TrimStart('\\', '/', ' ');
                    var target = Path.Combine(Path.Combine(destinationRoot, root.Item1), unrootedFile);

                    result.Add(new SenchaEntry()
                    {
                        SourceFilePath = file,
                        DestinationFilePath = target
                    });
                }
            }

            return result;
        }


        public async Task Process(Func<string, string, bool> decide)
        {
            if (!decide(SourceFilePath, DestinationFilePath))
            {
                return;
            }

            Console.Write(SourceFilePath);
            const int copyAttempts = 10;
            for(var i = 0; i <= copyAttempts; i++)
            {
                try
                {
                    var tdir = Path.GetDirectoryName(DestinationFilePath);
                    Directory.CreateDirectory(tdir);
                    File.Copy(SourceFilePath, DestinationFilePath, true);
                    Console.WriteLine(" OK");
                    break;
                }
                catch (Exception e)
                {                   
                    if (i == copyAttempts)
                    {
                        throw;
                    }

                    await Task.Delay(copyAttempts * 100);
                }
            }
        }
    }
}
