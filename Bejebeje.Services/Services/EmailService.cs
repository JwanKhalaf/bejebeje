namespace Bejebeje.Services.Services;

using System;
using System.Threading.Tasks;
using Amazon.SimpleEmailV2;
using Amazon.SimpleEmailV2.Model;
using Bejebeje.Services.Services.Interfaces;
using Microsoft.Extensions.Logging;

public class EmailService : IEmailService
{
  private const string SenderEmail = "jk.bejebeje@gmail.com";
  private readonly IAmazonSimpleEmailServiceV2 _sesClient;
  private readonly ILogger<EmailService> _logger;

  public EmailService(IAmazonSimpleEmailServiceV2 sesClient, ILogger<EmailService> logger)
  {
    _sesClient = sesClient;
    _logger = logger;
  }

  public async Task SendArtistSubmissionEmailAsync(
    string username,
    string artistFullName)
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
        ToAddresses = [SenderEmail],
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

  public async Task SendLyricSubmissionEmailAsync(
    string username,
    string lyricTitle,
    int lyricId,
    string artistFullName,
    int artistId)
  {
    string subject = $"New lyric submission ({lyricTitle} under {artistFullName})";
    string body = $"""

                   Hello,

                   {username} has added a lyric titled {lyricTitle} (Id: {lyricId}) under the artist {artistFullName} (Id: {artistId}).

                   Please review as soon as possible.

                   Thanks
                   """;

    var sendRequest = new SendEmailRequest
    {
      FromEmailAddress = SenderEmail,
      Destination = new Destination
      {
        ToAddresses = [SenderEmail],
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
      Console.WriteLine($"Lyric submission email sent! MessageId: {response.MessageId}");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Failed to send lyric submission email: {ex.Message}");
      throw;
    }
  }

  public async Task SendLyricReportNotificationEmailAsync(
    string reporterUsername,
    string lyricTitle,
    string artistName,
    string categoryDisplayLabel,
    string comment)
  {
    string subject = $"New lyric report: {lyricTitle} by {artistName}";
    string commentSection = string.IsNullOrEmpty(comment)
      ? "No comment provided."
      : $"Comment: {comment}";

    string body = $"""

                   Hello,

                   A new lyric report has been submitted.

                   Reporter: {reporterUsername}
                   Lyric: {lyricTitle}
                   Artist: {artistName}
                   Category: {categoryDisplayLabel}
                   {commentSection}

                   Please review as soon as possible.

                   Thanks
                   """;

    var sendRequest = new SendEmailRequest
    {
      FromEmailAddress = SenderEmail,
      Destination = new Destination
      {
        ToAddresses = [SenderEmail],
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
      _logger.LogInformation("lyric report notification email sent, message id: {MessageId}", response.MessageId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to send lyric report notification email for lyric {LyricTitle}", lyricTitle);
      throw;
    }
  }

  public async Task SendLyricReportConfirmationEmailAsync(
    string reporterEmail,
    string lyricTitle,
    string artistName)
  {
    string subject = $"Your report for \"{lyricTitle}\" has been received";
    string body = $"""

                   Hello,

                   Thank you for your report. Your report for "{lyricTitle}" by {artistName} has been received and will be reviewed.

                   We appreciate your help in keeping Bejebeje accurate and high quality.

                   Thanks,
                   The Bejebeje Team
                   """;

    var sendRequest = new SendEmailRequest
    {
      FromEmailAddress = SenderEmail,
      Destination = new Destination
      {
        ToAddresses = [reporterEmail],
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
      _logger.LogInformation("lyric report confirmation email sent, message id: {MessageId}", response.MessageId);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "failed to send lyric report confirmation email for lyric {LyricTitle}", lyricTitle);
      throw;
    }
  }
}