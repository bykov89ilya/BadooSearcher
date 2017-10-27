using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadooSearcher
{
    class MiniProfile
    {
        public string Name { get; protected set; }
        public int Age { get; protected set; }
        public string Link { get; protected set; }

         
        public MiniProfile(string name, int age, string link)
        {
            Name = name;
            Age = age;
            Link = link;
        }

        public override string ToString()
        {
            return String.Format("Age: {0}, Name:{}, Link:{}", Age, Name, Link);
        }
    }
}
