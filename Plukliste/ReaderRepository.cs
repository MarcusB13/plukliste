using System;
using System.Net;
using System.Text.Json;
using Microsoft.VisualBasic.FileIO;
using CsvHelper;
using Plukliste;
using System.Globalization;
using System.Reflection.PortableExecutable;
using CsvHelper.Configuration;
using System.Text;

namespace Plukliste
{
	public abstract class BaseReader
    {
		public BaseReader()
		{
        }

        public PluklistFile generatePluklistFile(List<string> fileNames, string fileName)
        {
            int fileNamesCount = fileNames.Count();
            PluklistFile pluklistFile = new PluklistFile
            {
                DataList = new List<string>(),
                FileName = fileName.Substring(fileName.LastIndexOf('/')),
                FileIndex = fileNames.IndexOf(fileName),
                NumberOfFiles = fileNamesCount,
                FilePath = fileName,
                PrintType = "",
            };

            return pluklistFile;
        }

        public void addPropetiesFromReader(PluklistFile pluklistFile, Pluklist plukliste)
        {
            pluklistFile.pluklist = plukliste;
            
            foreach (var item in plukliste.Lines)
            {
                string itemType = item.Type.ToString();
                if (itemType == "Print")
                {
                    pluklistFile.PrintType = item.ProductID;
                    break;
                }
            }
        }
    }

    public class PlukistFileRepositoryCSV : BaseReader, IPluklistReader
    {
        public PluklistFile Read(string fileName, List<string> fileNames)
        {
            PluklistFile pluklistFile = generatePluklistFile(fileNames, fileName);

            var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8, // Our file uses UTF-8 encoding.
                Delimiter = ";" // The delimiter is a comma.
            };

            using var reader = new StreamReader(fileName);
            using var csv = new CsvReader(reader, configuration);

            var records = csv.GetRecords<Item>().ToList();

            string[] fileNameSplit = fileName.Replace(".CSV", "").Split("_");
            pluklistFile.pluklist = new Pluklist
            {
                Adresse = "Pickup",
                Forsendelse = "Pickup",
                Name = String.Format("{0} {1}", fileNameSplit[1], fileNameSplit[2]),
                Lines = records
            };
                
            return pluklistFile;
        }
    }


    public class PlukistFileRepositoryXML : BaseReader, IPluklistReader
    {
        public PluklistFile Read(string fileName, List<string> fileNames)
        {
            PluklistFile pluklistFile = generatePluklistFile(fileNames, fileName);

            using (FileStream file = File.OpenRead(fileName))
            {

                System.Xml.Serialization.XmlSerializer xmlSerializer =
                        new System.Xml.Serialization.XmlSerializer(typeof(Pluklist));
                var plukliste = (Pluklist?)xmlSerializer.Deserialize(file);

                addPropetiesFromReader(pluklistFile, plukliste);
            }
            return pluklistFile;
        }
    }
}

