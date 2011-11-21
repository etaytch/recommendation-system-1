using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem
{
    class RecommenderSystem
    {
        //Class members here (e.g. a dataset)
        private List<UserRating> usersRatings;

        public RecommenderSystem()
        {
        }

        //load a dataset from a file
        public void Load(string sFileName)
        {
            throw new NotImplementedException();
        }
        //return an existing rating 
        public double GetRating(string sUID, string sIID)
        {
            throw new NotImplementedException();
        }
        //return an histogram of all ratings that the user has used
        public Dictionary<double, int> GetRatingsHistogram(string sUID)
        {
            throw new NotImplementedException();
        }
        //predict the rating that a user will give to an item using one of the methods "Pearson", "Cosine", "Random"
        public double PredictRating(string sMethod, string sUID, string sIID)
        {
            throw new NotImplementedException();
        }
        //return the predicted weights of all ratings that a user may give an item using one of the methods "Pearson", "Cosine", "Random"
        public Dictionary<double, double> PredictAllRatings(string sMethod, string sUID, string sIID)
        {
            throw new NotImplementedException();
        }
        //Compute the hit ratio of all the methods in the list for a given train-test split (e.g. 0.95 train set size)
        public Dictionary<string,double> ComputeHitRatio(List<string> lMethods, double dTrainSetSize)
        {
            throw new NotImplementedException();
        }
    }
}
