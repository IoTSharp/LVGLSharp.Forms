namespace LVGLSharp.Drawing
{
    /// <summary>
    /// Represents an image source used by LVGL controls.
    /// </summary>
    public interface IImageSource : IDisposable
    {
        /// <summary>
        /// Gets the pixel width of the image.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the pixel height of the image.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Converts the image into LVGL ARGB8888 byte data.
        /// </summary>
        /// <returns>The converted pixel buffer.</returns>
        byte[] ToLvglArgb8888Bytes();
    }
}