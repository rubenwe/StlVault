using System.Collections.Generic;
using System.Threading.Tasks;
using StlVault.Util.FileSystem;

namespace StlVault.Services
{
    internal interface IFileSourceSubscriber
    {
        /// <summary>
        /// Initialize is called first to set up a baseline.
        /// It should only be called once and should contain all infos.
        /// </summary>
        Task OnInitializeAsync(IFileSource source, IReadOnlyCollection<IFileInfo> files);
        
        /// <summary>
        /// Call for files added during the lifetime of the <see cref="IFileSource"/>
        /// </summary>
        Task OnItemsAddedAsync(IFileSource source, IReadOnlyCollection<IFileInfo> addedFiles); 
        
        /// <summary>
        /// Call for files removed during the lifetime of the <see cref="IFileSource"/>
        /// </summary>
        Task OnItemsRemovedAsync(IFileSource source, IReadOnlyCollection<string> removedItems);
    }
}