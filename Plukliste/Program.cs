//Eksempel på funktionel kodning hvor der kun bliver brugt et model lag
namespace Plukliste;

class PluklisteProgram {
    private static ConsoleColor standardColor = ConsoleColor.White;
    private static int index = -1;
    private static List<PluklistFile> files = new List<PluklistFile>();
    private static PluklistFile? currentFile;
    private static char readKey = ' ';

    static void Main()
    {
        //Arrange
        Directory.CreateDirectory("import");
        if (!DirectoryExists("export"))
        {
            Console.WriteLine("Directory \"export\" not found");
            Console.ReadLine();
            return;
        }

        GetFiles();

        //ACT
        while (readKey != 'Q')
        {
            if (files.Count == 0)
            {
                Console.WriteLine("No files found.");
            }
            else
            {
                if (index == -1) index = 0;
                currentFile = files[index];
                foreach (string line in currentFile.DataList)
                {
                    // Loop through each line of data in the file. And write it to the console
                    Console.WriteLine(line);
                }
            }

            // Print options
            ConsoleWriteOptions();

            // Readkey
            readKey = ReadUpperCaseKey();
            Console.Clear();

            // Use the user input to see what action to run
            SwitchCaseForReadKey();
            Console.ForegroundColor = standardColor; //reset color
        }
    }

    static bool DirectoryExists(string Dir)
    {
        bool exists = Directory.Exists(Dir);
        return exists;
    }

    static void ConsoleWriteStringInColor(string input, ConsoleColor ForegroundColor = ConsoleColor.Green)
    {
        char firstLetter = input.ToCharArray()[0];
        string inputWithoutFirstChar = input.Substring(1);

        Console.ForegroundColor = ForegroundColor;
        Console.Write(firstLetter);
        Console.ForegroundColor = standardColor;
        Console.WriteLine(inputWithoutFirstChar);
    }

    static void ConsoleWriteOptions()
    {
        Console.WriteLine("\n\nOptions:");
        ConsoleWriteStringInColor("Quit");
        if (index >= 0)
        {
            ConsoleWriteStringInColor("Afslut plukseddel");
        }
        if (index > 0)
        {
            ConsoleWriteStringInColor("Forrige plukseddel");
        }
        if (index < files.Count - 1)
        {
            ConsoleWriteStringInColor("Næste plukseddel");
        }
        ConsoleWriteStringInColor("Genindlæs pluksedler");
    }

    static char ReadUpperCaseKey()
    {
        char readKey = Console.ReadKey().KeyChar;
        readKey = Char.ToUpper(readKey);
        return readKey;
    }

    static void SwitchCaseForReadKey()
    {
        switch (readKey)
        {
            case 'G':
                GetFiles();
                index = -1;

                ConsoleWriteStringInColor("Pluklister genindlæst", ConsoleColor.Red);
                Console.WriteLine(); // New Line
                break;
            case 'F':
                if (currentFile.HasPrevious()) index--;
                break;
            case 'N':
                if (currentFile.HasNext()) index++;
                break;
            case 'A':
                //Move files to import directory
                var filewithoutPath = files[index].FileName;
                File.Move(files[index].FilePath, string.Format(@"import\\{0}", filewithoutPath));

                ConsoleWriteStringInColor($"Plukseddel {files[index]} afsluttet.", ConsoleColor.Red);
                Console.WriteLine(); // New Line

                files.Remove(files[index]);
                if (index == files.Count) index--;
                break;
        }
    }

    static void GetFiles()
    {
        List<string> fileNames = Directory.EnumerateFiles("export").ToList();
        int fileNamesCount = fileNames.Count();
        fileNames.ForEach(fileName =>
        {
            PluklistFile pluklistFile = new PluklistFile
            {
                DataList = new List<string>(),
                FileName = fileName.Substring(fileName.LastIndexOf('/')),
                FileIndex = fileNames.IndexOf(fileName),
                NumberOfFiles = fileNamesCount,
                FilePath = fileName
            };

            using (FileStream file = File.OpenRead(fileName))
            {
                System.Xml.Serialization.XmlSerializer xmlSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Pluklist));
                var plukliste = (Pluklist?)xmlSerializer.Deserialize(file);

                pluklistFile.DataList.Add(String.Format("\n{0, -13}{1}", "Name:", pluklistFile.FileName));
                pluklistFile.DataList.Add(String.Format("{0, -13}{1}", "Forsendelse:", plukliste.Forsendelse));
                pluklistFile.DataList.Add(String.Format("\n{0,-7}{1,-9}{2,-20}{3}", "Antal", "Type", "Produktnr.", "Navn"));

                foreach (var item in plukliste.Lines)
                {
                    pluklistFile.DataList.Add(String.Format("{0,-7}{1,-9}{2,-20}{3}", item.Amount, item.Type, item.ProductID, item.Title));
                }
            }

            files.Add(pluklistFile);
        });
    }
}
