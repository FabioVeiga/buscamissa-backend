namespace BuscaMissa.Models
{
    public class Contribuidor
    {
        public int Id { get; set; }
        public string Nome { get; set; } = default!;
        public DateTime DataContribuicao { get; set; } = DateTime.Now;
    }
}