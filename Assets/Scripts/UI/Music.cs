namespace IL6
{
    public static class Music
    {
        private static Phase? _currentPhase;

        public static float Volume
        {
            get => RuntimeAudioDirector.BgmVolume;
            set => RuntimeAudioDirector.SetBgmVolume(value);
        }

        public static void PlayForPhase(Phase phase)
        {
            bool isFirstCall = _currentPhase == null;
            bool phaseChanged = !isFirstCall && _currentPhase != phase;
            _currentPhase = phase;
            RuntimeAudioDirector.PlayCue(RuntimeAudioDirector.CueBgmLoop);
            if (phaseChanged) RuntimeAudioDirector.PlayCue(RuntimeAudioDirector.CueTransition);
        }

        public static void Stop() { _currentPhase = null; }
    }
}
