using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecommendationSystem;

namespace RecommenderSystem
{
    class RecommenderSystem
    {
        //Class members here (e.g. a dataset)
        private Dictionary<string, User> usersToItems;
        private Dictionary<string, Item> itemsToUsers;
        private db test;
        private db train;
        private Dictionary<string, double> usersWPearson;
        private Dictionary<string, double> usersWCosine;
        private List<string> usersIDs;
        private int numOfRecords;

        public RecommenderSystem()
        {
            
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
            usersWPearson = new Dictionary<string, double>();
            usersWCosine = new Dictionary<string, double>();
            usersIDs = new List<string>();
            test = new db();
            train = new db();
            numOfRecords = 0;
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
            return res;
        }
        //return the predicted weights of all ratings that a user may give an item using one of the methods "Pearson", "Cosine", "Random"
        public Dictionary<double, double> PredictAllRatings(string sMethod, string sUID, string sIID)
        {
            int k = 50;     // some random environment value
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
        public Dictionary<string,double> ComputeHitRatio(List<string> lMethods, double dTrainSetSize)
        {
            throw new NotImplementedException();
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

        public void splitDB(double p) {            
            List<string> users = new List<string>(usersToItems.Keys);   // a copy of users ids            
            int amountOfTestRecords = (int)((1 - p) * numOfRecords);    // size of test db
            int countTestRecords = 0;                                   // counter for test db
            Random ran = new Random();
            int ratingCounter = 0;

            while (countTestRecords < amountOfTestRecords) {
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
            
            Console.WriteLine("train's # of users: " + train.usersSize() + ", test's # of users: " + test.usersSize()+" total rating scanned: "+ratingCounter);
            train.calculateAvgRatings();
            test.calculateAvgRatings();
        }
    }
}
