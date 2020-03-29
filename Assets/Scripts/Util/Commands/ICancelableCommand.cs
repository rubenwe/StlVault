namespace StlVault.Util.Commands
{
    public interface ICancelableCommand : IAsyncCommand
    {
        IAsyncCommand CancelCommand { get; }
    }
}