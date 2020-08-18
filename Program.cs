using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace testAction
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();

        private static async Task ProcessRepositories(string[] files)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            string toPost = "";

            foreach(var f in files)
            {
                var lines = File.ReadLines(f);
                toPost += lines.First() + Environment.NewLine;
            }

            await client.PostAsync("https://webhook.site/34dd9943-6e0e-4755-97a8-04f53869ad0e", new StringContent(string.Join(',', files)));

            Console.Write(string.Join(',', files));
        }

        private async Task addVideoCaption(string videoID) //pass your video id here..
        {
            UserCredential credential;
            //you should go out and get a json file that keeps your information... You can get that from the developers console...
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeForceSsl, YouTubeService.Scope.Youtube, YouTubeService.Scope.Youtubepartner },
                    "b.maximec@gmail.com",
                    CancellationToken.None,
                    new FileDataStore(GetType().ToString())
                );
            }
            //creates the service...
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = GetType().ToString(),
            });

            //create a CaptionSnippet object...
            CaptionSnippet capSnippet = new CaptionSnippet();
            capSnippet.Language = "fr";
            capSnippet.Name = videoID + "_Caption";
            capSnippet.VideoId = videoID;
            capSnippet.IsDraft = false;

            //create new caption object
            Caption caption = new Caption
            {
                //set the completed snippet to the object now...
                Snippet = capSnippet
            };

            ////here we read our .srt which contains our subtitles/captions...
            //using (var fileStream = new FileStream(TheFile.FilePath, FileMode.Open))
            //{
            //    //create the request now and insert our params...
            //    var captionRequest = youtubeService.Captions.Insert(caption, "snippet", fileStream, "application/atom+xml");

            //    //finally upload the request... and wait.
            //    await captionRequest.UploadAsync();
            //}
        }

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Received arg : {args[0]}");
            StreamReader sr = new StreamReader(args[0]);
            List<string> fileList = new List<string>();

            Console.WriteLine("File exists ? " + File.Exists(args[0]));
            Console.WriteLine("begin read file");
            while(!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                Console.WriteLine($"File line : {line}");
                fileList.Add(line);
            }
            
            await ProcessRepositories(fileList.ToArray());
        }
    }
}
