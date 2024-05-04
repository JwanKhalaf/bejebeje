namespace Bejebeje.Services.Services;

using System;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Interfaces;
using Models.Artist;
using Newtonsoft.Json;

public class EmailNotificationsService : IEmailNotificationsService
{
  public async Task AcknowledgeArtistSubmission(string username, CreateArtistViewModel model)
  {
    ArtistSubmission submission = new ArtistSubmission
    {
      Type = "artist_submission",
      Username = username,
      ArtistName = $"{model.FirstName} {model.LastName}",
      ArtistImageUrl = "",
      ArtistUrl = "/artists/dino/lyrics",
    };

    string messageBody = JsonConvert.SerializeObject(submission);

    var sqsClient = new AmazonSQSClient();

    SendMessageRequest sendMsgRequest = new SendMessageRequest
    {
      QueueUrl = "https://sqs.your-region.amazonaws.com/your-account-id/your-queue-name",
      MessageBody = messageBody,
    };

    try
    {
      await sqsClient.SendMessageAsync(sendMsgRequest);
      Console.WriteLine("message sent successfully!");
    }
    catch (Exception ex)
    {
      Console.WriteLine("Failed to send message. Error: " + ex.Message);
    }
  }
}

public class ArtistSubmission
{
  public string Type { get; set; }

  public string Username { get; set; }

  public string ArtistName { get; set; }

  public string ArtistImageUrl { get; set; }

  public string ArtistUrl { get; set; }
}