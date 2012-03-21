using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace BeijingAirQuality.WindowsPhone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("No Internet connection.");
                return;
            }

            DownloadBeijingAirQualityData();
            Browser.Navigate(new Uri("http://iphone.bjair.info"));
        }

        void DownloadBeijingAirQualityData()
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            webClient.OpenReadAsync(new Uri("http://iphone.bjair.info/m/beijing/mobile"));
        }

        void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            byte[] response = new byte[e.Result.Length];
            if (e.Result.Read(response, 0, response.Length) == 0) // empty response :(
                return;

            string html = System.Text.Encoding.UTF8.GetString(response, 0, response.Length);
            Regex pattern = new Regex("<div id=\"current-aqi\".*?<h1>(\\d+)</h1>.*?Time: (\\d?\\d:\\d\\d).*?<div id=\"status\".*?>(.*?)<div", RegexOptions.Singleline);
            Match match = pattern.Match(html);
            if (!match.Success) // couldn't extract the PM2.5 value :(
                return;

            ShellTile tile = ShellTile.ActiveTiles.First();
            StandardTileData data = new StandardTileData
            {
                BackContent = "PM2.5: " + match.Groups[1].Value + "\n" + match.Groups[3].Value.Trim(),
                BackTitle = "Updated: " + match.Groups[2].Value
            };
            tile.Update(data);
        }
    }
}