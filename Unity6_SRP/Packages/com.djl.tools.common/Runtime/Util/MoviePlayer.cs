using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

// /********************************
//  * Author  ： Dai'jinlong
//  * Date   ： 2017-12-22-16:15
//  * Version  ： V 0.1.0 
//  *******************************/
public class MoviePlayer : MonoBehaviour
{
    public AudioClip [] _audioClip;
    public  VideoClip[] _VideoClip;//所有的影片  
    private VideoPlayer vPlayer;
    private AudioSource _myAudioSource;
    private int _movieLength;
    private int _sourceLength;
    // 其中 videoPlayer.SetTargetAudioSource(0, this.GetComponent<AudioSource>()); 此方法用来添加指定的AudioSource组件，不然没有声音，此处的this.GetComponent<AudioSource>() 
    //      是因为我的对象上面挂了AudioSource，你也可以引用其他的地方的AudioSource
    //videoPlayer.playOnAwake=false; 这句话很重要，你可能知道playOnAwake是上面意思，但不明白它会造成什么影响，其实就是如果设置为true时，在awake的时候就已经开始播放视频了，
    //      而此时声音模块还没来得及加载，就会出现没有声音的情况，只有设置为false，当你播放第一个视频时才会有声音，它保证了在声音AudioSource模块设置完成的情况先才开始播放视频。
    void Start()
    {
      //  UIEventListener.Get(this.gameObject).onClick = MovieChange;
        _myAudioSource = GetComponent<AudioSource>();
        vPlayer = GetComponent<VideoPlayer>();
        vPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vPlayer.SetTargetAudioSource(0, this.GetComponent<AudioSource>());
        vPlayer.IsAudioTrackEnabled(0);
        vPlayer.Play();
        vPlayer.isLooping = true;
    }


    void URLStart()
    {
        //vPlayer = gameObject.AddComponent<VideoPlayer>();
        //vPlayer.URL = "http://www.quirksmode.org/html5/videos/big_buck_bunny.mp4";
        //vPlayer.target = UnityEngine.Video.VideoTarget.CameraFrontPlane;
        //vPlayer.alpha = 0.5f;
        //vPlayer.prepareCompleted += Prepared;
        //vPlayer.Prepare();
    }


    void Update()
    {
        if (vPlayer.clip.name != "title1" && (ulong)vPlayer.frame >= vPlayer.frameCount)//也可以这么判断是否播放完毕  
        {

            AudioPlay();
        }
    }


    //监听点击，改变播放视屏
    private void MovieChange(GameObject quad)
    {
        if (vPlayer!=null)
        {
            if (_movieLength < _VideoClip.Length -1)
            {
                _movieLength++;
                vPlayer.clip = _VideoClip[_movieLength];
                _sourceLength = Random .Range (0,5);
                _myAudioSource.clip = _audioClip[_sourceLength];
                _myAudioSource.Play();
            }
            else
            {
                _movieLength = 0;
                vPlayer.clip = _VideoClip[_movieLength];
                _sourceLength = Random.Range(0, 5);
                _myAudioSource.clip = _audioClip[_sourceLength];
                _myAudioSource.Play();
            }
        }
        else
        {
            vPlayer = GetComponent<VideoPlayer>();
        }

    }

    //随机声音播放
    private void AudioPlay()
    {
        if (vPlayer.isPlaying)
        {
            if (_myAudioSource != null)
            {
                _sourceLength = Random.Range(0, 5);
                _myAudioSource.clip = _audioClip[_sourceLength];
                _myAudioSource.Play();
            }
            else
            {
                _myAudioSource = GetComponent<AudioSource>();
            }
        }
    }
}
