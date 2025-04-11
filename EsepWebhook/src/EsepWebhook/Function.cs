using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function {
    public string FunctionHandler(object input, ILambdaContext context) {
        context.Logger.LogInformation("FunctionHandler received an event.");

        var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
        if (string.IsNullOrEmpty(slackUrl)) {
            context.Logger.LogError("SLACK_URL environment variable not set.");
            throw new Exception("SLACK_URL environment variable not set.");
        }

        dynamic json;
        
        json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
        string issueUrl = json.issue.html_url;

        string payload = $"{{\"text\":\"Issue Created: {issueUrl}\"}}";

        var client = new HttpClient();
        var webRequest = new HttpRequestMessage(HttpMethod.Post, slackUrl) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        var response = client.Send(webRequest);
        using var reader = new StreamReader(response.Content.ReadAsStream());
        
        return reader.ReadToEnd();
    }
}
