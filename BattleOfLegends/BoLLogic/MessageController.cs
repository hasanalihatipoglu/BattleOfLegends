
namespace BoLLogic;

public sealed class MessageController
{
    private static readonly Lazy<MessageController> instance = new Lazy<MessageController>(() => new MessageController());

    public static MessageController Instance => instance.Value;


    public event EventHandler<MessageEventArgs> Message;
    public event EventHandler<MessageEventArgs> Info;


    public void Show(string msgText)
    {
        Message?.Invoke(this, new MessageEventArgs(msgText));
    }

    public void ShowWithOkButton(string msgText)
    {
        Message?.Invoke(this, new MessageEventArgs(msgText, requiresOkButton: true));
    }

    public void Write(string infoText)
    {
        Info?.Invoke(this, new MessageEventArgs(infoText));
    }
}
