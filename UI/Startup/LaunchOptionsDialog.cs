using DragonGlareAlpha.Domain.Startup;

namespace DragonGlareAlpha;

internal sealed class LaunchOptionsDialog : Form
{
    private readonly RadioButton window640Radio;
    private readonly RadioButton window720Radio;
    private readonly RadioButton window1080Radio;
    private readonly RadioButton fullscreenRadio;
    private readonly CheckBox promptOnStartupCheckBox;

    public LaunchSettings SelectedSettings { get; private set; }

    public LaunchOptionsDialog(LaunchSettings initialSettings)
    {
        SelectedSettings = initialSettings;

        Text = "DragonGlare Alpha";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterScreen;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;
        ClientSize = new Size(260, 184);
        Text = CreateCenteredCaption("DragonGlare Alpha");

        const string titleText = "ウィンドウモードを選択してください";
        var optionX = Math.Max(0, (ClientSize.Width - TextRenderer.MeasureText(
            titleText,
            Font,
            ClientSize,
            TextFormatFlags.NoPadding).Width) / 2);

        var titleLabel = new Label
        {
            AutoSize = false,
            Location = new Point(0, 8),
            Size = new Size(ClientSize.Width, 20),
            Text = titleText,
            TextAlign = ContentAlignment.MiddleCenter
        };

        fullscreenRadio = new RadioButton
        {
            AutoSize = true,
            Location = new Point(optionX, 34),
            Text = "フルスクリーン"
        };

        window640Radio = new RadioButton
        {
            AutoSize = true,
            Location = new Point(optionX, 57),
            Text = "ウィンドウ(640x480)"
        };

        window720Radio = new RadioButton
        {
            AutoSize = true,
            Location = new Point(optionX, 80),
            Text = "ウィンドウ(720p)"
        };

        window1080Radio = new RadioButton
        {
            AutoSize = true,
            Location = new Point(optionX, 103),
            Text = "ウィンドウ(1080p)"
        };

        promptOnStartupCheckBox = new CheckBox
        {
            AutoSize = true,
            Text = "起動時に毎回聞く",
            Checked = initialSettings.PromptOnStartup
        };
        promptOnStartupCheckBox.Location = new Point(
            Math.Max(0, (ClientSize.Width - promptOnStartupCheckBox.PreferredSize.Width) / 2),
            128);

        var startButton = new Button
        {
            AutoSize = true,
            Size = new Size(92, 28),
            Text = "ゲーム起動",
            UseVisualStyleBackColor = true
        };
        startButton.Location = new Point((ClientSize.Width - startButton.Width) / 2, 152);
        startButton.Click += (_, _) => ConfirmSelection();

        AcceptButton = startButton;

        Controls.Add(titleLabel);
        Controls.Add(fullscreenRadio);
        Controls.Add(window640Radio);
        Controls.Add(window720Radio);
        Controls.Add(window1080Radio);
        Controls.Add(promptOnStartupCheckBox);
        Controls.Add(startButton);

        SetSelectedDisplayMode(initialSettings.DisplayMode);
    }

    private string CreateCenteredCaption(string caption)
    {
        var captionWidth = TextRenderer.MeasureText(caption, Font, ClientSize, TextFormatFlags.NoPadding).Width;
        var spaceWidth = Math.Max(1, TextRenderer.MeasureText(" ", Font, ClientSize, TextFormatFlags.NoPadding).Width);
        var leadingSpaceCount = Math.Max(0, ((ClientSize.Width - captionWidth) / 2) / spaceWidth);
        return new string(' ', leadingSpaceCount) + caption;
    }

    private void SetSelectedDisplayMode(LaunchDisplayMode mode)
    {
        fullscreenRadio.Checked = mode == LaunchDisplayMode.Fullscreen;
        window640Radio.Checked = mode == LaunchDisplayMode.Window640x480;
        window720Radio.Checked = mode == LaunchDisplayMode.Window720p;
        window1080Radio.Checked = mode == LaunchDisplayMode.Window1080p;
    }

    private LaunchDisplayMode GetSelectedDisplayMode()
    {
        if (fullscreenRadio.Checked)
        {
            return LaunchDisplayMode.Fullscreen;
        }

        if (window720Radio.Checked)
        {
            return LaunchDisplayMode.Window720p;
        }

        if (window1080Radio.Checked)
        {
            return LaunchDisplayMode.Window1080p;
        }

        return LaunchDisplayMode.Window640x480;
    }

    private void ConfirmSelection()
    {
        SelectedSettings = new LaunchSettings
        {
            DisplayMode = GetSelectedDisplayMode(),
            PromptOnStartup = promptOnStartupCheckBox.Checked
        };

        DialogResult = DialogResult.OK;
        Close();
    }
}
