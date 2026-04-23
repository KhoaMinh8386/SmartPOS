using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace SmartPos.Module.Common.Services
{
    public class CloudinaryService
    {
        private const string CloudName = "drmwega70";
        private const string ApiKey = "536319214969819";
        private const string ApiSecret = "nlGY7UvMP4_OSnV_Nu6Y536QvKw";

        public async Task<string> UploadImageAsync(string filePath)
        {
            if (string.IsNullOrEmpty(CloudName) || CloudName == "YOUR_CLOUD_NAME")
            {
                throw new Exception("Vui lòng cấu hình Cloud Name trong CloudinaryService.cs");
            }

            using (var client = new HttpClient())
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                var signature = GenerateSignature(timestamp);

                using (var content = new MultipartFormDataContent())
                {
                    var fileBytes = File.ReadAllBytes(filePath);
                    var fileContent = new ByteArrayContent(fileBytes);
                    content.Add(fileContent, "file", Path.GetFileName(filePath));

                    // Signed upload thuần túy: Chỉ cần timestamp và signature
                    var url = $"https://api.cloudinary.com/v1_1/{CloudName}/image/upload" +
                              $"?api_key={ApiKey}" +
                              $"&timestamp={timestamp}" +
                              $"&signature={signature}";

                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var serializer = new JavaScriptSerializer();
                        var result = serializer.Deserialize<Dictionary<string, object>>(responseString);
                        return result["secure_url"].ToString();
                    }
                    else
                    {
                        throw new Exception(responseString);
                    }
                }
            }
        }

        private string GenerateSignature(string timestamp)
        {
            // Với signed upload thuần túy, tham số duy nhất cần ký là timestamp
            // Định dạng: timestamp=...<API_SECRET>
            var stringToSign = $"timestamp={timestamp}{ApiSecret}";
            
            using (var sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(stringToSign));
                var sb = new StringBuilder();
                foreach (byte b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}
