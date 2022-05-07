# youtube-dl-gui
C# WPF gui for youtube-dl / yt-dlp.

A personal learning project.  

Wraps basic download features of youtube-dl / yt-dlp.

https://user-images.githubusercontent.com/70348218/167243327-b8d9386b-6ef0-4472-9928-931f18c8cdbe.mp4


### Supports
* Youtube Videos
* Youtube Playlists ('best' format only, as each video may have differing resolutions)
* Twitch Vods
* Twitch Livestreams (sorta, but you can't cancel partway through like with via command line. Have to wait til the stream is finished.)
* More than likely able to handle a variety of other sites available via command line tool, but can't promise anything.

###### Requires
* [youtube-dl.exe](https://ytdl-org.github.io/youtube-dl/) / [yt-dlp.exe](https://github.com/yt-dlp/yt-dlp) (both or one, bundled together in the 'with exe' ver.)
* .net 5.0
* [ffmpeg](https://ffmpeg.org/download.html)

### Why should you use this?
You probably shouldn't. There are better youtube-dl GUI's out there that have much more features and development.  
This is okay if you only need to download youtube videos and twitch VODs, as those are the only websites I have tested for my personal use.  

Works okay with twitch livestreams, but you have to wait for the stream to end, if you cancel mid-way it will not save the file properly and will result in an unwatchable video. Unlike via CLI.  

Each website potentially has it's own quirks for what youtube-dl outputs which can makes it difficult to cover all edge cases without thorough planning / testing. I only really began development with youtube in mind, then twitch vods.

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
