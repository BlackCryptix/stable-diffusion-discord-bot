using SDDiscordBot.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;

namespace SDDiscordBot.Api
{
    public class StableDiffusionApi
    {
        private readonly string _baseUrl;
        private string _stableDiffusionApiUrl { get { return _baseUrl + "/sdapi/v1/txt2img"; } }
        private string _stableDiffusionProgressUrl { get { return _baseUrl + "/sdapi/v1/progress"; } }

        private readonly HttpClient _httpClient;

        public string SaveFolderPath { get; set; }
        public string DefaultPositiv { get; set; }
        public string DefaultNegative { get; set; }
        public string Upscaler { get; set; }

        public StableDiffusionApi(IConfiguration config)
        {
            var stableDiffusionConfig = config.GetSection("StableDiffusion");
            _baseUrl = stableDiffusionConfig["Host"] + ":" + stableDiffusionConfig["Port"];
            SaveFolderPath = stableDiffusionConfig["SaveFolderPath"] ?? "";

            Directory.CreateDirectory(SaveFolderPath);

            DefaultPositiv = stableDiffusionConfig["DefaultPositiv"] ?? "";
            DefaultNegative = stableDiffusionConfig["DefaultNegative"] ?? "";
            Upscaler = stableDiffusionConfig["Upscaler"] ?? "4xNMKDSuperscale";
            _httpClient = new HttpClient();
        }

        public async Task<SDImage?> GenerateImageAsync(SDSettings generationSettings)
        {
            generationSettings.InsureSeed();

            var requestBody = new
            {
                prompt = generationSettings.Prompt + ", " + DefaultPositiv,
                negative_prompt = DefaultNegative,
                steps = 25,
                width = 912,
                height = 1216,
                cfg_scale = 5,
                seed = generationSettings.Seed,
                sampler_name = "Euler a",
                restore_faces = false,
                enable_hr = generationSettings.HighResFix,
                hr_scale = generationSettings.HighResScale,
                hr_second_pass_steps = 15,
                hr_upscaler = Upscaler,
                denoising_strength = 0.5,
                hr_additional_modules = new List<string>() { "Use same choice" }
            };

            var json = JsonConvert.SerializeObject(requestBody);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_stableDiffusionApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                dynamic responseData = JsonConvert.DeserializeObject(responseString);

                // Assuming the API returns a base64-encoded image
                string base64Image = responseData.images[0];
                byte[] imageBytes = Convert.FromBase64String(base64Image);

                // Save image to a file
                string fileName = $"{Guid.NewGuid().ToString("N").Substring(0, 5)}-{generationSettings.Seed}.png";
                string filePath = $"{SaveFolderPath}{fileName}";
                await File.WriteAllBytesAsync(filePath, imageBytes);

                return new SDImage() { Name = fileName, Path = filePath };
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }

        }

        public async Task<SDProgress?> GetProgress()
        {

            var response = await _httpClient.GetAsync(_stableDiffusionProgressUrl);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<SDProgress>(responseString);
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
    }
}
