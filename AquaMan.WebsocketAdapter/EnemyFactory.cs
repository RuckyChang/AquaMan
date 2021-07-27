using AquaMan.WebsocketAdapter.Entity;
using System;
using System.Collections.Generic;

namespace AquaMan.WebsocketAdapter
{
    class EnemyFactory
    {

        private int width = 1680;
        private int height = 1024;

        private readonly Random _random = new Random();

        public List<Enemy> EnemyPool { get; private set; } = new List<Enemy>();
        public int EnemyCount
        {
            get
            {
                int count = 0;
                foreach(var enemy in EnemyPool)
                {
                    if (enemy.Active)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        private RespawnRegion _respawnRegion = new RespawnRegion(1680, 1024);
        public EnemyFactory()
        {

        }

        public List<Enemy> RespawnEnemies(int count)
        {
            List<Enemy> enemies = new List<Enemy>();

            for(int i = 0; i< count; i++)
            {
                enemies.Add(RespawnEnemy());
            }

            return enemies;
        }

        public Enemy RespawnEnemy()
        {
            Enemy enemy = GetFromPool();

            if(enemy != null)
            {
                var point = _respawnRegion.GetRanomRespawnPoint();

                enemy.Reset(
                    point,
                    patrolPoints: GenerateRandomPatrols(point, 3)
                    );
            }
            else
            {
                var point = _respawnRegion.GetRanomRespawnPoint();
                // create one.
                enemy = new Enemy(
                    point,
                    patrolPoints: GenerateRandomPatrols(point, 3)
                    );

                EnemyPool.Add(enemy);
            }
            return enemy;
        }

        private Enemy GetFromPool()
        {
            foreach(var enemy in EnemyPool)
            {
                if (!enemy.Active)
                {
                    return enemy;
                }
            }

            return null;
        }

        private List<PatrolPoint> GenerateRandomPatrols(Point respawnPoint, int count)
        {
            List<PatrolPoint> patrolPoints = new List<PatrolPoint>();
            
            for(int i = 0; i< count; i++)
            {
                // seconds
                int duration = _random.Next(3, 5);
                int speed = _random.Next(100, 150);
                if (i > 0)
                {
                    patrolPoints.Add(GenerateRandomPatrol(
                        patrolPoints[i-1].Coordinate.X,
                        patrolPoints[i-1].Coordinate.Y,
                        patrolPoints[i - 1].Timestamp,
                        speed,
                        patrolPoints[i-1].Timestamp + duration
                        ));
                }
                else
                {
                    patrolPoints.Add(GenerateRandomPatrol(
                        respawnPoint.X,
                        respawnPoint.Y,
                        0,
                        speed,
                        duration
                        ));
                }
            }

            
            patrolPoints.Add(GenerateTernimal(patrolPoints[patrolPoints.Count - 1]));

            return patrolPoints;
        }

        private PatrolPoint GenerateRandomPatrol(int x, int y, long previousTimestamp,int speed, long timestamp)
        {
            double randomAngle = _random.Next(0, 60);

            int distance = (int)(speed * (timestamp - previousTimestamp));
            int dx = (int)(Math.Cos(randomAngle) * distance);
            int dy = (int)(Math.Sin(randomAngle) * distance);

            Console.WriteLine($@"{x}, {y}, distance: {distance}, angle: {randomAngle} => {dx}, {dy}, speed: {speed},previous: {previousTimestamp}, timestamp: {timestamp}");

            return new PatrolPoint(
                   timestamp: timestamp,
                   coordinate: new Point(x+dx, y+dy)
                );
        }

        private PatrolPoint GenerateTernimal(PatrolPoint patrolPoint)
        {
            List<Point> edgeMiddlePoints = new List<Point>()
            {
                new Point(width/2, 0), // upper
                new Point(0, height/2), // left
                new Point(width, height/2), // right
                new Point(width/2, height), // down
            };

            int nearestIndex =  -1;
            int nearest = 0; ;

            for(int i = 0; i < edgeMiddlePoints.Count; i++)
            {
                Point point = edgeMiddlePoints[i];

                int t = getDistance(point.X, point.Y, patrolPoint.Coordinate.X, patrolPoint.Coordinate.Y);

                if ( nearest > t || i == 0)
                {
                    nearest = t;
                    nearestIndex = i;
                }
            }

            Point terminalPoint = _respawnRegion.GetRanomRespawnPoint(nearestIndex);
            int speed = _random.Next(100, 150);

            int distance = getDistance(terminalPoint.X, terminalPoint.Y, patrolPoint.Coordinate.X, patrolPoint.Coordinate.Y);
            int duration = distance / speed;

            return new PatrolPoint(timestamp: patrolPoint.Timestamp + duration,coordinate: terminalPoint);
        }

        private int getDistance(int x1, int y1, int x2, int y2)
        {
            int dx = x1 - x2;
            int dy = y1 - y2;
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
