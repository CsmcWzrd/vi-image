using System;
using System.Windows.Forms;

namespace vi_image;

public sealed class DpiDialog : Form
{
    private readonly NumericUpDown horizontalDpiBox;
    private readonly NumericUpDown verticalDpiBox;
    private readonly CheckBox sameDpiBox;

    private bool internalChange;

    public float HorizontalDpi => (float)horizontalDpiBox.Value;
    public float VerticalDpi => (float)verticalDpiBox.Value;

    public DpiDialog(float currentHorizontalDpi, float currentVerticalDpi)
    {
        Text = "Change Image DPI";
        Width = 340;
        Height = 220;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var horizontalLabel = new Label
        {
            Text = "Horizontal DPI:",
            Left = 20,
            Top = 25,
            Width = 110
        };

        horizontalDpiBox = new NumericUpDown
        {
            Left = 140,
            Top = 20,
            Width = 140,
            Minimum = 1,
            Maximum = 2400,
            DecimalPlaces = 0,
            Value = Math.Clamp((decimal)currentHorizontalDpi, 1, 2400)
        };

        var verticalLabel = new Label
        {
            Text = "Vertical DPI:",
            Left = 20,
            Top = 65,
            Width = 110
        };

        verticalDpiBox = new NumericUpDown
        {
            Left = 140,
            Top = 60,
            Width = 140,
            Minimum = 1,
            Maximum = 2400,
            DecimalPlaces = 0,
            Value = Math.Clamp((decimal)currentVerticalDpi, 1, 2400)
        };

        sameDpiBox = new CheckBox
        {
            Text = "Use same DPI for both",
            Left = 140,
            Top = 100,
            Width = 170,
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
            horizontalLabel,
            horizontalDpiBox,
            verticalLabel,
            verticalDpiBox,
            sameDpiBox,
            okButton,
            cancelButton
        });

        AcceptButton = okButton;
        CancelButton = cancelButton;

        horizontalDpiBox.ValueChanged += HorizontalDpiBox_ValueChanged;
        verticalDpiBox.ValueChanged += VerticalDpiBox_ValueChanged;
    }

    private void HorizontalDpiBox_ValueChanged(object? sender, EventArgs e)
    {
        if (!sameDpiBox.Checked || internalChange)
            return;

        internalChange = true;
        verticalDpiBox.Value = horizontalDpiBox.Value;
        internalChange = false;
    }

    private void VerticalDpiBox_ValueChanged(object? sender, EventArgs e)
    {
        if (!sameDpiBox.Checked || internalChange)
            return;

        internalChange = true;
        horizontalDpiBox.Value = verticalDpiBox.Value;
        internalChange = false;
    }
}
