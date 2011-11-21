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

        public void addRating(Rating r)
        {
            ratings[r.itemID] = r;
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
