namespace SynchronizationContextLearn;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
    }

    private async void button1_Click(object sender, EventArgs e)
    {
        var result = await LongRunningTaskAsync();
        button1.Text = result;
    }

    private async void button2_Click(object sender, EventArgs e)
    {
        var result = await LongRunningTaskAsync().ConfigureAwait(continueOnCapturedContext: false);
        button2.Text = result;
    }

    private async void button3_Click(object sender, EventArgs e)
    {
        var result = await LongRunningTaskAsync().ConfigureAwait(continueOnCapturedContext: false);
        button3.BeginInvoke(() => button3.Text = result);
    }

    private async Task<string> LongRunningTaskAsync()
    {
        await Task.Delay(50);
        return "Task completed";
    }
}
