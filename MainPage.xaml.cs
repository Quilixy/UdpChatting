﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UdpChatting;

public partial class MainPage : ContentPage
{
    private UdpClient udpClient;
    private int localPort = 11000;
    public string LocalIp { get; set; }
    private PlayfairCipher playfair;

    public MainPage()
    {
        InitializeComponent();
        

        LocalIp = WiFiService.GetLocalIpAddress();
        BindingContext = this;
        playfair = new PlayfairCipher("R.I.P.ALBERT(CAT)");
        playfair.PrintMatrix();
        StartListening();
    }

    private async void OnSendClicked(object sender, EventArgs e)
    {
        try
        {
            string ipAddress = ipEntry.Text;
            string username = usernameEntry.Text;
            string message = messageEntry.Text;

            string encryptedMessage = playfair.Encrypt(message);
            string fullMessage = $"{username}:{encryptedMessage}";

            if (string.IsNullOrWhiteSpace(ipAddress) || string.IsNullOrWhiteSpace(fullMessage))
                return;

            var remoteEndpoint = new IPEndPoint(IPAddress.Parse(ipAddress), localPort);
            byte[] sendBytes = Encoding.UTF8.GetBytes(fullMessage);

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
                string receivedFullMessage = Encoding.UTF8.GetString(result.Buffer);
                string[] splittedMessage = receivedFullMessage.Split(':', 2);
                string senderUserName = splittedMessage.Length > 1 ? splittedMessage[0] : "Bilinmiyor";
                string receivedCryptedMessage = splittedMessage.Length > 1 ? splittedMessage[1] : "Bilinmiyor";
                string receivedMessage = playfair.Decrypt(receivedCryptedMessage);
                string senderIp = result.RemoteEndPoint.Address.ToString();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    messagesLabel.Text += $"[Geldi <- {senderUserName}:({senderIp})] {receivedMessage}\n";
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
