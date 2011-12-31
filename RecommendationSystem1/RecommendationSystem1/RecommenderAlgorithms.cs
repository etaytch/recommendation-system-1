using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class RecommenderAlgorithms
    {
        public RecommenderAlgorithms() {
        }

        public List<string> recommendPopularity(string sUserId, int cRecommendations, Dictionary<string, Item> itemsToUsers)
        {
            //The result
            List<string> lRecommendations = new List<string>();

            List<Item> sortedByRatingsItems = new List<Item>();
            // the idea is to run on all the items possible and add them sorted to the List
            foreach (KeyValuePair<string, Item> currentItem in itemsToUsers)
            {
                sortedByRatingsItems.Add(currentItem.Value);
            }

            sortedByRatingsItems.Sort(Item.CompareItemsByRaters);
            int itemInd = 0;
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
