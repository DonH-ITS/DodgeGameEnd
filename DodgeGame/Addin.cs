namespace DodgeGame
{
    public class Addin
    {
        protected static double size;
        protected static double height;
        protected static double width;
        protected static Player? playerref;
        protected static AbsoluteLayout? mainL;
        protected static IDispatcher? mainDispatch;
        protected static readonly Random random = new();

        protected System.Timers.Timer? timer;
        public EventHandler? Remove;

        public static void RemoveMainLayoutRef() {
            mainL = null;
        }

        public Addin() {
            timer = new System.Timers.Timer
            {
                Interval = 20
            };
            timer.Elapsed += (s, e) =>
            {
                Update();
            };
        }

        protected virtual void Update() {

        }

        // These fields are shared with all Enemy objects so they should be static
        // The mainLayout is needed to know where to add the enemy
        // Dispatcher is needed to sometimes draw on the Main UI Thread
        // A Player object reference is needed to find out if enemy collides with player
        public static void SetStaticProperties(double s, Player player, AbsoluteLayout view, IDispatcher dispatcher) {
            size = s; playerref = player; mainL = view; mainDispatch = dispatcher;
            height = mainL.Height;
            width = mainL.Width;
        }

        protected virtual void FullyRemove() {
            timer.Stop();
            timer.Dispose();
            Remove?.Invoke(this, null);
        }
    }
}
