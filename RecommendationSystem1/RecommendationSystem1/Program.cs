using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class Program
    {

        static void Main(string[] args)
        {
            RecommenderSystem rs = new RecommenderSystem();

            rs.Load("MovieLens/u.data", 0.9);

            List<string> lMethods = new List<string>();

            lMethods.Add("Pearson");

            lMethods.Add("Cosine");

            lMethods.Add("Random");

            DateTime dtStart = DateTime.Now;

            Dictionary<string, double> dResults = rs.ComputeRMSE(lMethods);

            foreach (KeyValuePair<string, double> p in dResults)

                Console.Write(p.Key + "=" + Math.Round(p.Value, 4) + ", ");

            Console.WriteLine();

            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));

            Console.ReadLine();

            /*
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("MovieLens/u.data");
            Console.WriteLine("True rating of user 6 to item 86 is " + rs.GetRating("6", "86"));
            Console.WriteLine(rs.PredictRating("Pearson", "100", "345"));
            Console.WriteLine(rs.PredictRating("Cosine", "100", "345"));
            Console.WriteLine(rs.PredictRating("Random", "100", "345"));
            
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
            */
        }
    }
}
