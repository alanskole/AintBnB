using System;

using AintBnB.ViewModels;

using Windows.UI.Xaml.Controls;

namespace AintBnB.Views
{
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            InitializeComponent();
        }
    }
}
