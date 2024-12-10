using System.ComponentModel;
using Plugin.Maui.Audio;

namespace DodgeGame
{
    public class Player : INotifyPropertyChanged
    {        
        //Fields
        Task<bool>? animation = null;
        private double size;
        private Grid box;
        private Image img;
        private System.Timers.Timer timer;
        private double maxHeight, maxWidth;
        private bool overedge = false, audioInit = false;
        private int life;
        private int hits;
        private double speed;
        private int coins;
        private IAudioPlayer? audioHit, audioPickup, audioBoost, audioOver;
        public event EventHandler? GameOverEvent;

        private Rect pos;
        public Rect Position
        {
            get
            {
                return pos;
            }
        }

        private bool allowmoves = false;
        public bool Allowmoves
        {
            get => allowmoves;
            set
            {
                allowmoves = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EnableButton));
            }
        }
        public bool EnableButton
        {
            get
            {
                return !allowmoves;
            }
        }
        public string TopInformation
        {
            get
            {
                int left = life - hits;
                string result = "";
                if (left == 0)
                    result += "Game Over.";
                else {
                    result += left == 1 ? left + " life left." : left + " lives left.";
                }
                if (coins > 0) {
                    result += " (" + coins + (coins > 1 ? " coins" : " coin") + " collected)";
                }
                return result;
            }
        }
        public Command MoveCommand { get; }

        public Player(double size, AbsoluteLayout mainl, double maxh, double maxw) {
            MoveCommand = new Command<string>(async (dir) => await MovePlayer(dir));

            // Because of the whitespace around smile images we need to first create a grid of the size we want
            // Then have an image that is zoomed in as part of the grid, the whitespace is cut off that way.
            box = new Grid()
             {
                 WidthRequest = size,
                 HeightRequest = size,
                 BackgroundColor = Colors.Transparent
             };

            img = new Image
            {
                WidthRequest = size + size/2,
                HeightRequest = size + size/2,
                Source = ImageSource.FromFile("smile0.png"),
                ZIndex = 10
            };
            box.Add(img);

            maxHeight = maxh;
            maxWidth = maxw;
            this.size = size;
            mainl.Add(box);
            pos = new Rect(0, 0, size, size);
            timer = new System.Timers.Timer
            {
                Interval = 20
            };
            timer.Elapsed += (s, e) =>
            {
                Update();
            };
            life = 3;
            
            Allowmoves = false;
            StartPlayer();
            
        }

        public async Task MovePlayerTap(Point point) {
            if (allowmoves) {
                double x = box.TranslationX;
                double y = box.TranslationY;
                // We probably want the centre of the player to move to where the player taps/clicks so adjust point
                point.X -= size / 2;
                point.Y -= size / 2;

                // Make sure the player does not go off the side
                if (point.X + size >= maxWidth)
                    point.X = maxWidth - size - 1;
                else if (point.X <= 0)
                    point.X = 1;
                if (point.Y + size >= maxHeight)
                    point.Y = maxHeight - size - 1;
                else if (point.Y <= 0)
                    point.Y = 1;

                double distance = point.Distance(new Point(x, y));
                if (animation != null) {
                    box.CancelAnimations();
                }
                uint time = (uint)(distance/speed);
                animation = box.TranslateTo(point.X, point.Y, time);
                await animation;
                animation = null;
            }
        }

        public async void StartPlayer() {
            if (!allowmoves) {
                box.TranslationX = (maxWidth - size) / 2;
                box.TranslationY = (maxHeight - size) / 2;
                pos.X = box.TranslationX;
                pos.Y = box.TranslationY;
                hits = 0;
                coins = 0;
                speed = size / 250.0;
                img.Source = ImageSource.FromFile("smile0.png");
                timer.Start();
                if (!audioInit) {
                    // Try catch or something should be here if the file does not exist
                    audioHit = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("hit.mp3"));
                    audioPickup = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("ding.mp3"));
                    audioBoost = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("boost.mp3"));
                    audioOver = AudioManager.Current.CreatePlayer(await FileSystem.OpenAppPackageFileAsync("gameover.mp3"));
                    audioInit = true;
                }
                Allowmoves = true;
            }
        }

        public async void GotHit() {
            hits++;
            OnPropertyChanged(nameof(TopInformation));
            audioHit?.Play();
            if (hits < life) {
                img.Source= ImageSource.FromFile("shocked.png");
                await Task.Delay(300);
                img.Source = ImageSource.FromFile("smile"+hits+".png");
            }
            if(hits >= life) {
                // Game Over
                img.Source = ImageSource.FromFile("dead.png");
                timer.Stop();
                box.CancelAnimations();
                Allowmoves = false;
                pos.Y = -size - 1;
                pos.X = -size - 1;
                audioOver?.Play();
                GameOverEvent?.Invoke(this, null);
            }
        }

        public void GotPickup(bool coin) {
            if (coin) {
                audioPickup?.Play();
                coins++;
                OnPropertyChanged(nameof(TopInformation));
            }
            else {
                audioBoost?.Play();
                speed *= 1.25;
            }
        }

        // Every 20ms check the current position of the box and update the pos Rectangle. 
        // If it has gone over the edge, cancel the animation and signal the animation to fix the translation.
        // Do not modify box.TranslationX here as it is in a different thread than the UI
        private void Update() {
            if (box.TranslationY < 0 || box.TranslationX < 0 || box.TranslationY > maxHeight - size || box.TranslationX > maxWidth - size) {
                if (animation != null) {
                    overedge = true;
                    box.CancelAnimations(); 
                }
            }
            pos.Y = box.TranslationY;
            pos.X = box.TranslationX;

        }

        // Even with MovePlayerTap,the arrow buttons will still work.
        private async Task MovePlayer(string dir) {
            if (allowmoves) {
                double x = box.TranslationX;
                double y = box.TranslationY;
                switch (dir) {
                    case "0":
                        x -= size;
                        break;
                    case "1":
                        y -= size;
                        break;
                    case "2":
                        y += size;
                        break;
                    case "3":
                        x += size;
                        break;
                }
                if (animation != null) {
                    box.CancelAnimations();
                }
                // Distance is size here. If no boost pickups this should be 250ms (but there may be some rounding error)
                uint time = (uint)(size / speed);

                animation = box.TranslateTo(x, y, time);

                bool wascancelled = await animation;
                // if wascancelled is true, that means that box.CancelAnimations was called at some stage
                // We only care when box.CancelAnimations was called by the update, ie overedge is also true
                // We ensure the box does not go over the edge by doing this
                if (wascancelled && overedge) {
                    if (box.TranslationY < 0) {
                        box.TranslationY = 0;
                    }
                    else if (box.TranslationY > maxHeight - size) {
                        box.TranslationY = maxHeight - size;
                    }
                    if (box.TranslationX < 0) {
                        box.TranslationX = 0;
                    }
                    else if (box.TranslationX > maxWidth - size) {
                        box.TranslationX = maxWidth - size;
                    }
                    overedge = false;
                }
                animation = null;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
