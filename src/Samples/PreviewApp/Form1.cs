using System.ComponentModel;


namespace PreviewApp;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        button1.Click += Button1_Click;
    }

    private string? filePath;

    public void SetFilePath(string? filePath) => this.filePath = filePath;

    private void Button1_Click(object? sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            MessageBox.Show(filePath);
        }
    }

    protected override void OnClientSizeChanged(EventArgs e)
    {
        base.OnClientSizeChanged(e);

        button1.Location = new System.Drawing.Point((this.ClientSize.Width - button1.Width) / 2, (this.ClientSize.Height - button1.Height) / 2);
    }
}
