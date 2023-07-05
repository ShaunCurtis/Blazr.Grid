namespace Blazr.Grid.Data;

public static class CityDataProvider
{
    public static IEnumerable<City> Cities => _cities.AsEnumerable();

    private static List<City> _cities = new List<City>
        {
            new( Country: "UK", Name: "London"),
            new( Country: "UK", Name: "Birmingham"),
            new( Country: "Spain", Name: "Madrid"),
        };
}
