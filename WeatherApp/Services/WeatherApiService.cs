using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WeatherApp
{
    public class WeatherApiService
    {
        private readonly HttpClient _httpClient;
        private const string ApiKey = "8981bb73bb7342a7b53e32789fc8a6c0";
        private const string BaseUrl = "https://api.openweathermap.org/data/2.5/";

        public WeatherApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<WeatherResponse> GetWeatherAsync(string city)
        {
            try
            {
                var url = $"{BaseUrl}weather?q={city}&appid={ApiKey}&units=metric";
                var response = await _httpClient.GetStringAsync(url);
                return JsonConvert.DeserializeObject<WeatherResponse>(response);
            }
            catch (HttpRequestException ex)
            {
                
                throw new Exception("Nie znaleziono miasta.");
            }
            catch (Exception ex)
            {
                
                throw new Exception("An error occurred while fetching weather data.");
            }
        }
    }
}
