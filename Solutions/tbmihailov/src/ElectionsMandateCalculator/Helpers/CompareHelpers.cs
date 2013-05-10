using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Helpers
{
    public static class CompareHelpers
    {
        public static bool AreEqualCollections<TModel>(IList<TModel> collection1, IList<TModel> collection2) where TModel:class
        {
            if(collection1==collection2)
            {
                return true;
            }

            if(collection1 ==null || collection2 ==null)
            {
                return false;    
            }

            if (collection1.Count() != collection2.Count())
            {
                return false;
            }

            if (collection1.Count()==0 && collection2.Count()==0)
            {
                return true;
            }

            //item by item compare
            bool hasDifferentItems = false;
            for (int i = 0; i < collection1.Count(); i++)
            {
                if (!collection1[i].Equals(collection2[i]))
                {
                    hasDifferentItems = true;
                }
            }

            return !hasDifferentItems;
        }
    }
}
