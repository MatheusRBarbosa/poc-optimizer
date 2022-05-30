using CsvHelper.Configuration.Attributes;
namespace Poc.Models;

public class DestinationR
{
    [Name("Sequence")]
    public int Sequence { get; set; }
    [Name("Endereco")]
    public string Address { get; set; } = null!;
    [Name("Contato")]
    public string Contact { get; set; } = null!;
    [Name("Observacoes")]
    public string Observations { get; set; } = null!;
    [Name("Responsavel")]
    public string Responsible { get; set; } = null!;
    [Name("Latitude")]
    public double Lat { get; set; }
    [Name("Longitude")]
    public double Long { get; set; }

    public double? R { get; set; } = null;
}