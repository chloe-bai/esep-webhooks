using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook;

public class Function
{
    public string FunctionHandler(object input, ILambdaContext context)
    {
        context.Logger.LogInformation("FunctionHandler received an event.");
        context.Logger.LogInformation($"Raw input: {input}");

        var slackUrl = Environment.GetEnvironmentVariable("SLACK_URL");
        if (string.IsNullOrEmpty(slackUrl))
        {
            context.Logger.LogError("SLACK_URL environment variable not set.");
            throw new Exception("SLACK_URL environment variable not set.");
        }

        try
        {
            dynamic json = JsonConvert.DeserializeObject<dynamic>(input.ToString());
            string issueUrl = json.issue.html_url;

            context.Logger.LogInformation($"Extracted issue URL: {issueUrl}");

            string payload = $"{{\"text\":\"Issue Created: {issueUrl}\"}}";

            var client = new HttpClient();
            var webRequest = new HttpRequestMessage(HttpMethod.Post, slackUrl)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json")
            };

            var response = client.Send(webRequest);
            string result = new StreamReader(response.Content.ReadAsStream()).ReadToEnd();
            context.Logger.LogInformation($"Slack response: {result}");

            return result;
        }
        catch (Exception ex)
        {
            context.Logger.LogError($"Exception occurred: {ex.Message}");
            return $"Error: {ex.Message}";
        }
    }
}
