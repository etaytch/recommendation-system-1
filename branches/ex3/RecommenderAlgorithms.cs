using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class RecommenderAlgorithms
    {
        private RecommenderSystem rs;
        private Dictionary<String, double> CPMemo;

        public RecommenderAlgorithms(RecommenderSystem rs)
        {
            this.rs = rs;
            this.CPMemo = null;
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
            while (cRecommendations > 0 && itemInd < sortedByRatingsItems.Count)
            {
                Item curr = sortedByRatingsItems[itemInd];
                Dictionary<string, Rating> ratings = curr.getDictionary();
                if (!hasUserSeenMovie(sUserId, ratings))
                {
                    lRecommendations.Add(curr.ItemID);
                    cRecommendations--;
                }
                itemInd++;
            }

            //Console.WriteLine("calculating popularity recs");
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
                if (!hasUserSeenMovie(sUserId, currentItem.Value.getDictionary()))
                {
                    itemsUserDidntRate.Add(currentItem.Key);
                }
            }

            foreach (string currentItem in itemsUserDidntRate)
            {
                prediction[currentItem] = rs.PredictRating(algorithm, sUserId, currentItem);
            }

            var items = from k in prediction.Keys
                        orderby prediction[k] descending
                        select k;

            for (int i = 0; i < cRecommendations; i++)
            {
                lRecommendations.Add(items.ElementAt(i));
            }

            return lRecommendations;
        }

        bool hasUserSeenMovie(string sUserId, Dictionary<string, Rating> ratings)
        {
            bool res = false;
            foreach (KeyValuePair<string, Rating> currentRating in ratings)
            {
                Rating r = currentRating.Value;
                if (r.userID.Equals(sUserId))
                {
                    res = true;
                }
            }
            return res;
        }



        public List<string> recommendWeights(string sUserId, int cRecommendations, string algorithm, Dictionary<string, User> usersToItems)
        {
            Dictionary<double, List<string>> envUsers = getUserEnvironment(algorithm, sUserId, usersToItems);
            Dictionary<string, double> weightedItems = getRecommendedItemByEnvironment(envUsers, usersToItems);

            //The result
            List<string> lRecommendations = new List<string>();

            var items = from k in weightedItems.Keys
                        orderby weightedItems[k] descending
                        select k;

            for (int i = 0; i < cRecommendations; i++)
            {
                lRecommendations.Add(items.ElementAt(i));
            }

            return lRecommendations;

        }

        public Dictionary<string, double> getRecommendedItemByEnvironment(Dictionary<double, List<string>> envUsers, Dictionary<string, User> usersToItems)
        {
            Dictionary<string, double> ans = new Dictionary<string, double>();
            foreach (KeyValuePair<double, List<string>> envUsersEntry in envUsers)
            {
                foreach (string user in envUsersEntry.Value)
                {
                    double currentW = envUsersEntry.Key;
                    Dictionary<string, Rating> ratings = usersToItems[user].getDictionary();
                    foreach (string tmpItem in ratings.Keys)
                    {
                        if (ans.ContainsKey(tmpItem))
                        {
                            ans[tmpItem] += currentW;
                        }
                        else
                        {
                            ans[tmpItem] = currentW;
                        }
                    }
                }
            }

            return ans;
        }

        public Dictionary<double, List<string>> getUserEnvironment(string sMethod, string sUID, Dictionary<string, User> usersToItems)
        {
            int k = 20;     // some random environment value
            Dictionary<double, List<string>> ans = new Dictionary<double, List<string>>();  // will store the answer
            Dictionary<double, List<string>> weightUsers = new Dictionary<double, List<string>>();  // maps calculated weight to list of users            

            // for each user who rated the item, calculate the Wau and store them in the weightUsers Dictionary            
            foreach (KeyValuePair<string, User> usersEntry in usersToItems)
            {
                string otherUserID = usersEntry.Key;
                if (sUID.Equals(otherUserID)) continue;
                double Wau = rs.getWeight(sMethod, sUID, otherUserID);
                if (weightUsers.ContainsKey(Wau))
                {
                    weightUsers[Wau].Add(otherUserID);
                }
                else
                {
                    weightUsers[Wau] = new List<string>();
                    weightUsers[Wau].Add(otherUserID);
                }
            }

            // sort the Weights list (We want to choose the highest K Weights)
            List<double> sortedWeights = new List<double>(weightUsers.Keys);
            sortedWeights.Sort();
            sortedWeights.Reverse();
            int i = 0;
            int wIndex = 0;
            while (i < k)
            {
                double currentW = sortedWeights.ElementAt(wIndex);
                foreach (string u in weightUsers[currentW])
                {
                    if (i >= k) break;
                    if (ans.ContainsKey(currentW))
                    {
                        ans[currentW].Add(u);
                    }
                    else
                    {
                        ans[currentW] = new List<string>();
                        ans[currentW].Add(u);
                    }
                    i++;
                }
                wIndex++;
            }
            return ans;
        }

        public List<string> recommendCP(string sUserId, int cRecommendations, Dictionary<string, User> usersToItems,
            Dictionary<string, Item> itemsToUsers)
        {
            //The result
            List<string> lRecommendations = new List<string>();

            Dictionary<String, double> probabilities = new Dictionary<String, double>();
            List<KeyValuePair<String,double>> sortedList = new List<KeyValuePair<String,double>>();

            User u = usersToItems[sUserId];
            Dictionary<String, Rating> ratings = u.getDictionary();
            List<string> itemsUserHasSeen = new List<string>(ratings.Keys);

            List<string> itemsUserDidntRate = this.getItemsUserDidntRate(itemsUserHasSeen, itemsToUsers);

            List<String> relevantUsers = this.getUsersThatRateItems(itemsUserHasSeen, usersToItems);

            //Setup memo
            if (CPMemo == null)
            {
                CPMemo = new Dictionary<string, double>();
                List<String> allItems = new List<String>(itemsToUsers.Keys);
                foreach (String item1 in allItems)
                    foreach (String item2 in allItems)
                        CPMemo[item1 + "," + item2] = -1;
            }

            //Console.WriteLine("Relevant users: " + relevantUsers.Count + " All: " + usersToItems.Count);
            //Console.WriteLine("Relevant items: " + itemsUserDidntRate.Count + " All: " + itemsToUsers.Count);

            foreach (String globalItem in itemsUserDidntRate)
            {
                double maxProbe = 0;
                foreach (String item in itemsUserHasSeen)
                {
                    if(CPMemo[globalItem+","+item] != -1) {
                        maxProbe = CPMemo[globalItem + "," + item];
                        break;
                    }
                    double countIJ = 0;
                    double countJ = 0;
                    foreach (String tmpUStr in relevantUsers)
                    {
                        User tmpU = usersToItems[tmpUStr];
                        Dictionary<String, Rating> tmpRatings = tmpU.getDictionary();
                        if (tmpRatings.ContainsKey(item))
                        {
                            countJ++;
                            if (tmpRatings.ContainsKey(globalItem))
                            {
                                countIJ++;
                            }
                        }

                    }

                    double tmpProbe = (double)countIJ / (double)countJ;
                    if (tmpProbe > maxProbe)
                    {
                        maxProbe = tmpProbe;
                        CPMemo[globalItem + "," + item] = maxProbe;
                    }
                }
                probabilities[globalItem] = maxProbe;
                //Console.WriteLine("prob for item " + globalItem + " is " + maxProbe);
            }

            foreach (KeyValuePair<string, Double> curProb in probabilities)
            {
                sortedList.Add(curProb);
            }
            sortedList.Sort(sortByProb);

            int itemInd = 0;
            //now take the top 5 items that the user havn't seen yet
            while (cRecommendations > 0 && itemInd < sortedList.Count)
            {
                KeyValuePair<string, Double> curr = sortedList[itemInd];
                lRecommendations.Add(curr.Key);
                cRecommendations--;
                itemInd++;
            }
            return lRecommendations;
        }

        private int sortByProb(KeyValuePair<string, Double> x, KeyValuePair<string, Double> y) {
            if (x.Value > y.Value) return -1;
            if (x.Value < y.Value) return 1;
            return 0;
        }

        //If the user has rated at least one of the items in itemsUserHasSeen add him to res
        private List<String> getUsersThatRateItems(List<String> itemsUserHasSeen, Dictionary<string, User> usersToItems)
        {
            List<String> res = new List<String>();

            foreach (String itemSeen in itemsUserHasSeen)
            {
                //Add all user that has seen this item without duplications
                foreach (KeyValuePair<string, User> currentU in usersToItems)
                {
                    User tmpU = currentU.Value;
                    Dictionary<String, Rating> tmpRatings = tmpU.getDictionary();
                    List<string> itemsUser2HasSeen = new List<string>(tmpRatings.Keys);
                    if (itemsUser2HasSeen.Contains(itemSeen) && !res.Contains(currentU.Key))
                    {
                        res.Add(currentU.Key);
                    }

                }
            }

            return res;
        }

        //If the user has rated at least one of the items in itemsUserHasSeen add him to res
        private List<String> getItemsUserDidntRate(List<String> itemsUserHasSeen, Dictionary<string, Item> itemsToUsers)
        {
            List<String> res = new List<String>();

            foreach (KeyValuePair<string, Item> currentItem in itemsToUsers)
            {
                String itemID = currentItem.Key;
                if(!itemsUserHasSeen.Contains(itemID)) {
                    res.Add(itemID);
                }
            }

            return res;
        }
    }
}
