namespace BuscaMissa.DTOs.SettingsDto
{
    public class TipoEnum(int id, string nome)
    {
        public int Id { get; set; } = id;
        public string Nome { get; set; } = nome;
    }
}