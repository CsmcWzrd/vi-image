using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace vi_image;

public sealed class MainForm : Form
{
    private readonly PictureBox pictureBox;
    private readonly Panel imagePanel;
    private readonly StatusStrip statusStrip;
    private readonly ToolStripStatusLabel statusLabel;

    private Bitmap? currentImage;
    private string? currentFilePath;

    private bool isFullScreen;
    private FormBorderStyle previousBorderStyle;
    private FormWindowState previousWindowState;
    private Rectangle previousBounds;

    public MainForm()
    {
        Text = "vi-image";
        Width = 1200;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        KeyPreview = true;

        var menuStrip = BuildMenu();

        imagePanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = Color.Black
        };

        pictureBox = new PictureBox
        {
            SizeMode = PictureBoxSizeMode.Zoom,
            Dock = DockStyle.Fill,
            BackColor = Color.Black
        };

        imagePanel.Controls.Add(pictureBox);

        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel("No image loaded");
        statusStrip.Items.Add(statusLabel);

        Controls.Add(imagePanel);
        Controls.Add(statusStrip);
        Controls.Add(menuStrip);

        MainMenuStrip = menuStrip;
        KeyDown += MainForm_KeyDown;
    }

    private MenuStrip BuildMenu()
    {
        var menuStrip = new MenuStrip();

        var fileMenu = new ToolStripMenuItem("&File");
        var openItem = new ToolStripMenuItem("&Open Image", null, (_, _) => OpenImage())
        {
            ShortcutKeys = Keys.Control | Keys.O
        };
        var saveItem = new ToolStripMenuItem("&Save", null, (_, _) => SaveImage())
        {
            ShortcutKeys = Keys.Control | Keys.S
        };
        var saveAsItem = new ToolStripMenuItem("Save &As / Convert Type", null, (_, _) => SaveImageAs())
        {
            ShortcutKeys = Keys.Control | Keys.Shift | Keys.S
        };
        var exitItem = new ToolStripMenuItem("E&xit", null, (_, _) => Close());

        fileMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            openItem,
            saveItem,
            saveAsItem,
            new ToolStripSeparator(),
            exitItem
        });

        var viewMenu = new ToolStripMenuItem("&View");
        var fullScreenItem = new ToolStripMenuItem("&Full Screen", null, (_, _) => ToggleFullScreen())
        {
            ShortcutKeys = Keys.F11
        };
        var normalViewItem = new ToolStripMenuItem("&Fit to Window", null, (_, _) => SetPictureBoxMode(PictureBoxSizeMode.Zoom));
        var actualSizeItem = new ToolStripMenuItem("&Actual Size", null, (_, _) => SetPictureBoxMode(PictureBoxSizeMode.AutoSize));

        viewMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            fullScreenItem,
            normalViewItem,
            actualSizeItem
        });

        var imageMenu = new ToolStripMenuItem("&Image");

        var rotateRightItem = new ToolStripMenuItem("Rotate &Right 90 deg", null, (_, _) => RotateImage(RotateFlipType.Rotate90FlipNone));
        var rotateLeftItem = new ToolStripMenuItem("Rotate &Left 90 deg", null, (_, _) => RotateImage(RotateFlipType.Rotate270FlipNone));
        var rotate180Item = new ToolStripMenuItem("Rotate &180 deg", null, (_, _) => RotateImage(RotateFlipType.Rotate180FlipNone));
        var flipHorizontalItem = new ToolStripMenuItem("Flip &Horizontal", null, (_, _) => RotateImage(RotateFlipType.RotateNoneFlipX));
        var flipVerticalItem = new ToolStripMenuItem("Flip &Vertical", null, (_, _) => RotateImage(RotateFlipType.RotateNoneFlipY));
        var resizeItem = new ToolStripMenuItem("&Resize Resolution", null, (_, _) => ResizeImage());
        var dpiItem = new ToolStripMenuItem("Change &DPI", null, (_, _) => ChangeDpi());

        imageMenu.DropDownItems.AddRange(new ToolStripItem[]
        {
            rotateRightItem,
            rotateLeftItem,
            rotate180Item,
            new ToolStripSeparator(),
            flipHorizontalItem,
            flipVerticalItem,
            new ToolStripSeparator(),
            resizeItem,
            dpiItem
        });

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, viewMenu, imageMenu });
        return menuStrip;
    }

    private void OpenImage()
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Open Image",
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tif;*.tiff;*.ico;*.emf;*.wmf|JPEG Files|*.jpg;*.jpeg|PNG Files|*.png|Bitmap Files|*.bmp|GIF Files|*.gif|TIFF Files|*.tif;*.tiff|Icon Files|*.ico|All Files|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            using var temp = Image.FromFile(dialog.FileName);
            var loadedBitmap = new Bitmap(temp);
            SetCurrentImage(loadedBitmap);
            currentFilePath = dialog.FileName;
            UpdateStatus();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to open image.\n\n{ex.Message}", "Open Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveImage()
    {
        if (!EnsureImageLoaded())
            return;

        if (string.IsNullOrWhiteSpace(currentFilePath))
        {
            SaveImageAs();
            return;
        }

        try
        {
            var format = GetImageFormatFromExtension(Path.GetExtension(currentFilePath));
            SaveBitmapToFile(currentImage!, currentFilePath, format);
            UpdateStatus("Image saved.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to save image.\n\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveImageAs()
    {
        if (!EnsureImageLoaded())
            return;

        using var dialog = new SaveFileDialog
        {
            Title = "Save Image As",
            FileName = string.IsNullOrWhiteSpace(currentFilePath) ? "image.png" : Path.GetFileNameWithoutExtension(currentFilePath),
            Filter = "PNG Image|*.png|JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp|GIF Image|*.gif|TIFF Image|*.tif;*.tiff"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var format = GetImageFormatFromExtension(Path.GetExtension(dialog.FileName));
            SaveBitmapToFile(currentImage!, dialog.FileName, format);
            currentFilePath = dialog.FileName;
            UpdateStatus("Image saved.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to save image.\n\n{ex.Message}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RotateImage(RotateFlipType rotateFlipType)
    {
        if (!EnsureImageLoaded())
            return;

        currentImage!.RotateFlip(rotateFlipType);
        RefreshPictureBox();
        UpdateStatus("Image orientation changed.");
    }

    private void ResizeImage()
    {
        if (!EnsureImageLoaded())
            return;

        using var dialog = new ResizeDialog(currentImage!.Width, currentImage.Height);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            var resized = ResizeBitmap(currentImage, dialog.NewWidth, dialog.NewHeight);
            SetCurrentImage(resized);
            UpdateStatus("Image resolution changed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to resize image.\n\n{ex.Message}", "Resize Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ChangeDpi()
    {
        if (!EnsureImageLoaded())
            return;

        using var dialog = new DpiDialog(currentImage!.HorizontalResolution, currentImage.VerticalResolution);
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            currentImage.SetResolution(dialog.HorizontalDpi, dialog.VerticalDpi);
            RefreshPictureBox();
            UpdateStatus("Image DPI changed.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Unable to change DPI.\n\n{ex.Message}", "DPI Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Bitmap ResizeBitmap(Bitmap source, int width, int height)
    {
        var destination = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

        using var graphics = Graphics.FromImage(destination);
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.DrawImage(source, 0, 0, width, height);

        return destination;
    }

    private void SetPictureBoxMode(PictureBoxSizeMode mode)
    {
        pictureBox.Dock = mode == PictureBoxSizeMode.AutoSize ? DockStyle.None : DockStyle.Fill;
        pictureBox.SizeMode = mode;

        if (currentImage is not null && mode == PictureBoxSizeMode.AutoSize)
            pictureBox.Size = currentImage.Size;
    }

    private void ToggleFullScreen()
    {
        if (!isFullScreen)
        {
            previousBorderStyle = FormBorderStyle;
            previousWindowState = WindowState;
            previousBounds = Bounds;

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Normal;
            Bounds = Screen.FromControl(this).Bounds;

            MainMenuStrip!.Visible = false;
            statusStrip.Visible = false;
            isFullScreen = true;
        }
        else
        {
            FormBorderStyle = previousBorderStyle;
            WindowState = previousWindowState;
            Bounds = previousBounds;

            MainMenuStrip!.Visible = true;
            statusStrip.Visible = true;
            isFullScreen = false;
        }
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Escape && isFullScreen)
        {
            ToggleFullScreen();
            e.Handled = true;
        }
    }

    private void SetCurrentImage(Bitmap bitmap)
    {
        var oldImage = currentImage;
        currentImage = bitmap;
        pictureBox.Image = currentImage;

        if (pictureBox.SizeMode == PictureBoxSizeMode.AutoSize)
            pictureBox.Size = currentImage.Size;

        oldImage?.Dispose();
    }

    private void RefreshPictureBox()
    {
        pictureBox.Image = null;
        pictureBox.Image = currentImage;

        if (pictureBox.SizeMode == PictureBoxSizeMode.AutoSize && currentImage is not null)
            pictureBox.Size = currentImage.Size;
    }

    private bool EnsureImageLoaded()
    {
        if (currentImage is not null)
            return true;

        MessageBox.Show(this, "Please open an image first.", "No Image Loaded", MessageBoxButtons.OK, MessageBoxIcon.Information);
        return false;
    }

    private void UpdateStatus(string? message = null)
    {
        if (currentImage is null)
        {
            statusLabel.Text = "No image loaded";
            return;
        }

        var fileName = string.IsNullOrWhiteSpace(currentFilePath) ? "Unsaved image" : Path.GetFileName(currentFilePath);
        statusLabel.Text = $"{message ?? fileName} | {currentImage.Width} x {currentImage.Height}px | DPI {currentImage.HorizontalResolution:0} x {currentImage.VerticalResolution:0}";
    }

    private static ImageFormat GetImageFormatFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png" => ImageFormat.Png,
            ".bmp" => ImageFormat.Bmp,
            ".gif" => ImageFormat.Gif,
            ".tif" or ".tiff" => ImageFormat.Tiff,
            _ => ImageFormat.Png
        };
    }

    private static void SaveBitmapToFile(Bitmap bitmap, string path, ImageFormat format)
    {
        var tempPath = path + ".tmp";

        if (format.Equals(ImageFormat.Jpeg))
            SaveJpeg(bitmap, tempPath, 95L);
        else
            bitmap.Save(tempPath, format);

        if (File.Exists(path))
            File.Delete(path);

        File.Move(tempPath, path);
    }

    private static void SaveJpeg(Bitmap bitmap, string path, long quality)
    {
        var jpegCodec = ImageCodecInfo.GetImageEncoders().First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);

        using var encoderParameters = new EncoderParameters(1);
        encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);

        using var rgbBitmap = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb);
        rgbBitmap.SetResolution(bitmap.HorizontalResolution, bitmap.VerticalResolution);

        using (var graphics = Graphics.FromImage(rgbBitmap))
        {
            graphics.Clear(Color.White);
            graphics.DrawImage(bitmap, 0, 0, bitmap.Width, bitmap.Height);
        }

        rgbBitmap.Save(path, jpegCodec, encoderParameters);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        currentImage?.Dispose();
        base.OnFormClosed(e);
    }
}
