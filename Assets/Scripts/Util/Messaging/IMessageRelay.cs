namespace StlVault.Util.Messaging
{
    public interface IMessageRelay
    {
        void Send<TMessage>(object sender) where TMessage : new();
        void Send<TMessage>(object sender, TMessage message);
    }
}