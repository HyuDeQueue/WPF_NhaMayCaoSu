﻿using System.Windows;
using WPF_NhaMayCaoSu.Repository.Models;
using WPF_NhaMayCaoSu.Service.Services;
using WPF_NhaMayCaoSu.Core.Utils;
using WPF_NhaMayCaoSu.Service.Interfaces;

namespace WPF_NhaMayCaoSu
{
    /// <summary>
    /// Interaction logic for CustomerListWindow.xaml
    /// </summary>
    public partial class CustomerListWindow : Window
    {

        private ICustomerService _service = new CustomerService();
        private int _currentPage = 1;
        private int _pageSize = 10;
        private int _totalPages;
        public Account CurrentAccount { get; set; } = null;

        public CustomerListWindow()
        {
            InitializeComponent();
        }

        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CustomerManagementWindow customerManagementWindow = new CustomerManagementWindow();
            customerManagementWindow.CurrentAccount = CurrentAccount;
            customerManagementWindow.ShowDialog();
            LoadDataGrid();
        }

        private void EditCustomerButton1_Click(object sender, RoutedEventArgs e)
        {

            Customer selected = CustomerDataGrid.SelectedItem as Customer;

            if (selected == null)
            {
                MessageBox.Show(Constants.ErrorMessageSelectCustomer, Constants.ErrorTitleSelectCustomer, MessageBoxButton.OK, MessageBoxImage.Stop);
                return;
            }

            CustomerManagementWindow customerManagementWindow = new CustomerManagementWindow();
            customerManagementWindow.CurrentAccount = CurrentAccount;
            customerManagementWindow.SelectedCustomer = selected;
            customerManagementWindow.ShowDialog();
            LoadDataGrid();
        }


        private async void LoadDataGrid()
        {
            var sales = await _service.GetAllCustomers(_currentPage, _pageSize);
            int totalCustomerCount = await _service.GetTotalCustomersCountAsync();
            _totalPages = (int)Math.Ceiling((double)totalCustomerCount / _pageSize);

            CustomerDataGrid.ItemsSource = null;
            CustomerDataGrid.Items.Clear();
            CustomerDataGrid.ItemsSource = await _service.GetAllCustomers(1, 10);

            PageNumberTextBlock.Text = $"Trang {_currentPage} trên {_totalPages}";

            // Disable/Enable pagination buttons
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = _currentPage < _totalPages;
        }

        private void PreviousPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadDataGrid();
            }
        }

        private void NextPageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadDataGrid();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentAccount?.Role?.RoleName != "Admin")
            {
                EditCustomerButton1.Visibility = Visibility.Collapsed;
                AddCustomerButton.Visibility = Visibility.Collapsed;
            }
            LoadDataGrid();
        }
        public void OnWindowLoaded()
        {
            Window_Loaded(this, null);
        }

        private void CustomerManagementButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Bạn đang ở cửa sổ Quản lý Khách hàng!", "Lặp cửa sổ!", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RFIDManagementButton_Click(object sender, RoutedEventArgs e)
        {
            RFIDListWindow rFIDListWindow = new RFIDListWindow();
            rFIDListWindow.CurrentAccount = CurrentAccount;
            Close();
            rFIDListWindow.ShowDialog();
        }

        private void SaleManagementButton_Click(object sender, RoutedEventArgs e)
        {
            SaleListWindow saleListWindow = new SaleListWindow();
            saleListWindow.CurrentAccount = CurrentAccount;
            Close();
            saleListWindow.ShowDialog();
        }


        private void AccountManagementButton_Click(object sender, RoutedEventArgs e)
        {
            AccountManagementWindow accountManagementWindow = new AccountManagementWindow();
            accountManagementWindow.CurrentAccount = CurrentAccount;
            Close();
            accountManagementWindow.ShowDialog();
        }

        private void BrokerManagementButton_Click(object sender, RoutedEventArgs e)
        {
            BrokerWindow brokerWindow = new BrokerWindow();
            brokerWindow.CurrentAccount = CurrentAccount;
            Close();
            brokerWindow.ShowDialog();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            ConfigCamera configCamera = new ConfigCamera();
            configCamera.ShowDialog();
        }

        private void ShowButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.CurrentAccount = CurrentAccount;
            mainWindow.Show();
        }

        private void RoleManagementButton_Click(object sender, RoutedEventArgs e)
        {
            RoleListWindow roleListWindow = new();
            roleListWindow.CurrentAccount = CurrentAccount;
            roleListWindow.ShowDialog();
        }

        private void AddRFIDButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentAccount?.Role?.RoleName == "User")
            {
                MessageBox.Show(Constants.UnauthorizedMessage, Constants.UnauthorizedTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Customer selectedCustomer = CustomerDataGrid.SelectedItem as Customer;

            if (selectedCustomer != null)
            {
                RFIDManagementWindow rfidManagementWindow = new RFIDManagementWindow(selectedCustomer);
                rfidManagementWindow.ShowDialog();
            }
            else
            {
                MessageBox.Show("Xin hãy chọn khách hàng trước.", "Lưu ý", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            LoadDataGrid();
        }

        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = SearchTextBox.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(searchTerm))
            {
                LoadDataGrid();
            }
            else
            {
                CustomerDataGrid.ItemsSource = null;
                CustomerDataGrid.Items.Clear();
                var sales = await _service.GetAllCustomers(1, 10);
                CustomerDataGrid.ItemsSource = sales.Where(s => s.CustomerName.ToLower().Contains(searchTerm));
            }
        }

    }
}
