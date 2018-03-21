﻿using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;

namespace TwitchClient.Pages
{
	public sealed partial class TwitchStream : Page
	{
		public TwitchStream()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
			this.InitializeComponent();
		}

		private List<M3U> m3uParsed = new List<M3U>();

		// channel, token, sig, random
		private string USHER_API = "http://usher.twitch.tv/api/channel/hls/{0}.m3u8?player=twitchweb&token={1}&sig={2}&$allow_audio_only=true&allow_source=true&type=any&p={3}";

		// channel, client_id
		private string TOKEN_API = "http://api.twitch.tv/api/channels/{0}/access_token?client_id={1}";

		// Is there stream data correctly loaded?
		private bool loaded = false;

		private struct JSONTokenApiResponse
		{
			public string token;
			public string sig;
			public bool mobile_restricted;
		}

		private async void AppBar_OpenFile(object sender, TappedRoutedEventArgs e)
		{
			#region File Media Player

			/*
            var picker = new FileOpenPicker();

            picker.ViewMode = PickerViewMode.Thumbnail;

            picker.SuggestedStartLocation = PickerLocationId.VideosLibrary;

            picker.FileTypeFilter.Add(".3gp");
            picker.FileTypeFilter.Add(".avi");
            picker.FileTypeFilter.Add(".fla");
            picker.FileTypeFilter.Add(".flv");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".mov");
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mpeg");
            picker.FileTypeFilter.Add(".mpeg2");
            picker.FileTypeFilter.Add(".mpg");
            picker.FileTypeFilter.Add(".rm");
            picker.FileTypeFilter.Add(".rmvb");
            picker.FileTypeFilter.Add(".vob");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".webm");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // Try and pause or stop the current media
                if (mediaPlayer.CanPause == true)
                {
                    try
                    {
                        mediaPlayer.Pause();
                    }
                    catch(Exception)
                    {
                        
                    }
                }
                else
                {
                    try
                    {
                        mediaPlayer.Stop();
                    }
                    catch(Exception)
                    {

                    }
                }

                IRandomAccessStream readStream = await file.OpenAsync(FileAccessMode.Read);

                try
                {
                    FFmpegMSS = FFmpegInteropMSS.CreateFFmpegInteropMSSFromStream(readStream, true, true);
                    MediaStreamSource mss = FFmpegMSS.GetMediaStreamSource();

                    if (mss != null)
                    {

                        #region Enable the hidden transport controls

                        mediaPlayer.AreTransportControlsEnabled = true;

                        mediaPlayer.TransportControls.IsFastForwardButtonVisible = true;
                        mediaPlayer.TransportControls.IsFastForwardEnabled = true;

                        mediaPlayer.TransportControls.IsFastRewindButtonVisible = true;
                        mediaPlayer.TransportControls.IsFastRewindEnabled = true;

                        mediaPlayer.TransportControls.IsNextTrackButtonVisible = true;
                        mediaPlayer.TransportControls.IsPreviousTrackButtonVisible = true;

                        mediaPlayer.TransportControls.IsPlaybackRateButtonVisible = true;
                        mediaPlayer.TransportControls.IsPlaybackRateEnabled = true;

                        mediaPlayer.TransportControls.IsSkipBackwardButtonVisible = true;
                        mediaPlayer.TransportControls.IsSkipBackwardEnabled = true;

                        mediaPlayer.TransportControls.IsSkipForwardButtonVisible = true;
                        mediaPlayer.TransportControls.IsSkipForwardEnabled = true;

                        mediaPlayer.TransportControls.IsStopButtonVisible = true;
                        mediaPlayer.TransportControls.IsStopEnabled = true;

                        mediaPlayer.TransportControls.IsRightTapEnabled = true;

                        #endregion

                        mediaPlayer.SetMediaStreamSource(mss);

                        mediaPlayer.Play();
                    }
                    else
                    {
                        var msg = new MessageDialog("An error has occurred while opening the file.");
                        await msg.ShowAsync();
                    }
                }
                catch(Exception)
                {

                }
            }
            */

			#endregion
		}

		private async void bGetStream_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{
			loaded = false;
			cbQualitySelect.Items.Clear();

			HTTP http = new HTTP();

			string username = tbTest.Text;

			string json = await http.Get(String.Format(TOKEN_API, username, "ejho3mdl9ugrndt9ngwjf1dp3ebgkn"));

			JSONTokenApiResponse obj = JsonConvert.DeserializeObject<JSONTokenApiResponse>(json);
			Debug.WriteLine("JSON:");
			Debug.WriteLine(obj);

			Debug.WriteLine(json);

			Debug.WriteLine("token: " + obj.token);
			Debug.WriteLine("sig: " + obj.sig);

			string m3u = await http.Get(String.Format(USHER_API, username, obj.token, obj.sig, 9999));

			Debug.WriteLine("M3U8:");
			Debug.WriteLine(m3u);

			string infoText;

			bool success;

			try
			{
				m3uParsed = M3UParser.Parse(m3u);
				infoText = "Success!\nSelect the stream quality below to continue";
				success = true;
			}
			catch (Exception exception)
			{
				infoText = exception.ToString();
				success = false;
			}

			tInfo.Text = infoText;

			if (success)
			{
				foreach (var stream in m3uParsed)
				{
					cbQualitySelect.Items.Add(stream.name);
				}

				// Load the twitch chat via embedding it
				Uri chatUri = new Uri(String.Format("https://www.twitch.tv/{0}/chat", username));
				webView.Source = chatUri;

				loaded = true;
			}

			Debug.WriteLine("Done");
		}

		private void cbQualitySelect_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (loaded)
			{
				ringVideoLoading.IsActive = true;

				// Set media player source to stream URL
				int selected = cbQualitySelect.SelectedIndex;
				Uri streamUri = new Uri(m3uParsed[selected].url);
				mediaPlayer.Source = streamUri;
				mediaPlayer.Play();

				#region Set transport controls

				mediaPlayer.AreTransportControlsEnabled = true;

				mediaPlayer.TransportControls.IsFastForwardButtonVisible = false;
				mediaPlayer.TransportControls.IsFastForwardEnabled = false;

				mediaPlayer.TransportControls.IsFastRewindButtonVisible = false;
				mediaPlayer.TransportControls.IsFastRewindEnabled = false;

				mediaPlayer.TransportControls.IsNextTrackButtonVisible = false;
				mediaPlayer.TransportControls.IsPreviousTrackButtonVisible = false;

				mediaPlayer.TransportControls.IsPlaybackRateButtonVisible = false;
				mediaPlayer.TransportControls.IsPlaybackRateEnabled = false;

				mediaPlayer.TransportControls.IsSkipBackwardButtonVisible = false;
				mediaPlayer.TransportControls.IsSkipBackwardEnabled = false;

				mediaPlayer.TransportControls.IsSkipForwardButtonVisible = false;
				mediaPlayer.TransportControls.IsSkipForwardEnabled = false;

				mediaPlayer.TransportControls.IsStopButtonVisible = false;
				mediaPlayer.TransportControls.IsStopEnabled = false;

				mediaPlayer.TransportControls.IsSeekEnabled = false;

				mediaPlayer.TransportControls.IsZoomButtonVisible = true;
				mediaPlayer.TransportControls.IsZoomEnabled = true;

				mediaPlayer.TransportControls.IsRightTapEnabled = true;


				#endregion
			}
		}


		private void mediaPlayer_MediaOpened(object sender, Windows.UI.Xaml.RoutedEventArgs e)
		{

		}

		private void mediaPlayer_Tapped(object sender, TappedRoutedEventArgs e)
		{

		}
	}
}