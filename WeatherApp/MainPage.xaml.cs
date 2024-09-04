using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace WeatherApp
{
    public partial class MainPage : ContentPage
    {
        private WeatherApiService _weatherService;
        public ObservableCollection<FavoriteCity> FavoriteCities { get; set; }
        private readonly string _favoritesFilePath;

        public MainPage()
        {
            InitializeComponent();
            _weatherService = new WeatherApiService();
            FavoriteCities = new ObservableCollection<FavoriteCity>();
            BindingContext = this;

            _favoritesFilePath = Path.Combine(FileSystem.AppDataDirectory, "favorites.json");

            LoadDefaultLocationWeather();
            LoadFavorites();
        }

        private async void LoadDefaultLocationWeather()
        {
            try
            {
                var weather = await _weatherService.GetWeatherAsync("Bielsko-Biała");
                UpdateWeatherUI(weather, "Bielsko-Biała");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnGetWeatherClicked(object sender, EventArgs e)
        {
            var city = CityEntry.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                await DisplayAlert("Error", "Please enter a city name.", "OK");
                return;
            }

            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();
            try
            {
                var weather = await _weatherService.GetWeatherAsync(city);
                UpdateWeatherUI(weather, city);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnAddToFavoritesClicked(object sender, EventArgs e)
        {
            var city = CityEntry.Text;
            if (string.IsNullOrWhiteSpace(city))
            {
                await DisplayAlert("Error", "Please enter a city name.", "OK");
                return;
            }

            city = char.ToUpper(city[0]) + city.Substring(1).ToLower();
            if (!string.IsNullOrEmpty(city) && !FavoriteCities.Any(c => c.Name == city))
            {
                try
                {
                    var weather = await _weatherService.GetWeatherAsync(city);
                    var favoriteCity = new FavoriteCity
                    {
                        Name = city,
                        Icon = $"https://openweathermap.org/img/wn/{weather.Weather[0].Icon}@2x.png",
                        Temperature = $"{weather.Main.Temp:F1}°C"
                    };
                    favoriteCity.RemoveCommand = new Command(() => RemoveFavoriteCity(city));
                    FavoriteCities.Add(favoriteCity);
                    SaveFavorites();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }


        private void UpdateWeatherUI(WeatherResponse weather, string city)
        {
            CityLabel.Text = city;
            WeatherLabel.Text = weather.Weather[0].Description;
            TemperatureLabel.Text = $"{weather.Main.Temp:F1}°C";
            PressureLabel.Text = $"Pressure: {weather.Main.Pressure} hPa";
            WeatherIcon.Source = $"https://openweathermap.org/img/wn/{weather.Weather[0].Icon}@2x.png";

            switch (weather.Weather[0].Icon)
            {
                case "01d":
                case "01n":
                    BackgroundImageSource = "sunny.png";
                    break;
                case "02d":
                case "02n":
                    BackgroundImageSource = "partly_cloudy.png";
                    break;
                case "03d":
                case "03n":
                    BackgroundImageSource = "cloudy.png";
                    break;
                case "04d":
                case "04n":
                    BackgroundImageSource = "overcast.png";
                    break;
                case "09d":
                case "09n":
                    BackgroundImageSource = "shower_rain.png";
                    break;
                case "10d":
                case "10n":
                    BackgroundImageSource = "rain.png";
                    break;
                case "11d":
                case "11n":
                    BackgroundImageSource = "thunderstorm.png";
                    break;
                case "13d":
                case "13n":
                    BackgroundImageSource = "snow.png";
                    break;
                case "50d":
                case "50n":
                    BackgroundImageSource = "mist.png";
                    break;
                default:
                    BackgroundImageSource = "default_weather.png";
                    break;
            }
        }

        private void LoadFavorites()
        {
            if (File.Exists(_favoritesFilePath))
            {
                var json = File.ReadAllText(_favoritesFilePath);
                var favoriteCities = JsonSerializer.Deserialize<ObservableCollection<FavoriteCity>>(json);
                foreach (var city in favoriteCities)
                {
                    city.RemoveCommand = new Command(() => RemoveFavoriteCity(city.Name));
                    FavoriteCities.Add(city);
                }
            }
        }

        private void SaveFavorites()
        {
            var json = JsonSerializer.Serialize(FavoriteCities);
            File.WriteAllText(_favoritesFilePath, json);
        }

        private void RemoveFavoriteCity(string city)
        {
            var cityToRemove = FavoriteCities.FirstOrDefault(c => c.Name == city);
            if (cityToRemove != null)
            {
                FavoriteCities.Remove(cityToRemove);
                SaveFavorites();
            }
        }
    }

    public class FavoriteCity
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Temperature { get; set; }

        [JsonIgnore]
        public ICommand RemoveCommand { get; set; }
    }
}
