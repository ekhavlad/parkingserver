using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hermes.Parking.Server.ServerDictionary
{
    public class ServerDictionary : ICloneable
    {
        private int id;
        public int Id
        {
            get { return id; }
        }

        private string name;
        public string Name
        {
            get { return name; }
        }

        private IDictionary<int, string> items;
        public IDictionary<int, string> Items
        {
            get
            {
                return items;
            }
        }

        public ServerDictionary(int Id, string Name, IDictionary<int, string> Items)
        {
            this.id = Id;
            this.name = Name;
            this.items = Items;
        }

        public object Clone()
        {
            Dictionary<int, string> items = new Dictionary<int,string>();
            foreach(var item in this.Items)
                items.Add(item.Key, item.Value);

            return new ServerDictionary(this.Id, this.Name, items);
        }
    }
}
