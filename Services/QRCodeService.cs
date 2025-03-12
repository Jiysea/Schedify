
using System.Text;
using System.Text.Json;

namespace Schedify.Services;
public class QRCodeService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.qr-code-generator.com/v1/create";

        public QRCodeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<byte[]> GenerateQRCodeAsync(string data)
        {
            var requestBody = new
            {
                frame_name = "no-frame",
                qr_code_text = data,
                image_format = "png",
                qr_code_logo = "scan-me-square" // Optional logo
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            throw new Exception($"QR Code API Error: {response.StatusCode}");
        }
    }
