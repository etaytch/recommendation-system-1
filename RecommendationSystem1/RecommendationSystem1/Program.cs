using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class Program
    {
        static void Main(string[] args) {
            string[] lines = System.IO.File.ReadAllLines(@"ml-100k\u.data");
             

            // Display the file contents by using a foreach loop.
            System.Console.WriteLine("Contents of WriteLines2.txt = ");
            foreach (string line in lines) {
                // Use a tab to indent each line of the file.
                string[] words = line.Split('	');
                foreach (string word in words) {
                    Console.WriteLine(word);
                }
                Console.WriteLine("words's size: "+words.Length+"\nwords: " + words[0]);
                System.Console.ReadKey();
            }

            // Keep the console window open in debug mode.
            Console.WriteLine("Press any key to exit.");
            
        }

        static void Main1(string[] args)
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("MovieLens/u.data");
            Console.WriteLine("True rating of user 6 to item 86 is " + rs.GetRating("6", "86"));
            Dictionary<double, int> dAllRatings = rs.GetRatingsHistogram("100");
            foreach (KeyValuePair<double, int> p in dAllRatings)
                Console.WriteLine(p.Key + "," + p.Value);
            Console.WriteLine("Predicted rating of user 6 to item 88 using Pearson Correlation is " + rs.PredictRating("Pearson", "6", "88"));
            Console.WriteLine("Predicted rating of user 6 to item 88 using Cosine similarity is " + rs.PredictRating("Cosine", "6", "88"));
            Dictionary<double, double> dAllPredictions = rs.PredictAllRatings("Pearson", "6", "88");
            Console.WriteLine("All predicted ratings of user 6 to item 88 using Pearson Correlation are:");
            foreach (KeyValuePair<double, double> p in dAllPredictions)
                Console.WriteLine(p.Key + "," + Math.Round(p.Value, 4));
            List<string> lMethods = new List<string>();
            lMethods.Add("Pearson");
            lMethods.Add("Cosine");
            lMethods.Add("Random");
            Dictionary<string, double> dResults = rs.ComputeHitRatio(lMethods, 0.95);
            Console.WriteLine("Hit ratio scores for Pearson, Cosine, and Random are:");
            foreach (KeyValuePair<string, double> p in dResults)
                Console.Write(p.Key + "=" + Math.Round(p.Value,4) + ", ");
            Console.WriteLine();
            Console.ReadLine();
        }
    }
}
