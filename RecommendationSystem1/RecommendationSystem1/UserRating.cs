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
