namespace DragonBallApi.Models.ExternalApi
{
    public class ExternalCharacter
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ki { get; set; } = string.Empty;
        public string MaxKi { get; set; } = string.Empty;
        public string Race { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Affiliation { get; set; } = string.Empty;
        public DateTime? DeletedAt { get; set; }
        public ExternalOriginPlanet? OriginPlanet { get; set; }
        public List<ExternalTransformation> Transformations { get; set; } = new List<ExternalTransformation>();
    }

   
    public class ExternalOriginPlanet
    {
         public int Id { get; set; }
         public string Name { get; set; } = string.Empty;
         public bool IsDestroyed { get; set; }
        
    }
}