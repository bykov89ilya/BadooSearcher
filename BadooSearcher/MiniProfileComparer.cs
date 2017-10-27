using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadooSearcher
{
    class MiniProfileComparer : IEqualityComparer<MiniProfile>
    {
        public bool Equals(MiniProfile x, MiniProfile y)
        {
            if (x.Link.GetHashCode() == y.Link.GetHashCode())
                return true;
            return false;
        }

        public int GetHashCode(MiniProfile obj)
        {
            return obj.Link.GetHashCode();
        }
    }
}
