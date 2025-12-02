
namespace BoLLogic;

public sealed class SoundController
{
    private static readonly Lazy<SoundController> instance = new Lazy<SoundController>(() => new SoundController());

    public static SoundController Instance => instance.Value;


    public event EventHandler<SoundEventArgs> Play;


    public void PlaySound(string soundText)
    {
        Play?.Invoke(this, new SoundEventArgs(soundText));
    }

}
