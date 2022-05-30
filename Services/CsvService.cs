using Poc.Models;
using CsvHelper;
using System.Globalization;

namespace Poc.Services;

public class CsvService
{
    public List<Destination> ReadDestinations(string filePath)
    {
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Destination>().ToList();
            return records;
        }
    }

    public void WriteDestinations(string filePath, IList<Destination> destinations)
    {
        using (var writer = new StreamWriter(filePath))
        using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecords(destinations);
        }
    }
}