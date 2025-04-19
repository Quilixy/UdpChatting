using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpChatting;

public partial class MainPage : ContentPage
{
    private UdpClient udpClient;
    private int localPort = 11000; // Dinleme yapılacak port

    public MainPage()
    {
        InitializeComponent();
        StartListening();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        try
        {
            string ipAddress = ipEntry.Text;
            string message = messageEntry.Text;

            if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(message))
                return;

            var remoteEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), localPort);
            byte[] sendBytes = Encoding.UTF8.GetBytes(message);

            using (UdpClient senderClient = new UdpClient())
            {
                await senderClient.SendAsync(sendBytes, sendBytes.Length, remoteEndpoint);
            }

            messagesLabel.Text += $"[Gönderildi -> {ipAddress}] {message}\n";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    private async void StartListening()
    {
        try
        {
            udpClient = new UdpClient(localPort);

            while (true)
            {
                var result = await udpClient.ReceiveAsync();
                string receivedMessage = Encoding.UTF8.GetString(result.Buffer);
                string senderIp = result.RemoteEndPoint.Address.ToString();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    messagesLabel.Text += $"[Geldi <- {senderIp}] {receivedMessage}\n";
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Hata", ex.Message, "Tamam");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        udpClient?.Close();
    }
}