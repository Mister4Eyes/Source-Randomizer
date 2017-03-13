using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using NAudio.Wave;
using Fancy_Interface;
using System.Threading.Tasks;

namespace Hl2_Randomizer
{
    class Program
    {
        static Random r = new Random();
        static string input_Directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        static List<FileInfo> foundFiles = new List<FileInfo>();
        static FileInfo[][] fileTypes;
        static TaskFactory tf = new TaskFactory();
        static string title = "";
        static bool verbose;
		static bool isolateVoice;

        #region functions
        static bool isCustom(FileInfo fi)
        {
            string dir = fi.FullName.Substring(AppDomain.CurrentDomain.BaseDirectory.Length);
            
            return dir.Contains("custom\\") || dir.Contains("converted_images\\");
        }
        static void getFiles(DirectoryInfo dir)
        {
            //Adds all files in the directory
            if (dir.Exists)
            {
                foreach (FileInfo fi in
                dir.GetFiles())
                {
                    if (verbose)
                    {
                        Console.WriteLine(fi.Name + fi.Extension);
                    }
                    foundFiles.Add(fi);
                }

                //Loops through directories in file
                foreach (DirectoryInfo di in dir.GetDirectories())
                {
                    if (verbose)
                    {
                        Console.WriteLine("Entering {0}...", di.Name);
                    }
                    getFiles(di);
                    if (verbose)
                    {
                        Console.WriteLine("Exeted {0}!", di.Name);
                    }
                }
            }
        }

        static void seperate()
        {
            List<string> extentions = new List<string>();

            //Loops through all files and adds all unique extentions
            foreach (FileInfo f in foundFiles)
            {
                foreach (string ext in extentions)
                {
                    if (f.Extension.Equals(ext))
                    {
                        goto not_unique;
                    }
                }
                extentions.Add(f.Extension);
                not_unique:;
            }

            //Adds files in appropriate sopts
            List<FileInfo>[] files = new List<FileInfo>[extentions.Count];

            for(int i = 0; i < files.Length; i++)
            {
                files[i] = new List<FileInfo>();
            }

            foreach (FileInfo f in foundFiles)
            {
                for (int i = 0; i < extentions.Count; i++)
                {
                    if (f.Extension.Equals(extentions[i]))
                    {
                        files[i].Add(f);
                        break;
                    }
                }
            }

            //Converts last bit of info into array
            FileInfo[][] file = new FileInfo[files.Length][];

            for (int i = 0; i < file.Length; i++)
            {
                file[i] = files[i].ToArray();
            }

            fileTypes = file;
        }

        //Adds a file in the directory
        public static void addFile(int ext, int row, DirectoryInfo i, DirectoryInfo o, List<FileInfo> assets)
        {
            string newDir = o.FullName+fileTypes[ext][row].Directory.FullName.Substring(i.FullName.Length);

            //Cheaks if directory exists
            if(!new DirectoryInfo(newDir).Exists)
            {
                Directory.CreateDirectory(newDir);
            }

            string newFil = newDir +"\\"+fileTypes[ext][row].Name;

            if (File.Exists(newFil))
            {
                File.Delete(newFil);
            }
            if (verbose)
            {
                Console.WriteLine(fileTypes[ext][row].Name);
            }
            File.Copy(assets[r.Next(assets.Count)].FullName, newFil);

        }

        //Converts Mp3 To Wav
        private static void ConvertMp3ToWav(string _inPath_, string _outPath_)
        {
            using (Mp3FileReader mp3 = new Mp3FileReader(_inPath_))
            {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                {
                    WaveFileWriter.CreateWaveFile(_outPath_, pcm);
                }
            }
        }

        //Keeps the structure of the previous method even though the means are completly different
        private static void ConvertWavToMp3(string _inPath_, string _outPath_)
        {
            byte[] data = File.ReadAllBytes(_inPath_);
            File.WriteAllBytes(_outPath_, WavToMP3(data));
        }

        public static byte[] WavToMP3(byte[] wavFile)
        {
            using (MemoryStream source = new MemoryStream(wavFile))
            using (WaveFileReader rdr = new WaveFileReader(source))
            {
                WaveLib.WaveFormat fmt = new WaveLib.WaveFormat(rdr.WaveFormat.SampleRate, rdr.WaveFormat.BitsPerSample, rdr.WaveFormat.Channels);

                // convert to MP3 at 96kbit/sec...
                Yeti.Lame.BE_CONFIG conf = new Yeti.Lame.BE_CONFIG(fmt, 96);

                // Allocate a 1-second buffer
                int blen = rdr.WaveFormat.AverageBytesPerSecond;
                byte[] buffer = new byte[blen];

                // Do conversion
                using (MemoryStream output = new MemoryStream())
                {
                    Yeti.MMedia.Mp3.Mp3Writer mp3 = new Yeti.MMedia.Mp3.Mp3Writer(output, fmt, conf);

                    int readCount;
                    while ((readCount = rdr.Read(buffer, 0, blen)) > 0)
                        mp3.Write(buffer, 0, readCount);

                    mp3.Close();
                    return output.ToArray();
                }
            }
        }

        //Converts images to vtf files using an external process due to not being able to find adequit processes
        static void imagesToVTF(bool force)
        {
            //Cheaks if directory exists
            if (!Directory.Exists(input_Directory + "\\converted_images"))
            {
                Directory.CreateDirectory(input_Directory + "\\converted_images");
            }

            //List to store running processes

            List<Task> tasks = new List<Task>();

            foreach (FileInfo file in foundFiles)
            {
                //Checks if file is an image
                if(file.Extension.Equals(".bmp")|| file.Extension.Equals(".dds")|| file.Extension.Equals(".jpg") || file.Extension.Equals(".jpeg") || file.Extension.Equals(".png")|| file.Extension.Equals(".tga"))
                {
                    if (!force)
                    {
                        foreach (FileInfo fi in new DirectoryInfo(input_Directory + "\\converted_images").GetFiles())
                        {
                            if (fi.Name.Split('.')[0].Equals(file.Name.Split('.')[0]))
                            {
                                goto skip;
                            }
                        }
                    }

                    ret:
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i].IsCompleted)
                        {
                            if (verbose)
                            {
                                Console.WriteLine(tasks[i].Id + " done");
                            }
                            tasks.RemoveAt(i);
                            i--;
                        }
                    }
                    if (tasks.Count >= Environment.ProcessorCount)
                    {
                        goto ret;
                    }
                    else
                    {
                        tasks.Add(tf.StartNew(() => processWatcher(input_Directory + "\\bin\\VTFCmd.exe", "-file \"" + file.FullName + "\" -resize -output \"" + input_Directory + "\\converted_images\"")));
                    }

                    skip:;
                }
            }
            foreach (Task t in tasks)
            {
                try
                {
                    t.Wait();
                    if (verbose)
                    {
                        Console.WriteLine(t.Id + " done");
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            
            
        }

        private static void addingSounds(DirectoryInfo i, DirectoryInfo o, bool Repaint)
        {
            int wavLoc = -1;
            int mp3Loc = -1;

            for(int j = 0; j < fileTypes.Length; j++)
           {
                switch (fileTypes[j][0].Extension)
                {
                    case ".wav":
                        wavLoc = j;
                        break;

                    case ".mp3":
                        mp3Loc = j;
                        break;
                }
            }

            List<FileInfo> files = new List<FileInfo>();
            List<FileInfo> assets = new List<FileInfo>();
            if(wavLoc != -1)
            {
                files.AddRange(fileTypes[wavLoc]);

                //Sets all info to 0 in order to not have the randomizer randomize it again
                fileTypes[wavLoc] = new FileInfo[0];
            }

            if(mp3Loc != -1)
            {
                files.AddRange(fileTypes[mp3Loc]);

                //Sets all info to 0 in order to not have the randomizer randomize it again
                fileTypes[mp3Loc] = new FileInfo[0];
            }

            foreach (FileInfo fi in files)
            {
                if (!((Repaint ^ isCustom(fi)) && Repaint))
                {
                    assets.Add(fi);
                }
            }

			List<FileInfo> Voice = new List<FileInfo>();
			List<FileInfo> Sound = new List<FileInfo>();

			if (isolateVoice)
			{
				foreach (FileInfo fi in assets)
				{
					if (fi.FullName.Contains("vo"))
					{
						Voice.Add(fi);
					}
					else
					{
						Sound.Add(fi);
					}
				}
			}

			if (assets.Count > 0)
            {
                foreach (FileInfo fi in files)
                {
                    if (!isCustom(fi))
                    {
                        string newDir = o.FullName + fi.Directory.FullName.Substring(i.FullName.Length);

                        string newFil = newDir + "\\" + fi.Name;
                        if (File.Exists(newFil))
                        {
                            File.Delete(newFil);
                        }

						failure:

						FileInfo randFile;

						if (isolateVoice)
						{
							//Checks if in voice.
							if (Voice.Contains(fi))
							{
								randFile = Voice[r.Next(Voice.Count)];
							}
							else
							{
								randFile = Sound[r.Next(Sound.Count)];
							}
						}
						else
						{
							//Goes about your day normally
							randFile = assets[r.Next(assets.Count)];
						}

                        if (!Directory.Exists(newDir))
                        {
                            Directory.CreateDirectory(newDir);
                        }

                        if (verbose)
                        {
                            Console.WriteLine(fi.Name);
                        }

                        if (fi.Extension == randFile.Extension)
                        {
                            File.Copy(randFile.FullName, newFil);
                        }
                        else
                        {
                            try
                            {
                                switch (fi.Extension)
                                {
                                    case ".wav":

                                        ConvertMp3ToWav(randFile.FullName, newFil);
                                        break;

                                    case ".mp3":

                                        ConvertWavToMp3(randFile.FullName, newFil);
                                        break;

                                    default:
                                        if (verbose)
                                        {
                                            Console.WriteLine("Unknown file type");
                                        }

                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                if (verbose)
                                {
                                    Console.WriteLine("Error: " + e.Message + " Restarting...");
                                }
                                goto failure;
                            }
                        }
                    }
                }
            }else
            {
                Console.WriteLine("Failed to find assets");
            }
        }

        private static string[] fileExtentions()
        {
            List<string> foundExtend = new List<string>();
            foreach(FileInfo fi in foundFiles)
            {
                foreach(string str in foundExtend)
                {
                    if (fi.Extension.Equals(str))
                    {
                        goto fail;
                    }
                }
                foundExtend.Add(fi.Extension);
                fail:;
            }

            return foundExtend.ToArray();
        }

        private static void extractAssets(DirectoryInfo dir, DirectoryInfo Out)
        {
            string vpkExtract = input_Directory + "\\bin\\vpk.exe";

            DirectoryInfo[] dirs = dir.GetDirectories();
            string[] strs = new string[dirs.Length];
            for (int i = 0; i < dirs.Length; i++)
            {
                strs[i] = dirs[i].Name;
            }

            bool[] includeDirs = FancyInterface.fancySelect(strs, "Select directories to be included in asset extraction.");
            Console.WriteLine("Beginning extraction of assets");

            List<Task> tasks = new List<Task>();
            for(int i = 0; i < dirs.Length; i++)
            {
                if (includeDirs[i])
                {
                    Improved_Extraction.improvedExtraction(vpkExtract, dirs[i].FullName, Out.FullName, verbose);
                }
            }
        }

        private static void FilesToVtf(DirectoryInfo dir)
        {
            string vtfExtract = input_Directory + "\\bin\\vpk.exe";
            List<Task> tasks = new List<Task>();
            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ret:
                for(int i = 0; i < tasks.Count; i++)
                {
                    if (tasks[i].IsCompleted)
                    {
                        if (verbose)
                        {
                            Console.WriteLine(tasks[i].Id + " done");
                        }
                        tasks.RemoveAt(i);
                        i--;
                    }
                }
                if(tasks.Count >= Environment.ProcessorCount)
                {
                    goto ret;
                }
                else
                {
                    tasks.Add(tf.StartNew(() => processWatcher(vtfExtract, "-M \"" + di.FullName + "\"")));
                }
            }
            foreach(Task t in tasks)
            {
                t.Wait();
                if (verbose)
                {
                    Console.WriteLine(t.Id + " done");
                }
            }
        }

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

        private static void appendToTitle(string str)
        {
            title += str;
            Console.Title = title;
        }

        private static void randomizeModels(DirectoryInfo i, DirectoryInfo o, double strength)
        {
            List<FileInfo> models = new List<FileInfo>();
            for(int j = 0; j < fileTypes.Length; j++)
            {
                if(fileTypes[j].Length != 0)
                {
                    if (fileTypes[j][0].Extension.Equals(".mdl"))
                    {
                        models = new List<FileInfo>(fileTypes[j]);
                        fileTypes[j] = new FileInfo[0];
                        break;
                    }
                }
            }
            if(models.Count > 0)
            {
                for(int j = 0; j < models.Count; j++)
                {
                    if(r.NextDouble() < strength)
                    {
                        string newDir = o.FullName + models[j].Directory.FullName.Substring(i.FullName.Length);
                        AutoHex.autoHex(models[r.Next(models.Count)], models[j], new DirectoryInfo(newDir));
                    }
                }
            }
        }
        #endregion
        static void Main(string[] args)
        {
            appendToTitle("Source Randomizer");
            string output_Directory;
            foreach (string dir in args)
            {
                if (Directory.Exists(dir))
                {
                    output_Directory = dir;
                    goto found;
                }
            }

            //If a directory was not found the code will run into this
            while (true)
            {
                Console.WriteLine("Enter Game directory");
                output_Directory = Console.ReadLine();
                if (Directory.Exists(output_Directory))
                {
                    break;
                }
                else
                {
                    Console.Clear();
                }
            }
            //Skips over while true if the directory is found
            found:
            bool[] options = FancyInterface.fancySelect(new[] {"Advanced options",
                                                             "Beep on completion",
                                                                 "Verbose output",
                                                             "Normalmap swapping",
                                                              "Use Custom assets",
                                                           "Force asset grabbing",
                                                           "Force VTF conversion",
                                                            "Output files as VPK",
                                                      "Run game after completion",
                                                                   "Repaint Mode",
																  "Isolate Voice"}, "Options:");
            bool swapNormals = options[3];
            bool forceAsset = options[5];
            bool forceVTF = options[6];
            bool outputVPK = options[7];
            bool useCustom = options[4];
            bool runGame = options[8];
            verbose = options[2];
            bool advanced = options[0];
            bool beep = options[1];
            bool RepaintMode = options[9];
			isolateVoice = options[10];

            //Gets the directory info in the specified files
            DirectoryInfo output = new DirectoryInfo(output_Directory);
            bool sounds = false;
            bool textures = false;
            bool models = false;
            double strength = 1;
            if (!advanced)
            {
                options = FancyInterface.fancySelect(new[] { "Sounds", "Textures", "Models" }, "Select type to randomize");
                sounds = options[0];
                textures = options[1];
                models = options[2];
                if (models)
                {
					while (true)
					{
						Console.Clear();
						Console.WriteLine("Enter strength of model randomization \n(a value between 0 and 1 where 1 is all models swapped and 0 is none)");

						string str = Console.ReadLine();
						double num;
						if (double.TryParse(str, out num))
						{
							if (0 <= num && num <= 1)
							{
								strength = num;
								break;
							}
						}
					}
                }
            }
            //gets files
            DirectoryInfo[] dirs = output.GetDirectories();
            string[] strs = new string[dirs.Length];

            for(int i = 0; i < dirs.Length; i++)
            {
                strs[i] = dirs[i].Name;
            }

            int directory = FancyInterface.fancyItemSelect(strs, "Select Game Directory");
            DirectoryInfo assetFolder = new DirectoryInfo(input_Directory + "\\"+dirs[directory].Name);
            appendToTitle(" (" + dirs[directory].Name + ")");

            if (assetFolder.Exists)
            {
                if (forceAsset)
                {
                    Console.WriteLine("Clearing assets...");
                    Directory.Delete(assetFolder.FullName, true);
                    extractAssets(output, assetFolder);
                }
            }
            else
            {
                Directory.CreateDirectory(assetFolder.FullName);
                extractAssets(output, assetFolder);
                assetFolder = new DirectoryInfo(input_Directory + "\\" + dirs[directory].Name);
            }

            if (useCustom)
            {
                Console.WriteLine("Getting Custom Assets...");
                getFiles(new DirectoryInfo(input_Directory + "\\custom"));

                Console.WriteLine("Converting images to vtf...");
                imagesToVTF(forceVTF);
            }

            Console.WriteLine("Getting converted files...");
            getFiles(new DirectoryInfo(input_Directory + "\\converted_images"));

            Console.WriteLine("Getting Assets...");
            getFiles(assetFolder);

            string[] extentions = fileExtentions();
            bool[] acceptExtentions = new bool[extentions.Length];
            if (advanced)
            {
                acceptExtentions = FancyInterface.fancySelect(extentions, "Select extentions to be used/swapped.");
            }
            else
            {
                for(int i = 0; i < extentions.Length; i++)
                {
                    //Cheaks for sound filetypes
                    if (sounds)
                    {
                        if(extentions[i].Equals(".mp3") || extentions[i].Equals(".wav"))
                        {
                            acceptExtentions[i] = true;
                        }
                    }

                    //Cheaks for texture filetypes
                    if (textures)
                    {
                        if (extentions[i].Equals(".vtf"))
                        {
                            acceptExtentions[i] = true;
                        }
                    }

                    //Cheaks for model filetypes
                    if (models)
                    {
                        if (extentions[i].Equals(".mdl"))
                        {
                            acceptExtentions[i] = true;
                        }
                    }
                }
            }

            List<FileInfo> acceptedFiles = new List<FileInfo>();
            foreach(FileInfo fi in foundFiles)
            {
                for(int i = 0; i < extentions.Length; i++)
                {
                    if (acceptExtentions[i])
                    {
                        if (fi.Extension.Equals(extentions[i]))
                        {
                            acceptedFiles.Add(fi);
                            break;
                        }
                    }
                }
            }
            
            foundFiles = acceptedFiles;
            if (!swapNormals)
            {
                Console.WriteLine("Deleting normal maps...");
                for(int i = 0; i < foundFiles.Count; i++)
                {
                    FileInfo fi = foundFiles[i];
                    //Commpleted before images are converted just in case some asshole names their images with a normal inside of it
                    if (fi.Extension.Equals(".vtf"))
                    {
                        if (fi.Name.Contains("normal"))
                        {
                            if (verbose)
                            {
                                Console.WriteLine("Removed " + fi.Name);
                            }
                            foundFiles.RemoveAt(i);
                            //Decrements i so the next file can be read
                            i--;
                        }
                    }
                }
            }

            //seperates files
            Console.WriteLine("Seperating files...");
            seperate();

            if (new DirectoryInfo(output + "\\" + dirs[directory].Name + "\\custom").Exists)
            {
                Console.WriteLine("Clearing old randomization");
				while (true)
				{
					try
					{
						Directory.Delete(output + "\\" + dirs[directory].Name + "\\custom", true);
						break;
					}
					catch (Exception) { /*Suppresses Exceptions*/ }
				}

                Directory.CreateDirectory(output + "\\" + dirs[directory].Name + "\\custom");
            }
            Console.WriteLine("Adding files...");

            //Does sounds first
            if (sounds)
            {
                addingSounds(new DirectoryInfo(input_Directory), new DirectoryInfo(output + "\\" + dirs[directory].Name + "\\custom"), RepaintMode);
            }

            //Next does models
            if (models)
            {
                randomizeModels(new DirectoryInfo(input_Directory), new DirectoryInfo(output + "\\" + dirs[directory].Name + "\\custom"), strength);
            }

            //Does rest of files
            List<FileInfo> assets = new List<FileInfo>();
            for(int i = 0; i < fileTypes.Length; i++)
            {
                for(int j = 0; j < fileTypes[i].Length; j++)
                {
                    if (!((RepaintMode ^ isCustom(fileTypes[i][j])) && RepaintMode))
                    {
                        assets.Add(fileTypes[i][j]);
                    }
                }
            }

            if(assets.Count > 0)
            {
                for (int i = 0; i < fileTypes.Length; i++)
                {
                    for (int j = 0; j < fileTypes[i].Length; j++)
                    {
                        addFile(i, j, new DirectoryInfo(input_Directory), new DirectoryInfo(output + "\\" + dirs[directory].Name + "\\custom"), assets);
                    }
                }
            }else
            {
                Console.WriteLine("Failed to find assets.");
            }

            //Outputs as VPK
            if (outputVPK)
            {
                FilesToVtf(new DirectoryInfo(output.FullName+"\\" +dirs[directory].Name+ "\\custom"));
                foreach(DirectoryInfo di in new DirectoryInfo(output.FullName + "\\" + dirs[directory].Name + "\\custom").GetDirectories())
                {
                    di.Delete(true);
                }
            }

            //Runs game after completion
            if (runGame)
            {
                Console.WriteLine("Finding game file");
                foreach (FileInfo fi in output.GetFiles())
                {
                    if (fi.Extension.Equals(".exe"))
                    {
                        Console.WriteLine("Starting game");
                        Process p = new Process();
                        p.StartInfo.FileName = fi.FullName;
                        p.StartInfo.Arguments = "-game " + dirs[directory].Name;
                        p.Start();
                        if (beep)
                        {
                            Console.Beep(800, 750);
                        }
                        Console.WriteLine("Game started");
                        p.WaitForExit();
                        Console.WriteLine("Game ended");
                       
                        return;
                    }
                }

                Console.WriteLine("Couldn't find game");
                if (beep)
                {
                    Console.Beep(800, 750);
                }
            }
            else
            {
                Console.WriteLine("Complete");
                if (beep)
                {
                    Console.Beep(800, 750);
                }
            }
        }
    }

    struct modelInfo
    {
        public string name
        {
            get
            {
                if (mdlFil != null)
                {
                    return mdlFil.Name.Split('.')[0];
                } else if (phyFil != null)
                {
                    return phyFil.Name.Split('.')[0];
                }

                //Vtf files
                else if (vtxdx80Fil != null)
                {
                    return vtxdx80Fil.Name.Split('.')[0];
                }
                else if (vtxdx90Fil != null)
                {
                    return vtxdx90Fil.Name.Split('.')[0];
                }
                else if (vtxswFil != null)
                {
                    return vtxswFil.Name.Split('.')[0];
                }

                else if (vvdFil != null)
                {
                    return vvdFil.Name.Split('.')[0];
                }
                else if (aniFil != null)
                {
                    return aniFil.Name.Split('.')[0];
                }
                else
                {
                    return "";
                }
            }
        }
        public FileInfo mdlFil;
        public FileInfo phyFil;

        public FileInfo vtxdx80Fil;
        public FileInfo vtxdx90Fil;
        public FileInfo vtxswFil;

        public FileInfo vvdFil;
        public FileInfo aniFil;
    }
}
