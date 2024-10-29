using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace BachelorThesis.src
{
    public class MeanSquareError
    {
        public List<Vector2[]> positions;
        public List<Vector2[]> previousPositions;
        public WorldEntity[] entities;
        public const string PREVIOUS_POSITION_PATH = "positions_last_run.txt";

        public MeanSquareError(WorldEntity[] entities)
        {
            positions = new();
            previousPositions = new();
            this.entities = entities;
        }

        public void Update(GameTime gameTime)
        {
            Vector2[] currentPosition = new Vector2[entities.Length];
            for (int i = 0; i < currentPosition.Length; i++)
                currentPosition[i] = entities[i].Position;
            positions.Add(currentPosition);
        }

        public void LoadPreviousPositions()
        {
            string filePath = Directory.GetCurrentDirectory();
            string previousPositionPath = Path.GetFullPath(Path.Combine(filePath, PREVIOUS_POSITION_PATH));
            string[] lines;
            try
            {
                lines = File.ReadAllLines(previousPositionPath);
            }
            catch (Exception e) {lines = null;}

            if (lines != null)
            {
                foreach (String line in lines)
                {
                    String[] positionsText = line.Split(';');
                    Vector2[] positions = new Vector2[positionsText.Length];
                    for (int i = 0; i < positionsText.Length - 1; i++)
                    {
                        String[] xy = positionsText[i].Split(':');
                        positions[i] = new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
                    }
                    previousPositions.Add(positions);
                }
            }
        }

        public void Exit()
        {
            // Set a variable to the Documents path.
            string docPath =
              Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Write the previous times to file
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, PREVIOUS_POSITION_PATH)))
            {
                foreach (Vector2[] positionVector in positions)
                {
                    foreach (Vector2 position in positionVector)
                        outputFile.Write(position.X + ":" + position.Y + ";");
                    outputFile.WriteLine();
                }
            }
            // Write MSE between last two runs to file
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "MSE.txt")))
            {
                int min = Math.Min(previousPositions.Count, positions.Count);
                for(int i = 0; i<min; i++)
                {
                    double error = 0;
                    for(int j = 0; j<positions[i].Length; j++){
                        Vector2 position = positions[i][j];
                        Vector2 positionPrevious = previousPositions[i][j];
                        error += Math.Pow((position-positionPrevious).Length(), 2);
                        
                    }
                    error /= positions[i].Length;
                    outputFile.Write(error);
                    outputFile.WriteLine();
                    
                }

            }
        }
    }
}