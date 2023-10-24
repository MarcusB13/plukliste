using System;
using Plukliste;

namespace Plukliste
{
	public class PluklistReaderFactory
	{
		public PluklistReaderFactory()
		{
		}

		static public IPluklistReader GetReader(string filename)
		{
			string className = $"Plukliste.PlukistFileRepository{Path.GetExtension(filename).ToUpper().Replace(".", "")}";

            IPluklistReader? reader = (IPluklistReader?)Activator.CreateInstance(
				Type.GetType(className));

			if (reader == null) throw new NotSupportedException("File format not supported");
			return (IPluklistReader)reader;
		}
    }
}

