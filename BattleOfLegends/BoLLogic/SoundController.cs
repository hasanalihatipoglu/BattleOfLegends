
namespace BoLLogic;

public sealed class SoundController
{
    private static SoundController instance = null;

    public static SoundController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SoundController();
            }
            return instance;
        }
    }


    public event EventHandler<SoundEventArgs> Play;


    public void PlaySound(string soundText)
    {
        Play?.Invoke(this, new SoundEventArgs(soundText));
    }

}
