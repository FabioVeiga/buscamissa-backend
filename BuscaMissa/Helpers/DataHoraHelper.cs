namespace BuscaMissa.Helpers
{
    public static class DataHoraHelper
    {
        public static string Formatar(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }
    }
}