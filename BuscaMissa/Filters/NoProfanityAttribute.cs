using System.ComponentModel.DataAnnotations;

namespace BuscaMissa.Filters
{
    public class NoProfanityAttribute : ValidationAttribute
    {
        // Lista de palavrões (adicione os que desejar)
        private readonly string[] _bannedWords = new[] { "boquete", "Caralho", "Do caralho", "Foda", "Foda-se", "Foder", "Nem fodendo", "Pau", "Pica", "Porra", "Porra nenhuma", "Pra caralho", "Puta merda", "Puta que pariu", "Punheta", "Que porra é essa?", "Teu cu", "Trepar", "Olho do cú", "Buceta", "Xoxota", "Sacanagem", "Cacete", "Caceta", "Siririca" };

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return ValidationResult.Success;
            }

            var input = value.ToString();

            // Verifica se algum palavrão está contido na string
            if (_bannedWords.Any(word => input.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return new ValidationResult("O campo contém palavras proibidas.");
            }

            return ValidationResult.Success;
        }
    }
}