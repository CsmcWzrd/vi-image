using System;
using System.Windows.Forms;

namespace vi_image;

public sealed class ResizeDialog : Form
{
    private readonly NumericUpDown widthBox;
    private readonly NumericUpDown heightBox;
    private readonly CheckBox keepAspectRatioBox;

    private readonly decimal originalWidth;
    private readonly decimal originalHeight;
    private bool internalChange;

    public int NewWidth => (int)widthBox.Value;
    public int NewHeight => (int)heightBox.Value;

    public ResizeDialog(int currentWidth, int currentHeight)
    {
        Text = "Resize Image Resolution";
        Width = 340;
        Height = 220;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        originalWidth = currentWidth;
        originalHeight = currentHeight;

        var widthLabel = new Label
        {
            Text = "Width:",
            Left = 20,
            Top = 25,
            Width = 100
        };

        widthBox = new NumericUpDown
        {
            Left = 130,
            Top = 20,
            Width = 150,
            Minimum = 1,
            Maximum = 50000,
            Value = currentWidth
        };

        var heightLabel = new Label
        {
            Text = "Height:",
            Left = 20,
            Top = 65,
            Width = 100
        };

        heightBox = new NumericUpDown
        {
            Left = 130,
            Top = 60,
            Width = 150,
            Minimum = 1,
            Maximum = 50000,
            Value = currentHeight
        };

        keepAspectRatioBox = new CheckBox
        {
            Text = "Keep aspect ratio",
            Left = 130,
            Top = 100,
            Width = 160,
            Checked = true
        };

        var okButton = new Button
        {
            Text = "OK",
            Left = 115,
            Width = 80,
            Top = 135,
            DialogResult = DialogResult.OK
        };

        var cancelButton = new Button
        {
            Text = "Cancel",
            Left = 205,
            Width = 80,
            Top = 135,
            DialogResult = DialogResult.Cancel
        };

        Controls.AddRange(new Control[]
        {
            widthLabel,
            widthBox,
            heightLabel,
            heightBox,
            keepAspectRatioBox,
            okButton,
            cancelButton
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;

        widthBox.ValueChanged += WidthBox_ValueChanged;
        heightBox.ValueChanged += HeightBox_ValueChanged;
    }

    private void WidthBox_ValueChanged(object? sender, EventArgs e)
    {
        if (!keepAspectRatioBox.Checked || internalChange)
            return;

        internalChange = true;
        heightBox.Value = Math.Max(1, Math.Round(widthBox.Value * originalHeight / originalWidth));
        internalChange = false;
    }

    private void HeightBox_ValueChanged(object? sender, EventArgs e)
    {
        if (!keepAspectRatioBox.Checked || internalChange)
            return;

        internalChange = true;
        widthBox.Value = Math.Max(1, Math.Round(heightBox.Value * originalWidth / originalHeight));
        internalChange = false;
    }
}
