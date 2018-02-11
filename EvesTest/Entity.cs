using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvesTest
{
    public enum EntityType
    {
        directory,
        file
    };
    public class Entity
    {
        public string ParentName { get; set; }
        public string Name { get; set; }
        public float Size { get; set; }
        public string MimeType { get; set; }
        public EntityType EntityType { get; set; }
        public List<Entity> Entity_List { get; set; }
    }
}
