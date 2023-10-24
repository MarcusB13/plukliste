using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace Plukliste;
public class Pluklist
{
    public string? Name;
    public string? Forsendelse;
    public string? Adresse;
    public List<Item> Lines = new List<Item>();
    public void AddItem(Item item) { Lines.Add(item); }

    public string getNameAsString()
    {
        return String.Format("{0, -13}{1}", "Name:", Name);
    }

    public string getForsendelse()
    {
        return String.Format("{0, -13}{1}", "Forsendelse:", Forsendelse);
    }

    public string getLinesAsString(string newLineString = "\n", string space = "")
    {
        string data = String.Format("{0,-7}{5}{1,-9}{5}{2,-20}{5}{3}{5}{4}", "Antal", "Type", "Produktnr.", "Navn", newLineString, space);
        foreach (var item in Lines)
        {
            if (item.Type.ToString() == "Print") { continue; }
            data += String.Format("{0,-7}{5}{1,-9}{5}{2,-20}{5}{3}{5}{4}", item.Amount, item.Type, item.ProductID, item.Title, newLineString, space);
        }
        return data;
    }

    private string getNewColumn(string Item)
    {
        string columnStyle = "text-align:center; padding: 5px; border: 1px solid black;";
        return String.Format("<div style=\"{0}\">{1}</div>", columnStyle, Item);
    }

    public string getLinesAsStringForHTML()
    {
        string gridStyle = "display:grid; grid-template-columns: auto auto auto auto; padding: 5px;";
        string gridLines = String.Format("<div style=\"{0}\">", gridStyle);

        // Headers
        string headerStyle = "text-align:center; border: 1px solid black; padding: 5px; font-weight: bold;";

        gridLines += String.Format("<div style=\"{0}\">Antal</div>", headerStyle);
        gridLines += String.Format("<div style=\"{0}\">Type</div>", headerStyle);
        gridLines += String.Format("<div style=\"{0}\">Produktnr.</div>", headerStyle);
        gridLines += String.Format("<div style=\"{0}\">Navn</div>", headerStyle);

        foreach (var item in Lines)
        {
            if (item.Type.ToString() == "Print") { continue; }
            gridLines += getNewColumn(item.Amount.ToString());
            gridLines += getNewColumn(item.Type.ToString());
            gridLines += getNewColumn(item.ProductID);
            gridLines += getNewColumn(item.Title);
        }

        gridLines += "</div>";
        return gridLines;
        //string spaces = String.Join("", Enumerable.Repeat("&nbsp;", 4));
        //return getLinesAsString("<br>", spaces);
    }
}

public class Item
{
    [Name("productid")]
    public string ProductID { get; set; }
    [Name("description")]
    public string Title { get; set; }
    [Name("type")]
    public ItemType Type { get; set; }
    [Name("amount")]
    public int Amount { get; set; }
}

public class PluklistFile
{
    public string FileName;
    public int FileIndex;
    public int NumberOfFiles;

    public Pluklist pluklist;
    public List<string> DataList;

    public string FilePath;
    public string PrintType;

    public bool HasPrevious()
    {
        if(FileIndex > 0)
        {
            return true;
        }
        return false;
    }

    public bool HasNext()
    {
        if(FileIndex < NumberOfFiles - 1)
        {
            return true;
        }
        return false;
    }

    public string GetFileTemplate()
    {
        string templateName = String.Format("templates/{0}.html", PrintType);
        string templateData = File.ReadAllText(templateName);

        templateData = templateData.Replace("[Name]", pluklist.Name);

        string pluklistLines = pluklist.getLinesAsStringForHTML();
        templateData = templateData.Replace("[Plukliste]", pluklistLines);

        templateData = templateData.Replace("[Adresse]", pluklist.Adresse);
        return templateData;
    }
}

public class BuisnessLayer
{
    private string _exportPath;
    private string _importPath;

    public List<string> files = new List<string>();
    public PluklistFile? currentFile { get; private set; }
    public int currentIndex = 0;

    public BuisnessLayer(string exportPath, string importPath)
    {
        _exportPath = exportPath;
        _importPath = importPath;
    }

    public bool doesFilesExist()
    {
        return files.Count > 0;
    }

    public void enumerateFiles()
    {
        files = Directory.EnumerateFiles("export").ToList();
    }

    public void reloadFiles()
    {
        currentIndex = 0;
        enumerateFiles();
        getCurrentPlukListFile();
    }

    public void previous()
    {
        if(currentFile == null)
        {
            currentIndex = 0;
            getCurrentPlukListFile();
            return;
        }

        if (currentFile.HasPrevious())
        {
            currentIndex--;
            getCurrentPlukListFile();
            return;
        }
        currentIndex = 0;
        getCurrentPlukListFile();
    }

    public void next()
    {
        if (currentFile == null)
        {
            currentIndex = 0;
            getCurrentPlukListFile();
            return;
        }

        if (currentFile.HasNext())
        {
            currentIndex++;
            getCurrentPlukListFile();
            return;
        }
        currentIndex = 0;
        getCurrentPlukListFile();
    }

    public PluklistFile getCurrentPlukListFile()
    {
        if (files[currentIndex].Contains(".DS_Store"))
        {
            next();
            return getCurrentPlukListFile();
        }
        IPluklistReader reader = PluklistReaderFactory.GetReader(files[currentIndex]);
        currentFile = reader.Read(files[currentIndex], files);

        return currentFile;
    }

    public string moveCurrentFileToImport()
    {
        var filewithoutPath = currentFile.FileName;
        File.Move(files[currentIndex], string.Format(@"import\\{0}", filewithoutPath));

        files.Remove(files[currentIndex]);
        previous();

        return filewithoutPath;
    }

    public string currentFileToHtml()
    {
        if (currentFile.FilePath.Contains(".CSV"))
        {
            currentFile.PrintType = "PRINT-WELCOME";
        }
        string fileToCreate = currentFile.FileName;
        fileToCreate = fileToCreate.Replace(".XML", "");
        fileToCreate = fileToCreate.Replace(".CSV", "");
        string path = String.Format("print/{0}.html", fileToCreate);

        File.WriteAllText(path, currentFile.GetFileTemplate());
        return fileToCreate;
    }
}

public enum ItemType
{
    Fysisk, Print
}



