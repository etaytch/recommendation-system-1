using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class Item
    {
        private Dictionary<string, Rating> ratings;
        
        private string itemID;
        public string ItemID
        {
            get { return itemID; }
            set { itemID = value; }
        }
        public Item()
        {
            ratings = new Dictionary<string, Rating>();
        }

        public void addRating(Rating r)
        {
            ratings[r.userID] = r;
        }

        public Rating getRating(string itemID)
        {
            return ratings[itemID];
        }

        public Dictionary<string, Rating> getDictionary()
        {
            return ratings;
        }

        //Comperator by number of raters. the one with less raters is getting 1!!
        public static int CompareItemsByRaters(Item i1, Item i2)
        {
            int result = 0;
            if (i1.getDictionary().Keys.Count > i2.getDictionary().Keys.Count) result = -1;
            if (i1.getDictionary().Keys.Count < i2.getDictionary().Keys.Count) result = 1;
            if (i1.getDictionary().Keys.Count == i2.getDictionary().Keys.Count) result = 0;

            return result;
            
        }
    }
}
