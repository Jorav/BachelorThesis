using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace BachelorThesis.src
{
    public class PerformanceMeasurer
    {
        public List<decimal> elapsedTimes;
        Stopwatch timer;
        int state = -1; //0=build, 1=CD, 2=CH, 3=other
        public List<decimal> buildTimes;
        public List<decimal> CDTimes;
        public List<decimal> CHTimes;
        public List<decimal> otherTimes;
        public List<decimal>[] timesByCategory;

        public PerformanceMeasurer()
        {
            elapsedTimes = new List<decimal>();
            timer = new();
            buildTimes = new();
            CDTimes = new();
            CHTimes = new();
            otherTimes = new();
            timesByCategory = new List<decimal>[4];
            timesByCategory[0] = buildTimes;
            timesByCategory[1] = CDTimes;
            timesByCategory[2] = CHTimes;
            timesByCategory[3] = otherTimes;
        }

        public void Update(GameTime gameTime)
        {
            elapsedTimes.Add((decimal)gameTime.ElapsedGameTime.TotalSeconds);
        }

        public void Tick()
        {
            if (state == -1){
                state = 0;
                timer.Start();
            }
            else
            {
                timesByCategory[state].Add((decimal)timer.Elapsed.TotalSeconds);
                state = (state + 1) % 4;
                timer.Restart();
            }
        }
        public void SubmitTimes(String fileName){
            string docPath =
              Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            /*// Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "elapsedTimes.txt")))
            {
                foreach (double time in elapsedTimes)
                    outputFile.WriteLine(time.ToString());
            }*/

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, fileName)))
            {
                int nrLines = Math.Min(Math.Min(buildTimes.Count, CDTimes.Count), Math.Min(otherTimes.Count, CHTimes.Count));
                for(int i = 0; i<nrLines; i++)
                {
                    outputFile.WriteLine(buildTimes[i].ToString() + ";" + CDTimes[i].ToString() + ";" + CHTimes[i].ToString() + ";" + otherTimes[i].ToString());
                }
            }
            timer.Reset();
            state = -1;
            buildTimes.Clear();
            CDTimes.Clear();
            CHTimes.Clear();
            otherTimes.Clear();
        }

        public void Exit()
        {
            // Set a variable to the Documents path.
            
        }
    }
}
