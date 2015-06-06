using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace GenerateManifest
{
    class Program
    {
        static TextWriter manifestFile;
        static string rootDir;

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("\nUsage: GenerateManifest <root_directory> <manifest_file>");
                return;
            }

            rootDir = args[0];
            if (!rootDir.EndsWith("\\")) rootDir += '\\';

            manifestFile = File.CreateText(args[1]);

            ProcessDir(string.Empty);

            manifestFile.Close();
        }

        static void ProcessDir(string relPath)
        {
            if (relPath.Length > 0 && !relPath.EndsWith("\\")) relPath += '\\';

            string fullPath = rootDir;
            if (relPath.Length > 0) fullPath += relPath;

            DirectoryInfo di = new DirectoryInfo(fullPath);

            FileSystemInfo[] fsi = di.GetFileSystemInfos();

            foreach (FileSystemInfo i in fsi)
            {
                string relName = relPath + i.Name;
                string relNameOut = relName.Replace('\\', '/');

                if (i is FileInfo)
                {
                    FileInfo f = (FileInfo)i;
                    string hash = f.Length == 0 ? "0" : ComputeFileHash(rootDir + relName);
                    string line = "-|" + relNameOut + "|" + f.Length + "|" + hash;
                    Console.WriteLine(line);
                    manifestFile.WriteLine(line);
                    continue;
                }

                if (i is DirectoryInfo)
                {
                    ProcessDir(relName);
                    string line = "D|" + relNameOut;
                    Console.WriteLine(line);
                    manifestFile.WriteLine(line);
                    continue;
                }

                throw new ApplicationException("Unknown file type: " + i.GetType().Name);
            }
        }

        static string ComputeFileHash(string fileName)
        {
            FileStream fs = File.OpenRead(fileName);
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            string hash = string.Concat(md5.ComputeHash(fs).Select(x => x.ToString("X2")));
            fs.Close();
            return hash;
        }
    }
}
