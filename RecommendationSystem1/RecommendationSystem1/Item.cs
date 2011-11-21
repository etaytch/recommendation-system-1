using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class Item
    {
        private Dictionary<string, Rating> ratings;

        public Item()
        {
            ratings = new Dictionary<string, Rating>();
        }

        public void addRating(string itemID, double rating, string timestamp)
        {
            Rating ur = new Rating(rating, timestamp);
            ratings[itemID] = ur;
        }

        public Rating getRating(string itemID)
        {
            return ratings[itemID];
        }

        public Dictionary<string, Rating> getDictionary()
        {
            return ratings;
        }
    }
}
