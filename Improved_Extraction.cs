using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.IO;

namespace Hl2_Randomizer
{
    class Improved_Extraction
    {
        static bool verbose = false;
        static TaskFactory tf = new TaskFactory();
        private static void processWatcher(string filename, string arguements)
        {
            Process p = new Process();
            p.StartInfo.FileName = filename;
            p.StartInfo.Arguments = arguements;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.Start();
            if (verbose)
            {
                Console.WriteLine(p.StandardOutput.ReadToEnd());
            }
            p.WaitForExit();
        }

        public static void improvedExtraction(string exePath, string directory, string output, bool verb)
        {
            verbose = verb;
            Console.WriteLine("Getting valid vpks");
            List<string> dirs = extractor(exePath, new DirectoryInfo(directory));

            //Cheaks if the directory exists if so, deletes it
            if (Directory.Exists(output))
            {
                Directory.Delete(output, true);
            }

            Directory.CreateDirectory(output);
            //Moves all file contents
            DirectoryInfo outp = new DirectoryInfo(output);
            foreach (string dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);

                Console.WriteLine("Moving contents of " + di.Name);
                AdvDir.MoveContents(di.FullName, outp.FullName);

                Console.WriteLine("Deleting " + di.Name);
                di.Delete(true);
            }
        }

        private static List<string> extractor(string exePath, DirectoryInfo di)
        {
            //Dosent take files from custom
            if (!di.Name.Equals("custom"))
            {
                List<string> outputDir = new List<string>();
                List<Task> extractions = new List<Task>();
                foreach (FileInfo fi in di.GetFiles())
                {
                    if (fi.Extension.Equals(".vpk") && fi.Name.Contains("_dir"))
                    {
                        extractions.Add(tf.StartNew(() => processWatcher(exePath, "\"" + fi.FullName + "\"")));
                        outputDir.Add(fi.Directory + "\\" + fi.Name.Split('.')[0]);
                    }
                }

                //Loops through directories
                foreach (DirectoryInfo d in di.GetDirectories())
                {
                    outputDir.AddRange(extractor(exePath, d));
                }

                //Waits for processes to end
                foreach (Task t in extractions)
                {
                    try
                    {
                        if (verbose)
                        {
                            Console.WriteLine("Waiting for " + t.Id);
                        }
                        t.Wait();
                        if (verbose)
                        {
                            Console.WriteLine(t.Id + " is done!");
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }

                return outputDir;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}
