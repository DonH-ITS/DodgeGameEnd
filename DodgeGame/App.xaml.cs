using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Windowing;
using Windows.Graphics;

namespace DodgeGame
{
    public partial class App : Application
    {
        public App() {
            InitializeComponent();

            MainPage = new AppShell();
          
        }
    }
}
