using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class UserRating
    {

        private string _userID;
        private string _itemID;
        private int _rating;
        private string _timestamp;

        public UserRating()
        {

        }

        public UserRating(string _userID, string _itemID, int _rating, string _timestamp)
        {
            this._userID = _userID;
            this._itemID = _itemID;
            this._rating = _rating;
            this._timestamp = _timestamp;
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

        public int rating
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
