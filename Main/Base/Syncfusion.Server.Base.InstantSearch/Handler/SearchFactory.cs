
namespace Syncfusion.UMP.InstantSearch.Base
{
    public class SearchFactory
    {
        public static ISearchProcessor Create(string type)
        {
            if (string.IsNullOrWhiteSpace(type)) throw new SearchException("Invalid Search Type");
            switch (type.ToLower().Trim())
            {
                case "usersearch": return new UserProcessor();
                case "reportsearch": return new ReportsProcessor();
                case "groupsearch": return new GroupsProcessor();
                case "projectsearch": return new ProjectProcessor();
                case "all": return new AllProcesser();
                default: throw new SearchException("Invalid Search Type");
            }
        }
    }
}