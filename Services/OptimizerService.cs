using Poc.Models;
using AutoMapper;

namespace Poc.Services;

public class OptimizerService
{
    private IMapper mapper;

    public OptimizerService(IMapper mapper)
    {
        this.mapper = mapper;
    }

    public IList<Destination> VmpMethod(IList<Destination> destinations)
    {
        var medians = MonteCarlo(destinations);
        destinations = GilletJohnson(destinations, medians);
        destinations = NearstNeigbbors(destinations);
        destinations = Swap(destinations);

        destinations = ReindexSequences(destinations);

        return destinations;
    }

    private Location MonteCarlo(IList<Destination> destinations)
    {
        var collect = destinations[0];
        var further = new List<Destination>();

        further.Add(FindMostFurther(collect, destinations));
        further.Add(FindMostFurther(further[0], destinations));

        var minLat = Math.Min(further[0].Lat, further[1].Lat);
        var maxLat = Math.Max(further[0].Lat, further[1].Lat);
        var minLong = Math.Min(further[0].Long, further[1].Long);
        var maxLong = Math.Max(further[0].Long, further[1].Long);

        return new Location
        {
            Lat = RandomLocation(minLat, maxLat),
            Long = RandomLocation(minLong, maxLong)
        };
    }

    private IList<Destination> GilletJohnson(IList<Destination> destinations, Location medians)
    {
        var firstClosestDistance = double.MaxValue;
        var secondClosestDistance = double.MaxValue;
        var destinationsR = mapper.Map<DestinationR[]>(destinations);

        foreach (var destination in destinationsR)
        {
            var location = new Location
            {
                Lat = destination.Lat,
                Long = destination.Long
            };

            var distance = HaversineDistance(location, medians);
            if (distance < firstClosestDistance)
            {
                firstClosestDistance = distance;
            }
            else if (distance < secondClosestDistance)
            {
                secondClosestDistance = distance;
            }

            // Razao
            var r = firstClosestDistance / secondClosestDistance;
            destination.R = r;
        }

        // Ordenar destinatinos por razao
        var orderedDestinations = destinationsR.OrderByDescending(d => d.R).ToList();

        return mapper.Map<Destination[]>(orderedDestinations);
    }

    private IList<Destination> NearstNeigbbors(IList<Destination> destinations)
    {
        IList<Destination> path = new List<Destination>();
        IList<int> visited = new List<int>();

        for (var i = 0; i < destinations.Count(); i++)
        {
            var destination = destinations[i];
            if (destination.Sequence == 0)
            {
                path.Insert(0, destination);
            }
            else
            {
                var nearst = FindNearst(destinations, i, visited);
                path.Add(nearst);
            }

            visited.Add(destination.Sequence);
        }

        return path;
    }

    private IList<Destination> Swap(IList<Destination> destinations)
    {
        var bestDistance = CalcTotalDistance(destinations);
        for (var i = 1; i <= destinations.Count(); i++)
        {
            for (var j = i + 1; j < destinations.Count(); j++)
            {
                var newDestinations = SwapTwo(destinations, i, j);
                var newDistance = CalcTotalDistance(newDestinations);
                if (newDistance < bestDistance)
                {
                    bestDistance = newDistance;
                    destinations = newDestinations;
                }
            }
        }

        return destinations;
    }

    private IList<Destination> SwapTwo(IList<Destination> destinations, int i, int j)
    {
        var toSwap = destinations[i];
        destinations[i] = destinations[j];
        destinations[j] = toSwap;

        return destinations;
    }

    private Destination FindNearst(IList<Destination> destinations, int index, IList<int> visited)
    {
        var nearstDistance = double.MaxValue;
        var nearst = new Destination();

        for (var i = index; i < destinations.Count() && !visited.Contains(destinations[i].Sequence); i++)
        {
            var l1 = new Location
            {
                Lat = destinations[index].Lat,
                Long = destinations[index].Long
            };

            var l2 = new Location
            {
                Lat = destinations[i].Lat,
                Long = destinations[i].Long
            };

            var distance = HaversineDistance(l1, l2);
            if (distance < nearstDistance)
            {
                nearstDistance = distance;
                nearst = destinations[i];
            }
        }

        return nearst;
    }

    private Destination FindMostFurther(Destination startLocation, IList<Destination> destinations)
    {
        var distance = -double.MaxValue;
        var furthest = new Destination();

        foreach (var destination in destinations)
        {
            var l1 = new Location
            {
                Lat = startLocation.Lat,
                Long = startLocation.Long
            };

            var l2 = new Location
            {
                Lat = destination.Lat,
                Long = destination.Long
            };

            var newDistance = HaversineDistance(l1, l2);
            if (newDistance > distance)
            {
                distance = newDistance;
                furthest = destination;
            }
        }

        return furthest;
    }

    public double CalcTotalDistance(IList<Destination> destinations)
    {
        double total = 0;
        for (var i = 0; i < destinations.Count() - 1; i++)
        {
            var l1 = new Location
            {
                Lat = destinations[i].Lat,
                Long = destinations[i].Long
            };

            var l2 = new Location
            {
                Lat = destinations[i + 1].Lat,
                Long = destinations[i + 1].Long
            };

            total += HaversineDistance(l1, l2);
        }

        return total;
    }

    private double HaversineDistance(Location pos1, Location pos2)
    {
        double R = 6371;
        var lat = ToRad(pos2.Lat - pos1.Lat);
        var lng = ToRad(pos2.Long - pos1.Long);
        var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
                      Math.Cos(ToRad(pos1.Lat)) * Math.Cos(ToRad(pos2.Lat)) *
                      Math.Sin(lng / 2) * Math.Sin(lng / 2);
        var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        return R * h2;
    }

    private double ToRad(double angle)
    {
        return (Math.PI / 180) * angle;
    }

    private double RandomLocation(double min, double max)
    {
        double random = new Random().NextDouble();
        var median = (max * random) + (min * (1d - random));

        return median;
    }

    private IList<Destination> ReindexSequences(IList<Destination> destinations)
    {
        for (var i = 1; i < destinations.Count(); i++)
        {
            destinations[i].Sequence = i;
        }

        return destinations;
    }
}