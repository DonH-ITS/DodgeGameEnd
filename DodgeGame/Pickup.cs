
namespace DodgeGame
{
    public class Pickup : Addin
    {
        // The best way of doing this would actually have a parent class
        // and have Pickup and Enemy inherit from it
        private Image box;
        private Rect pos;
        private bool coin;
        private uint counter;

        public Pickup() {
            double size = Addin.size/2;

            // Most of the pickups should be coins, but give some chance it is a boost pickup
            coin = !(random.Next(6) == 5);
            box = new Image
            {
                WidthRequest = size,
                HeightRequest = size,
                Source = ImageSource.FromFile(coin ? "coin.png" : "arrows.png"),
            };
           
            int x = random.Next((int)(width - size + 1));
            int y = random.Next((int)(height - size + 1));

            pos = new Rect(x, y, size, size);
            if(mainL != null) {
                mainL.Add(box);
                mainL.SetLayoutBounds(box, pos);
                timer.Start();
                
                StartRotation();
            }
            
        }

        protected override void Update() {
            // See if the box has collided with the player
            if (playerref != null) {
                if (pos.IntersectsWith(playerref.Position)) {
                    box.CancelAnimations();
                    FullyRemove();
                    playerref.GotPickup(coin);
                }
            }
            counter += 20;
            //if it has been there for 8 seconds, remove it
            if(counter > 8000) {
                box.CancelAnimations();
                FullyRemove();
            }
        }

        public async Task StartRotation() {
            if (coin) {
                while (!await box.RotateYTo(360, 2000)) {
                    box.RotationY = 0;
                }
            }
            else {
                while (!await box.RotateXTo(360, 6000)) {
                    box.RotationX = 0;
                }
            }
        }


        protected override void FullyRemove() {
            mainDispatch?.Dispatch(() => { mainL?.Remove(box); });
            // Remove the timer's resources
            base.FullyRemove();
        }


    }

}

