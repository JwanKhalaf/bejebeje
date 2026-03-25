namespace Bejebeje.Services.Tests.Services
{
  using Bejebeje.Services.Services;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class CognitoUserInfoTests
  {
    [Test]
    public void should_store_sub_value()
    {
      var info = new CognitoUserInfo("user-sub-123", "testuser");
      info.Sub.Should().Be("user-sub-123");
    }

    [Test]
    public void should_store_preferred_username_value()
    {
      var info = new CognitoUserInfo("user-sub-123", "testuser");
      info.PreferredUsername.Should().Be("testuser");
    }

    [Test]
    public void should_support_value_equality()
    {
      var info1 = new CognitoUserInfo("sub-1", "user1");
      var info2 = new CognitoUserInfo("sub-1", "user1");
      info1.Should().Be(info2);
    }
  }
}
