using System;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter.Entity
{
    public struct Point
    {
        public int X { get;  }
        public int Y { get;  }
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
    class Region
    {
        private readonly Random _random = new Random();
        public string Name { get; }
        public Point TopLeft { get;}
        public Point DownRight { get; }

        public Region (string name, Point topLeft, Point downRight)
        {
            Name = name;
            TopLeft = topLeft;
            DownRight = downRight;
        }

        public Point GetRandomPoint()
        {
            int x = _random.Next(TopLeft.X, DownRight.X);
            int y = _random.Next(TopLeft.Y, DownRight.Y);

            return new Point(x,y);
        }

    }
    class RespawnRegion
    {
        private readonly Random _random = new Random();

        public List<Region> Regions { get; private set; } = new List<Region>();
        private int thickness = 100;
        private int padding = 100;


        public RespawnRegion(
            int width,
            int height
            )
        {
            Regions.Add(new Region(
                name: "top",
                topLeft: new Point(0, 0 - thickness - padding),
                downRight: new Point(width, 0 - padding)
                ));
            Regions.Add(new Region(
                name: "left",
                topLeft: new Point(0 - thickness - padding, 0),
                downRight: new Point(-padding, height)
                ));
            Regions.Add(new Region(
                name: "right",
                topLeft: new Point(width + padding, 0),
                downRight: new Point(width + thickness + padding, height)
                ));
            Regions.Add(new Region(
                name: "bottom",
                topLeft: new Point(0, height + padding),
                downRight: new Point(width, height + thickness + padding)
                ));
        }

        public Point GetRanomRespawnPoint()
        {
            int index = _random.Next(0, 3);
            return Regions[index].GetRandomPoint();

            return Regions[1].GetRandomPoint();
        }

        public Point GetRanomRespawnPoint(int index)
        {
            //int index = _random.Next(0, 3);
            //return Regions[index].GetRandomPoint();

            return Regions[index].GetRandomPoint();
        }
    }
}
