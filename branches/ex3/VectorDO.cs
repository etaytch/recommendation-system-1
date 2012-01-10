using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RecommenderSystem {
    class VectorDO {
        private double[] _vec;
        private double _val;

        public VectorDO(double[] vec, double val) {
            _vec = vec;
            _val = val;
        }

        public double getVal() {
            return _val;
        }

        public double[] getVec() {
            return _vec;
        }

        public void setVal(double val) {
            _val = val;
        }

        public void setVec(double[] vec) {
            _vec = vec;
        }
    }
}
