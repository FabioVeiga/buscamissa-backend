namespace BuscaMissa.Helpers
{
    public static class DataHoraHelper
    {
        public static string Formatar(DateTime dateTime)
        {
            return dateTime.ToString("dd/MM/yyyy HH:mm");
        }

        public static string Ano()
        {
            return DateTime.Now.Year.ToString();
        }
    }
}