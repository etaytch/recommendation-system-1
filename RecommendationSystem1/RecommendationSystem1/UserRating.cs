using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class UserRating
    {
        private double _rating;
        private string _timestamp;

        public UserRating(double rating, string timestamp)
        {
            this._rating = rating;
            this._timestamp = timestamp;
        }

        public double rating
        {
            get
            {
                return _rating;
            }
            set
            {
                _rating = value;
            }
        }

        public string timestamp
        {
            get
            {
                return _timestamp;
            }
            set
            {
                _timestamp = value;
            }
        }
    }
}
