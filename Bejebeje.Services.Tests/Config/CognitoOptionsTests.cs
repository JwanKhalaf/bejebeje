namespace Bejebeje.Services.Tests.Config
{
  using Bejebeje.Services.Config;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class CognitoOptionsTests
  {
    [Test]
    public void should_have_client_id_property()
    {
      var options = new CognitoOptions();
      options.ClientId = "test-client-id";
      options.ClientId.Should().Be("test-client-id");
    }

    [Test]
    public void should_have_client_secret_property()
    {
      var options = new CognitoOptions();
      options.ClientSecret = "test-secret";
      options.ClientSecret.Should().Be("test-secret");
    }

    [Test]
    public void should_have_authority_property()
    {
      var options = new CognitoOptions();
      options.Authority = "https://cognito-idp.eu-west-2.amazonaws.com/test-pool";
      options.Authority.Should().Be("https://cognito-idp.eu-west-2.amazonaws.com/test-pool");
    }

    [Test]
    public void should_have_user_pool_id_property()
    {
      var options = new CognitoOptions();
      options.UserPoolId = "eu-west-2_abc123";
      options.UserPoolId.Should().Be("eu-west-2_abc123");
    }
  }
}
