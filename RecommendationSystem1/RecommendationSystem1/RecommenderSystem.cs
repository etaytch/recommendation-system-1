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
        private Dictionary<string, VectorDO> usersVector;
        private Dictionary<string, VectorDO> itemsVector;
        private Dictionary<string, Item> itemsToUsers;
        private Db test;
        private Db train;
        private Db validation;
        private Dictionary<string, double> usersWPearson;
        private Dictionary<string, double> usersWCosine;
        private List<string> usersIDs;
        private int numOfRecords;

        private const double limitEnvironmentPearson = -0.3;
        private const double limitEnvironmentCosine = 0.0;

        public RecommenderSystem()
        {
            
            usersToItems = new Dictionary<string, User>();
            usersVector = new Dictionary<string, VectorDO>();            
            itemsToUsers = new Dictionary<string, Item>();
            itemsVector = new Dictionary<string, VectorDO>();
            usersWPearson = new Dictionary<string, double>();
            usersWCosine = new Dictionary<string, double>();
            usersIDs = new List<string>();
            test = new Db();
            train = new Db();
            numOfRecords = 0;
       }

        //load a dataset from a file
        public void Load(string sFileName, double dTrainSetSize)
        {
            string line;

            // Read the file and display it line by line.
            System.IO.StreamReader file =
               new System.IO.StreamReader("ml-100k\\u.data");
            while ((line = file.ReadLine()) != null)
            {
                numOfRecords++;
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

            //Split the database
            splitDB(dTrainSetSize); //First splitting the database
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
            else if (sMethod.Equals("Cosine"))
            {
                res = cosine(sUID, sIID);
            }
            else if (sMethod.Equals("Random"))
            {
                res = generateRandomRating(sUID);
            }
            else if (sMethod.Equals("SVD")) 
            {                
                res = getMu(usersToItems) + usersVector[sUID].getVal() + itemsVector[sUID].getVal() + multVectors(itemsVector[sIID].getVec(), usersVector[sUID].getVec());
            }
            return res;
        }
        //return the predicted weights of all ratings that a user may give an item using one of the methods "Pearson", "Cosine", "Random"
        public Dictionary<double, double> PredictAllRatings(string sMethod, string sUID, string sIID)
        {
            int k = 500;     // some random environment value
            Dictionary<double, double> ans = new Dictionary<double, double>();  // will store the answer
            Dictionary<double, List<string>> weightUsers = new Dictionary<double, List<string>>();  // maps calculated weight to list of users
            Item item = itemsToUsers[sIID];     // given Item
            Dictionary<string, Rating> usersRating = item.getDictionary();      // users who rates the Item

            // for each user who rated the item, calculate the Wau and store them in the weightUsers Dictionary
            foreach (KeyValuePair<string, Rating> usersRatingEntry in usersRating) {    
                string otherUserID = usersRatingEntry.Key;
                double tmpRating = usersRatingEntry.Value.rating;
                double Wau = getWeight(sMethod, sUID, usersRatingEntry.Key);
                if (weightUsers.ContainsKey(Wau)) {
                    weightUsers[Wau].Add(otherUserID);
                }
                else {
                    weightUsers[Wau] = new List<string>();
                    weightUsers[Wau].Add(otherUserID);
                }               
            }

            // sort the Weights list (We want to choose the highest K Weights)
            List<double> sortedWeights = new List<double>(weightUsers.Keys);
            sortedWeights.Sort();
            sortedWeights.Reverse();

            int i = 0;
            // for each Weight - store it in the Dictionary
            foreach (double w in sortedWeights) {
                if (i > k) break;
                foreach(string u in weightUsers[w]){
                    if (i > k) break;
                    double tmpRating = usersRating[u].rating;
                    if (ans.ContainsKey(tmpRating)) {
                        ans[tmpRating] += w;
                    }
                    else ans[tmpRating] = w;
                    i++;
                }                
            }

            return ans;
        }


        //Compute the hit ratio of all the methods in the list for a given train-test split (e.g. 0.95 train set size)
        public Dictionary<string,double> ComputeHitRatio(List<string> lMethods)
        {
            Dictionary<string, double> res = new Dictionary<string, double>(); //the result will be stored here

            Dictionary<string, User> users = test.UsersToItems; //All the users in the test

            int pearsonHits = 0;
            int cosineHits = 0;
            int randomHits = 0;
            int totalRatingsCount = 0;

            //Iterates on all the users, and then for each user iterates on all his ratings.
            //then evaluates the predicted rating for each method in lMethod.
            foreach (KeyValuePair<string, User> userEntry in users)
            {
                Dictionary<string, Rating> itemsOfUser = userEntry.Value.getDictionary();
                foreach (KeyValuePair<string, Rating> itemEntry in itemsOfUser)
                {

                    totalRatingsCount++;

                    if (lMethods.Contains("Pearson"))
                    {
                        int pearsonPred = 0;
                        pearsonPred = (int)Math.Round(PredictRating("Pearson", userEntry.Key, itemEntry.Key));
                        if (pearsonPred == itemEntry.Value.rating)
                        {
                            pearsonHits++;
                        }
                    }

                    if (lMethods.Contains("Cosine"))
                    {
                        int cosinePred = 0;
                        cosinePred = (int)Math.Round(PredictRating("Cosine", userEntry.Key, itemEntry.Key));
                        if (cosinePred == itemEntry.Value.rating)
                        {
                            cosineHits++;
                        }
                    }

                    if (lMethods.Contains("Random"))
                    {
                        double randomPred = 0;
                        randomPred = (int)Math.Round(PredictRating("Random", userEntry.Key, itemEntry.Key));
                        if (randomPred == itemEntry.Value.rating)
                        {
                            randomHits++;
                        }
                    }
                }
            }

            if(lMethods.Contains("Pearson"))
            {
                res["Pearson"] = (double)pearsonHits / totalRatingsCount;
            }
            if (lMethods.Contains("Cosine"))
            {
                res["Cosine"] = (double)cosineHits / totalRatingsCount;
            }
            if (lMethods.Contains("Random"))
            {
                res["Random"] = (double)randomHits / totalRatingsCount;
            }
            return res;
        }

        //Calculates the average rating of every user and updates it.
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

        private double getWeight(string method,string activeUID, string otherUID) {
            if (method.Equals("Pearson")) return getPearsonWeight(activeUID, otherUID);
            else if (method.Equals("Cosine")) return getCosineWeight(activeUID, otherUID);
            else return 0.0;
        }

        // Gets PearsonWeight for Active user and Other user
        private double getPearsonWeight(string activeUID, string otherUID) {
            if (usersWPearson.ContainsKey(activeUID + "," + otherUID)) {
                return usersWPearson[activeUID + "," + otherUID];
            }
            if (usersWPearson.ContainsKey(otherUID + "," + activeUID)) {
                return usersWPearson[otherUID + "," + activeUID];
            }
            // if the weigth has not been calculated before - calculate it!
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


            // iterates through Active's and Other's shared items. 
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
            // avoids division by zero
            if ((denomanator1 == 0.0) || (denomanator2 == 0.0)) {
                wau = 0.0;
            }
            else {
                wau = (numerator / (Math.Sqrt(denomanator1) * Math.Sqrt(denomanator2)));
            }

            // updates dictionaries so next time this calculation will be avoided
            usersWPearson[activeUID + "," + otherUID] = wau;
            usersWPearson[otherUID + "," + activeUID] = wau;
            return wau;
        }

        // Calculate Pearson correlation for active user and item 
        private double pearson(string sUID, string sII) {
            double numerator = 0.0;
            double denomanator = 0.0;
            Dictionary<string, Rating> currentItemUsers = itemsToUsers[sII].getDictionary();
            foreach (KeyValuePair<string, Rating> userEntry in currentItemUsers) {
                if (sUID.Equals(userEntry.Key)) continue;
                double Wau = getPearsonWeight(sUID, userEntry.Key);
                if (Wau < limitEnvironmentPearson) continue;
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
            double numerator = 0.0;
            double denomanator = 0.0;
            Dictionary<string, Rating> currentItemUsers = itemsToUsers[sII].getDictionary();
            foreach (KeyValuePair<string, Rating> userEntry in currentItemUsers) {
                if (sUID.Equals(userEntry.Key)) continue;
                double Wau = getCosineWeight(sUID, userEntry.Key);
                //Console.Write(Wau + " ");
                if (Wau < limitEnvironmentCosine) continue;
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
            // if the weigth has not been calculated before - calculate it!
            else return calcCosineWeightSecondFormula(activeUID, otherUID);
        }

        // Calculates CosineWeight for Active user and Other user with the first Formula
        private double calcCosineWeightFirstFormula(string activeUID, string otherUID)
        {

            Dictionary<string, Rating> activeItems = usersToItems[activeUID].getDictionary();
            Dictionary<string, Rating> otherItems = usersToItems[otherUID].getDictionary();
            double activeAverageRating = usersToItems[activeUID].getAverageRating();
            double otherAverageRating = usersToItems[otherUID].getAverageRating();

            double numerator = 0.0;
            double denomanator1 = 0.0;
            double denomanator2 = 0.0;
            double tmpNumerator = 0.0;


            // iterates through Active's and Other's shared items. 
            foreach (KeyValuePair<string, Rating> activeItemEntry in activeItems)
            {
                String itemID = activeItemEntry.Key;
                if (otherItems.ContainsKey(itemID))
                {
                    double Rai = activeItems[itemID].rating;
                    double Rui = otherItems[itemID].rating;
                    tmpNumerator = Rai * Rui;
                    numerator += tmpNumerator;
                    denomanator1 += Math.Pow(tmpNumerator, 2);
                    denomanator2 += Math.Pow(tmpNumerator, 2);
                }
            }
            double wau;
            // avoids division by zero
            if ((denomanator1 == 0.0) || (denomanator2 == 0.0))
            {
                wau = 0.0;
            }
            else
            {
                wau = (numerator / (Math.Sqrt(denomanator1) * Math.Sqrt(denomanator2)));
            }

            // updates dictionaries so next time this calculation will be avoided
            usersWCosine[activeUID + "," + otherUID] = wau;
            usersWCosine[otherUID + "," + activeUID] = wau;
            return wau;
        }

        // Calculates CosineWeight for Active user and Other user with the second formula
        private double calcCosineWeightSecondFormula(string activeUID, string otherUID) {
            Dictionary<string, Rating> activeItems = usersToItems[activeUID].getDictionary();
            Dictionary<string, Rating> otherItems = usersToItems[otherUID].getDictionary();
            double activeAverageRating = usersToItems[activeUID].getAverageRating();
            double otherAverageRating = usersToItems[otherUID].getAverageRating();

            double numerator = 0.0;
            double denomanator1 = 0.0;
            double denomanator2 = 0.0;
            double tmpNumerator1 = 0.0;
            double tmpNumerator2 = 0.0;

            // iterates through Active's items. If the other user did not rates this item, it get 0 value.
            // this iteration covers all the item the either boths Active and Other user rated, and those 
            // only the Active rated
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

            // iterates through Other's items. Calculate only the items that the Active user DID NOT rate.
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
            // avoids division by zero
            if ((denomanator1 == 0.0) || (denomanator2 == 0.0)) {
                wau = 0.0;
            }
            else {
                wau = (numerator / (Math.Sqrt(denomanator1) * Math.Sqrt(denomanator2)));
            }

            // updates dictionaries so next time this calculation will be avoided
            usersWCosine[activeUID + "," + otherUID] = wau;
            usersWCosine[otherUID + "," + activeUID] = wau;
            return wau;
        }

        //Returns a random rating (that the user already voted) with the proper probability
        private double generateRandomRating(string UID)
        {
            double res = 0;
            double totalRates = 0;
            Dictionary<double, int> userRates = GetRatingsHistogram(UID);
            foreach (KeyValuePair<double, int> currentRate in userRates)
            {
                totalRates += currentRate.Value;
            }

            //generate random number in [1,totalRates]
            Random random = new Random();
            double rndm = random.NextDouble()*(totalRates-1)+1;

            //Loop until rndm becomes negative then return the current rate
            foreach (KeyValuePair<double, int> currentRate in userRates)
            {
                rndm -= currentRate.Value;
                if (rndm <= 0) {
                    res = currentRate.Key;
                    break;
                }
            }

            return res;
        }

        private void splitDB(double p) {
            test = new Db();
            train = new Db();
            List<string> users = new List<string>(usersToItems.Keys);   // a copy of users ids            
            int amountOfTestRecords = (int)((1 - p) * numOfRecords);    // size of test Db
            int countTestRecords = 0;                                   // counter for test Db
            Random ran = new Random();
            int ratingCounter = 0;

            //We assumed that on low values of P, the test size would not necessarily be the declared "amountOfTestRecords"
            //because k is chosen randomally and we can't control it
            while (countTestRecords < amountOfTestRecords && users.Count > 0) {
                // randomize the next user
                int nextUser = ran.Next(0, users.Count);        
                string currentUserID = users.ElementAt(nextUser);
                User currentUser = usersToItems[currentUserID];
                Dictionary<string, Rating> currentUserRatings = currentUser.getDictionary();

                train.addUser(currentUserID);
                test.addUser(currentUserID);    // is it necessary when all user's rating are at the train?


                List<string> itemsIDs = new List<string>(currentUserRatings.Keys);        // copy of items IDs

                int amountOfItems = currentUser.getDictionary().Count;                    // amount of items

                int k = ran.Next(0, amountOfItems);                                       // randomize amount of items
                k = Math.Min(k, (amountOfTestRecords - countTestRecords));                // to put in the train

                // for each item - add it to the train and remove from temp item list
                for (int i = 0; i < k;i++ ) {
                    int nextItem = ran.Next(0, itemsIDs.Count);
                    string nextItemID = itemsIDs.ElementAt(nextItem);
                    train.addRating(currentUserID, currentUserRatings[nextItemID]);
                    itemsIDs.RemoveAt(nextItem);
                    ratingCounter++;
                }                

                // for all other items - add to test
                foreach (string restItems in itemsIDs) {
                    test.addRating(currentUserID, currentUserRatings[restItems]);
                    ratingCounter++;
                }
                countTestRecords += itemsIDs.Count;
                
                // remove user from temp list
                users.RemoveAt(nextUser);                
            }

            // for all the other users (those who not yet been selected because the test is full): add each user's rating to the train
            foreach (string uID in users) {
                train.addUser(uID);
                User currentUser = usersToItems[uID];
                Dictionary<string, Rating> currentUserRatings = currentUser.getDictionary();
                foreach (KeyValuePair<string, Rating> ratingEntry in currentUserRatings) {
                    train.addRating(uID, ratingEntry.Value);
                    ratingCounter++;
                }
            }
            train.calculateAvgRatings();
            test.calculateAvgRatings();
        }

        /*************************************/
        /*********** Assignment 2 ************/
        /*************************************/

        //Compute the RMSE of all the methods in the list for a given train-test split (e.g. 0.95 train set size)
        public Dictionary<string, double> ComputeRMSE(List<string> lMethods)
        {
            Dictionary<string, double> res = new Dictionary<string, double>(); //the result will be stored here
            Dictionary<string, User> users = test.UsersToItems; //All the users in the test

            double pearsonNumerator = 0;
            double cosineNumerator = 0;
            double randomNumerator = 0;

            foreach (KeyValuePair<string, User> currentUser in users)
            {
                Dictionary<string, Rating> userRatings = currentUser.Value.getDictionary();
                foreach (KeyValuePair<string, Rating> currentRating in userRatings)
                {
                    if (lMethods.Contains("Pearson"))
                    {
                        pearsonNumerator += Math.Pow((currentRating.Value.rating - PredictRating("Pearson", currentUser.Key, currentRating.Key)), 2);
                    }

                    if (lMethods.Contains("Cosine"))
                    {
                        cosineNumerator += Math.Pow((currentRating.Value.rating - PredictRating("Cosine", currentUser.Key, currentRating.Key)), 2);
                    }

                    if (lMethods.Contains("Random"))
                    {
                        randomNumerator += Math.Pow((currentRating.Value.rating - PredictRating("Random", currentUser.Key, currentRating.Key)), 2);
                    }
                }
            }

            if (lMethods.Contains("Pearson"))
            {
                res["Pearson"] = Math.Sqrt(pearsonNumerator / this.numOfRecords);
            }
            if (lMethods.Contains("Cosine"))
            {
                res["Cosine"] = Math.Sqrt(cosineNumerator / this.numOfRecords);
            }
            if (lMethods.Contains("Random"))
            {
                res["Random"] = Math.Sqrt(randomNumerator / this.numOfRecords);
            }

            return res;

        }

        private double ComputeValidationRMSE() {
            double acc = 0;
            int count = 0;
            foreach (KeyValuePair<string, User> currentUser in validation.UsersToItems) {
                Dictionary<string, Rating> userItems = currentUser.Value.getDictionary();
                foreach (KeyValuePair<string, Rating> rating in userItems) {
                    double tmp = PredictRating("SVD", currentUser.Key, rating.Key);
                    acc += Math.Pow(userItems[rating.Key].rating - tmp,2);
                    count++;
                }
            }
            return Math.Sqrt(acc / count);
        }


        public void TrainBaseModel(int latentFeatures)
        {
            //Splitting the train into train and validation DBs
            splitTrain(0.95);

            double mu = this.getMu(train.UsersToItems);
            Console.WriteLine("mu = " + getMu(train.UsersToItems));
                       
            Random ran = new Random();
            
            foreach (KeyValuePair<string, User> userEntry in usersToItems) { 
                double[] pu = new double[latentFeatures];
                for(int i=0;i<latentFeatures;i++){
                    pu[i] = (double)(ran.Next(-5, 5)) / 10000;
                }
                double bu = (double)(ran.Next(-5, 5)) / 10000;
                VectorDO vecDO = new VectorDO(pu,bu);
                usersVector[userEntry.Key] = vecDO;
            }

            foreach (KeyValuePair<string, Item> itemsEntry in itemsToUsers) { 
                double[] qi = new double[latentFeatures];
                for(int i=0;i<latentFeatures;i++){
                    qi[i] = (double)(ran.Next(-5, 5)) / 10000;
                }    
                double bi = (double)(ran.Next(-5, 5)) / 10000;
                VectorDO vecDO = new VectorDO(qi,bi);
                itemsVector[itemsEntry.Key] = vecDO;
            }

            double y = 0.05;//(double)(ran.Next(-5, 5)) / 100;
            double gamma = 0.05;//(double)(ran.Next(-5, 0)) / 100;
            
            double prevRMSE = ComputeValidationRMSE();
            double currentRMSE = prevRMSE;            
            do {
                prevRMSE = currentRMSE;
                updateTrainSet(mu,y,gamma);
                currentRMSE = ComputeValidationRMSE();
            } while(currentRMSE <= prevRMSE);
                        
        }

        public void updateTrainSet(double mu,double y, double gamma){
             foreach (KeyValuePair<string, User> userEntry in train.UsersToItems) { 
                String userID = userEntry.Key;
                VectorDO userVec = usersVector[userID];
                double bu = userVec.getVal();
                double[] pu = userVec.getVec();
                double eui=0;
                foreach (KeyValuePair<string, Rating> itemEntry in userEntry.Value.getDictionary()){
                    String itemID = itemEntry.Key;
                    VectorDO itemVec = itemsVector[itemID];
                    double bi = itemVec.getVal();
                    double[] qi = itemVec.getVec();
                    double rui = itemEntry.Value.rating;
                    eui = rui-mu-bi-bu-multVectors(pu,qi);

                    bu = bu + y * (eui-(gamma*bu));
                    userVec.setVal(bu);
                    bi = bi + y * (eui-(gamma*bi));
                    itemVec.setVal(bi);
                    qi = addVectors (qi,multScalarVector(y,(subVectors(multScalarVector(eui,pu),multScalarVector(gamma,qi)))));
                    itemVec.setVec(qi);
                    pu = addVectors (pu,multScalarVector(y,(subVectors(multScalarVector(eui,qi),multScalarVector(gamma,pu)))));
                    userVec.setVec(pu);
                }
            }
        }

        private double[] multScalarVector(double scalar, double[] vector) {
            double[] ans = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++) {
                 ans[i] = vector[i]*scalar;
            }
            return ans;
        }

        private double[] addScalarVector(double scalar, double[] vector) {
            double[] ans = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++) {
                ans[i] = vector[i] + scalar;
            }
            return ans;
        }

        private double[] addVectors(double[] vec1, double[] vec2) {
            if (vec1.Length != vec2.Length) {
                return null;
            }
            double[] ans = new double[vec1.Length];
            for (int i = 0; i < vec1.Length; i++) {
                ans[i] += (vec1[i] + vec2[i]);
            }
            return ans;
        }

        private double[] subVectors(double[] vec1, double[] vec2) {
            if (vec1.Length != vec2.Length) {
                return null;
            }
            double[] ans = new double[vec1.Length];
            for (int i = 0; i < vec1.Length; i++) {
                ans[i] += (vec1[i] - vec2[i]);
            }
            return ans;
        }

        private double multVectors(double[] vec1, double[] vec2) {
            if (vec1.Length!= vec2.Length) {
                return -99999;
            }
            double acc = 0;
            for(int i=0;i<vec1.Length;i++){
                acc += (vec1[i] * vec2[i]);
            }
            return acc;
        }

        private void splitTrain(double p)
        {
            validation = new Db();
            List<string> users = new List<string>(train.UsersToItems.Keys);   // a copy of users ids            
            int amountOfValidationRecords = (int)((1 - p) * train.usersSize());    // size of train Db
            int countValidationRecords = 0;                                   // counter for train Db
            Random ran = new Random();
            //Console.WriteLine("train size = " + train.usersSize() + " amount = " + amountOfValidationRecords);

            //We assumed that on low values of P, the test size would not necessarily be the declared "amountOfTestRecords"
            //because k is chosen randomally and we can't control it
            while (countValidationRecords < amountOfValidationRecords && users.Count > 0)
            {
                // randomize the next user
                int nextUser = ran.Next(0, users.Count);
                string currentUserID = users.ElementAt(nextUser);
                User currentUser = train.UsersToItems[currentUserID];
                Dictionary<string, Rating> currentUserRatings = currentUser.getDictionary();

                validation.addUser(currentUserID);


                List<string> itemsIDs = new List<string>(currentUserRatings.Keys);        // copy of items IDs

                int amountOfItems = currentUser.getDictionary().Count;                    // amount of items

                int k = ran.Next(0, amountOfItems);                                       // randomize amount of items
                k = Math.Min(k, (amountOfValidationRecords - countValidationRecords));                // to put in the train
                // for each item - add it to the train and remove from temp item list
                for (int i = 0; i < k; i++)
                {
                    int nextItem = ran.Next(0, itemsIDs.Count);
                    string nextItemID = itemsIDs.ElementAt(nextItem);
                    validation.addRating(currentUserID, currentUserRatings[nextItemID]);
                    train.removeRating(currentUserID, nextItemID);
                    itemsIDs.RemoveAt(nextItem);
                }
                //If all the ratings were chosen so user should be removed from train
                if (k == amountOfItems) {
                    train.removeUser(currentUserID);
                }
                countValidationRecords += k;

                // remove user from temp list
                users.RemoveAt(nextUser);
            }

            //Console.WriteLine("train size = " + train.usersSize() + " validation size = " + validation.usersSize() + " countvalrec = " + countValidationRecords);
        }

        private double getMu(Dictionary<string, User> users) {
            //Dictionary<string, User> users = train.UsersToItems;
            int numOfRatings = 0;
            double muNumerator = 0;
            foreach (KeyValuePair<string, User> currentUser in users)
            {
                Dictionary<string, Rating> userRatings = currentUser.Value.getDictionary();
                foreach (KeyValuePair<string, Rating> currentItem in userRatings)
                {
                    muNumerator += currentItem.Value.rating;
                    numOfRatings++;
                }
            }
            return muNumerator / numOfRatings;
            
        }
    }



}
