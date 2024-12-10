namespace DodgeGame
{
    public partial class MainPage : ContentPage {
        double charHeight = 0;
        Random random = new Random();
        bool startGame = false;
        IDispatcherTimer timerDispatch, pickupTimer;
        Player? player;
        

        public MainPage() {
            InitializeComponent();
            timerDispatch = Dispatcher.CreateTimer();
            timerDispatch.Tick += OnTimerTick;
            pickupTimer = Dispatcher.CreateTimer();
            pickupTimer.Tick += OnPickupTimerTick;
        }

        // Everytime OnTimerTick is called, create a new enemy
        // The next enemy should have a random start time too
        private void OnTimerTick(object? sender, EventArgs e) {
            timerDispatch.Stop();
            CreateEnemy();
            int mseconds = random.Next(500, 3000);
            timerDispatch.Interval = TimeSpan.FromMilliseconds(mseconds);
            timerDispatch.Start();
        }

        private void OnPickupTimerTick(object? sender, EventArgs e) {

            pickupTimer.Stop();
            Pickup? pickup = new Pickup();
            pickup.Remove += (s, data) =>
            {
                pickup = null;
            };
            int mseconds = random.Next(5000, 12000);
            pickupTimer.Interval = TimeSpan.FromMilliseconds(mseconds);
            pickupTimer.Start();
        }

        // Create a new Enemy object, StartMove is async (in Enemy class) so 
        // multiple threading is fine
        private void CreateEnemy() {
            Enemy? enemy = new Enemy();
            
            // Subscribe to the removeEnemy event. If this event is raised set the object to null
            enemy.Remove += (s, data) =>
            {
                enemy = null;
            };

        }

        // OnAppearing is when we want to set the Window's height and width. Fixing them in Windows
        // In Android, this will not change
        protected override void OnAppearing() {
            base.OnAppearing();

#if WINDOWS
            this.Window.X = 0;
            this.Window.Y = 0;
            this.Window.MaximumHeight = DeviceDisplay.Current.MainDisplayInfo.Height-50;
            this.Window.MinimumHeight = DeviceDisplay.Current.MainDisplayInfo.Height-50;
            this.Window.MaximumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
            this.Window.MinimumWidth = DeviceDisplay.Current.MainDisplayInfo.Width;
#endif
        }


        // Figure out what size we can make the GameLayout and do that
        // We need room for the Arrow Keys
        // Put the StartButton in the centre of the screen
        protected override void OnSizeAllocated(double width, double height) {
            base.OnSizeAllocated(width, height);
            GameLayout.WidthRequest = width;
            GameLayout.HeightRequest = height - ControlsGrid.Height;
        }

        private void Start_Button_Clicked(object sender, EventArgs e) {
            if (!startGame) {
                charHeight = GameLayout.Height / 12.0;
                player = new Player(charHeight, GameLayout, GameLayout.Height, GameLayout.Width);
                Addin.SetStaticProperties(charHeight, player, GameLayout, Dispatcher);
                BindingContext = player;
                //When GameOver even received, stop the timers
                player.GameOverEvent += (s, data) =>
                {
                    pickupTimer.Stop();
                    timerDispatch.Stop();
                };
                this.SetBinding(TitleProperty, "TopInformation");

                startGame = true;
                int mseconds = random.Next(400, 3000);
                timerDispatch.Interval = TimeSpan.FromMilliseconds(mseconds);
                mseconds = random.Next(100, 300);
                pickupTimer.Interval = TimeSpan.FromMilliseconds(mseconds);
                pickupTimer.Start();
                timerDispatch.Start();
            }
            else {
                player?.StartPlayer();
                pickupTimer.Start();
                timerDispatch.Start();
            }
            
        }


        private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e) {
            if (player != null) {
                Point p = (e.GetPosition((AbsoluteLayout)sender)).Value;
                await player.MovePlayerTap(p);
            }
        }

        // When the page is disappearing, try to clear up some of the objects
        protected override void OnDisappearing() {
            pickupTimer.Stop();
            timerDispatch.Stop();
            Addin.RemoveMainLayoutRef();
            base.OnDisappearing();
        }

    }

}
