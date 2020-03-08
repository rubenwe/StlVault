namespace StlVault.Util.Messaging
{
    public interface IMessageReceiver
    {
    }

    public interface IMessageReceiver<in TMessage> : IMessageReceiver
    {
        void Receive(TMessage message);
    }
}