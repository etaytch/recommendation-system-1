using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class User
    {
        private Dictionary<string, UserRating> ratings;

        public User()
        {
            ratings = new Dictionary<string, UserRating>();
        }

        public void addRating(string itemID, double rating, string timestamp)
        {
            UserRating ur = new UserRating(rating, timestamp);
            ratings[itemID] = ur;
        }

        public UserRating getRating(string itemID)
        {
            return ratings[itemID];
        }

        public Dictionary<string, UserRating> getDictionary() {
            return ratings;
        }
    }
}
