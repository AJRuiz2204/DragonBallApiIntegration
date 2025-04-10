using DragonBallApi.Models.ExternalApi;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DragonBallApi.Services
{
    public class DragonBallApiService : IDragonBallApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly ILogger<DragonBallApiService> _logger;



        public DragonBallApiService(IHttpClientFactory clientFactory, IConfiguration configuration, ILogger<DragonBallApiService> logger)
        {
            _httpClient = clientFactory.CreateClient("DragonBallApiClient");
            _baseUrl = configuration.GetValue<string>("ExternalApi:BaseUrl")
                         ?? throw new ArgumentNullException("ExternalApi:BaseUrl not configured.");
            _logger = logger;


            if (!_baseUrl.EndsWith('/'))
            {
                _baseUrl += "/";
            }
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        public async Task<ApiResponse<ExternalCharacter>?> GetAllCharactersAsync(int page = 1, int limit = 58)
        {

            var url = $"characters?page={page}&limit={limit}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                
                return JsonConvert.DeserializeObject<ApiResponse<ExternalCharacter>>(json);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching characters from {Url}", url);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing characters response from {Url}", url);
                return null;
            }
        }


        public async Task<ExternalCharacter?> GetCharacterByIdAsync(int id)
        {
            var url = $"characters/{id}";
            try
            {
                var response = await _httpClient.GetAsync(url);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Character with ID {CharacterId} not found in external API.", id);
                    return null;
                }
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<ExternalCharacter>(json);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error fetching character {CharacterId} from {Url}", id, url);
                return null;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing character {CharacterId} response from {Url}", id, url);
                return null;
            }
        }
    }
}