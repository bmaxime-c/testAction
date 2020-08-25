using Firebase.Database;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Auth.OAuth2.Responses;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace testAction
{
    public class MyCodeReceiver : ICodeReceiver
    {
        /// <inheritdoc/>
        public string RedirectUri
        {
            get; set;
        }

        public string Key { get; set; }

        public MyCodeReceiver(string key, string redirectUri)
        {
            Key = key;
            RedirectUri = redirectUri;
        }

        /// <inheritdoc/>
        public Task<AuthorizationCodeResponseUrl> ReceiveCodeAsync(AuthorizationCodeRequestUrl url,
            CancellationToken taskCancellationToken)
        {
            var authorizationUrl = url.Build().AbsoluteUri;
            TaskCompletionSource<AuthorizationCodeResponseUrl> completionSource = new TaskCompletionSource<AuthorizationCodeResponseUrl>();

            Console.WriteLine("Please visit the following URL in a web browser");
            Console.WriteLine(authorizationUrl);
            Console.WriteLine();

            var firebase = new FirebaseClient("https://uploader-1d84f.firebaseio.com/", new FirebaseOptions()
            {
            });

            var observable = firebase
              .Child("AUTHCODE")
              .AsObservable<string>()
              .Subscribe(d =>
              {
                  if (d.EventType == Firebase.Database.Streaming.FirebaseEventType.InsertOrUpdate)
                  {
                      if (d.Key == Key)
                      {
                          completionSource.SetResult(new AuthorizationCodeResponseUrl() { Code = d.Object });
                      }

                  }
              });

            taskCancellationToken.Register(() => { observable.Dispose(); });

            return completionSource.Task;
        }
    }
}
