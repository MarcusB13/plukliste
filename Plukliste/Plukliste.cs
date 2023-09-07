namespace Plukliste;
public class Pluklist
{
    public string? Name;
    public string? Forsendelse;
    public string? Adresse;
    public List<Item> Lines = new List<Item>();
    public void AddItem(Item item) { Lines.Add(item); }
}

public class Item
{
    public string ProductID;
    public string Title;
    public ItemType Type;
    public int Amount;
}

public class PluklistFile
{
    public string FileName;
    public int FileIndex;
    public int NumberOfFiles;
    public List<string> DataList;
    public string FilePath;

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
}

public enum ItemType
{
    Fysisk, Print
}



