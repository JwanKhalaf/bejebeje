namespace Bejebeje.Mvc.Tests.Extensions
{
  using System;
  using System.Collections.Generic;
  using System.Security.Claims;
  using FluentAssertions;
  using Mvc.Extensions;
  using NUnit.Framework;

  [TestFixture]
  public class ClaimsPrincipalExtensionsTests
  {
    [Test]
    public void should_return_cognito_user_id_from_sub_claim()
    {
      // arrange
      var claims = new List<Claim> { new Claim("sub", "abc-123") };
      var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

      // act
      var result = principal.GetCognitoUserId();

      // assert
      result.Should().Be("abc-123");
    }

    [Test]
    public void should_return_empty_string_when_sub_claim_missing()
    {
      // arrange
      var principal = new ClaimsPrincipal(new ClaimsIdentity());

      // act
      var result = principal.GetCognitoUserId();

      // assert
      result.Should().BeEmpty();
    }

    [Test]
    public void should_return_preferred_username_from_claim()
    {
      // arrange
      var claims = new List<Claim> { new Claim("preferred_username", "testuser") };
      var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));

      // act
      var result = principal.GetPreferredUsername();

      // assert
      result.Should().Be("testuser");
    }

    [Test]
    public void should_return_empty_string_when_preferred_username_missing()
    {
      // arrange
      var principal = new ClaimsPrincipal(new ClaimsIdentity());

      // act
      var result = principal.GetPreferredUsername();

      // assert
      result.Should().BeEmpty();
    }
  }
}
