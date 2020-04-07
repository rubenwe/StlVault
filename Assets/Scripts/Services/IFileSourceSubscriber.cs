using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Util.FileSystem;

namespace StlVault.Services
{
    internal interface IFileSourceSubscriber
    {
        /// <summary>
        /// Call for files added during the lifetime of the <see cref="IFileSource"/>
        /// </summary>
        Task OnItemsAddedAsync(IFileSource source, IReadOnlyCollection<IFileInfo> addedFiles); 
        
        /// <summary>
        /// Call for files removed during the lifetime of the <see cref="IFileSource"/>
        /// </summary>
        void OnItemsRemoved(IFileSource source, IReadOnlyCollection<string> removedItems);
    }
}