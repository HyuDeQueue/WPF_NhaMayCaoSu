﻿using System.Windows;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using System.IO;
using WPF_NhaMayCaoSu.Repository.Models;
using WPF_NhaMayCaoSu.Service.Services;

namespace WPF_NhaMayCaoSu
{
    public partial class ConfigCamera : Window
    {
        private readonly CameraService _cameraService = new();

        public Account CurrentAccount { get; set; }

        public ConfigCamera()
        {
            InitializeComponent();
            LoadSavedUrlsAsync();
        }

        private async Task LoadSavedUrlsAsync()
        {
            var camera = await _cameraService.GetNewestCameraAsync();
            if (camera != null)
            {
                txtUrl1.Text = camera.Camera1;
                txtUrl2.Text = camera.Camera2;
            }
        }

        private void btnCapture1_Click(object sender, RoutedEventArgs e)
        {
            CaptureAndDisplayImage(txtUrl1.Text, imgCamera1);
        }

        private void btnCapture2_Click(object sender, RoutedEventArgs e)
        {
            CaptureAndDisplayImage(txtUrl2.Text, imgCamera2);
        }

        private void CaptureAndDisplayImage(string rtspUrl, System.Windows.Controls.Image imageControl)
        {
            try
            {
                using (VideoCapture capture = new VideoCapture(rtspUrl))
                {
                    using (Mat frame = new Mat())
                    {
                        capture.Read(frame);

                        if (!frame.IsEmpty)
                        {
                            Image<Bgr, byte> image = frame.ToImage<Bgr, byte>();
                            Bitmap bitmap = image.ToBitmap();
                            imageControl.Source = ConvertBitmapToBitmapImage(bitmap);

                            string imagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                            bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                            MessageBox.Show($"Image saved at: {imagePath}");
                        }
                        else
                        {
                            MessageBox.Show("Failed to capture image from the RTSP stream.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}");
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Bmp);
                memoryStream.Seek(0, SeekOrigin.Begin);

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private async void SaveUrlCamera1_Click(object sender, RoutedEventArgs e)
        {
            var camera = await _cameraService.GetNewestCameraAsync();
            if (camera == null)
            {
                camera = new Camera
                {
                    CameraId = Guid.NewGuid(),
                    Camera1 = txtUrl1.Text,
                    Camera2 = "NoURLYet",
                    Status = 1
                };
                await _cameraService.CreateCameraAsync(camera);
            }
            else
            {
                camera.Camera1 = txtUrl1.Text;
                await _cameraService.UpdateCameraAsync(camera);
            }

            MessageBox.Show("Camera 1 URL has been saved.");
        }

        private async void SaveUrlCamera2_Click(object sender, RoutedEventArgs e)
        {
            var camera = await _cameraService.GetNewestCameraAsync();
            if (camera == null)
            {
                camera = new Camera
                {
                    CameraId = Guid.NewGuid(),
                    Camera1 = "NoURLYet",
                    Camera2 = txtUrl2.Text,
                    Status = 1
                };
                await _cameraService.CreateCameraAsync(camera);
            }
            else
            {
                camera.Camera2 = txtUrl2.Text;
                await _cameraService.UpdateCameraAsync(camera);
            }

            MessageBox.Show("Camera 2 URL has been saved.");
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {

            Close();
        }
    }
}
