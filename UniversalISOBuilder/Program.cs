using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace UniversalISOBuilder
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("Asu's Riivolution Universal ISO Builder - v0.5.0");
            Console.WriteLine("Special Beta Test Build - Please build all the mods you can think of with this ISO Builder and report me any bugs on discord: Asu-chan#2929");
            RiivolutionUniversalISOBuilder ruib = new RiivolutionUniversalISOBuilder();
            ruib.Main(args);
        }
    }

    public class RiivolutionUniversalISOBuilder
    {
        riivolutionXML riivolution = new riivolutionXML();

        public void Main(string[] args)
        {
            string ISOPath = "", xmlPath = "", rootPath = "", outPath = "";
            bool isSilent = false, deleteISO = true,  isThereDefinedPaths = false;
            if (args.Length > 0) // Arguments
            {
                if (args[0] == "-h" || args[0] == "--help" || args[0] == "/?")
                {
                    Console.WriteLine("A tool to patch Nintendo Wii ISO files using Riivolution XML files.\r\n\r\n");
                    Console.WriteLine("Usage: UniversalISOBuilder.exe <ISO Path> <Riivolution XML file path> <Root folder path> <Output ISO/WBFS path> [options]");
                    Console.WriteLine("       UniversalISOBuilder.exe [options]");
                    Console.WriteLine("       UniversalISOBuilder.exe");
                    Console.WriteLine("       In the 2nd and 3rd cases, you will be asked for the file paths.\r\n\r\n"); // Thanks to Mullkaw for correcting my weird-sounding english! ^^
                    Console.WriteLine("Options: --silent                  -> Prevents from displaying any console outputs apart from the necessary ones");
                    Console.WriteLine("         --keep-extracted-iso      -> Prevents the extractedISO folder from being deleted after the end of the process");
                    return;
                }

                if (args.Contains("--silent"))
                {
                    isSilent = true;
                    Console.WriteLine("Silent Mode: true");
                }

                if (args.Contains("--keep-extracted-iso"))
                {
                    deleteISO = false;
                }

                if (args[0].Contains(".iso"))
                {
                    ISOPath = args[0];
                    xmlPath = args[1];
                    rootPath = args[2];
                    outPath = args[3];
                    isThereDefinedPaths = true;
                }
            }

            if (isThereDefinedPaths) // Paths are already defined? Directly do the stuff.
            {
                if (!File.Exists(ISOPath) || !File.Exists(xmlPath))
                {
                    Console.WriteLine("Can't find ISO or XML file: No such file or directory.");
                }
                doStuff(ISOPath, xmlPath, rootPath, outPath, isSilent, deleteISO);
                return;
            }
            else // No paths defined? Ask for them.
            {
                // Getting ISO path
                Console.WriteLine("Please select an ISO file to patch.");
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    dialog.Filter = "Nintendo Wii ISO Rom File|*.iso|All files (*.*)|*.*";
                    dialog.FilterIndex = 1;
                    dialog.RestoreDirectory = true;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        Console.WriteLine("Please select a Riivolution XML file.");

                        // Getting XML path
                        using (OpenFileDialog dialog2 = new OpenFileDialog())
                        {
                            dialog2.Filter = "Riivolution Extensible Markup Language File|*.xml|All files (*.*)|*.*";
                            dialog2.FilterIndex = 1;
                            dialog2.RestoreDirectory = true;
                            if (dialog2.ShowDialog() == DialogResult.OK)
                            {
                                Console.WriteLine("Please select the root folder of your mod.");

                                // Getting root folder path

                                using (CommonOpenFileDialog dialog3 = new CommonOpenFileDialog())
                                {
                                    dialog3.IsFolderPicker = true;
                                    if (dialog3.ShowDialog() == CommonFileDialogResult.Ok)
                                    {
                                        Console.WriteLine("Please select where do you want your patched rom file to be saved.");

                                        // Getting output ISO/WBFS path
                                        SaveFileDialog textDialog;
                                        textDialog = new SaveFileDialog();
                                        textDialog.Filter = "Nintendo Wii ISO Rom File|*.iso|Nintendo Wii WBFS Rom File|*.wbfs|All files (*.*)|*.*";
                                        textDialog.DefaultExt = "wbfs";
                                        if (textDialog.ShowDialog() == DialogResult.OK)
                                        {
                                            System.IO.Stream fileStream = textDialog.OpenFile();
                                            System.IO.StreamWriter sw = new System.IO.StreamWriter(fileStream);
                                            outPath = ((FileStream)(sw.BaseStream)).Name;
                                            sw.Close();

                                            doStuff(dialog.FileName, dialog2.FileName, dialog3.FileName, outPath, isSilent, deleteISO);
                                        }
                                        else
                                        {
                                            Console.WriteLine("ISO/WBFS saving cancelled. Closing...");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Root folder selecting cancelled. Closing...");
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("XML selecting cancelled. Closing...");
                                return;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("ISO selecting cancelled. Closing...");
                        return;
                    }
                }
            }
        }

        public void doStuff(string ISOPath, string xmlPath, string rootPath, string outPath, bool isSilent, bool deleteISO)
        {

            XmlRootAttribute xRoot = new XmlRootAttribute();
            xRoot.ElementName = "wiidisc";
            xRoot.IsNullable = true;

            XmlSerializer serializer = new XmlSerializer(typeof(riivolutionXML), xRoot);
            MemoryStream stream = new MemoryStream(File.ReadAllBytes(xmlPath));
            riivolutionXML fullXML = (riivolutionXML)serializer.Deserialize(stream);

            if (!isSilent) { Console.WriteLine("XML Deserialized."); } else { Console.WriteLine("Please wait, this process can take up a few minutes...\r\nXML Deserialized."); }

            List<byte> id = new List<byte>();
            List<byte> name = new List<byte>();
            using (FileStream iso = new FileStream(ISOPath, FileMode.Open))
            {
                for (int i = 0; i < 8; i++)
                {
                    id.Add((byte)iso.ReadByte());
                }

                iso.Position = 0x20;

                while(true)
                {
                    byte nameByte = (byte)iso.ReadByte();
                    if (nameByte == 0)
                    {
                        break;
                    }
                    name.Add(nameByte);
                }
            }
            string titleID = Encoding.UTF8.GetString(id.ToArray(), 0, 6);
            string gameName = Encoding.UTF8.GetString(name.ToArray(), 0, name.Count);

            string gameID = titleID.Substring(0, 3);
            string region = titleID.Substring(3, 1);

            Console.WriteLine("TitleID found: " + titleID + "\r\nGame name found: " + gameName + "\r\n");

            List<string> supportedRegions = new List<string>();
            foreach(riivolutionIDRegion reg in fullXML.id.region)
            {
                supportedRegions.Add(reg.type);
            }
            if(!supportedRegions.Contains(region))
            {
                Console.WriteLine("Unsupported region (" + region + ").");
                return;
            }

            Console.WriteLine("Extracting ISO...");

            if (ISOPath.Contains(" "))
            {
                ISOPath = "\"" + ISOPath + "\"";
            }

            runCommand("tools\\wit.exe", "extract -s " + ISOPath + " -1 -n " + titleID + " . extractedISO --psel=DATA -ovv", isSilent);

            Console.WriteLine("ISO Extracted.\r\nCopying files from " + fullXML.patch[0].folder.Length + " folders...");

            string rootEnd = "";
            if (rootPath.Contains(" "))
            {
                rootPath = "\"" + rootPath;
                rootEnd = "\"";
            }

            System.Diagnostics.Process copy = new System.Diagnostics.Process();
            copy.StartInfo.FileName = "cmd.exe";
            copy.StartInfo.UseShellExecute = false;
            copy.StartInfo.RedirectStandardOutput = true;
            copy.StartInfo.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            foreach (riivolutionPatchFolder folder in fullXML.patch[0].folder)
            {
                // Region-changing folders
                if(folder.external.Contains("{$__region}"))
                {
                    if(!isSilent) { Console.WriteLine(folder.external + " is a region-changing folder, changing it to " + folder.external.Replace("{$__region}", region)); }
                    folder.external = folder.external.Replace("{$__region}", region);
                }

                // Creating unexisting folders
                if (folder.create && !Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc.Replace('/', '\\')))
                {
                    if (!isSilent) { Console.WriteLine("Creating directory " + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc.Replace('/', '\\')); }
                    runCommand("cmd.exe", "/C mkdir extractedISO\\files" + folder.disc.Replace('/', '\\'), isSilent);
                }

                // Copying files
                if ((folder.disc == null || folder.disc == "") ? true : Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc.Replace('/', '\\'))) // Avoid running useless copy commands for folders that doesn't exist AND don't have the "created" flag enabled (this is used for region-specific folders in NewerSMBW, for example)
                {
                    if (folder.disc != "" && folder.disc != null)
                    {
                        if (!isSilent) { Console.WriteLine("Copying " + rootPath + "\\" + folder.external.Replace('/', '\\') + rootEnd); }
                        copy.StartInfo.Arguments = "/C xcopy " + rootPath + "\\" + folder.external.Replace('/', '\\') + rootEnd + " extractedISO\\files" + folder.disc.Replace('/', '\\') + "\\ /E /C /I /Y";
                    }
                    else
                    {
                        if (!isSilent) { Console.WriteLine("Searching manually for files contained in " + rootPath + "\\" + folder.external + "\\" + rootEnd); }
                        foreach (string file in Directory.GetFiles((rootPath + "\\" + folder.external + "\\" + rootEnd).Replace("\"", "")))
                        {
                            if (!isSilent) { Console.WriteLine("Searching for " + file); }
                            string foundFile = ProcessDirectory(Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files\\", file.Split('\\')[file.Split('\\').Length - 1]);
                            if (foundFile != "")
                            {
                                if (!isSilent) { Console.WriteLine("Found file " + foundFile); }
                                copy.StartInfo.Arguments = "/C copy /b \"" + file + "\" \"" + foundFile + "\"";

                                copy.Start();
                                if (!isSilent) { Console.WriteLine(copy.StandardOutput.ReadToEnd()); }
                                copy.WaitForExit();
                            }
                            else
                            {
                                if (!isSilent) { Console.WriteLine("Cannot find file " + file + " in the disc\r\n"); }
                                continue;
                            }
                        }
                        if (!isSilent) { Console.WriteLine(""); } // Just for good-looking purposes.
                        continue;
                    }

                    copy.Start();
                    if (!isSilent) { Console.WriteLine(copy.StandardOutput.ReadToEnd()); } else { copy.StandardOutput.ReadToEnd(); }
                    copy.WaitForExit();
                }
            }
            
            Console.WriteLine("Files copied.\r\nPatching DOL...");

            runCommand("tools\\Asu's Dolpatcher.exe", "\"" + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\sys\\main.dol\" \"" + xmlPath + "\" \"" + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\sys\\main.dol\" --binary-files-dir " + rootPath + rootEnd + " --always-create-sections", isSilent);

            Console.WriteLine("Rebuilding...");

            runCommand("tools\\wit.exe", "copy extractedISO \"" + outPath + "\" -ovv --disc-id=" + titleID + " --tt-id=" + gameID + " --name \"" + gameName + " [MODDED]\"", isSilent);

            if(deleteISO)
            {
                if (!isSilent) { Console.WriteLine("Removing extractedISO directory..."); }
                runCommand("cmd.exe", "/C rmdir extractedISO /s /q", isSilent);
                if (!isSilent) { Console.WriteLine("Removed sucessfully"); }
            }

            Console.WriteLine("All done!");
        }

        // This part is from https://docs.microsoft.com/en-us/dotnet/api/system.io.directory.getfiles?view=net-5.0
        public static string ProcessDirectory(string targetDirectory, string wantedFile)
        {
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
            {
                string maybeFound = ProcessFile(fileName, wantedFile);
                if(maybeFound != "")
                {
                    return maybeFound;
                }
            }

            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
            {
                string maybeFound = ProcessDirectory(subdirectory, wantedFile);
                if (maybeFound != "")
                {
                    return maybeFound;
                }
            }

            return "";
        }
        public static string ProcessFile(string path, string wantedFile)
        {
            string filename = path.Split('\\')[path.Split('\\').Length - 1];
            if(filename == wantedFile)
            {
                return path;
            }
            return "";
        }

        public void runCommand(string executable, string arguments, bool isSilent)
        {
            System.Diagnostics.Process command = new System.Diagnostics.Process();
            command.StartInfo.FileName = executable;
            command.StartInfo.Arguments = arguments;
            command.StartInfo.UseShellExecute = false;
            command.StartInfo.RedirectStandardOutput = true;
            command.StartInfo.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath);
            command.Start();
            if (!isSilent) { Console.WriteLine(command.StandardOutput.ReadToEnd()); }
            command.WaitForExit();
        }
    }

    public class riivolutionXML
    {
        [XmlAttribute(AttributeName = "version")]
        public int version { get; set; }
        
        [XmlAttribute(AttributeName = "shiftfiles")]
        public bool shiftfiles { get; set; }
        
        [XmlAttribute(AttributeName = "root")]
        public string root { get; set; }

        [XmlAttribute(AttributeName = "log")]
        public bool log { get; set; }


        [XmlElement(ElementName = "id")]
        public riivolutionID id { get; set; }

        [XmlElement(ElementName = "options")]
        public riivolutionOptions options { get; set; }

        [XmlElement(ElementName = "patch")]
        public riivolutionPatch[] patch { get; set; }
    }

    public class riivolutionID
    {
        [XmlElement(ElementName = "region")]
        public riivolutionIDRegion[] region { get; set; }
    }

    public class riivolutionIDRegion
    {
        [XmlAttribute(AttributeName = "type")]
        public string type { get; set; }
    }
    
    public class riivolutionOptions
    {
        [XmlElement(ElementName = "section")]
        public riivolutionOptionsSection[] section { get; set; }
    }
    
    public class riivolutionOptionsSection
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }

        [XmlElement(ElementName = "option")]
        public riivolutionOptionsSectionOption[] option { get; set; }
    }
    
    public class riivolutionOptionsSectionOption
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }
        
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
        
        [XmlAttribute(AttributeName = "default")]
        public int defaultValue { get; set; }

        [XmlElement(ElementName = "choice")]
        public riivolutionOptionsSectionOptionChoice[] choice { get; set; }

    }
    
    public class riivolutionOptionsSectionOptionChoice
    {
        [XmlAttribute(AttributeName = "name")]
        public string name { get; set; }

        [XmlElement(ElementName = "patch")]
        public riivolutionOptionsSectionOptionChoicePatch[] patch { get; set; }
    }

    public class riivolutionOptionsSectionOptionChoicePatch
    {
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }
    }

    public class riivolutionPatch
    {
        [XmlAttribute(AttributeName = "id")]
        public string id { get; set; }

        [XmlElement(ElementName = "savegame")]
        public riivolutionPatchSavegame[] savegame { get; set; }

        [XmlElement(ElementName = "folder")]
        public riivolutionPatchFolder[] folder { get; set; }

        [XmlElement(ElementName = "memory")]
        public riivolutionPatchMemory[] memory { get; set; }
    }

    public class riivolutionPatchSavegame
    {
        [XmlAttribute(AttributeName = "external")]
        public string external { get; set; }

        [XmlAttribute(AttributeName = "clone")]
        public bool clone { get; set; }
    }

    public class riivolutionPatchFolder
    {
        [XmlAttribute(AttributeName = "external")]
        public string external { get; set; }

        [XmlAttribute(AttributeName = "disc")]
        public string disc { get; set; }

        [XmlAttribute(AttributeName = "create")]
        public bool create { get; set; }
    }

    public class riivolutionPatchMemory
    {
        [XmlAttribute(AttributeName = "offset")]
        public string offset { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string value { get; set; }

        [XmlAttribute(AttributeName = "original")]
        public string original { get; set; }

        [XmlAttribute(AttributeName = "valuefile")]
        public string valuefile { get; set; }
    }
}
