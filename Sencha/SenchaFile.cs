using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Sencha
{
    public class SenchaFile
    {
        public List<string> SenchaRoots { get; set; } = new List<string>();

        public static async Task<SenchaFile> LoadAsync(string filePath)
        {
            using var strm = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<SenchaFile>(strm);
        }

        public static async Task<List<Tuple<string, string>>> FindAllSenchaRoots(string rootDiectoryPath)
        {
            var roots = new List<Tuple<string, string>>();

            var sfiles = Directory.GetFiles(rootDiectoryPath, "sencha.json", SearchOption.AllDirectories);
            foreach(var sr in sfiles)
            {
                var senchaFile = await LoadAsync(sr);
                var sourceRoot = Path.GetDirectoryName(sr);

                foreach(var rt in senchaFile.SenchaRoots)
                {
                    roots.Add(new Tuple<string, string>(rt, Path.Combine(sourceRoot, rt)));
                }
            }

            return roots.ToList();
        }

    }
}
