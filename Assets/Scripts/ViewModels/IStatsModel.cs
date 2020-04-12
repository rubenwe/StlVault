using StlVault.Util;

namespace StlVault.ViewModels
{
    internal interface IStatsModel
    {
        BindableProperty<string> FileName { get; } 
        BindableProperty<int> VertexCount { get; } 
        BindableProperty<int> TriangleCount { get; }
        BindableProperty<float> Width { get; } 
        BindableProperty<float> Height { get; } 
        BindableProperty<float> Depth { get; } 
        BindableProperty<float> Volume { get; } 
    }
}