using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Microsoft.Modeling;

namespace Robot
{
    public static class RobotModelProgram
    {
        public static Set<Robot> Instances = new Set<Robot>();

        static int maxNoOfRobots = 2;
        static Set<int> ids = new Set<int>();
        static int nextId = 1;

        public static Set<Robot> NextRobot()
        {
            string name = "Robot" + nextId;
            return new Set<Robot>(new Robot(name, nextId, 0, 0));
        }       

        [Rule]
        static void Start([Domain("NextRobot")]Robot robot)
        {            
            Condition.IsTrue(nextId <= maxNoOfRobots);
            nextId = nextId + 1;
            Instances = Instances.Add(robot);
        }

        [Rule]
        static void Search([Domain("Instances")]Robot robot)
        {
            Condition.IsTrue(robot.power > 30);
            robot.power = robot.power - 30;
            robot.reward = robot.reward + 2;
        }       

        [Rule]
        static void Wait([Domain("Instances")]Robot robot)
        {
            Condition.IsTrue(robot.power <= 50);
            robot.reward = robot.reward + 1;
        }        

        [Rule]
        static void Recharge([Domain("Instances")]Robot robot)
        {
            Condition.IsTrue(robot.power < 100);
            robot.power = 100;
        }

    }

    public struct Robot
    {
        public string name;
        public int id;
        public int power;
        public int reward;

        static Set<Robot> robots = new Set<Robot>();

        public Robot(string name, int id, int power, int reward)
        {
            this.id = id;
            this.power = power;
            this.reward = reward;
            this.name = name;
        }
    }
}
