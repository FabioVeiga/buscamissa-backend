namespace BuscaMissa.Helpers
{
    public static class EnumHelper
    {
        public static bool ValidarEnum<T>(int value) where T : Enum
            {
                return Enum.IsDefined(typeof(T), value);
            }

        public static bool ValidarEnum<T>(string value) where T : struct, Enum
        {
            return Enum.TryParse(value, true, out T parsedValue) && Enum.IsDefined(typeof(T), parsedValue);
        }

    }
}