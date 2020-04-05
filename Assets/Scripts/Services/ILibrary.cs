using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Config;
using StlVault.Util;
using StlVault.Util.Collections;


namespace StlVault.Services
{
    internal interface ILibrary : ITagIndex
    {
        IPreviewList GetItemPreviewMetadata(IReadOnlyList<string> filters);

        Task AddTagAsync(IEnumerable<string> hashes, string tag);
        Task RemoveTagAsync(IEnumerable<string> hashes, string tag);
    }
}