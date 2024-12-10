
namespace DodgeGame
{
    // An Idea could be randomising the size of the enemy
    public class Enemy : Addin
    {
        private Grid box;
        private Image img;
        private readonly int top;
        private Task<bool>? animation;
        private readonly double enemySize;

        public Enemy() : base() {
            // Starting Position, top, from right, from left or bottom
            top = random.Next(4);

            // Allow the enemy size to vary
            enemySize = size + random.Next((int)-size / 2, (int)size/2);

            // Like player, use a Grid and then an image to ensure the full image is in the rectangle
            box = new Grid()
            {
                WidthRequest = enemySize,
                HeightRequest = enemySize,
                BackgroundColor = Colors.Transparent
            };
            // Random Enemy Image
            string filename = "bad" + random.Next(1, 5) + ".png";
            img = new Image
            {
                WidthRequest = enemySize + enemySize / 2,
                HeightRequest = enemySize + enemySize / 2,
                Source = ImageSource.FromFile(filename),
                ZIndex = 10
            };
            box.Add(img);
            if (mainL == null)
                return;
            
            int x, y = 0;

            // Top will determine does the enemy appear from the left, right, top or bottom. The rotation will change depending
            // Pick a random starting position on the side it is starting from
            if (top == 0) {
                x = random.Next((int)(width - enemySize + 1));
                y = 0;
                box.Rotation = 180;
            }
            else if(top == 1){
                x = (int)(width - enemySize);
                y = random.Next((int)(height - enemySize + 1));
                box.Rotation = 270;
            }
            else if(top == 2) {
                x = random.Next((int)(width - enemySize + 1));
                y = (int)(height-enemySize);
            }
            else {
                x = 0;
                y = random.Next((int)(height - enemySize + 1));
                box.Rotation = 90;
            }
            // Insert the box at position 0,0 and then translate it to the position we want
            //MainL.SetLayoutBounds(box, new Rect(0, 0, size, size));
            mainL.Add(box);
            box.TranslationX = x;
            box.TranslationY = y;
            
            // Use Task so StartMove is on a different thread, not locking up the UI
            Task.Run(() => StartMove());
        }

        // Every 20ms check if an enemy intersects with the player character
        protected override void Update() {
            // The current position of the enemy (the animation changes its position
            Rect rect = new Rect(box.TranslationX, box.TranslationY, enemySize, enemySize);
            if (playerref != null) {
                if (rect.IntersectsWith(playerref.Position)) {
                    timer.Stop();
                    box.CancelAnimations();

                    mainDispatch?.Dispatch(() =>
                    {
                        playerref.GotHit();

                    });
                }
            }
            
        }

        public async Task StartMove() {
            // Give it a random delay before actually moving
            int randDelay = random.Next(200, 1000);
            await Task.Delay(randDelay);

            // Make the box have a random speed for its animation
            uint randSpeed = (uint)random.Next(1700, 5000);
            switch (top) {
                case 0:
                    animation = box.TranslateTo(box.TranslationX, height, randSpeed, Easing.Linear);
                    break;
                case 1:
                    animation = box.TranslateTo(-enemySize, box.TranslationY, randSpeed, Easing.Linear);
                    break;
                case 2:
                    animation = box.TranslateTo(box.TranslationX, -enemySize, randSpeed, Easing.Linear);
                    break;
                case 3:
                    animation = box.TranslateTo(width + enemySize, box.TranslationY, randSpeed, Easing.Linear);
                    break;
            }
            timer.Start();
            await animation;
            // When the animation has finished (either by going off screen or colliding with player)
            // Remove the box from the layout
            FullyRemove();
        }

        protected override void FullyRemove() {
            mainDispatch?.Dispatch(() => { 
                box.Remove(img);  
                mainL?.Remove(box); 
            });
            // Remove the timer's resources
            base.FullyRemove();
        }

    }
}
