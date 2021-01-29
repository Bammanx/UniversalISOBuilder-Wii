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
            Console.WriteLine("Asu's Riivolution Universal ISO Builder - v0.6.0");
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
            string ISOPath = "", xmlPath = "", rootPath = "", outPath = "", singleChoice = "Ask", newTitleID = "";
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
                    Console.WriteLine("         --always-single-choice    -> Enables by default any option that has only one choice");
                    Console.WriteLine("         --never-single-choice     -> Disable by default any option that has only one choice");
                    Console.WriteLine("         --title-id <TitleID>      -> Changes the TitleID of the output rom; Replace with dots the characters that should be kept");
                    Console.WriteLine("         --keep-extracted-iso      -> Prevents the extractedISO folder from being deleted after the end of the process");
                    return;
                }

                if (args.Contains("--silent"))
                {
                    isSilent = true;
                    Console.WriteLine("Silent Mode: true");
                }

                if (args.Contains("--always-single-choice"))
                {
                    singleChoice = "Always";
                }
                else if (args.Contains("--never-single-choice"))
                {
                    singleChoice = "Never";
                }

                if (args.Contains("--title-id"))
                {
                    newTitleID = args[Array.IndexOf(args, "--title-id") + 1];
                    if(newTitleID.Length != 6) { Console.WriteLine("Invalid TitleID " + newTitleID + "."); return; }
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
                doStuff(ISOPath, xmlPath, rootPath, outPath, singleChoice, newTitleID, isSilent, deleteISO);
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

                                            doStuff(dialog.FileName, dialog2.FileName, dialog3.FileName, outPath, singleChoice, newTitleID, isSilent, deleteISO);
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

        public void doStuff(string ISOPath, string xmlPath, string rootPath, string outPath, string singleChoice, string newTitleID, bool isSilent, bool deleteISO)
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

            if(fullXML.id.game != gameID)
            {
                Console.WriteLine("This riivolution patch only applied to the game that has the TitleID " + fullXML.id.game + ".\r\nYours uses the TitleID " + gameID + " and therefore cannot be patched.");
                return;
            }

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

            if(newTitleID == "")
            {
                Console.WriteLine("No custom Title ID were specified, therefore your mod will use the same save slot as the game you're modding, which could cause issues with some mods!");
            }

            Console.WriteLine("Extracting ISO...");

            if (ISOPath.Contains(" "))
            {
                ISOPath = "\"" + ISOPath + "\"";
            }

            runCommand("tools\\wit.exe", "extract -s " + ISOPath + " -1 -n " + titleID + " . extractedISO --psel=DATA -ovv", isSilent);

            Console.WriteLine("ISO Extracted.");

            List<riivolutionPatch> patches = new List<riivolutionPatch>();

            //Console.WriteLine("More than one patch was found! Please choose which patches do you want to enable:\r\n");*/

            if (!(fullXML.options.section.Length == 1 && fullXML.options.section[0].option.Length == 1 && fullXML.options.section[0].option[0].choice.Length == 1))
            {
                foreach (riivolutionOptionsSection section in fullXML.options.section)
                {
                    Console.WriteLine("-Section: \"" + section.name + "\"");
                    foreach (riivolutionOptionsSectionOption option in section.option)
                    {
                        Console.WriteLine("  Option: \"" + option.name + "\"");
                        if (option.choice.Length > 1)
                        {
                            Console.WriteLine("   Choices available:\r\n    0. None");
                            List<string> choices = new List<string>();
                            for (int i = 0; i < option.choice.Length; i++)
                            {
                                Console.WriteLine("    " + (i + 1) + ". " + option.choice[i].name);
                                choices.Add(option.choice[i].name);
                            }
                            int choosed = -1;
                            while (true)
                            {
                                Console.Write("Please enter the number of the choice you want to use: ");
                                try {
                                    choosed = Convert.ToInt32(Console.ReadLine());
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("That's not a number!");
                                    continue;
                                }

                                if (choosed <= choices.Count && choosed >= 0)
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("This isn't a valid choice!");
                                }
                            }
                            if(choosed == 0)
                            {
                                continue;
                            }
                            foreach (riivolutionOptionsSectionOptionChoicePatch patch in option.choice[choosed - 1].patch)
                            {
                                patches.Add(fullXML.patch[findPatchIndexByName(patch.id, fullXML.patch)]);
                            }
                        }
                        else
                        {
                            string answer = "";
                            if (singleChoice == "Ask")
                            {
                                Console.Write("   Only one choice found: " + option.choice[0].name + " - Use it? (Yes/No): ");
                                answer = Console.ReadLine();
                            }
                            else if (singleChoice == "Always")
                            {
                                answer = "yes";
                            }

                            if (answer.Equals("yes", StringComparison.InvariantCultureIgnoreCase) || answer.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                            {
                                foreach (riivolutionOptionsSectionOptionChoicePatch patch in option.choice[0].patch)
                                {
                                    patches.Add(fullXML.patch[findPatchIndexByName(patch.id, fullXML.patch)]);
                                }
                            }
                        }
                    }
                }
            }
            else // No need to ask what to enable and what to disable if there's only one possiblity.
            {
                foreach (riivolutionOptionsSectionOptionChoicePatch patch in fullXML.options.section[0].option[0].choice[0].patch)
                {
                    patches.Add(fullXML.patch[findPatchIndexByName(patch.id, fullXML.patch)]);
                }
            }

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

            foreach(riivolutionPatch patch in patches)
            {
                if(patch.folder == null) { patch.folder = new riivolutionPatchFolder[0]; }
                if(patch.folder.Length > 0)
                {
                    Console.WriteLine("Copying files from " + patch.folder.Length + " folders...");
                }
                else
                {
                    if (!isSilent) { Console.WriteLine("No folder patches found for patch " + patch.id); }
                }

                foreach (riivolutionPatchFolder folder in patch.folder)
                {
                    // Changing some stuff
                    if(folder.external != "" && folder.external != null)
                    {
                        folder.external = folder.external.Replace('/', '\\');
                        if (folder.external.EndsWith("\\")) { folder.external = folder.external.Substring(0, folder.external.Length - 1); }
                    }

                    if(folder.disc != "" && folder.disc != null)
                    {
                        folder.disc = folder.disc.Replace('/', '\\');
                        if (folder.disc.EndsWith("\\")) { folder.disc = folder.disc.Substring(0, folder.disc.Length - 1); }
                    }

                    // Region-changing folders
                    if (folder.external.Contains("{$__region}"))
                    {
                        if (!isSilent) { Console.WriteLine(folder.external + " is a region-changing folder, changing it to " + folder.external.Replace("{$__region}", region)); }
                        folder.external = folder.external.Replace("{$__region}", region);
                    }

                    // Creating unexisting folders
                    if (folder.create && !Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc))
                    {
                        if (!isSilent) { Console.WriteLine("Creating directory " + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc); }
                        runCommand("cmd.exe", "/C mkdir extractedISO\\files" + folder.disc, isSilent);
                    }

                    // Copying files
                    if ((folder.disc == null || folder.disc == "") ? true : Directory.Exists(Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\files" + folder.disc)) // Avoid running useless copy commands for folders that doesn't exist AND don't have the "created" flag enabled (this is used for region-specific folders in NewerSMBW, for example)
                    {
                        if (folder.disc != "" && folder.disc != null)
                        {
                            if (!isSilent) { Console.WriteLine("Copying " + rootPath + "\\" + folder.external + rootEnd); }
                            copy.StartInfo.Arguments = "/C xcopy " + rootPath + "\\" + folder.external + rootEnd + " extractedISO\\files" + folder.disc + "\\ /E /C /I /Y";
                        }
                        else if(Directory.Exists((rootPath + "\\" + folder.external).Replace("\"", "")))
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

                Console.WriteLine("Files copied.\r\nPatching DOL...\r\n");

                runCommand("tools\\Asu's Dolpatcher.exe", "\"" + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\sys\\main.dol\" \"" + xmlPath + "\" \"" + Path.GetDirectoryName(Application.ExecutablePath) + "\\extractedISO\\sys\\main.dol\" --binary-files-dir " + rootPath + rootEnd + " --only-patches \"" + patch.id + "\" --region " + region + " --always-create-sections", isSilent);

            }

            Console.WriteLine("Rebuilding...");

            if(newTitleID != "")
            {
                string oldtid = titleID;
                char[] oldttid = titleID.ToCharArray();
                char[] newttid = newTitleID.ToCharArray();
                for(int i = 0; i < 6; i++)
                {
                    if(newttid[i] == '.')
                    {
                        continue;
                    }
                    oldttid[i] = newttid[i];
                }
                titleID = new string(oldttid);
                gameID = titleID.Substring(0, 3);
                Console.WriteLine("Changing TitleID from " + oldtid + " to " + newTitleID + " -> " + titleID);
            }

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

        public int findPatchIndexByName(string name, riivolutionPatch[] patches)
        {
            for(int i = 0; i < patches.Length; i++)
            {
                if(patches[i].id == name)
                {
                    return i;
                }
            }
            return -1;
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
        [XmlAttribute(AttributeName = "game")]
        public string game { get; set; }


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

        [XmlAttribute(AttributeName = "target")]
        public string target { get; set; }

        [XmlAttribute(AttributeName = "valuefile")]
        public string valuefile { get; set; }
    }
}
