using StlVault.Messages;
using StlVault.Util;
using StlVault.Util.Messaging;

namespace StlVault.ViewModels
{
    internal class ProgressModel : IMessageReceiver<ProgressMessage>
    {
        public BindableProperty<string> ProgressText { get; } = new BindableProperty<string>();
        
        public void Receive(ProgressMessage message)
        {
            ProgressText.Value = message.Text;
        }
    }
}