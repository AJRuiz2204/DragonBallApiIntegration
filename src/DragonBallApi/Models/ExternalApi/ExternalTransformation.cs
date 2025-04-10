namespace DragonBallApi.Models.ExternalApi
{
    public class ExternalTransformation
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ki { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
    }
}