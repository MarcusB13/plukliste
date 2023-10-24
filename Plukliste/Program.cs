//Eksempel på funktionel kodning hvor der kun bliver brugt et model lag
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace Plukliste;

class PluklisteProgram {
    private static ConsoleColor standardColor = ConsoleColor.White;
    private static char readKey = ' ';

    private static BuisnessLayer buisnessLayer = new BuisnessLayer("export", "import");
    private static string ConnString = "Data Source=10.130.54.120;Database=Plukliste;User ID=Plukliste;Password=1234; TrustServerCertificate=True;";

    static void Main()
    {
        using (var conn = new SqlConnection(ConnString))
        {
            Console.WriteLine(ConnString);
            conn.Open();
            List<Pluklist> result = conn.Query<Pluklist>("SELECT * FROM pluklist").ToList();

            result.ForEach(pluklist =>
            {
                Console.WriteLine(pluklist.Name);
            });
        }


        //Arrange
        Directory.CreateDirectory("import");
        Directory.CreateDirectory("print");
        if (!DirectoryExists("export"))
        {
            Console.WriteLine("Directory \"export\" not found");
            return;
        }

        if (!DirectoryExists("templates"))
        {
            Console.WriteLine("Directory \"templates\" not found");
            return;
        }

        //ACT
        buisnessLayer.reloadFiles();
        while (readKey != 'Q')
        {
            if (!buisnessLayer.doesFilesExist())
            {
                Console.WriteLine("No files found.");
            }
            else
            {
                buisnessLayer.getCurrentPlukListFile();
                Console.WriteLine(String.Format("{0}/{1}", buisnessLayer.currentIndex + 1, buisnessLayer.fileCount));
                Console.WriteLine(buisnessLayer.currentFile.pluklist.getNameAsString());
                Console.WriteLine(buisnessLayer.currentFile.pluklist.getForsendelse());
                Console.WriteLine(buisnessLayer.currentFile.pluklist.getLinesAsString());
                
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
        if (buisnessLayer.currentIndex >= 0)
        {
            ConsoleWriteStringInColor("Afslut plukseddel");
        }
        if (buisnessLayer.currentIndex > 0)
        {
            ConsoleWriteStringInColor("Forrige plukseddel");
        }
        if (buisnessLayer.currentIndex < buisnessLayer.files.Count - 1)
        {
            ConsoleWriteStringInColor("Næste plukseddel");
        }
        ConsoleWriteStringInColor("Template pluksedel");
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
                buisnessLayer.reloadFiles();

                ConsoleWriteStringInColor("Pluklister genindlæst", ConsoleColor.Red);
                Console.WriteLine(); // New Line
                break;
            case 'F':
                buisnessLayer.previous();
                break;
            case 'N':
                buisnessLayer.next();
                break;
            case 'A':
                //Move files to import directory
                string moveCurrentFileToImport = buisnessLayer.moveCurrentFileToImport();

                ConsoleWriteStringInColor($"Plukseddel {moveCurrentFileToImport} afsluttet.", ConsoleColor.Red);
                Console.WriteLine(); // New Line
                break;
            case 'T':
                string fileToCreate = buisnessLayer.currentFileToHtml();

                Console.WriteLine("Added the template for {0} at print/{0}.html", fileToCreate);
                break;

        }
    }
}
