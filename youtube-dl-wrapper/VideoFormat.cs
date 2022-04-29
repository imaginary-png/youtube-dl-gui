﻿using System.Runtime.CompilerServices;

namespace youtube_dl_gui_wrapper
{
    public class VideoFormat
    {
        public string FormatCode { get; set; }
        public string Extension { get; set; }
        public string Resolution { get; set; }
        public string ResolutionLabel { get; set; }
        public string Height { get; set; } //should these be ints? does it matter? They're never really used as numbers...
        public string Width { get; set; }
        public string Fps { get; set; }
        public string FileSize { get; set; }


        public VideoFormat(string formatCode, string extension, string resolution, string resolutionLabel, string height, string width, string fps)
        {
            FormatCode = formatCode;
            Extension = extension;
            Resolution = resolution;
            ResolutionLabel = resolutionLabel;
            Height = height;
            Width = width;
            Fps = fps;
        }

        public VideoFormat(string formatCode, string extension, string resolution, string resolutionLabel, string height, string width, string fps, string fileSize)
        {
            FormatCode = formatCode;
            Extension = extension;
            Resolution = resolution;
            ResolutionLabel = resolutionLabel;
            Height = height;
            Width = width;
            Fps = fps;
            FileSize = fileSize;
        }

        public override string ToString()
        {
            return $"Format Code: {FormatCode}\n" +
                   $"Extension: {Extension}\n" +
                   $"Resolution: {Resolution}\n" +
                   $"ResolutionLabel: {ResolutionLabel}\n" +
                   $"Height: {Height}\n"+
                   $"Width: {Width}\n"+
                   $"Fps: {Fps}\n" +
                   $"Size: {FileSize}\n";
        }
    }
}