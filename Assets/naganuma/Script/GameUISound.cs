using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//-----------------------------------------------------------------------------
//! [制作者]     長沼豪琉
//! [最終更新日] 2021/10/27
//! [内容]       UIサウンド再生
//-----------------------------------------------------------------------------
public class GameUISound : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    //! ENUM
    //-----------------------------------------------------------------------------
    public enum SETYPE {  // SE
        COUNTDOWN
       ,TIMEUP
       ,BUTTON
    }

    public enum BGMTYPE { // BGM
        DOTABATA_FAST
       ,DOTABATA_SLOW
    }

    //-----------------------------------------------------------------------------
    //! private変数
    //-----------------------------------------------------------------------------
    private AudioSource audioSource;
    private float       currentTime;

    //-----------------------------------------------------------------------------
    //! public変数
    //-----------------------------------------------------------------------------

    //-----------------------------------------------------------------------------
    //! Inspectorに公開する変数
    //-----------------------------------------------------------------------------
    [Header("カウントダウン")] public AudioClip countDownSE;
    [Header("タイムアップ")]   public AudioClip timeUpSE;
    [Header("ボタン")]         public AudioClip buttonSE;
    [Header("BGM")]            public AudioClip dotabataFastBGM;
    [Header("BGM")]            public AudioClip dotabataSlowBGM;

    //-----------------------------------------------------------------------------
    //! [内容]    開始処理
    //-----------------------------------------------------------------------------
    void Start()
    {
        // コンポーネント取得
        if (!this.TryGetComponent<AudioSource>(out audioSource)) {
            Debug.LogError("AudioSourceがありません。");
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    SE再生
    //-----------------------------------------------------------------------------
    public void PlaySE(SETYPE seType) {
        switch (seType) {
            case SETYPE.COUNTDOWN:
                audioSource.PlayOneShot(countDownSE);
                break;
            case SETYPE.TIMEUP:
                audioSource.PlayOneShot(timeUpSE);
                break;
            case SETYPE.BUTTON:
                audioSource.PlayOneShot(buttonSE);
                break;
            default:
                break;
        }
    }

    //-----------------------------------------------------------------------------
    //! [内容]    BGM再生
    //-----------------------------------------------------------------------------
    public void PlayBGM(BGMTYPE bgmType) {
        switch (bgmType) {
            case BGMTYPE.DOTABATA_FAST:
                audioSource.clip = dotabataFastBGM;
                break;
            case BGMTYPE.DOTABATA_SLOW:
                audioSource.clip = dotabataSlowBGM;
                break;
            default:
                break;
        }
        audioSource.Play();
    }

    //-----------------------------------------------------------------------------
    //! [内容]    BGMフェードアウト
    //! [戻り値]  フェードが完了するとTrueを返します。
    //-----------------------------------------------------------------------------
    public bool StopFade(float fadeTime) {
        currentTime += Time.deltaTime;
        if (fadeTime <= 0.0f) {
            audioSource.Stop();
            return true;
        }
        var currentVolume = 1.0f - currentTime / fadeTime;
        audioSource.volume = currentVolume <= 0.0f ? 0.0f : currentVolume;
        if (audioSource.volume <= 0.0f) {
            audioSource.Stop();
            return true;
        }
        return false;
    }
}
