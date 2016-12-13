/* Copyright (c) 2009-11, ReactionGrid Inc. http://reactiongrid.com
 * See License.txt for full licence information. 
 * 
 * WebTexture.cs Revision 1.0.1103.01
 * Slide viewer script, needs to be paired with the GetData component for a constant updating 
 * change of images, alternatively you could just use this script and overwrite the same image 
 * over and over on the server to show changing images using the timer in this script*/

using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class WebTexture : MonoBehaviour
{
    public string serverUrl = "http://jibemix.com/uploads/"; // folder containing images, video or audio sources
    public string mediaUrl = ""; // name of image, video, or audio source

    WWW webRequest;

    public bool runOnTimer = true;
    public float timerInterval = 5;
    private float timerTicks = 0;
    public bool forceUpdate = false; // set to true to force a reload of an image each time a timer tick elapses if set to run on timer

    public Material ScreenOff;
    public Material ScreenOn;

    public string currentMedia = "";
    private bool doUpdateScreen = false;
    private string mediaPath = "";

    public enum MediaType { Image, SilentMovie, Movie, Audio };
    private MediaType currentMediaType = MediaType.Image;
    public bool attemptMovieWithSound = false; // sound not yet available using this script (to be corrected in future revisions

    private bool isBusy = false;
    public GUISkin skin;
    public Texture2D screenTexture;
    private float downloadProgress = 0.0f;
    private Texture2D currentScreenImage;
    public float progressBarWidth = 200.0f;

    void Start()
    {
        UpdateScreen();
    }

    private void UpdateScreen()
    {
        mediaPath = gameObject.GetComponent<GetData>().currentData;
        if (!string.IsNullOrEmpty(mediaPath))
        {
            mediaUrl = serverUrl + mediaPath;
            if (mediaUrl != currentMedia || forceUpdate)
            {
                if (!isBusy)
                {
                    if (mediaUrl.ToLower().EndsWith(".png") || mediaUrl.ToLower().EndsWith(".jpg"))
                    {
                        isBusy = true;
                        Debug.Log("Getting new image from " + mediaUrl);
                        currentMediaType = MediaType.Image;
                        StartCoroutine(LoadWeb(mediaUrl));
                    }
                    else if (mediaUrl.ToLower().EndsWith(".ogg"))
                    {
                        isBusy = true;
                        Debug.Log("Getting new audio source from " + mediaUrl);
                        currentMediaType = MediaType.Audio;
                        StartCoroutine(LoadWeb(mediaUrl));
                    }
                    else if (mediaUrl.ToLower().EndsWith(".ogv"))
                    {
                        isBusy = true;
                        Debug.Log("Getting new movie source from " + mediaUrl);
                        if (attemptMovieWithSound)
                        {
                            currentMediaType = MediaType.Movie;
                        }
                        else
                            currentMediaType = MediaType.SilentMovie;
                        StartCoroutine(LoadWeb(mediaUrl));
                    }
                    else
                    {
                        Debug.Log("Invalid media type");
                    }
                }
            }
        }
    }
    void OnMouseDown()
    {
        Debug.Log("Screen clicked");
        UpdateScreen();
    }

    public void ForceUpdateScreen()
    {
        doUpdateScreen = true;
    }

    void OnGUI()
    {
        GUI.skin = skin;
        if (downloadProgress > 0)
        {
            // show progress bar
            float fullWidth = progressBarWidth;
            float barWidth = fullWidth * (downloadProgress / 100);

            GUI.Label(new Rect(0, 0, fullWidth, 20), "Loading " + currentMediaType.ToString() + "... " + downloadProgress.ToString("f0") + "%", "ProgressBarContent");
            GUI.Label(new Rect(0, 0, barWidth, 20), "", "GUIBar");
        }
    }

    void Update()
    {
        if (doUpdateScreen)
        {
            doUpdateScreen = false;
            UpdateScreen();
        }
        if (runOnTimer)
        {
            // increment timer
            timerTicks += Time.deltaTime;
            if (timerTicks > timerInterval)
            {
                timerTicks = 0;
                UpdateScreen();
            }
        }
    }
    private IEnumerator DisplayProgress(float progress)
    {
        // used by progress bar
        downloadProgress = progress;
        yield return (downloadProgress);
    }

    private IEnumerator LoadWeb(string requestUrl)
    {
        // async web request for new data
        Debug.Log("Initiating web request...");
        webRequest = new WWW(requestUrl);
        while (!webRequest.isDone)
        {
            // update progress bar
            downloadProgress = webRequest.progress * 100;
            yield return (downloadProgress);
        }
        yield return (webRequest);

        downloadProgress = 0.0f;
        if (webRequest.error != null)
        {
            Debug.Log(webRequest.error);
        }
        else
        {
            if (webRequest.size > 0)
            {
                Debug.Log("Processing data");
                switch (currentMediaType)
                {
                    case MediaType.Image:
                        Debug.Log("Loading image");
                        currentScreenImage = webRequest.texture;
                        if (GetComponent<Renderer>().material != ScreenOn)
                            GetComponent<Renderer>().material = ScreenOn;
                        if (currentScreenImage != null && currentScreenImage.width == 8 && currentScreenImage.height == 8)
                        {
                            // assume red question mark of doom caused by invalid / missing image! dirty hack, but there's no way to get an HTTP response code
                            Debug.Log("Invalid image!");
                            GetComponent<Renderer>().material.mainTexture = screenTexture;
                        }
                        else
                        {
                            GetComponent<Renderer>().material.mainTexture = webRequest.texture;
                        }                                               
                        break;
                    case MediaType.SilentMovie:
                        Debug.Log("Loading silent movie");
					//if ( webRequest.movie.isReadyToPlay)  
                      //  {
                        /*   
						Debug.Log("movie ready");
						#if UNITY_WEBGL
							// use WebGLMovieTexture here...
							WebGLMovieTexture movieTex = webRequest.movie;
						#else
							// use MovieTexture on other platforms
							UnityEngine.MovieTexture movieTex = webRequest.movie;
						#endif
                            if (movieTex != null)
                            {
                                GetComponent<Renderer>().material.mainTexture = movieTex;
                                movieTex.Play();
                            }
                            */
                        //}
                        break;
                    case MediaType.Audio:
					
                        Debug.Log("Loading audio");
					if (webRequest.audioClip.loadState == AudioDataLoadState.Loaded)
                        {
                            GetComponent<AudioSource>().clip = webRequest.audioClip;
                            GetComponent<AudioSource>().Play();
                        }
                        break;
                    case MediaType.Movie:
                        Debug.Log("Loading movie");
					//if (webRequest.movie.isReadyToPlay && (webRequest.audioClip.loadState == AudioDataLoadState.Loaded ))
                      //  {
						/*
						#if UNITY_WEBGL
						// use WebGLMovieTexture here...
						WebGLMovieTexture movieTex = webRequest.movie;
						#else
						// use MovieTexture on other platforms
						UnityEngine.MovieTexture movieTex = webRequest.movie;
						#endif
							GetComponent<Renderer>().material.mainTexture = movieTex;
                            GetComponent<AudioSource>().clip = webRequest.audioClip;
                            movieTex.Play();
                            GetComponent<AudioSource>().Play();
						*/
                        //}
                        break;
                    default:
                        break;
                }
            }
        }
        currentMedia = mediaUrl;
        isBusy = false;
    }
}
