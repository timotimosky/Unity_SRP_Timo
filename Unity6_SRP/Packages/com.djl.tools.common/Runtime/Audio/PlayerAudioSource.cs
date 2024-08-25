using UnityEngine;

    public class PlayerAudioSource
    {
        #region Singleton
        private static PlayerAudioSource m_Instance = new PlayerAudioSource();
        public static PlayerAudioSource Instance { get { return m_Instance; } }
        #endregion

        //统一调节整个游戏的音量大小
        [SerializeField][Range(0f, 1f)] private float m_Volume = 1f;

        /// <summary>
        /// Gets or sets the volume level.
        /// </summary>
        public float volume { get { return this.m_Volume; } set { this.m_Volume = value; } }

         public void PlayAudio(AudioSource m_AudioSource, float volume =1)
        {
            m_AudioSource.PlayOneShot(m_AudioSource.clip, this.m_Volume * volume);
        }
    }

