using Poc.Services;
using Poc.Models;
using AutoMapper;

class Program
{
    static async Task Main(string[] args)
    {
        var watch = new System.Diagnostics.Stopwatch();
        Console.WriteLine("Starting optimization...");
        watch.Start();

        int tries = 1;
        int asyncTries = 5000;

        List<Task> tasks = new List<Task>();

        var mapper = InitializeAutomapper();
        var csv = new CsvService();
        var optimizer = new OptimizerService(mapper);

        IList<Destination> destinations = csv.ReadDestinations("testes50.csv");
        var route = new Route
        {
            Path = destinations,
            Distance = double.MaxValue
        };

        if (true)
        {
            Parallel.For(0, asyncTries, i =>
            {
                var result = Optimization(destinations, optimizer, tries);
                if (result.Distance < route.Distance)
                {
                    route = result;
                }
            });
        }
        else
        {
            route = Optimization(destinations, optimizer, tries);
        }

        csv.WriteDestinations("results.csv", route.Path);
        Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");
        Console.WriteLine($"Best distance: {route.Distance}");

        Console.WriteLine("Resultado finalizado!");
    }

    private static Route Optimization(IList<Destination> destinations, OptimizerService optimizer, int tries)
    {
        var bestRoute = optimizer.VmpMethod(destinations);
        var bestDistance = optimizer.CalcTotalDistance(bestRoute);
        for (var i = 0; i < tries; i++)
        {
            var newRoute = optimizer.VmpMethod(destinations);
            var newDistance = optimizer.CalcTotalDistance(newRoute);

            if (newDistance < bestDistance)
            {
                bestDistance = newDistance;
                bestRoute = newRoute;
            }
        }

        var route = new Route
        {
            Path = bestRoute,
            Distance = bestDistance
        };

        return route;
    }

    private static IMapper InitializeAutomapper()
    {
        var config = new AutoMapper.MapperConfiguration(
            cfg => cfg.CreateMap<Destination, DestinationR>().ReverseMap()
        );

        return new Mapper(config);
    }
}