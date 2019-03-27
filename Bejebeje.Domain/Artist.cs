namespace Bejebeje.Domain
{
  public class Artist : BaseEntity
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Slug { get; set; }
    public string ImageUrl { get; set; }
    public bool IsApproved { get; set; }
    public string UserId { get; set; }
  }
}
