using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
namespace Hl2_Randomizer
{
    class AdvDir
    {

        static TaskFactory tf = new TaskFactory();
        public static void MoveContents(string inDir, string outDir)
        {
            if (!Directory.Exists(inDir))
            {
                return;
            }
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            DirectoryInfo Di = new DirectoryInfo(inDir);
            DirectoryInfo Do = new DirectoryInfo(outDir);
            if (Di.Root.Equals(Do.Root))
            {
                List<Task> dirSearch = new List<Task>();
                foreach (DirectoryInfo d in Di.GetDirectories())
                {
                    try
                    {
                        if (Directory.Exists(Do.FullName + "\\" + d.Name))
                        {
                            Console.WriteLine("New thread created for " + Do.FullName + "\\" + d.Name);
                            dirSearch.Add(tf.StartNew(() => MoveContents(d.FullName, Do.FullName + "\\" + d.Name)));
                        }
                        else
                        {
                            d.MoveTo(outDir + "\\" + d.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (FileInfo f in Di.GetFiles())
                {
                    try
                    {
                        if (File.Exists(Do.FullName + "\\" + f.Name))
                        {
                            File.Delete(Do.FullName + "\\" + f.Name);
                        }
                        f.MoveTo(Do.FullName + "\\" + f.Name);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (Task t in dirSearch)
                {
                    Console.WriteLine(t.Id + " waiting...");
                    t.Wait();
                    Console.WriteLine(t.Id + " Done!");
                }
            }
            else
            {
                List<Task> dirSearch = new List<Task>();
                foreach (DirectoryInfo d in Di.GetDirectories())
                {
                    try
                    {
                        Console.WriteLine("New thread created for " + Do.FullName + "\\" + d.Name);
                        dirSearch.Add(tf.StartNew(() => MoveContents(d.FullName, Do.FullName + "\\" + d.Name)));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (FileInfo f in Di.GetFiles())
                {
                    try
                    {
                        f.CopyTo(Do.FullName + "\\" + f.Name, true);
                        f.Decrypt();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                foreach (Task t in dirSearch)
                {
                    Console.WriteLine(t.Id + " waiting...");
                    t.Wait();
                    Console.WriteLine(t.Id + " Done!");
                }
            }
        }

        public static FileInfo[] allFiles(DirectoryInfo dir)
        {
            List<FileInfo> files = new List<FileInfo>();
            foreach (FileInfo fi in dir.GetFiles())
            {
                files.Add(fi);
            }
            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                files.AddRange(allFiles(di));
            }
            return files.ToArray();
        }
    }
}
