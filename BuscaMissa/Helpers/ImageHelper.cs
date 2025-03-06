namespace BuscaMissa.Helpers
{
    public static class ImageHelper
    {
        public static byte[] ConverterStringEmByte(string imagem)
        {
            return Convert.FromBase64String(imagem);
        }

        public static string BuscarExtensao(string imagem)
        {
            if (imagem.Contains(","))
            {
                var data = imagem.Substring(0, imagem.IndexOf(","));
                if (data.Contains("image/jpeg")) return ".jpg";
                if (data.Contains("image/png")) return ".png";
                if (data.Contains("image/gif")) return ".gif";
                if (data.Contains("image/bmp")) return ".bmp";
                if (data.Contains("image/webp")) return ".webp";
            }
            return ".jpg"; 
        }

        public static MemoryStream ConvertermemoryStream(byte[] imageBase64){
            return new MemoryStream(imageBase64);
        }
    }
    
}