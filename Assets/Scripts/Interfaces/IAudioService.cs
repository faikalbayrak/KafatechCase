namespace Interfaces
{
    public interface IAudioService
    {
        public void PlayOneShot(string clipName, float volumeScale = 1);
        public void PlayMusic();
        public void StopMusic();
        public void SetSoundState(bool state);
    }
}
