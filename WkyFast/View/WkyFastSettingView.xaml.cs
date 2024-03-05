﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WkyApiSharp.Events.Account;
using WkyFast.Service;
using WkyFast.Utils;

namespace WkyFast.View
{
    /// <summary>
    /// WkyFastSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WkyFastSettingView : Page
    {
        public WkyFastSettingView()
        {
            InitializeComponent();

            AccountTextBlock.Text = WkyApiManager.Instance.API.User;

            WkyApiManager.Instance.API.EventReceived
                        .OfType<LoginResultEvent>()
                        .Subscribe(async r =>
                        {
                            AccountTextBlock.Text = r.Account;
                        });



#if DEBUG
        TestTurnServerPanel.Visibility = Visibility.Visible;
#endif

        }

        private void AccountCardAction_Click(object sender, RoutedEventArgs e)
        {
            //询问登出
            //if (MessageBox.Show("是否登出账号？", "提示", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            //{
            //}

            MainWindow.Instance.ShowMessageBox("提示", "是否登出账号？", () => {
                MainWindow.Instance.ReLoginFunc();
                this.AccountTextBlock.Text = "-";
            }, () => {
                //没有操作
            });
        }

        public void OnLoginResult(LoginResultEvent e)
        {
            if (e.IsSuccess)
            {
                this.AccountTextBlock.Text = e.Account;
            }
        }

        private void BadgeNewVersion_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //跳转至最新
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast/releases/latest");
        }

        private async void TestTurnServerButton_Click(object sender, RoutedEventArgs e)
        {
            await WkyApiManager.Instance.API.GetTurnServer(WkyApiManager.Instance.NowDevice.Device.DeviceSn);
        }

        private async void LinkAIKEY_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://aikey.one/register?aff=qHFBWX");
        }

        private async void LinkAPI2D_Click(object sender, RoutedEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://api2d.com/r/211572");
        }

        private void HomePageTextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BrowserHelper.OpenUrlBrowser("https://github.com/aiqinxuancai/WkyFast");
        }
    }
}
