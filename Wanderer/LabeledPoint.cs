using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wanderer
{
    class LabeledPoint
    {
        private readonly int x;
        public readonly int X
        {
            get { return x; }
        }
        
        private readonly int y;
        public readonly int Y
        {
            get { return y; }
        }
        
        private readonly LabeledPointType type;
        public readonly LabeledPointType Type
        {
            get { return type; }
        }

        private readonly String boldText;
        public readonly String BoldText
        {
            get { return boldText; }
        }

        private readonly String normalText;
        public readonly String NormalText
        {
            get { return normalText; }
        }
        
        public LabeledPoint(int x, int y, LabeledPointType type, String boldText, String normalText) {
            this.x = x;
            this.y = y;
            this.type = type;
            this.boldText = boldText;
            this.normalText = normalText;
        }


    }
}
