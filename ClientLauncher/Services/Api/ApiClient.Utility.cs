using System;
using System.Threading.Tasks;

namespace ClientLauncher.Services.Api
{
    public partial class ApiClient
    {
        public async Task<byte[]> DownloadImage(string url)
        {
            Console.WriteLine($"DownloadImage(): {url}");
            return await _client.GetByteArrayAsync(url);
        }
    }
}