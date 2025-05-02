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

        public static string GetDescription<T>(T enumValue) where T : Enum
        {
            var fieldInfo = typeof(T).GetField(enumValue.ToString());
            if (fieldInfo != null)
            {
                var attributes = (System.ComponentModel.DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if (attributes.Length > 0)
                {
                    return attributes[0].Description;
                }
            }
            return enumValue.ToString();
        }
    }
}