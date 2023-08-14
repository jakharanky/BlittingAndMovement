using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using static Windows.UI.Xaml.Media.Imaging.WriteableBitmapExtensions;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Devices.Enumeration;
using Windows.System;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace test6
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private bool isPointerPressed = false;
        private Point lastPointerPosition;
        private TranslateTransform imageTranslationEditable = new TranslateTransform();
        private TranslateTransform imageTranslationOutfit = new TranslateTransform();
        private WriteableBitmap writeableBitmapImageOutfit;
        
        public MainPage()
        {
            this.InitializeComponent();
            LoadWritableFromImageUriAsync();
            ImagePreview2.RenderTransform = imageTranslationOutfit;
        }

        private async void LoadWritableFromImageUriAsync()
        {
            Uri imageUri = new Uri("ms-appx:///Assets/bmwm4.jpg"); // Replace with the actual image path
            ImagePreview.Source = await LoadWritableFromImageUriAsync(imageUri);
            
        }

        private async Task<WriteableBitmap> LoadWritableFromImageUriAsync(Uri imageUri)
        {
            WriteableBitmap writeableBitmap = null;

            StorageFile imageFile = await StorageFile.GetFileFromApplicationUriAsync(imageUri);
            using (IRandomAccessStream stream = await imageFile.OpenReadAsync())
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(stream);

                // Create WriteableBitmap from BitmapImage
                writeableBitmap = new WriteableBitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight);
                stream.Seek(0); // Reset stream position
                await writeableBitmap.SetSourceAsync(stream);
            }

            // Now 'writeableBitmap' contains the image data from the provided URI
            return writeableBitmap;
        }
    
    private void OutfitImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            isPointerPressed = true;
            ImagePreview2.CapturePointer(e.Pointer);

            Debug.WriteLine($"Pointer Pressed: X = {e.GetCurrentPoint(ImagesCanvas).Position.X}, Y = {e.GetCurrentPoint(ImagesCanvas).Position.Y}");

        }

        private void OutfitImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (isPointerPressed)
            {
                Point currentPointerPosition = e.GetCurrentPoint(ImagesCanvas).Position;

                double deltaX = currentPointerPosition.X - lastPointerPosition.X;
                double deltaY = currentPointerPosition.Y - lastPointerPosition.Y;

                imageTranslationOutfit.X += deltaX;
                imageTranslationOutfit.Y += deltaY;

                lastPointerPosition = currentPointerPosition;

            }
        }
        private void OutfitImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            isPointerPressed = false;
            ImagePreview2.ReleasePointerCapture(e.Pointer);

            Debug.WriteLine($"Pointer Released: X = {e.GetCurrentPoint(ImagesCanvas).Position.X}, Y = {e.GetCurrentPoint(ImagesCanvas).Position.Y}");

        }

        private async void SampleOutfit(object sender, RoutedEventArgs e)
        {
            if (ImagePreview2.Visibility == Visibility.Collapsed)
            {
                var picker = new FileOpenPicker();
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");

                // Pick a single file
                StorageFile file = await picker.PickSingleFileAsync();

                if (file != null)
                {
                    // Load the image from the selected file
                    using (var stream = await file.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                        writeableBitmapImageOutfit = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                        await writeableBitmapImageOutfit.SetSourceAsync(stream);
                    }

                    ImagePreview2.Source = writeableBitmapImageOutfit;
                }
                ImagePreview2.Visibility = Visibility.Visible;
            }
            else
            {
                ImagePreview2.Visibility = Visibility.Collapsed;
            }
        }

        private void BlitImages(object sender, RoutedEventArgs e)
        {
            writeableBitmapImageOutfit = writeableBitmapImageOutfit.Resize((int)ImagePreview2.ActualWidth, (int)ImagePreview2.ActualHeight, WriteableBitmapExtensions.Interpolation.Bilinear);

            double xPosition = imageTranslationOutfit.X;
            double yPosition = imageTranslationOutfit.Y;


            if (ImagePreview != null && writeableBitmapImageOutfit != null)
            {
                var blit1 = ImagePreview.Source as WriteableBitmap;
                Rect sourceRect = new Rect(0, 0, writeableBitmapImageOutfit.PixelWidth, writeableBitmapImageOutfit.PixelHeight);
                Rect destRect = new Rect(xPosition, yPosition, writeableBitmapImageOutfit.PixelWidth, writeableBitmapImageOutfit.PixelHeight);

                blit1.Blit(destRect, writeableBitmapImageOutfit, sourceRect, BlendMode.None);
            }
        }
    }
}
