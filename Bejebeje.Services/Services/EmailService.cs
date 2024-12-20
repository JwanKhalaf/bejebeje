namespace Bejebeje.Services.Services;

using System;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Bejebeje.Services.Services.Interfaces;

public class EmailService : IEmailService
{
  private const string SenderEmail = "jk.bejebeje@gmail.com";
  private readonly IAmazonSimpleEmailServiceV2 _sesClient;

  public EmailService(IAmazonSimpleEmailServiceV2 sesClient)
  {
    _sesClient = sesClient;
  }

  public async Task SendArtistSubmissionEmailAsync(string username, string artistFullName)
  {
    string subject = $"New artist submission ({artistFullName})";
    string body = $"""

                   Hello,

                   {username} has added {artistFullName} as an artist.

                   Please review as soon as possible.

                   Thanks
                   """;

    var sendRequest = new SendEmailRequest
    {
      FromEmailAddress = SenderEmail,
      Destination = new Destination
      {
        ToAddresses = ["jk.bejebeje@gmail.com"],
      },
      Content = new EmailContent
      {
        Simple = new Message
        {
          Subject = new Content
          {
            Data = subject,
          },
          Body = new Body
          {
            Text = new Content
            {
              Data = body,
            },
          },
        },
      },
    };

    try
    {
      var response = await _sesClient.SendEmailAsync(sendRequest);
      Console.WriteLine($"Email sent! MessageId: {response.MessageId}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failed to send email: {ex.Message}");
      throw;
    }
  }
}