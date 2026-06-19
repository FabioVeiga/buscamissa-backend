namespace BuscaMissa.Helpers;

public static class GeoHelper
{
    private const double RaioTerraKm = 6371.0;

    /// <summary>
    /// Calcula a distância em km entre dois pontos geográficos usando a fórmula de Haversine.
    /// </summary>
    public static double DistanciaKm(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
              + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
              * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return RaioTerraKm * c;
    }

    private static double ToRad(double graus) => graus * Math.PI / 180.0;
}
