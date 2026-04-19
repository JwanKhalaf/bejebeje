namespace Bejebeje.Mvc.Tests.Domain
{
  using System;
  using Bejebeje.Shared.Domain;
  using Bejebeje.Shared.Domain.Interfaces;
  using FluentAssertions;
  using NUnit.Framework;

  [TestFixture]
  public class LyricReportTests
  {
    [Test]
    public void should_implement_ibase_entity()
    {
      var report = new LyricReport();
      report.Should().BeAssignableTo<IBaseEntity>();
    }

    [Test]
    public void should_have_default_status_of_pending()
    {
      var report = new LyricReport();
      report.Status.Should().Be(0);
    }

    [Test]
    public void should_have_default_is_deleted_of_false()
    {
      var report = new LyricReport();
      report.IsDeleted.Should().BeFalse();
    }

    [Test]
    public void should_have_nullable_comment()
    {
      var report = new LyricReport();
      report.Comment.Should().BeNull();
    }

    [Test]
    public void should_have_nullable_actioned_by()
    {
      var report = new LyricReport();
      report.ActionedBy.Should().BeNull();
    }

    [Test]
    public void should_have_nullable_actioned_at()
    {
      var report = new LyricReport();
      report.ActionedAt.Should().BeNull();
    }

    [Test]
    public void should_have_nullable_modified_at()
    {
      var report = new LyricReport();
      report.ModifiedAt.Should().BeNull();
    }

    [Test]
    public void should_store_lyric_id()
    {
      var report = new LyricReport { LyricId = 42 };
      report.LyricId.Should().Be(42);
    }

    [Test]
    public void should_store_user_id()
    {
      var report = new LyricReport { UserId = "abc-123" };
      report.UserId.Should().Be("abc-123");
    }

    [Test]
    public void should_store_category()
    {
      var report = new LyricReport { Category = (int)ReportCategory.Duplicate };
      report.Category.Should().Be(2);
    }

    [Test]
    public void should_store_comment()
    {
      var report = new LyricReport { Comment = "this is a duplicate" };
      report.Comment.Should().Be("this is a duplicate");
    }

    [Test]
    public void should_store_created_at()
    {
      var now = DateTime.UtcNow;
      var report = new LyricReport { CreatedAt = now };
      report.CreatedAt.Should().Be(now);
    }

    [Test]
    public void should_store_actioned_by()
    {
      var report = new LyricReport { ActionedBy = "admin-user-id" };
      report.ActionedBy.Should().Be("admin-user-id");
    }

    [Test]
    public void should_store_actioned_at()
    {
      var now = DateTime.UtcNow;
      var report = new LyricReport { ActionedAt = now };
      report.ActionedAt.Should().Be(now);
    }
  }
}
