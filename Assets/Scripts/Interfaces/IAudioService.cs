namespace Interfaces
{
    public interface IAudioService
    {
        public void PlayOneShot(string clipName);
        public void PlayMusic();
        public void StopMusic();
        public void SetSoundState(bool state);
    }
}
