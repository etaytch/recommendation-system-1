using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecommenderSystem;

namespace RecommendationSystem {
    class db {

        private Dictionary<string, User> usersToItems;
        public Dictionary<string, User> UsersToItems
        {
            get { return usersToItems; }
            set { usersToItems = value; }
        }

        private Dictionary<string, Item> itemsToUsers;
        public Dictionary<string, Item> ItemsToUsers
        {
            get { return itemsToUsers; }
            set { itemsToUsers = value; }
        }

        public db() { 
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
        }

        public int usersSize() {
            return usersToItems.Count;
        }

        public void addUser(string user_id) {
            usersToItems[user_id] = new User();
        }
        public void addRating(string user_id,Rating rating) {
            usersToItems[user_id].addRating(rating);
            if (itemsToUsers.ContainsKey(rating.itemID)) {
                itemsToUsers[rating.itemID].addRating(rating);

            }
            else{
                Item i = new Item();
                i.addRating(rating);
                itemsToUsers[rating.itemID] = i;                
            }
        }

        private Dictionary<double, int> GetRatingsHistogram(string sUID) {
            Dictionary<double, int> hist = new Dictionary<double, int>();
            User user = usersToItems[sUID];
            foreach (KeyValuePair<string, Rating> entry in user.getDictionary()) {
                if (hist.ContainsKey(entry.Value.rating)) {
                    hist[entry.Value.rating]++;
                }
                else {
                    hist[entry.Value.rating] = 1;
                }
            }
            return hist;
        }

        public void calculateAvgRatings() {
            foreach (KeyValuePair<string, User> userEntry in usersToItems) {
                if (userEntry.Value.getAverageRating() != -1) continue;
                double mone = 0.0;
                int counter = 0;
                Dictionary<double, int> hist = GetRatingsHistogram(userEntry.Key);
                foreach (KeyValuePair<double, int> histEntry in hist) {
                    mone += histEntry.Key * histEntry.Value;
                    counter += histEntry.Value;
                }
                userEntry.Value.setAverageRating((mone / counter));
            }
        }
    }
}
