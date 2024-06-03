using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Audio
{
    public enum BGMusic
    {
        None,
        MainMenu,
        GameTrack1,
        GameTrack2,
    }
    public enum AudioClips
    {
        Axe,
        Pick,
        Attack,
        Fireball,
        Hit,
    }




    public class AudioManager : StaticInstance<AudioManager>
    {
        public AudioSource GlobalSource;
        public AudioClip Axe;
        public AudioClip Pick;
        public AudioClip Attack;
        public AudioClip Fireball;
        public AudioClip Hit;

        public class AudioWrapper : MonoBehaviour
        {
            private bool isInitialized = false;
            private bool isPlaying = false;

            private AudioSource audioSource;
            private float defaultAudioVolume;
            private float volumeMultiplier = 0.5f;

            public bool Initialize(AudioSource audioSource, float volumeMultiplier = 0.5f)
            {
                if (isInitialized) return false;

                this.audioSource = audioSource;
                audioSource.transform.position = Vector3.zero;

                //Add default volume and volume mutiplier
                defaultAudioVolume = this.audioSource.volume;
                this.volumeMultiplier = volumeMultiplier;

                ChangeVolume(volumeMultiplier);

                return true;
            }

            public void ChangeVolume(float volumeMultiplier)
            {
                this.audioSource.volume = defaultAudioVolume * volumeMultiplier;
            }

            public AudioSource GetAudioSource()
            {
                return audioSource;
            }

            public AudioClip GetAudioClip()
            {
                return audioSource.clip;
            }

            public void Play()
            {
                Play(Vector3.zero);
            }

            public void Play(Vector3 position)
            {
                if (audioSource.isPlaying) return;

                gameObject.SetActive(true);
                Instance.PlayClip(this, position);
            }

            public bool IsPlaying()
            {
                if (audioSource.isPlaying) return true;

                else
                {
                    gameObject.SetActive(false);
                    return false;
                }
            }
        }

        [SerializeField, Header("SFX Audio")] private List<AudioSource> SFX_Sources;
        [SerializeField] private float SFXMuffleRange = 5;
        [SerializeField] private float SFXMuffleTime = 0.1f;

        [SerializeField, Header("Background Audio")] private AudioSource bg_source;
        [SerializeField] private AudioClip MainMenu_BG_Clip;
        [SerializeField] private AudioClip GameTrack1;
        [SerializeField] private AudioClip GameTrack2;
        [SerializeField, Range(0, 100)] private float BG_Max_Volume = 50;
        [SerializeField, Range(0, 5)] private float BG_Fade_Time = 1;

        private BGMusic bgMusicState = BGMusic.None;
        private Coroutine currentTransitionRoutine;


        [SerializeField, Header("Pool")] int initalPoolSize = 100;
        [SerializeField] int poolExtension = 25;

        [SerializeField] private AudioSource audioSourceTest;

        private Dictionary<AudioSource, Queue<AudioWrapper>> audioPools = new();
        private HashSet<AudioWrapper> currentDequeuedAudio = new(50);
        private List<AudioWrapper> audioToRemove = new(50);
        private List<(Vector3 pos, float time)> currentAudioPositions = new(50);
        private List<(Vector3 pos, float time)> audioPositionsToRemove = new(50);

        [ReadOnly, SerializeField] float _SFXVolume = 1f;
        public float SFXVolume { get => _SFXVolume / 2f; }//Remap 0-2 to 0-1
        public float BGMVolume { get => BG_Max_Volume / 100f; }

        public Dictionary<AudioClips, AudioClip> audioDict;

        protected override void Awake()
        {
            base.Awake();

            foreach (AudioSource audioSource in SFX_Sources)
            {
                CreateNewAudioPool(audioSource);
            }

            audioDict = new()
            {
                [AudioClips.Attack] = Attack,
                [AudioClips.Pick] = Pick,
                [AudioClips.Axe] = Axe,
                [AudioClips.Fireball] = Fireball,
                [AudioClips.Hit] = Hit,
            };

            bg_source.volume = 0;
            //BG_Source.Play();
        }

        public void ChangeBGMusicState(BGMusic bgMusic)
        {
            if (bgMusic == bgMusicState) return;

            AudioClip nextClip = bgMusic switch
            {
                BGMusic.None => null,
                BGMusic.MainMenu => MainMenu_BG_Clip,
                BGMusic.GameTrack1 => GameTrack1,
                BGMusic.GameTrack2 => GameTrack2,
                _ => null
            };

            if(currentTransitionRoutine != null)
            {
                StopCoroutine(currentTransitionRoutine);
            }

            currentTransitionRoutine = StartCoroutine(TransitionMusicBG(nextClip));
            
        }

        IEnumerator TransitionMusicBG(AudioClip newClip)
        {
            float elapsedTime = 0f;
            float startVolume = bg_source.volume;

            while (elapsedTime < BG_Fade_Time)
            {
                elapsedTime += Time.deltaTime;
                bg_source.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / BG_Fade_Time);
                yield return null;
            }

            bg_source.volume = 0f;

            if (newClip != null)
            {

                bg_source.clip = newClip;
                bg_source.Play();

                elapsedTime = 0f;

                while (elapsedTime < BG_Fade_Time)
                {
                    elapsedTime += Time.deltaTime;
                    bg_source.volume = Mathf.Lerp(0f, BG_Max_Volume/100f, elapsedTime / BG_Fade_Time);
                    yield return null;
                }

                bg_source.volume = BG_Max_Volume / 100f;
            }
            else
            {
                bg_source.volume = BG_Max_Volume / 100f;
            }
        }

        private void CreateNewAudioPool(AudioSource audioSource)
        {
            audioPools.Add(audioSource, new());
            ExtendAudioPool(audioSource, initalPoolSize);
        }

        private void ExtendAudioPool(AudioSource audioSource, int amount)
        {

            for (int i = 0; i < amount; i++)
            {


                AudioSource newAudio = Instantiate(audioSource, Vector3.zero, Quaternion.identity, transform);
                AudioWrapper audioWrapper = newAudio.AddComponent<AudioWrapper>();
                audioWrapper.gameObject.SetActive(false);
                audioWrapper.Initialize(newAudio, _SFXVolume);

                audioPools[audioSource].Enqueue(audioWrapper);
            }
        }

        public AudioWrapper RequestAudioSource(AudioSource audioClip)
        {

            if (!audioPools.ContainsKey(audioClip))
            {
                CreateNewAudioPool(audioClip);
            }

            if (!audioPools[audioClip].TryDequeue(out AudioWrapper result))
            {
                ExtendAudioPool(audioClip, poolExtension);
                result = audioPools[audioClip].Dequeue();
            }

            currentDequeuedAudio.Add(result);

            return result;
        }

        public void PlayClip(AudioWrapper wrapper)
        {
            PlayClip(wrapper, Vector3.zero);
        }

        public void PlayClip(AudioWrapper wrapper, Vector3 pos)
        {
            foreach (var audioPosition in currentAudioPositions)
            {
                float distance = Vector3.Distance(pos, audioPosition.pos);

                if (distance < SFXMuffleRange)
                {
                    return;
                }
            }

            currentAudioPositions.Add((pos, Time.time));
            wrapper.transform.position = pos;
            wrapper.GetAudioSource().Play();
        }

        public void ReturnAudioToPool(AudioWrapper wrapper)
        {
            if (!audioPools.ContainsKey(wrapper.GetAudioSource()))
            {
                Destroy(wrapper);
                Destroy(wrapper.GetAudioSource());
            }

            audioPools[wrapper.GetAudioSource()].Enqueue(wrapper);
            currentDequeuedAudio.Remove(wrapper);
        }

        //public void SpawnTestAudio(Vector3 position)
        //{

        //    foreach (var audioPosition in currentAudioPositions)
        //    {
        //        float distance = Vector3.Distance(position, audioPosition.pos);

        //        if (distance < SFXMuffleRange)
        //        {
        //            return;
        //        }
        //    }

        //    var source = RequestAudioClip(audioSourceTest);

        //    currentAudioPositions.Add((position, Time.time));
        //    source.transform.position = position;
        //    source.Play();
        //}

        //[Button(enabledMode: EButtonEnableMode.Playmode)]
        //public void SpawnTestAudio()
        //{
        //    var source = RequestAudioClip(audioSourceTest);
        //    source.transform.position = Vector3.zero;

        //    source.Play();
        //}

        #region Volume Setting
        public void SetBGMVolume(float volume)
        {
            bg_source.volume = volume;
            BG_Max_Volume = volume * 100f;
        }

        public void SetSFXVolume(float volume)
        {
            foreach(KeyValuePair<AudioSource,Queue<AudioWrapper>> kinds in audioPools)
            {
                foreach(AudioWrapper audioWrapper in kinds.Value)
                {
                    //Remap 0-1 to 0-2
                    audioWrapper.ChangeVolume(volume * 2f);
                }
            }
        }

        #endregion Volume Setting

        private void Update()
        {

            audioToRemove.Clear();
            audioPositionsToRemove.Clear();

            foreach (var currentAudio in currentAudioPositions)
            {

                if (Time.time - currentAudio.time >= SFXMuffleTime)
                {
                    audioPositionsToRemove.Add(currentAudio);
                }
            }

            for (int i = audioPositionsToRemove.Count - 1; i >= 0; i--)
            {
                currentAudioPositions.Remove(audioPositionsToRemove[i]);
            }

            foreach (AudioWrapper audioWrapper in currentDequeuedAudio)
            {
                if(!audioWrapper.IsPlaying())
                {
                    audioToRemove.Add(audioWrapper);
                }
            }

            for (int i = audioToRemove.Count - 1; i >= 0; i--)
            {
                ReturnAudioToPool(audioToRemove[i]);
            }
        }
    }

}