
namespace BoLLogic;

public sealed class MessageController
{
    private static MessageController instance = null;

    public static MessageController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MessageController();
            }
            return instance;
        }
    }


    public event EventHandler<MessageEventArgs> Message;
    public event EventHandler<MessageEventArgs> Info;


    public void Show(string msgText)
    {
        Message?.Invoke(this, new MessageEventArgs(msgText));
    }


    public void Write(string infoText)
    {
        Info?.Invoke(this, new MessageEventArgs(infoText));
    }
}
