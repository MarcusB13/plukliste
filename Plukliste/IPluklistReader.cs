using System;
namespace Plukliste
{
	public interface IPluklistReader
	{

		public PluklistFile Read(string fileName, List<string> fileNames);
	}
}

