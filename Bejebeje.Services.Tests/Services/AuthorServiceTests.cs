//namespace Bejebeje.Services.Tests.Services
//{
//  using System;
//  using System.Collections.Generic;
//  using System.Threading.Tasks;
//  using Bejebeje.Common.Exceptions;
//  using Bejebeje.Domain;
//  using Bejebeje.Models.Author;
//  using Bejebeje.Services.Services;
//  using Bejebeje.Services.Tests.Helpers;
//  using FluentAssertions;
//  using NUnit.Framework;

//  [TestFixture]
//  public class AuthorServiceTests : DatabaseTestBase
//  {
//    private AuthorService authorService;

//    [SetUp]
//    public void Setup()
//    {
//      SetupDataContext();

//      authorService = new AuthorService(Context);
//    }

//    [Test]
//    public async Task GetAuthorDetailsAsync_WhenAuthorDoesNotExist_ThrowsAuthorNotFoundException()
//    {
//      // arrange
//      string authorSlug = "mr-orange";

//      // act
//      Func<Task> action = async () => await authorService.GetAuthorDetailsAsync(authorSlug);

//      // assert
//      await action.Should().ThrowAsync<AuthorNotFoundException>();
//    }

//    [Test]
//    public async Task GetAuthorDetailsAsync_WhenAuthorExists_ReturnsAuthorDetails()
//    {
//      // arrange
//      string authorSlug = "acdc";

//      int authorImageId = 4;
//      string authorFirstName = "AC/DC";
//      string authorBiography = "Awesome bio";
//      DateTime authorCreatedAt = new DateTime(2019, 7, 6, 8, 20, 0, DateTimeKind.Utc);

//      Author acdcAuthor = new Author
//      {
//        FirstName = authorFirstName,
//        CreatedAt = authorCreatedAt,
//        Biography = authorBiography,
//        Slugs = new List<AuthorSlug>
//        {
//          new AuthorSlug
//          {
//            Name = authorSlug,
//            CreatedAt = authorCreatedAt,
//            IsPrimary = true,
//          },
//        },
//        Image = new AuthorImage
//        {
//          Id = authorImageId,
//          CreatedAt = authorCreatedAt,
//        },
//      };

//      Context.Authors.Add(acdcAuthor);
//      Context.SaveChanges();

//      // act
//      AuthorDetailsResponse result = await authorService.GetAuthorDetailsAsync(authorSlug);

//      // assert
//      result.Should().NotBeNull();
//      result.FirstName.Should().Be(authorFirstName);
//      result.LastName.Should().BeNull();
//      result.Biography.Should().Be(authorBiography);
//      result.CreatedAt.Should().Be(authorCreatedAt);
//      result.Slug.Should().Be(authorSlug);
//      result.ModifiedAt.Should().BeNull();
//    }
//  }
//}
