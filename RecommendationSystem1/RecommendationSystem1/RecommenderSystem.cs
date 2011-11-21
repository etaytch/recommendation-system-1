using System;
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
        private Dictionary<string, double> usersW;

        public RecommenderSystem()
        {
            
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
            usersW = new Dictionary<string, double>();
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
    }
}
