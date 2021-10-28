using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonobitEngine;

public class SoundManager : MonobitEngine.MonoBehaviour
{
    private AudioSource bgmAudio;

    MonobitView m_MonobitView = null;

    public AudioClip mainBGM;
    public AudioClip titleBGM;
    public AudioClip waitTimeBGM;

    [System.NonSerialized]
    public float fadeTime;

    [System.NonSerialized]
    public bool fadeInFlg;

    [System.NonSerialized]
    public bool fadeOutFlg;

    float fadeDeltaTime = 0;
    private bool isMainSoundPlay;
    private bool isTitleSoundPlay;
    private bool isWaitSoundPlay;

    private bool isStart;

    GameManager gameManager;
    
    // Start is called before the first frame update
    void Start()
    {
        bgmAudio = this.gameObject.GetComponent<AudioSource>();

        m_MonobitView = GetComponent<MonobitView>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        fadeDeltaTime = 0.0f;
        fadeInFlg = false;
        fadeOutFlg = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isStart == false)
        {
            if (!MonobitNetwork.inRoom)
            {
                if (isTitleSoundPlay == false)
                {
                    bgmAudio.clip = titleBGM;
                    AudioPlay();
                    isTitleSoundPlay = true;
                }
            }
            else
            {
                AudioStop();
                isStart = true;
                isTitleSoundPlay = false;
            }
        }
        else
        {
            if (!gameManager.IsPlaying() && isWaitSoundPlay == false)
            {
                bgmAudio.clip = waitTimeBGM;
                AudioPlay();
                isWaitSoundPlay = true;
            }
            else if (gameManager.IsPlaying() && isMainSoundPlay == false)
            {
                bgmAudio.clip = mainBGM;
                AudioPlay();
                isMainSoundPlay = true;
                isWaitSoundPlay = false;
            }
            else if (!gameManager.IsPlaying() && isMainSoundPlay == true)
            {
                AudioStop();
                isStart = false;
                isMainSoundPlay = false;
            }
        }
     

        if (fadeInFlg == true)
        {
            fadeDeltaTime += Time.deltaTime;
            if (fadeDeltaTime >= fadeTime)
            {
                fadeDeltaTime = 0.0f;
                fadeInFlg = false;
            }
            else
            {
                bgmAudio.volume = (fadeDeltaTime / fadeTime) * 0.7f;
            }
        }
        if (fadeOutFlg == true)
        {
            fadeDeltaTime += Time.deltaTime;
            if (fadeDeltaTime >= fadeTime)
            {
                fadeDeltaTime = 0.0f;
                fadeOutFlg = false;
                bgmAudio.Stop();
            }
            else
            {
                bgmAudio.volume = (1.0f - fadeDeltaTime / fadeTime) * 1.0f;
            }
        }

    }

    //-----------------------------------------------------------------------------
    //! [内容]		オーディオを最初から再生する
    //-----------------------------------------------------------------------------
    public void AudioPlay()
    {
        bgmAudio.Play();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		オーディオを完全に止める
    //-----------------------------------------------------------------------------
    public void AudioStop()
    {
        bgmAudio.Stop();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		オーディオを一時的に止める
    //-----------------------------------------------------------------------------
    public void AudioPause()
    {
        bgmAudio.Pause();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		オーディオの再生をを再開する
    //-----------------------------------------------------------------------------
    public void AudioUnPause()
    {
        bgmAudio.UnPause();
    }

    //-----------------------------------------------------------------------------
    //! [内容]		オーディオの音量を変更する
    //-----------------------------------------------------------------------------
    public void SetAudioVolume(float vol)
    {
        bgmAudio.volume = vol;
    }
    //-----------------------------------------------------------------------------
    //! [内容]		オーディオの音量を次第に大きくする
    //-----------------------------------------------------------------------------
    public void FadeInAudio(float time)
    {
        fadeTime = time;
        fadeInFlg = true;
    }
    //-----------------------------------------------------------------------------
    //! [内容]		オーディオの音量を次第に小さくする
    //-----------------------------------------------------------------------------
    public void FadeOutAudio(float time)
    {
        fadeTime = time;
        fadeOutFlg = true;
    }

}
