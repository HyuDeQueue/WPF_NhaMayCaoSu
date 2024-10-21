﻿using Serilog;
using System.Windows;
using WPF_NhaMayCaoSu.Core.Utils;
using WPF_NhaMayCaoSu.OTPService;
using WPF_NhaMayCaoSu.Repository.Models;
using WPF_NhaMayCaoSu.Service.Interfaces;
using WPF_NhaMayCaoSu.Service.Services;

namespace WPF_NhaMayCaoSu.Content
{
    /// <summary>
    /// Interaction logic for MainControl.xaml
    /// </summary>
    public partial class MainControl : Window
    {
        public Account CurrentAccount { get; set; }

        private readonly MqttServerService _mqttServerService;
        private readonly MqttClientService _mqttClientService;
        private readonly IBoardService _boardService;
        private readonly BrokerWindow broker;
        private CustomerListWindow customerListWindow;
        private SaleListWindow saleListWindow;
        private AccountManagementWindow accountManagementWindow;
        private RFIDListWindow rfidListWindow;
        private BoardListWindow boardListWindow;
        private RoleListWindow roleListWindow;
        private MainWindow mainWindow;
        private ConfigCamera configCamera;

        public MainControl()
        {
            InitializeComponent();
            _mqttServerService = MqttServerService.Instance;
            Start_Broker();
            _mqttServerService.BrokerStatusChanged += (sender, e) => UpdateMainWindowUI();
            _mqttClientService = new MqttClientService();
            _mqttServerService.DeviceCountChanged += OnDeviceCountChanged;
            broker = new BrokerWindow();
            customerListWindow = new CustomerListWindow();
            boardListWindow = new BoardListWindow();
            accountManagementWindow = new AccountManagementWindow();
            rfidListWindow = new RFIDListWindow();
            roleListWindow = new RoleListWindow();
            mainWindow = new();
            saleListWindow = new SaleListWindow(mainWindow);
            configCamera = new();

            mainWindow.CurrentAccount = CurrentAccount;
            broker.CurrentAccount = CurrentAccount;
            customerListWindow.CurrentAccount = CurrentAccount;
            saleListWindow.CurrentAccount = CurrentAccount;
            accountManagementWindow.CurrentAccount = CurrentAccount;
            rfidListWindow.CurrentAccount = CurrentAccount;
            roleListWindow.CurrentAccount = CurrentAccount;
            boardListWindow.CurrentAccount = CurrentAccount;
            keyCheck();
            ValidCheck();
            MainContentControl.Content = broker.Content;
            UpdateMainWindowUI();
        }
        private void keyCheck()
        {
            try
            {
                RegistryHelper _registryHelper = new();

                if (!_registryHelper.IsUnlocked())
                {
                    DateTime? demoStartDate = _registryHelper.GetDemoStartDate();
                    if (demoStartDate == null)
                    {
                        _registryHelper.SetDemoStartDate(DateTime.Now);
                    }

                    AccessKeyWindow accessKeyWindow = new AccessKeyWindow();
                    accessKeyWindow.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Log.Error(ex, "Error occurred in keyCheck method.");
                MessageBox.Show("An error occurred while checking the registry: " + ex.Message);
            }
        }


        private async void Start_Broker()
        {
            await _mqttServerService.StartBrokerAsync();
            MqttServerService.IsBrokerRunning = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {


            App.Current.Shutdown();

        }

        private bool ValidCheck()
        {

            if (CurrentAccount is null)
            {
                LoginWindow window = new();
                window.LoginSucceeded += HandleLoginSucceeded;
                window.ShowDialog();
                if (CurrentAccount is null)
                {
                    return false;
                }
            }
            else if (MqttServerService.IsBrokerRunning == false)
            {
                MessageBox.Show("Xin hãy khởi động server trước khi sử dụng", "Sever status offline", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }

        // Toggle the visibility of BrokerWindow content
        private void BrokerManagementButton_Click(object sender, RoutedEventArgs e)
        {

            MainContentControl.Content = broker.Content;
            broker.OnWindowLoaded();
            Title = broker.Title;

        }

        private void CustomerManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = customerListWindow.Content;
            customerListWindow.OnWindowLoaded();
            Title = customerListWindow.Title;
        }

        private void BoardManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = boardListWindow.Content;
            boardListWindow.OnWindowLoaded();
            Title = boardListWindow.Title;
        }

        private void SaleManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = saleListWindow.Content;
            saleListWindow.OnWindowLoaded();
            Title = saleListWindow.Title;
        }

        private void AccountManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = accountManagementWindow.Content;
            accountManagementWindow.OnWindowLoaded();
            Title = accountManagementWindow.Title;
        }

        private void RFIDManagementButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = rfidListWindow.Content;
            rfidListWindow.OnWindowLoaded();
            Title = rfidListWindow.Title;
        }

        private void RoleManagementButton_Click(object sender, RoutedEventArgs e)
        {
            //if (!ValidCheck())
            //{
            //    return;
            //}

            MainContentControl.Content = roleListWindow.Content;
            roleListWindow.OnWindowLoaded();
            Title = roleListWindow.Title;
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }

            MainContentControl.Content = configCamera.Content;
            configCamera.OnWindowLoaded();
            Title = configCamera.Title;
        }

        private async void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidCheck())
            {
                return;
            }


            MainContentControl.Content = mainWindow.Content;
            mainWindow.OnWindowLoaded();
            Title = mainWindow.Title;
        }


        //private void LoginButton_Click(object sender, RoutedEventArgs e)
        //{
        //    LoginWindow window = new();
        //    window.LoginSucceeded += HandleLoginSucceeded;
        //    window.ShowDialog();
        //}
        private void HandleLoginSucceeded(Account account)
        {
            CurrentAccount = account;
            mainWindow.CurrentAccount = account;
            broker.CurrentAccount = account;
            customerListWindow.CurrentAccount = account;
            saleListWindow.CurrentAccount = account;
            accountManagementWindow.CurrentAccount = account;
            rfidListWindow.CurrentAccount = account;
            roleListWindow.CurrentAccount = account;
            //LoginButton.Visibility = Visibility.Hidden;

        }


        private string GetLocalIpAddress()
        {
            System.Net.IPAddress[] ipAddresses = System.Net.Dns.GetHostAddresses(System.Net.Dns.GetHostName());

            foreach (System.Net.IPAddress ip in ipAddresses)
            {
                // Check for IPv4 addresses
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "N/A";
        }

        private void UpdateMainWindowUI()
        {
            if (MqttServerService.IsBrokerRunning)
            {
                OpenServerButton.Content = Constants.CloseServerText;
                ServerStatusTextBlock.Text = Constants.ServerOnlineStatus;
                int deviceCount = _mqttServerService.GetDeviceCount();
                NumberofconnectionTextBlock.Text = $"Onl: {deviceCount} Thiết bị";
                IPconnecttionSmallLabel.Content = $"Local IP: {GetLocalIpAddress()}";
            }
            else
            {
                OpenServerButton.Content = Constants.OpenServerText;
                ServerStatusTextBlock.Text = Constants.ServerOfflineStatus;
                IPconnecttionSmallLabel.Content = "Không có kết nối";
            }
        }

        private void OnDeviceCountChanged(object sender, int deviceCount)
        {
            Dispatcher.Invoke(() =>
            {
                NumberofconnectionTextBlock.Text = $"Onl:{deviceCount} Thiết bị";
            });
        }

        private async void Broker_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MqttServerService.IsBrokerRunning)
                {
                    await _mqttServerService.StopBrokerAsync();
                    await _mqttClientService.CloseConnectionAsync();
                }
                else
                {
                    await _mqttServerService.StartBrokerAsync();
                    await _mqttClientService.ConnectAsync();
                }
                UpdateMainWindowUI();
            }
            catch (Exception ex)
            {
                if (OpenServerButton.Content.ToString() == Constants.OpenServerText)
                {
                    MessageBox.Show(Constants.BrokerStartErrorMessage + "\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error(ex, $"{Constants.BrokerStartErrorMessage}");
                }
                else
                {
                    MessageBox.Show(Constants.BrokerStopErrorMessage + "\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Log.Error(ex, $"{Constants.BrokerStopErrorMessage}");
                }
            }
        }
        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Hành động này sẽ đóng ứng dụng, bạn chắc chứ?", "Thoát", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                App.Current.Shutdown();
            }

        }

        // Sự kiện để mở popup
        private void OpenPopup(object sender, RoutedEventArgs e)
        {
            TimePopup.IsOpen = true;
        }

        // Sự kiện để lưu thời gian vào cơ sở dữ liệu
        private void SaveTime(object sender, RoutedEventArgs e)
        {
            // Lấy giá trị từ TextBox
            string selectedTime = TimeTextBox.Text;

            // Kiểm tra xem thời gian có hợp lệ không
            if (DateTime.TryParse(selectedTime, out DateTime result))
            {
                // Lưu vào cơ sở dữ liệu
                //SaveToDatabase(result);
                MessageBox.Show("Thời gian đã được lưu.");
                TimePopup.IsOpen = false; // Đóng popup
            }
            else
            {
                MessageBox.Show("Vui lòng nhập thời gian hợp lệ.");
            }
        }


    }
}


