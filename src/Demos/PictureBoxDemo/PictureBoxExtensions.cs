using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace LVGLSharp.Forms
{
    /// <summary>
    /// Extension methods for System.Windows.Forms.PictureBox to provide LVGL-like transformation features.
    /// </summary>
    public static class PictureBoxExtensions
    {
        private static readonly Dictionary<PictureBox, PictureBoxTransform> _transforms = new();

        private class PictureBoxTransform
        {
            public int Rotation { get; set; }
            public float Zoom { get; set; } = 1.0f;
            public bool AntiAlias { get; set; } = true;
            public Image? OriginalImage { get; set; }
        }

        /// <summary>
        /// Sets the rotation angle of the image (0-360 degrees).
        /// </summary>
        /// <param name="pictureBox">The PictureBox control.</param>
        /// <param name="angle">Rotation angle in degrees (0-360).</param>
        public static void SetRotation(this PictureBox pictureBox, int angle)
        {
            ArgumentNullException.ThrowIfNull(pictureBox);
            
            var transform = GetOrCreateTransform(pictureBox);
            transform.Rotation = angle % 360;
            if (transform.Rotation < 0) transform.Rotation += 360;
            
            ApplyTransforms(pictureBox, transform);
        }

        /// <summary>
        /// Sets the zoom/scale of the image.
        /// </summary>
        /// <param name="pictureBox">The PictureBox control.</param>
        /// <param name="zoom">Zoom factor (256 = 100%, 512 = 200%, 128 = 50%).</param>
        public static void SetZoom(this PictureBox pictureBox, uint zoom)
        {
            ArgumentNullException.ThrowIfNull(pictureBox);
            
            var transform = GetOrCreateTransform(pictureBox);
            // Convert LVGL zoom format (256 = 100%) to percentage
            transform.Zoom = zoom / 256.0f;
            
            ApplyTransforms(pictureBox, transform);
        }

        /// <summary>
        /// Enables or disables anti-aliasing for the image.
        /// </summary>
        /// <param name="pictureBox">The PictureBox control.</param>
        /// <param name="enabled">True to enable anti-aliasing, false to disable.</param>
        public static void SetAntiAlias(this PictureBox pictureBox, bool enabled)
        {
            ArgumentNullException.ThrowIfNull(pictureBox);
            
            var transform = GetOrCreateTransform(pictureBox);
            transform.AntiAlias = enabled;
            
            ApplyTransforms(pictureBox, transform);
        }

        private static PictureBoxTransform GetOrCreateTransform(PictureBox pictureBox)
        {
            if (!_transforms.TryGetValue(pictureBox, out var transform))
            {
                transform = new PictureBoxTransform();
                _transforms[pictureBox] = transform;
                
                // Store original image on first access
                if (pictureBox.Image != null && transform.OriginalImage == null)
                {
                    transform.OriginalImage = (Image)pictureBox.Image.Clone();
                }
                
                // Clean up when control is disposed
                pictureBox.Disposed += (s, e) =>
                {
                    if (_transforms.TryGetValue(pictureBox, out var t))
                    {
                        t.OriginalImage?.Dispose();
                        _transforms.Remove(pictureBox);
                    }
                };
            }
            return transform;
        }

        private static void ApplyTransforms(PictureBox pictureBox, PictureBoxTransform transform)
        {
            if (pictureBox.Image == null) return;

            // Store original image if not already stored
            if (transform.OriginalImage == null)
            {
                transform.OriginalImage = (Image)pictureBox.Image.Clone();
            }

            // If no transforms applied, use original image
            if (transform.Rotation == 0 && Math.Abs(transform.Zoom - 1.0f) < 0.001f)
            {
                if (pictureBox.Image != transform.OriginalImage)
                {
                    pictureBox.Image = transform.OriginalImage;
                }
                return;
            }

            // Create transformed image
            var transformedImage = CreateTransformedImage(
                transform.OriginalImage,
                transform.Rotation,
                transform.Zoom,
                transform.AntiAlias
            );

            // Update PictureBox
            var oldImage = pictureBox.Image;
            pictureBox.Image = transformedImage;
            
            // Clean up old transformed image (but not the original)
            if (oldImage != null && oldImage != transform.OriginalImage)
            {
                oldImage.Dispose();
            }
        }

        private static Image CreateTransformedImage(Image originalImage, int rotation, float zoom, bool antiAlias)
        {
            // Calculate new size based on rotation and zoom
            double angleRad = rotation * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(angleRad));
            double sin = Math.Abs(Math.Sin(angleRad));
            
            int newWidth = (int)Math.Ceiling((originalImage.Width * cos + originalImage.Height * sin) * zoom);
            int newHeight = (int)Math.Ceiling((originalImage.Width * sin + originalImage.Height * cos) * zoom);

            if (newWidth <= 0) newWidth = 1;
            if (newHeight <= 0) newHeight = 1;

            var bitmap = new Bitmap(newWidth, newHeight);
            
            using (var g = Graphics.FromImage(bitmap))
            {
                // Set quality based on anti-alias setting
                if (antiAlias)
                {
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                }
                else
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                }

                g.Clear(Color.Transparent);

                // Move to center
                g.TranslateTransform(newWidth / 2f, newHeight / 2f);
                
                // Apply rotation
                g.RotateTransform(rotation);
                
                // Apply zoom
                g.ScaleTransform(zoom, zoom);
                
                // Draw image centered
                g.DrawImage(
                    originalImage,
                    -originalImage.Width / 2f,
                    -originalImage.Height / 2f,
                    originalImage.Width,
                    originalImage.Height
                );
            }

            return bitmap;
        }

        /// <summary>
        /// Updates the stored original image when the Image property changes.
        /// Call this after setting a new image to the PictureBox.
        /// </summary>
        public static void UpdateOriginalImage(this PictureBox pictureBox)
        {
            ArgumentNullException.ThrowIfNull(pictureBox);
            
            if (_transforms.TryGetValue(pictureBox, out var transform))
            {
                transform.OriginalImage?.Dispose();
                transform.OriginalImage = pictureBox.Image != null ? (Image)pictureBox.Image.Clone() : null;
            }
        }
    }
}
