using System.Text.RegularExpressions;

namespace MLService.Models
{
    public class SpamInputParam
    {
        public string Input { get; set; }

        public string SanitizedInput => Regex.Replace(Input ?? string.Empty, "<.*?>", string.Empty);
    }
}
