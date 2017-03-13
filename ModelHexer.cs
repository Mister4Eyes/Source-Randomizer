using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace Hl2_Randomizer
{
    class AutoHex
    {
        public static void autoHex(FileInfo Inputmdl, FileInfo hexMdl, DirectoryInfo outpDir)
        {

            modelGroup mg = new modelGroup();
            mg.mdl = Inputmdl;
            foreach (FileInfo fi in Inputmdl.Directory.GetFiles())
            {
                switch (fi.Extension)
                {
                    case ".vvd":
                        mg.vvd = fi;
                        break;

                    case ".phy":
                        mg.phy = fi;
                        break;

                    case ".vtx":
                        mg.vtxDx90 = fi;

                        break;
                }
            }
            if (mg.isInit)
            {
                if (!Directory.Exists(outpDir.FullName))
                {
                    Directory.CreateDirectory(outpDir.FullName);
                }
                string outputName = hexMdl.Name.Split('.')[0];

                Console.WriteLine("Hexing model "+mg.name);
                //Gets file input stream
                using (FileStream inp = new FileStream(mg.mdl.FullName, FileMode.Open))
                {
                    //Puts it in byte buffer
                    byte[] inpByte = new byte[inp.Length];
                    inp.Read(inpByte, 0, inpByte.Length);

                    try
                    {
                        //Gets hex file info
                        using (FileStream hexImp = new FileStream(hexMdl.FullName, FileMode.Open))
                        {
                            //Puts it into byte buffer
                            byte[] hexByte = new byte[hexImp.Length];
                            hexImp.Read(hexByte, 0, hexByte.Length);

                            //Deletes file if exists
                            if (File.Exists(outpDir + "\\" + hexMdl.Name))
                            {
                                File.Delete(outpDir + "\\" + hexMdl.Name);
                            }

                            //Outputs data
                            using (FileStream output = new FileStream(outpDir + "\\" + hexMdl.Name, FileMode.CreateNew))
                            {
                                for (int i = 0; i < inpByte.Length; i++)
                                {
                                    if (i > 11 && i < 77)
                                    {
                                        output.WriteByte(hexByte[i]);
                                    }
                                    else
                                    {
                                        output.WriteByte(inpByte[i]);
                                    }
                                }
                            }
                        }
                    }catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (mg.phy != null)
                {
                    mg.phy.CopyTo(outpDir.FullName + "\\" + outputName + ".phy", true);
                }
                mg.vvd.CopyTo(outpDir.FullName + "\\" + outputName + ".vvd", true);
                mg.vtxDx90.CopyTo(outpDir.FullName + "\\" + outputName + ".dx90.vtx", true);
            }
        }
    }

    struct modelGroup
    {
        public bool isInit
        {
            get
            {
                if (smdl == null)
                {
                    return false;
                }
                if (svvd == null)
                {
                    return false;
                }
                if (svtxDx80 == null && svtxDx90 == null && svtxSw == null)
                {
                    return false;
                }
                return true;
            }
        }
        public FileInfo mdl
        {
            get
            {
                return smdl;
            }
            set
            {
                if (value.Extension.Equals(".mdl"))
                {
                    if (name.Equals(""))
                    {
                        smdl = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        smdl = value;
                    }
                }
            }
        }
        public FileInfo vvd
        {
            get
            {
                return svvd;
            }
            set
            {
                if (value.Extension.Equals(".vvd"))
                {
                    if (name.Equals(""))
                    {
                        svvd = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        svvd = value;
                    }
                }
            }
        }
        public FileInfo phy
        {
            get
            {
                return sphy;
            }
            set
            {
                if (value.Extension.Equals(".phy"))
                {
                    if (name.Equals(""))
                    {
                        sphy = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        sphy = value;
                    }
                }
            }
        }
        public FileInfo vtxDx80
        {
            get
            {
                return svtxDx90;
            }
            set
            {
                if (value.Extension.Equals(".vtx") && value.Name.Split('.')[1].Equals("dx80"))
                {
                    if (name.Equals(""))
                    {
                        svtxDx90 = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        svtxDx90 = value;
                    }
                }
            }
        }
        public FileInfo vtxDx90
        {
            get
            {
                return svtxDx90;
            }
            set
            {
                if (value.Extension.Equals(".vtx") && value.Name.Split('.')[1].Equals("dx90"))
                {
                    if (name.Equals(""))
                    {
                        svtxDx90 = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        svtxDx90 = value;
                    }
                }
            }
        }
        public FileInfo vtxSw
        {
            get
            {
                return svtxSw;
            }
            set
            {
                if (value.Extension.Equals(".vtx") && value.Name.Split('.')[1].Equals("sw"))
                {
                    if (name.Equals(""))
                    {
                        svtxSw = value;
                    }
                    else if (value.Name.Split('.')[0].Equals(name))
                    {
                        svtxSw = value;
                    }
                }
            }
        }
        public string name
        {
            get
            {
                if (smdl != null)
                {
                    return smdl.Name.Split('.')[0];
                }
                if (svvd != null)
                {
                    return svvd.Name.Split('.')[0];
                }
                if (sphy != null)
                {
                    return sphy.Name.Split('.')[0];
                }

                if (svtxDx80 != null)
                {
                    return svtxDx80.Name.Split('.')[0];
                }
                if (svtxDx90 != null)
                {
                    return svtxDx90.Name.Split('.')[0];
                }
                if (svtxSw != null)
                {
                    return svtxSw.Name.Split('.')[0];
                }
                return "";
            }
        }
        FileInfo smdl;
        FileInfo svvd;
        FileInfo sphy;
        FileInfo svtxDx80;
        FileInfo svtxDx90;
        FileInfo svtxSw;
    }
}
