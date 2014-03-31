using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Awagat.OneDrive.Util
{
    public static class HttpClientUtility
    {
        public static byte[] ToBytes<T>(this T item)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T))
                    .WriteObject(stream, item);

                return stream.ToArray();
            }
        }

        public static void Send(string uri, string method = "POST")
        {
            using (var message = new HttpRequestMessage(new HttpMethod(method), uri))
            using (var client = new HttpClient())
            using (HttpResponseMessage result = client.SendAsync(message).Result)
            {
                result.EnsureSuccessStatusCode();
            }
        }

        public static T SendAndReceive<T>(this IEnumerable<KeyValuePair<string, string>> values, string uri, string method = "POST") where T : class
        {
            using (var content = new FormUrlEncodedContent(values))
            using (var message = new HttpRequestMessage(new HttpMethod(method), uri) { Content = content })
            using (var client = new HttpClient())
            using (HttpResponseMessage result = client.SendAsync(message).Result)
            {
                result.EnsureSuccessStatusCode();

                using (var stream = result.Content.ReadAsStreamAsync().Result)
                {
                    return new DataContractJsonSerializer(typeof(T))
                        .ReadObject(stream) as T;
                }
            }
        }

        public static T SendAndReceive<X, T>(this X item, string uri, string method = "POST") where T : class
        {
            using (var content = new ByteArrayContent(item.ToBytes<X>()))
            using (var message = new HttpRequestMessage(new HttpMethod(method), uri))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                message.Content = content;

                using (var client = new HttpClient())
                using (HttpResponseMessage result = client.SendAsync(message).Result)
                {
                    result.EnsureSuccessStatusCode();

                    using (var stream = result.Content.ReadAsStreamAsync().Result)
                    {
                        return new DataContractJsonSerializer(typeof(T))
                            .ReadObject(stream) as T;
                    }
                }
            }
        }

        public static void Send<X>(this X item, string uri, string method = "POST")
        {
            using (var content = new ByteArrayContent(item.ToBytes<X>()))
            using (var message = new HttpRequestMessage(new HttpMethod(method), uri))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                message.Content = content;

                using (var client = new HttpClient())
                using (HttpResponseMessage result = client.SendAsync(message).Result)
                {
                    result.EnsureSuccessStatusCode();
                }
            }
        }

        public static void Send(this byte[] data, string uri, string fileName, string method = "POST")
        {
            // Not using MultipartFormDataContent
            // It sets Content-Type as:
            // Content-Type: multipart/form-data; boundary="..."
            // OneDrive Rest API doesn't like these double quotation marks enclosing a boundary string

            string boundary = Guid.NewGuid().ToString();

            using (var stream = new MemoryStream())
            {
                byte[] preamble =
                    string.Format("--{0}\r\nContent-Disposition: form-data; name=\"file\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
                        boundary,
                        fileName).ToBytes(Encoding.ASCII);

                byte[] postAmble =
                    string.Format("\r\n\r\n--{0}\r\n", boundary).ToBytes(Encoding.ASCII);

                stream.Write(preamble,  0, preamble.Length);
                stream.Write(data,      0, data.Length);
                stream.Write(postAmble, 0, postAmble.Length);

                using (var content = new ByteArrayContent(stream.ToArray()))
                using (var message = new HttpRequestMessage(new HttpMethod(method), uri))
                {
                    content.Headers.Add("Content-Type", string.Format("multipart/form-data; boundary={0}", boundary));
                    message.Content = content;

                    using (var client = new HttpClient())
                    using (HttpResponseMessage result = client.SendAsync(message).Result)
                    {
                        result.EnsureSuccessStatusCode();
                    }
                }
            }
        }
    }
}
