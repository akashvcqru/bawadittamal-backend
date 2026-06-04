namespace BawaDittaMal.Api.Models
{
    public class Setting
    {
        public string Key { get; set; } = string.Empty; // e.g. "contact", "social"
        public string Value { get; set; } = string.Empty; // JSON string
    }
}
