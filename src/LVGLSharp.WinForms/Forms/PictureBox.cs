using LVGLSharp.Interop;

namespace LVGLSharp.Forms
{
    /// <summary>Displays an image using the LVGL image widget.</summary>
    public class PictureBox : Control
    {
        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Normal;
        /// <remarks>Image loading from a file path is not yet implemented.</remarks>
        public string? ImageLocation { get; set; }

        internal override unsafe void CreateLvglObject(nint parentHandle)
        {
            _lvglObjectHandle = (nint)lv_image_create((lv_obj_t*)parentHandle);
            ApplyLvglProperties();
            CreateChildrenLvglObjects();
        }
    }
}
