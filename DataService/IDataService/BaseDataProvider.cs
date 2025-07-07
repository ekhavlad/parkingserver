using System;
using System.Collections.Generic;

namespace Hermes.Parking.Server.DataService
{
    public abstract class BaseDataProvider : IDataProvider
    {
        public abstract string GetName();

        public virtual void Create(string ObjectType, object InitialObject, ref object Output)
        {
        }

        public virtual void Get(string ObjectType, IDictionary<string, object> Filter, ref object Output)
        {
        }

        public virtual void Get(string ObjectType, long Id, ref object Output)
        {
        }

        public virtual void GetList(string ObjectType, IDictionary<string, object> Filter, ref IEnumerable<object> Output)
        {
        }

        public virtual void Delete(string ObjectType, object Object)
        {
        }

        public virtual void Save(string ObjectType, object Object)
        {
        }
    }
}