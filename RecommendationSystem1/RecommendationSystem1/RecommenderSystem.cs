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

        public RecommenderSystem()
        {
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
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
            throw new NotImplementedException();
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
    }
}
