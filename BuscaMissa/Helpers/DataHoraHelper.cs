namespace BuscaMissa.Helpers
{
    public static class DataHoraHelper
    {
        public static DateTime AdicionarHoraEArredondarParaCima(DateTime dateTime, int AdicionarHora)
        {
            int minute = dateTime.Minute;
            int roundedMinutes = minute < 30 ? 0 : 30;

            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour + AdicionarHora, roundedMinutes, 0);
        }

        public static string Formatar(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }
}