namespace Bejebeje.Models.Homepage
{
  using System.Collections.Generic;
  using Bejebeje.Models.Artist;

  public class HomepageViewModel
  {
    public bool IsAuthenticated { get; set; }

    public IReadOnlyList<OpportunityCardViewModel> OpportunityCards { get; set; }

    public CommunityImpactStatsViewModel CommunityImpact { get; set; }

    public IEnumerable<RandomFemaleArtistItemViewModel> FemaleArtists { get; set; }
  }
}
