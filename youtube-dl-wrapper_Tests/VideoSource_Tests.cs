using System;
using Xunit;
using youtube_dl_gui_wrapper;

namespace youtube_dl_wrapper_Tests
{
    public class VideoSource_Tests
    {
        private readonly string fakeURL = @"https://www.youtube.com/watch?v=dQw4w9WgXc";
        private readonly string realURL = @"https://www.youtube.com/watch?v=dQw4w9WgXcQ";

        [Fact]
        public async void Formats_HasNoCountWhenIncorrectURL()
        {
            //arrange
            var source = new VideoSource(fakeURL);

            //act
            try
            {
                await source.GetVideoFormats();
            }
            catch (ArgumentException)
            {

            }
            var actual = source.Formats.Count > 0;

            //assert
            Assert.False(actual);
        }

        [Fact]
        public async void Formats_HasCountWhenCorrectURL()
        {
            //arrange
            var source = new VideoSource(realURL);

            //act
            try
            {
                await source.GetVideoFormats();
            }
            catch (ArgumentException)
            {

            }
            var actual = source.Formats.Count > 0;

            //assert
            Assert.True(actual);
        }

        [Fact]
        public async void Formats_HasErrorMessageWhenIncorrectURL()
        {
            //arrange
            var source = new VideoSource(fakeURL);
            var errorMsg = string.Empty;

            //act
            try
            {
                await source.GetVideoFormats();
            }
            catch (ArgumentException e)
            {
                errorMsg = e.Message;
            }

            //assert
            Assert.True(errorMsg.Length > 0);
        }

        [Fact]
        public async void Formats_HasNoErrorMessageWhenCorrectURL()
        {
            //arrange
            var source = new VideoSource(realURL);
            var errorMsg = string.Empty;

            //act
            try
            {
                await source.GetVideoFormats();
            }
            catch (ArgumentException e)
            {
                errorMsg = e.Message;
            }

            //assert
            Assert.True(errorMsg.Length == 0);
        }


    }
}
