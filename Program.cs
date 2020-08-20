﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace testAction
{
    class Program
    {
        /// <summary>
        /// Extensions autorisées pour les fichiers de sous-titre
        /// </summary>
        private static readonly string[] SUBTITLE_EXTENSIONS = { ".sbv", ".srt", ".sub", ".mpsub", ".lrc", ".cap", ".smi", ".sami", ".rt", ".vtt", ".ttml", ".dfxp", ".scc", ".stl", ".tds", ".cin", ".asc" };

        /// <summary>
        /// Processes the list of files, checking wether the file exists, 
        /// and if it is a subtitle, sends it to YouTube
        /// </summary>
        /// <param name="files">Array of files to process</param>
        /// <returns></returns>
        private static async Task ProcessModifiedFiles(string[] files)
        {
            foreach(FileInfo fi in files.Select(f => new FileInfo(f)))
            {
                Console.WriteLine($"Processing file : {fi.Name}");
                try
                {
                    if(!fi.Exists)
                    {
                        Console.WriteLine($"File {fi.Name} does not exists, skip");
                    }
                    else if(!SUBTITLE_EXTENSIONS.Contains(fi.Extension))
                    {
                        Console.WriteLine($"File {fi.Name} is not recognized as a subtitle file, skip");
                    }
                    else
                    {
                        //It is a subtitle file, upload it to the video
                        var dirParts = fi.Directory.Name.Split('.');
                        var fileParts = fi.Name.Split('-');
                        var language = fileParts[0].ToLower();
                        var captionName = language;
                        switch(language)
                        {
                            case "fr":
                                captionName = "Français";
                                break;

                            case "en":
                                captionName = "English";
                                break;
                        }

                        await UploadVideoCaption(dirParts[0], language, captionName, fi);
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine($"Error reading file {fi.Name} : {e}");
                }
            }
        }

        /// <summary>
        /// Uploads given file as caption on YouTube video
        /// </summary>
        /// <param name="videoID">ID of the video where to import caption</param>
        /// <param name="language">Language of the caption (ex: fr, en, ...)</param>
        /// <param name="captionName">Name of the caption (should be human readable language name)</param>
        /// <param name="fi">FileInfo pointing to caption file</param>
        /// <returns></returns>
        private static async Task UploadVideoCaption(string videoID, string language, string captionName, FileInfo fi)
        {
            UserCredential credential;
            //you should go out and get a json file that keeps your information... You can get that from the developers console...
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { YouTubeService.Scope.YoutubeForceSsl, YouTubeService.Scope.Youtube, YouTubeService.Scope.Youtubepartner },
                    "b.maximec@gmail.com",
                    CancellationToken.None
                );
            }

            //creates the service...
            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "UploadFromGithub",
            });

            //create a CaptionSnippet object...
            CaptionSnippet capSnippet = new CaptionSnippet
            {
                Language = language.ToLower(),
                Name = captionName,
                VideoId = videoID,
                IsDraft = false
            };

            //create new caption object
            Caption caption = new Caption
            {
                //set the completed snippet to the object now...
                Snippet = capSnippet
            };

            //here we read our .srt which contains our subtitles/captions...
            using (var fileStream = new FileStream(fi.FullName, FileMode.Open))
            {
                //create the request now and insert our params...
                var captionRequest = youtubeService.Captions.Insert(caption, "snippet", fileStream, "application/atom+xml");

                //finally upload the request... and wait.
                await captionRequest.UploadAsync();
            }
        }

        static async Task Main(string[] args)
        {
            //Reçoit en argument, le fichier contenant la liste des fichiers modifiés sur ce commit
            Console.WriteLine($"Received arg : {args[0]}");
            //var files = File.ReadAllText(args[0]);

            //await ProcessModifiedFiles(files.Split(','));
            Console.WriteLine($"Read secret : {Environment.GetEnvironmentVariable("CLIENT_SECRET")}");


        }
    }
}
