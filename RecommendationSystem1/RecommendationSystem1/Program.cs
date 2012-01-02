using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RecommenderSystem
{
    class Program
    {
        

        static void Assignment1()
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("MovieLens/u.data");
            Console.WriteLine("True rating of user 6 to item 86 is " + rs.GetRating("6", "86"));
            Dictionary<double, int> dAllRatings = rs.GetRatingsHistogram("100");
            foreach (KeyValuePair<double, int> p in dAllRatings)
                Console.WriteLine(p.Key + "," + p.Value);
            Console.WriteLine("Predicted rating of user 6 to item 88 using Pearson Correlation is " + Math.Round(rs.PredictRating("Pearson", "6", "88"), 4));
            Console.WriteLine("Predicted rating of user 6 to item 88 using Cosine similarity is " + Math.Round(rs.PredictRating("Cosine", "6", "88"), 4));
            Dictionary<double, double> dAllPredictions = rs.PredictAllRatings("Pearson", "6", "88");
            Console.WriteLine("All predicted ratings of user 6 to item 88 using Pearson Correlation are:");
            foreach (KeyValuePair<double, double> p in dAllPredictions)
                Console.WriteLine(p.Key + "," + Math.Round(p.Value, 4));
            List<string> lMethods = new List<string>();
            lMethods.Add("Pearson");
            lMethods.Add("Cosine");
            lMethods.Add("Random");
            DateTime dtStart = DateTime.Now;
            Dictionary<string, double> dResults = rs.ComputeHitRatio(lMethods, 0.9);
            Console.WriteLine("Hit ratio scores for Pearson, Cosine, and Random are:");
            foreach (KeyValuePair<string, double> p in dResults)
                Console.Write(p.Key + "=" + Math.Round(p.Value, 4) + ", ");
            Console.WriteLine();
            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));
            Console.ReadLine();
        }
        static void Assignment2()
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("MovieLens/u.data", 0.9);
            rs.TrainBaseModel(10);
            Console.WriteLine("Predicted rating of user 6 to item 88 using SVD is " + Math.Round(rs.PredictRating("SVD", "6", "88"), 4));
            Dictionary<double, double> dAllPredictions = rs.PredictAllRatings("SVD", "6", "88");
            Console.WriteLine("All predicted ratings of user 6 to item 88 using SVD are:");
            foreach (KeyValuePair<double, double> p in dAllPredictions)
                Console.WriteLine(p.Key + "," + Math.Round(p.Value, 4));
            List<string> lMethods = new List<string>();
            lMethods.Add("Pearson");
            lMethods.Add("Cosine");
            lMethods.Add("SVD");
            lMethods.Add("Random");
            DateTime dtStart = DateTime.Now;
            Dictionary<string, Dictionary<string, double>> dConfidence = null;
            Dictionary<string, double> dResults = rs.ComputeHitRatio(lMethods, out dConfidence);
            Console.WriteLine("Hit ratio scores for Pearson, Cosine, SVD, and Random are:");
            foreach (KeyValuePair<string, double> p in dResults)
                Console.Write(p.Key + "=" + Math.Round(p.Value, 4) + ", ");
            Console.WriteLine("Confidence P-values are:");
            foreach (string sFirst in dConfidence.Keys)
                foreach (string sSecond in dConfidence[sFirst].Keys)
                    Console.WriteLine("p(" + sFirst + "=" + sSecond + ")=" + dConfidence[sFirst][sSecond].ToString("F3"));
            dResults = rs.ComputeRMSE(lMethods, out dConfidence);
            Console.WriteLine("RMSE scores for Pearson, Cosine, SVD, and Random are:");
            foreach (KeyValuePair<string, double> p in dResults)
                Console.Write(p.Key + "=" + Math.Round(p.Value, 4) + ", ");
            Console.WriteLine("Confidence P-values are:");
            foreach (string sFirst in dConfidence.Keys)
                foreach (string sSecond in dConfidence[sFirst].Keys)
                    Console.WriteLine("p(" + sFirst + "=" + sSecond + ")=" + dConfidence[sFirst][sSecond].ToString("F3"));
            Console.WriteLine();
            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));
            Console.ReadLine();
        }
        static void Assignment3()
        {
            RecommenderSystem rs = new RecommenderSystem();
            rs.Load("MovieLens/u.data", 0.9);
            rs.TrainBaseModel(10);

            //Dictionary<double, List<string>> env = rs.getUserEnvironment("Pearson", "6");

            List<string> lRecommendations = rs.Recommend("NNSVD", "6", 5);
            //List<string> lRecommendations = rs.Recommend("Popularity", "32", 5);
            Console.Write("Recommended movies for user 6 ");
            foreach(string sMovie in lRecommendations)
                Console.Write(sMovie + ",");
            Console.WriteLine();

            List<string> lMethods = new List<string>();
            lMethods.Add("Pearson");
            lMethods.Add("Cosine");
            lMethods.Add("SVD");
            lMethods.Add("NNPearson");
            lMethods.Add("NNCosine");
            lMethods.Add("NNSVD");
            lMethods.Add("CP");
            lMethods.Add("Popularity");
            List<int> lLengths = new List<int>();
            lLengths.Add(1);
            lLengths.Add(3);
            lLengths.Add(5);
            lLengths.Add(10);
            lLengths.Add(20);

            DateTime dtStart = DateTime.Now;
            Dictionary<string, Dictionary<string, double>> dConfidence = null;
            Dictionary<int,Dictionary<string,Dictionary<string, double>>> dResults = rs.ComputePrecisionRecall(lMethods, lLengths);
            Console.WriteLine("Precision-recall scores for all methods are:");
            foreach (int iLength in dResults.Keys)
            {
                foreach (string sMethod in dResults[iLength].Keys)
                {
                    foreach (string sMetric in dResults[iLength][sMethod].Keys)
                    {
                        Console.WriteLine(iLength + "," + sMethod + "," + sMetric + " = " + Math.Round(dResults[iLength][sMethod][sMetric], 4));
                    }
                }
            }
            Console.WriteLine("Execution time was " + Math.Round((DateTime.Now - dtStart).TotalSeconds, 0));
            Console.ReadLine();
        }


        static void Main(string[] args)
        {
            Assignment3();
        }
    }
}
