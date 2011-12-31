using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class RecommenderAlgorithms
    {
        private RecommenderSystem rs;

        public RecommenderAlgorithms(RecommenderSystem rs) {
            this.rs = rs;
        }

        public List<string> recommendPopularity(string sUserId, int cRecommendations, Dictionary<string, Item> itemsToUsers)
        {
            //The result
            List<string> lRecommendations = new List<string>();

            List<Item> sortedByRatingsItems = new List<Item>();
            // the idea is to check all the items  and add them, then sort them
            foreach (KeyValuePair<string, Item> currentItem in itemsToUsers)
            {
                sortedByRatingsItems.Add(currentItem.Value);
            }

            sortedByRatingsItems.Sort(Item.CompareItemsByRaters);
            int itemInd = 0;
            //now take the top 5 items that the user havn't seen yet
            while(cRecommendations > 0 && itemInd < sortedByRatingsItems.Count) {
                Item curr = sortedByRatingsItems[itemInd];
                Dictionary<string, Rating> ratings = curr.getDictionary();
                if (!hasUserSeenMovie(sUserId, ratings)) {
                    lRecommendations.Add(curr.ItemID);
                    cRecommendations--;
                }
                itemInd++;
            }

            Console.WriteLine("calculating popularity recs");
            return lRecommendations;
        }

        public List<string> recommendPrediction(string sUserId, int cRecommendations, string algorithm, Dictionary<string, Item> itemsToUsers)
        {
            //The result
            List<string> lRecommendations = new List<string>();

            List<string> itemsUserDidntRate = new List<string>();
            Dictionary<string, double> prediction = new Dictionary<string, double>();

            foreach (KeyValuePair<string, Item> currentItem in itemsToUsers)
            {
                if(!hasUserSeenMovie(sUserId, currentItem.Value.getDictionary())) {
                    itemsUserDidntRate.Add(currentItem.Key);
                }
            }

            foreach (string currentItem in itemsUserDidntRate) {
                prediction[currentItem] = rs.PredictRating(algorithm, sUserId, currentItem);
            }

            var items = from k in prediction.Keys
                        orderby prediction[k] descending
                        select k;

            for (int i = 0; i < cRecommendations; i++) {
                lRecommendations.Add(items.ElementAt(i));
            }

                return lRecommendations;
        }

        bool hasUserSeenMovie(string sUserId, Dictionary<string, Rating> ratings) {
            bool res = false;
            foreach (KeyValuePair<string, Rating> currentRating in ratings) {
                Rating r = currentRating.Value;
                if (r.userID.Equals(sUserId))
                {
                    res = true;
                }
            }
            return res;
        }
    }
}
