namespace AquaMan.WebsocketAdapter.Entity
{
    public struct PatrolPoint
    {
        public long Timestamp { get;}
        public Point Coordinate { get;}

        public PatrolPoint(long timestamp, Point coordinate)
        {
            Timestamp = timestamp;
            Coordinate = coordinate;
        }
    }
}
