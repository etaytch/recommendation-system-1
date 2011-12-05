using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class User
    {
        private Dictionary<string, Rating> ratings;
        private double averageRating;

        public User()
        {
            ratings = new Dictionary<string, Rating>();
            averageRating = -1.0;
        }


        public void setAverageRating(double rating) {
            this.averageRating = rating;
        }

        public double getAverageRating() {
            return this.averageRating;
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
