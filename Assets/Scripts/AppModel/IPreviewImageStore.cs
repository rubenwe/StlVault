﻿using System.Threading.Tasks;
using StlVault.Config;

namespace StlVault.AppModel
{
    internal interface IPreviewImageStore
    {
        Task<byte[]> GetPreviewImageForItemAsync(string fileHash, ConfigVector3 rotation);
        Task StorePreviewImageForItemAsync(string fileHash, byte[] imageData, ConfigVector3 rotation);
    }
}