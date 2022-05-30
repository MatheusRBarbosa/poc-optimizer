namespace Poc.Models;

public class Route
{
    public IList<Destination> Path { get; set; }
    public double Distance { get; set; }
}