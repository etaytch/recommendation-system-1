using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RecommenderSystem;

namespace RecommenderSystem
{
    class Db {

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

        public Db() { 
            usersToItems = new Dictionary<string, User>();
            itemsToUsers = new Dictionary<string, Item>();
        }

        public int usersSize() {
            return usersToItems.Count;
        }
        
        public void addUser(string user_id) {
            usersToItems[user_id] = new User();
        }
        public void removeUser(String user_id) {
            usersToItems.Remove(user_id);
        }
        public void addRating(string user_id,Rating rating) {
            usersToItems[user_id].addRating(rating);
            if (itemsToUsers.ContainsKey(rating.itemID)) {
                itemsToUsers[rating.itemID].addRating(rating);

            }
            else{
                Item i = new Item();
                i.ItemID = rating.itemID;
                i.addRating(rating);
                itemsToUsers[rating.itemID] = i;                
            }
        }
        public void removeRating(string user_id, string item_id)
        {
            usersToItems[user_id].getDictionary().Remove(item_id);
            ItemsToUsers[item_id].getDictionary().Remove(user_id);
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
