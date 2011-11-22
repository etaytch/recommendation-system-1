﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class RecommenderSystem
    {
        //Class members here (e.g. a dataset)
        private Dictionary<string, User> usersToItems;
        private Dictionary<string, Item> itemsToUsers;
        private Dictionary<string, double> usersWPearson;
        private Dictionary<string, double> usersWCosine;
        private List<string> usersIDs;

        public RecommenderSystem()
        {
            
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
            usersWPearson = new Dictionary<string, double>();
            usersWCosine = new Dictionary<string, double>();
            usersIDs = new List<string>();
       }

        //load a dataset from a file
        public void Load(string sFileName)
        {
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader("ml-100k\\u.data");
            while ((line = file.ReadLine()) != null)
            {
                string[] words = line.Split('	');
                Rating r = new Rating(words[0], words[1], Convert.ToInt32(words[2]), words[3]);

                User u;
                if (usersToItems.ContainsKey(words[0])) // True
                {
                    //User exists
                    u = usersToItems[words[0]];
                    u.addRating(r);               
                } else
                {
                    //Create new user rating object
                    u = new User();
                    u.addRating(r);
                    usersToItems[words[0]] = u;
                    usersIDs.Add(words[0]);
                }

                Item i;
                if (itemsToUsers.ContainsKey(words[1])) // True
                {
                    //Item exists
                    i = itemsToUsers[words[1]];
                    i.addRating(r);
                }
                else
                {
                    //Create new user rating object
                    i = new Item();
                    i.addRating(r);
                    itemsToUsers[words[1]] = i;
                }
        
            }
            file.Close();
            this.calculateAvgRatings();
        }
        //return an existing rating 
        public double GetRating(string sUID, string sIID)
        {
            User u = usersToItems[sUID];
            return u.getRating(sIID).rating;
        }
        //return an histogram of all ratings that the user has used
        public Dictionary<double, int> GetRatingsHistogram(string sUID)
        {
            Dictionary<double, int> hist = new Dictionary<double, int>();
            User user = usersToItems[sUID];
            foreach (KeyValuePair<string, Rating> entry in user.getDictionary()) {
                if (hist.ContainsKey(entry.Value.rating)) {
                    hist[entry.Value.rating]++;
                }
                else {
                    hist[entry.Value.rating]=1;
                }
            }
            return hist;
        }
        //predict the rating that a user will give to an item using one of the methods "Pearson", "Cosine", "Random"
        public double PredictRating(string sMethod, string sUID, string sIID)
        {
            double res = -1;
            if (sMethod.Equals("Pearson")) {
                res = pearson(sUID, sIID);
            }
            else if (sMethod.Equals("Cosine")) {
                res = cosine(sUID, sIID);
            }
            return res;
        }
        //return the predicted weights of all ratings that a user may give an item using one of the methods "Pearson", "Cosine", "Random"
        public Dictionary<double, double> PredictAllRatings(string sMethod, string sUID, string sIID)
        {
            throw new NotImplementedException();
        }
        //Compute the hit ratio of all the methods in the list for a given train-test split (e.g. 0.95 train set size)
        public Dictionary<string,double> ComputeHitRatio(List<string> lMethods, double dTrainSetSize)
        {
            throw new NotImplementedException();
        }



        // Gets PearsonWeight for Active user and Other user
        private double getPearsonWeight(string activeUID, string otherUID) {
            if (usersWPearson.ContainsKey(activeUID + "," + otherUID)) {
                return usersWPearson[activeUID + "," + otherUID];
            }
            if (usersWPearson.ContainsKey(otherUID + "," + activeUID)) {
                return usersWPearson[otherUID + "," + activeUID];
            }
            else return calcPearsonWeight(activeUID, otherUID);
        }

        // Calculates PearsonWeight for Active user and Other user
        private double calcPearsonWeight(string activeUID, string otherUID) {
            
            Dictionary<string, Rating> activeItems = usersToItems[activeUID].getDictionary();
            Dictionary<string, Rating> otherItems = usersToItems[otherUID].getDictionary();
            double activeAverageRating = usersToItems[activeUID].getAverageRating();
            double otherAverageRating = usersToItems[otherUID].getAverageRating();

            double numerator = 0.0;
            double denomanator1 = 0.0;
            double denomanator2 = 0.0;
            double tmpNumerator1 = 0.0;
            double tmpNumerator2 = 0.0;

            foreach (KeyValuePair<string, Rating> activeItemEntry in activeItems) {
                String itemID = activeItemEntry.Key;                                
                if (otherItems.ContainsKey(itemID)) {
                    double Rai = activeItems[itemID].rating;
                    double Rui = otherItems[itemID].rating;
                    tmpNumerator1 = (Rai - activeAverageRating);
                    tmpNumerator2 = (Rui - otherAverageRating);
                    numerator += tmpNumerator1 * tmpNumerator2;
                    denomanator1 += Math.Pow(tmpNumerator1, 2);
                    denomanator2 += Math.Pow(tmpNumerator2, 2);
                }
            }
            double wau;
            if ((denomanator1 == 0.0) || (denomanator2 == 0.0)) {
                wau = 0.0;
            }
            else {
                wau = (numerator / (Math.Sqrt(denomanator1) * Math.Sqrt(denomanator2)));
            }
            usersWPearson[activeUID + "," + otherUID] = wau;
            usersWPearson[otherUID + "," + activeUID] = wau;
            return wau;
        }

        // Calculate Pearson correlation for active user and item 
        private double pearson(string sUID, string sII) {
            double ans=0.0;
            double numerator = 0.0;
            double denomanator = 0.0;
            Dictionary<string, Rating> currentItemUsers = itemsToUsers[sII].getDictionary();
            foreach (KeyValuePair<string, Rating> userEntry in currentItemUsers) {
                if (sUID.Equals(userEntry.Key)) continue;
                double Wau = getPearsonWeight(sUID, userEntry.Key);
                numerator += Wau * currentItemUsers[userEntry.Key].rating;
                denomanator += Wau;
            }

            if (denomanator==0.0) {
                return 0.0;
            }
            return numerator / denomanator;        
        }

        // Calculate Cosine correlation for active user and item
        private double cosine(string sUID, string sII) {
            double ans = 0.0;
            double numerator = 0.0;
            double denomanator = 0.0;
            Dictionary<string, Rating> currentItemUsers = itemsToUsers[sII].getDictionary();
            foreach (KeyValuePair<string, Rating> userEntry in currentItemUsers) {
                if (sUID.Equals(userEntry.Key)) continue;
                double Wau = getCosineWeight(sUID, userEntry.Key);
                numerator += Wau * currentItemUsers[userEntry.Key].rating;
                denomanator += Wau;
            }

            if (denomanator == 0.0) {
                return 0.0;
            }
            return numerator / denomanator;
        }

        // Gets CosineWeight for Active user and Other user
        private double getCosineWeight(string activeUID, string otherUID) {
            if (usersWCosine.ContainsKey(activeUID + "," + otherUID)) {
                return usersWCosine[activeUID + "," + otherUID];
            }
            if (usersWCosine.ContainsKey(otherUID + "," + activeUID)) {
                return usersWCosine[otherUID + "," + activeUID];
            }
            else return calcCosineWeight(activeUID, otherUID);
        }

        // Calculates CosineWeight for Active user and Other user
        private double calcCosineWeight(string activeUID, string otherUID) {
            Dictionary<string, Rating> activeItems = usersToItems[activeUID].getDictionary();
            Dictionary<string, Rating> otherItems = usersToItems[otherUID].getDictionary();
            double activeAverageRating = usersToItems[activeUID].getAverageRating();
            double otherAverageRating = usersToItems[otherUID].getAverageRating();

            double numerator = 0.0;
            double denomanator1 = 0.0;
            double denomanator2 = 0.0;
            double tmpNumerator1 = 0.0;
            double tmpNumerator2 = 0.0;

            foreach (KeyValuePair<string, Rating> activeItemEntry in activeItems) {
                String itemID = activeItemEntry.Key;
                double Rai = activeItems[itemID].rating;
                tmpNumerator1 = (Rai - activeAverageRating);
                double Rui=0.0;
                if (otherItems.ContainsKey(itemID)) {                    
                    Rui = otherItems[itemID].rating;                    
                }
                tmpNumerator2 = (Rui - otherAverageRating);
                numerator += tmpNumerator1 * tmpNumerator2;
                denomanator1 += Math.Pow(tmpNumerator1, 2);
                denomanator2 += Math.Pow(tmpNumerator2, 2);                
            }

            foreach (KeyValuePair<string, Rating> otherItemEntry in otherItems) {
                String itemID = otherItemEntry.Key;
                if (!activeItems.ContainsKey(itemID)) {
                    double Rai = 0.0;
                    tmpNumerator1 = (Rai - activeAverageRating);
                    double Rui = otherItems[itemID].rating;                    
                    tmpNumerator2 = (Rui - otherAverageRating);
                    numerator += tmpNumerator1 * tmpNumerator2;
                    denomanator1 += Math.Pow(tmpNumerator1, 2);
                    denomanator2 += Math.Pow(tmpNumerator2, 2);                
                }                
            }

            double wau;
            if ((denomanator1 == 0.0) || (denomanator2 == 0.0)) {
                wau = 0.0;
            }
            else {
                wau = (numerator / (Math.Sqrt(denomanator1) * Math.Sqrt(denomanator2)));
            }
            usersWCosine[activeUID + "," + otherUID] = wau;
            usersWCosine[otherUID + "," + activeUID] = wau;
            return wau;
        }

        private void calculateAvgRatings() 
        {
            foreach (KeyValuePair<string, User> userEntry in usersToItems)
            {
                if (userEntry.Value.getAverageRating() != -1) continue;
                double mone = 0.0;
                int counter = 0;
                Dictionary<double, int> hist = GetRatingsHistogram(userEntry.Key);
                foreach (KeyValuePair<double, int> histEntry in hist)
                {
                    mone += histEntry.Key * histEntry.Value;
                    counter += histEntry.Value;
                }
                userEntry.Value.setAverageRating((mone / counter));
            }
        }

        /*
         private double pearson(string sUID, string sII)
        {
            double numerator = 0;
            double denomanator1 = 0;
            double denomanator2 = 0;
            //Pearson here
            //calculate w's
            //<itemID, Rating>
            Dictionary<string, Rating> userItems = usersToItems[sUID].getDictionary();

            foreach (KeyValuePair<string, Rating> itemEntry in userItems)
            {
                //rating of a for this item
                Rating ratingA = userItems[itemEntry.Key];

                //itemEntry is a specific item of user sUID and we want to know which other
                //users rated this item
                //<userID, Rating>
                Dictionary<string, Rating> usersOfItem = itemsToUsers[itemEntry.Key].getDictionary();

                foreach (KeyValuePair<string, Rating> ratingEntry in usersOfItem)
                {
                    if (ratingEntry.Key.Equals(sUID)) continue;

                    double tmpNumerator1 = (ratingA.rating - usersToItems[sUID].getAverageRating());
                    double tmpNumerator2 = (ratingEntry.Value.rating - usersToItems[ratingEntry.Key].getAverageRating());
                    numerator += tmpNumerator1 * tmpNumerator2;
                    denomanator1 += Math.Pow(tmpNumerator1, 2);
                    denomanator2 += Math.Pow(tmpNumerator2, 2);
                }    
            }

            return (numerator / (Math.Sqrt(denomanator1)*Math.Sqrt(denomanator2)));
        }
         
         */

    }
}
