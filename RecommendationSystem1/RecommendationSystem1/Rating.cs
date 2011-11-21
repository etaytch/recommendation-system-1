using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class Rating
    {
        private string _userID;
        private string _itemID;
        private double _rating;
        private string _timestamp;

        public Rating(string userID, string itemID, double rating, string timestamp)
        {
            this._userID = userID;
            this._itemID = itemID;
            this._rating = rating;
            this._timestamp = timestamp;
        }

        public Rating(double rating, string timestamp)
        {
            this._rating = rating;
            this._timestamp = timestamp;
        }

        public string userID
        {
            get
            {
                return _userID;
            }
            set
            {
                _userID = value;
            }
        }

        public string itemID
        {
            get
            {
                return _itemID;
            }
            set
            {
                _itemID = value;
            }
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
