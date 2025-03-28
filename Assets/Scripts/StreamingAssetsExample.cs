using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class StreamingAssetsExample : MonoBehaviour
{
    // VideoPlayerコンポーネント
    [SerializeField] private VideoPlayer _videoPlayer;

    // StreamingAssetsの動画ファイルへのパス
    [SerializeField] private string _streamingAssetsMoviePath;

    private void Start()
    {
        // URL指定
        _videoPlayer.source = VideoSource.Url;

        // StreamingAssetsフォルダ配下のパスの動画をURLとして指定する
        _videoPlayer.url = Path.Combine(Application.streamingAssetsPath, _streamingAssetsMoviePath);

        // 再生
        _videoPlayer.Play();
    }
}