namespace Bejebeje.Mvc.Tests.Controllers
{
  using System;
  using FluentAssertions;
  using Microsoft.AspNetCore.Diagnostics;
  using Microsoft.AspNetCore.Http;
  using Microsoft.AspNetCore.Http.Features;
  using Microsoft.AspNetCore.Mvc;
  using Microsoft.Extensions.Logging;
  using Moq;
  using Mvc.Controllers;
  using NUnit.Framework;

  [TestFixture]
  public class StatusCodeControllerNotFoundTests
  {
    private Mock<ILogger<StatusCodeController>> _mockLogger;
    private StatusCodeController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockLogger = new Mock<ILogger<StatusCodeController>>();
      _controller = new StatusCodeController(_mockLogger.Object);
      SetupHttpContext();
    }

    [Test]
    public void should_return_not_found_view()
    {
      // act
      var result = _controller.NotFound();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.ViewName.Should().Be("NotFound");
    }

    [Test]
    public void should_set_response_status_code_to_404()
    {
      // act
      _controller.NotFound();

      // assert
      _controller.Response.StatusCode.Should().Be(404);
    }

    [Test]
    public void should_capture_original_path_from_status_code_reexecute_feature()
    {
      // arrange
      var feature = new Mock<IStatusCodeReExecuteFeature>();
      feature.Setup(f => f.OriginalPath).Returns("/some/missing/page");
      _controller.HttpContext.Features.Set(feature.Object);

      // act
      _controller.NotFound();

      // assert
      _controller.ViewData["OriginalPath"].Should().Be("/some/missing/page");
    }

    [Test]
    public void should_set_original_path_to_null_when_feature_not_present()
    {
      // act
      _controller.NotFound();

      // assert
      _controller.ViewData["OriginalPath"].Should().BeNull();
    }

    private void SetupHttpContext()
    {
      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext(),
      };
    }
  }

  [TestFixture]
  public class StatusCodeControllerServerErrorTests
  {
    private Mock<ILogger<StatusCodeController>> _mockLogger;
    private StatusCodeController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockLogger = new Mock<ILogger<StatusCodeController>>();
      _controller = new StatusCodeController(_mockLogger.Object);
      SetupHttpContext();
    }

    [Test]
    public void should_return_server_error_view()
    {
      // act
      var result = _controller.ServerError();

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.ViewName.Should().Be("ServerError");
    }

    [Test]
    public void should_set_response_status_code_to_500()
    {
      // act
      _controller.ServerError();

      // assert
      _controller.Response.StatusCode.Should().Be(500);
    }

    [Test]
    public void should_set_request_id_in_view_data()
    {
      // arrange
      _controller.HttpContext.TraceIdentifier = "test-trace-id";

      // act
      _controller.ServerError();

      // assert
      _controller.ViewData["RequestId"].Should().Be("test-trace-id");
    }

    [Test]
    public void should_log_exception_when_exception_feature_present()
    {
      // arrange
      var exception = new InvalidOperationException("test error");
      var feature = new Mock<IExceptionHandlerPathFeature>();
      feature.Setup(f => f.Error).Returns(exception);
      feature.Setup(f => f.Path).Returns("/broken/page");
      _controller.HttpContext.Features.Set(feature.Object);

      // act
      _controller.ServerError();

      // assert
      _mockLogger.Verify(
        x => x.Log(
          LogLevel.Error,
          It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => true),
          exception,
          It.IsAny<Func<It.IsAnyType, Exception, string>>()),
        Times.Once);
    }

    [Test]
    public void should_not_throw_when_exception_feature_not_present()
    {
      // act
      var act = () => _controller.ServerError();

      // assert
      act.Should().NotThrow();
    }

    private void SetupHttpContext()
    {
      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext(),
      };
    }
  }

  [TestFixture]
  public class StatusCodeControllerStatusCodePageTests
  {
    private Mock<ILogger<StatusCodeController>> _mockLogger;
    private StatusCodeController _controller;

    [SetUp]
    public void SetUp()
    {
      _mockLogger = new Mock<ILogger<StatusCodeController>>();
      _controller = new StatusCodeController(_mockLogger.Object);
      _controller.ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext(),
      };
    }

    [Test]
    public void should_redirect_to_not_found_for_404()
    {
      // act
      var result = _controller.StatusCodePage(404);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("NotFound");
    }

    [Test]
    public void should_redirect_to_server_error_for_500()
    {
      // act
      var result = _controller.StatusCodePage(500);

      // assert
      var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
      redirect.ActionName.Should().Be("ServerError");
    }

    [Test]
    public void should_return_generic_error_view_for_403()
    {
      // act
      var result = _controller.StatusCodePage(403);

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.ViewName.Should().Be("GenericError");
      view.Model.Should().Be(403);
    }

    [Test]
    public void should_return_generic_error_view_for_401()
    {
      // act
      var result = _controller.StatusCodePage(401);

      // assert
      var view = result.Should().BeOfType<ViewResult>().Subject;
      view.ViewName.Should().Be("GenericError");
      view.Model.Should().Be(401);
    }

    [Test]
    public void should_set_response_status_code_to_provided_code()
    {
      // act
      _controller.StatusCodePage(403);

      // assert
      _controller.Response.StatusCode.Should().Be(403);
    }
  }
}
