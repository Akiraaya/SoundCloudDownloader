using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SCDL.Services
{
    public class SoundCloudClientIdParse
    {
        private readonly HttpClient _client;

        public SoundCloudClientIdParse(HttpClient client)
        {
            _client = client;
        }

        public async Task<string?> GetClientIdAsync()
        {
            try
            {
                string url = "https://soundcloud.com/";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                request.Headers.Add("Referer", "https://soundcloud.com/");

                var response = await _client.SendAsync(request);
                string responseContent = await response.Content.ReadAsStringAsync();

                var jsMatches = Regex.Matches(responseContent, @"src=""(https://a-v2\.sndcdn\.com/assets/[^""]+\.js)""");

                if (jsMatches.Count == 0)
                {
                    MessageBox.Show("Не найдено ни одного JS-файла. Возможно, структура сайта изменилась.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }

                foreach (Match match in jsMatches)
                {
                    string jsUrl = match.Groups[1].Value;

                    var jsRequest = new HttpRequestMessage(HttpMethod.Get, jsUrl);
                    jsRequest.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
                    jsRequest.Headers.Add("Referer", "https://soundcloud.com/");

                    var jsResponse = await _client.SendAsync(jsRequest);
                    string jsContent = await jsResponse.Content.ReadAsStringAsync();

                    var clientIdMatch = Regex.Match(jsContent, @"client_id\s*[:=]\s*[""']([a-zA-Z0-9]{32})[""']");

                    if (clientIdMatch.Success)
                    {
                        string clientId = clientIdMatch.Groups[1].Value;
                        return clientId;
                        
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при получении client ID: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
