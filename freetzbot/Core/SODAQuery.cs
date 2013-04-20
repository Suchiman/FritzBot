using Db4objects.Db4o.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FritzBot.Core
{
    public class SODAQuery<T>
    {
        private IQuery _query;

        public SODAQuery(IQuery query)
        {
            _query = query;
            _query.Constrain(typeof(T));
        }

        public IQuery Member<P>(Expression<Func<T, P>> selektor)
        {
            MemberInfo info = (selektor.Body as MemberExpression).Member;
            switch (info.MemberType)
            {
                case MemberTypes.Property:
                    return _query.Descend(DBProvider.AutoProperty(info.Name));
                case MemberTypes.Field:
                    return _query.Descend(info.Name);
                default:
                    throw new NotImplementedException();
            }
        }

        public IEnumerable<T> Execute()
        {
            return _query.Execute().OfType<T>();
        }
    }
}