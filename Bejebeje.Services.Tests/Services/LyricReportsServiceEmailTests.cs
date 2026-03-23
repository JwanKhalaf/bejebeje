namespace Bejebeje.Services.Tests.Services
{
  using System;
  using System.Threading.Tasks;
  using Bejebeje.Services.Config;
  using Bejebeje.Services.Services;
  using Bejebeje.Services.Services.Interfaces;
  using FluentAssertions;
  using Microsoft.Extensions.Logging;
  using Microsoft.Extensions.Options;
  using Moq;
  using NUnit.Framework;

  [TestFixture]
  public class LyricReportsServiceEmailTests
  {
    private Mock<IOptionsMonitor<DatabaseOptions>> _mockOptions;
    private Mock<IEmailService> _mockEmailService;
    private Mock<ICognitoService> _mockCognitoService;
    private Mock<ILogger<LyricReportsService>> _mockLogger;
    private LyricReportsService _service;

    [SetUp]
    public void SetUp()
    {
      _mockOptions = new Mock<IOptionsMonitor<DatabaseOptions>>();
      _mockOptions.Setup(o => o.CurrentValue).Returns(new DatabaseOptions { ConnectionString = "fake" });
      _mockEmailService = new Mock<IEmailService>();
      _mockCognitoService = new Mock<ICognitoService>();
      _mockLogger = new Mock<ILogger<LyricReportsService>>();

      _service = new LyricReportsService(
        _mockOptions.Object,
        _mockEmailService.Object,
        _mockCognitoService.Object,
        _mockLogger.Object);
    }

    // --- 12.9: email error isolation tests ---

    [Test]
    public async Task should_not_throw_when_admin_notification_email_fails()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      _mockEmailService
        .Setup(e => e.SendLyricReportNotificationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("ses error"));

      // act
      Func<Task> act = () => _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task should_not_throw_when_reporter_confirmation_email_fails()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      _mockEmailService
        .Setup(e => e.SendLyricReportNotificationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>()))
        .Returns(Task.CompletedTask);

      _mockEmailService
        .Setup(e => e.SendLyricReportConfirmationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("ses error"));

      // act
      Func<Task> act = () => _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task should_not_throw_when_both_emails_fail()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      _mockEmailService
        .Setup(e => e.SendLyricReportNotificationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
          It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("ses error 1"));

      _mockEmailService
        .Setup(e => e.SendLyricReportConfirmationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("ses error 2"));

      // act
      Func<Task> act = () => _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task should_still_send_admin_email_when_reporter_email_fails()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      _mockEmailService
        .Setup(e => e.SendLyricReportConfirmationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("ses error"));

      // act
      await _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", "some comment");

      // assert
      _mockEmailService.Verify(
        e => e.SendLyricReportNotificationEmailAsync(
          "testuser", "Test Lyric", "Test Artist", "Duplicate", "some comment"),
        Times.Once);
    }

    [Test]
    public async Task should_skip_reporter_email_when_email_claim_is_null()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      // act
      await _service.SendReportEmailsAsync(
        "user-1", null, "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      _mockEmailService.Verify(
        e => e.SendLyricReportConfirmationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    [Test]
    public async Task should_skip_reporter_email_when_email_claim_is_empty()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      // act
      await _service.SendReportEmailsAsync(
        "user-1", "", "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      _mockEmailService.Verify(
        e => e.SendLyricReportConfirmationEmailAsync(
          It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
        Times.Never);
    }

    [Test]
    public async Task should_use_fallback_username_when_cognito_lookup_fails()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ThrowsAsync(new Exception("cognito error"));

      // act
      await _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", null);

      // assert
      _mockEmailService.Verify(
        e => e.SendLyricReportNotificationEmailAsync(
          "Unknown User", "Test Lyric", "Test Artist", "Duplicate", null),
        Times.Once);
    }

    [Test]
    public async Task should_send_both_emails_on_success()
    {
      // arrange
      _mockCognitoService
        .Setup(c => c.GetPreferredUsernameAsync("user-1"))
        .ReturnsAsync("testuser");

      // act
      await _service.SendReportEmailsAsync(
        "user-1", "test@example.com", "Test Lyric", "Test Artist", "Duplicate", "my comment");

      // assert
      _mockEmailService.Verify(
        e => e.SendLyricReportNotificationEmailAsync(
          "testuser", "Test Lyric", "Test Artist", "Duplicate", "my comment"),
        Times.Once);

      _mockEmailService.Verify(
        e => e.SendLyricReportConfirmationEmailAsync(
          "test@example.com", "Test Lyric", "Test Artist"),
        Times.Once);
    }
  }
}
