using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;

    void Start()
    {
        videoPlayer.source = VideoSource.Url;

#if UNITY_ANDROID && !UNITY_EDITOR
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "your_video.mp4");
#elif UNITY_WEBGL && !UNITY_EDITOR
            videoPlayer.url = System.IO.Path.Combine(Application.streamingAssetsPath, "your_video.mp4");
#else
        videoPlayer.url = "file://" + System.IO.Path.Combine(Application.streamingAssetsPath, "your_video.mp4");
#endif

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }
}
