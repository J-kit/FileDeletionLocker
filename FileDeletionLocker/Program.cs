using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileDeletionLocker
{
    internal class Program
    {
        private static bool _run = true;

        private static void Main(string[] args)
        {
#if DEBUG
            if (args == null || args.Length == 0)
            {
                args = new[] { @"C:\ProgramData\Microsoft\Windows\WER\Temp" };
            }
#endif
            var watchTasks = args
                .Select(directoryPath => Task.Run(() => InspectionLoop(directoryPath))
                    .ContinueWith(x => Console.WriteLine("Watching task stopped")))
                .ToArray();

            Console.WriteLine("Watching task(s) started. Press ENTER to stop");
            Console.ReadLine();
            _run = false;
            Task.WaitAll(watchTasks);
        }

        private static async Task InspectionLoop(string dirPath)
        {
            var streamDict = new Dictionary<string, Stream>();
            while (_run)
            {
                foreach (var file in Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories))
                {
                    if (!streamDict.TryGetValue(file, out var stream))
                    {
                        streamDict[file] = File.Open(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        Console.WriteLine($"Created file handle for {file}");
                    }

                    await Task.Delay(1);
                }
            }

            foreach (var fileKeyPair in streamDict)
            {
                fileKeyPair.Value.Dispose();
                Console.WriteLine($"File handle for {fileKeyPair.Key} closed");
            }
        }
    }
}