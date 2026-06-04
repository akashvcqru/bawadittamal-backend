namespace BawaDittaMal.Api.Models
{
    public class SliderItem
    {
        public int Id { get; set; }
        public string Image { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BtnText { get; set; } = string.Empty;
        public string BtnLink { get; set; } = string.Empty;
        public string Btn1Text { get; set; } = string.Empty;
        public string Btn1Link { get; set; } = string.Empty;
        public string Btn2Text { get; set; } = string.Empty;
        public string Btn2Link { get; set; } = string.Empty;
        public string Status { get; set; } = "active"; // active, inactive
    }
}
