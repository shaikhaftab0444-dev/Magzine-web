using Digital_Magzine.Models;

namespace Digital_Magzine.Models
{
    public class StorefrontViewModel
    {
        public List<MagazineIssue> MostPopular { get; set; } = new();
        public List<MagazineIssue> NewArrivals { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public List<Magazine> FeaturedMagazines { get; set; } = new();
        public Dictionary<string, List<MagazineIssue>> IssuesByCategory { get; set; } = new();
    }
}
