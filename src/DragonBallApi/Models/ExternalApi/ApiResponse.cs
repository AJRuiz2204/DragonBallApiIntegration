using Newtonsoft.Json;

namespace DragonBallApi.Models.ExternalApi
{
    public class ApiResponse<T>
    {
        [JsonProperty("items")]
        public List<T> Items { get; set; } = new List<T>();

        [JsonProperty("meta")]
        public MetaData Meta { get; set; } = new MetaData();

        [JsonProperty("links")]
        public LinksData Links { get; set; } = new LinksData();
    }

    public class MetaData
    {
        public int TotalItems { get; set; }
        public int ItemCount { get; set; }
        public int ItemsPerPage { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class LinksData
    {
        public string? First { get; set; }
        public string? Previous { get; set; }
        public string? Next { get; set; }
        public string? Last { get; set; }
    }
}