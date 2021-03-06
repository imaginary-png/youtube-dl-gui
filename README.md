## youtube-dl / yt-dlp gui 
C# WPF gui for youtube-dl / yt-dlp. Wraps basic download and video data gathering features of youtube-dl / yt-dlp.  

https://user-images.githubusercontent.com/70348218/167243327-b8d9386b-6ef0-4472-9928-931f18c8cdbe.mp4   

Urls must be separated by spaces (which is applied automatically at the end when you paste)

### Supports
* Youtube Videos ✔
* Youtube Playlists ✔ ('best' only, as there may be differing resolutions. --equiv. to "(youtube-dl|yt-dlp) URL"
* Twitch Vods ✔
* Twtich Clips ✔
* Twitch Livestreams ✔(works, but you can't cancel partway like with via command line. Have to wait til the stream is finished.)
* More than likely able to handle a variety of other sites available via the command line tool, but I have not personally tested.

###### Requires
* [youtube-dl.exe](https://ytdl-org.github.io/youtube-dl/) / [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp) (both or one, bundled together in the 'with exe' ver.)
* .net 5.0
* [ffmpeg](https://ffmpeg.org/download.html)

### Why should you use this?
No particular reason. There are better youtube-dl GUI's out there that have more features and development.  

This gets the job done if you want a graphical interface to download videos - particularly Youtube and Twitch, as these are websites I've personally tested.   
Other sites will more than likely work, but I only really began development with youtube in mind, then twitch vods.  

Works with twitch livestreams, but you have to wait for the stream to end, if you cancel mid-way it will not save the file properly and will result in an unwatchable video. Unlike via CLI.  

## info 
Download Page:  
<img src="https://i.imgur.com/XpvYsyL.png" width="800" height="500"/>   

Settings Page:  
<img src="https://i.imgur.com/UtBhpaG.png" width="800" height="500"/>      

* **Use Youtube DL** -- Disabled by default. Currently uses yt-dlp as youtube-dl has issues with youtube downloads. Toggle this on if you want to use youtube-dl for some reason, but it shouldn't make a difference for the GUI supported websites.  
* **Bulk Download** -- Enabled by default. Uncheck to download videos one-by-one, if internet slow or something.
* **Download Folder** -- Set the location to download to. 

Settings are saved in config.json. You can manually edit this file or delete it to generate default settings, if something goes wrong.

Executables are stored in the Exe folder, if you don't have youtube-dl / yt-dlp available via an env variable / PATH.

### ToDo
* idk, fix any unfound bugs
* maybe test other websites
* add an icon


#### Icon Image attributions
<a href="https://www.flaticon.com/free-icons/download" title="download icons">Download icons created by Ayub Irawan - Flaticon</a>  
<a href="https://www.flaticon.com/free-icons/folder" title="folder icons">Folder icons created by Freepik - Flaticon</a>  
<a href="https://www.flaticon.com/free-icons/settings" title="settings icons">Settings icons created by Freepik - Flaticon</a>  

<a target="_blank" href="https://icons8.com/icon/8112/close">Close</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>  
<a target="_blank" href="https://icons8.com/icon/11152/subtract">Subtract</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>  
<a target="_blank" href="https://icons8.com/icon/vU8WkCSNnXng/maximize-button">Maximize Button</a> icon by <a target="_blank" href="https://icons8.com">Icons8</a>  

---------  
 
[youtube-dl](https://github.com/ytdl-org/youtube-dl)  
[yt-dlp](https://github.com/yt-dlp/yt-dlp/tree/master/.github)


A personal learning project.
