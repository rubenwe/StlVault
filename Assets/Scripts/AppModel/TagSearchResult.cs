namespace StlVault.AppModel
{
    public struct TagSearchResult
    {
        public string SearchTag { get; }
        public int MatchingItemCount { get; }
        
        public TagSearchResult(string searchTag, int matchingItemCount)
        {
            MatchingItemCount = matchingItemCount;
            SearchTag = searchTag;
        }
    }
}