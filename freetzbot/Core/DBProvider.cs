using Db4objects.Db4o;
using Db4objects.Db4o.Config;
using Db4objects.Db4o.Constraints;
using Db4objects.Db4o.Defragment;
using Db4objects.Db4o.Linq;
using FritzBot.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace FritzBot.Core
{
    public class DBProvider : IDisposable
    {
        private static object _lock = new object();
        private static IEmbeddedObjectContainer _db;
        private static IEmbeddedObjectContainer Datenbank
        {
            get
            {
                lock (_lock)
                {
                    if (_db == null)
                    {
                        _db = Db4oEmbedded.OpenFile(GetConfiguration(), DBPath);
                    }
                }
                return _db;
            }
        }

        public static string DBPath
        {
            get
            {
                return Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath), "datenbank.db");
            }
        }

        private static IEmbeddedConfiguration GetConfiguration()
        {
            Contract.Ensures(Contract.Result<IEmbeddedConfiguration>() != null);

            IEmbeddedConfiguration conf = Db4oEmbedded.NewConfiguration();

            conf.Common.ExceptionsOnNotStorable = true;

            EnsureIndex<AliasEntry>(conf.Common, "Key");
            conf.Common.Add(new UniqueFieldValueConstraint(typeof(AliasEntry), AutoProperty("Key")));

            EnsureIndex<User>(conf.Common, "Names");

            return conf;
        }

        private static void EnsureIndex<T>(ICommonConfiguration conf, string property)
        {
            Contract.Requires(conf != null);
            Contract.Requires(property != null);

            conf.ObjectClass(typeof(T)).ObjectField(AutoProperty(property)).Indexed(true);
        }

        public static string AutoProperty(string name)
        {
            Contract.Requires(name != null);
            Contract.Ensures(Contract.Result<string>() != null);

            return String.Format("<{0}>k__BackingField", name);
        }

        public static void Defragmentieren()
        {
            if (File.Exists(DBPath))
            {
                Shutdown();
                DefragmentConfig config = new DefragmentConfig(DBPath);
                config.Db4oConfig(GetConfiguration());
                config.ForceBackupDelete(true);
                Defragment.Defrag(config);
            }
        }

        public static void ReCreate()
        {
            List<object> allObjects;
            using (DBProvider db = new DBProvider())
            {
                allObjects = db.Query<object>().ToList();
            }
            Shutdown();
            File.Move(DBPath, DBPath + "." + DateTime.Now.ToString().Replace(".", "").Replace(" ", "").Replace(":", ""));
            using (DBProvider db = new DBProvider())
            {
                foreach (object item in allObjects)
                {
                    db.SaveOrUpdate(item);
                }
            }
        }

        public static void Shutdown()
        {
            if (_db != null)
            {
                _db.Close();
                _db = null;
            }
        }

        public SODAQuery<T> SODAQuery<T>()
        {
            Contract.Ensures(Contract.Result<SODAQuery<T>>() != null);

            return new SODAQuery<T>(Datenbank.Query());
        }

        public IQueryable<T> Query<T>()
        {
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            return Datenbank.AsQueryable<T>();
        }

        public IQueryable<T> Query<T>(Expression<Func<T, bool>> match)
        {
            Contract.Requires(match != null);
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            return Datenbank.AsQueryable<T>().Where(match);
        }

        public IQueryable<T> QueryLinkedData<T, L>(L instance)
            where T : LinkedData<L>
            where L : class
        {
            Contract.Ensures(Contract.Result<IQueryable<T>>() != null);

            return Datenbank.AsQueryable<T>().Where(x => x.Reference == instance);
        }

        public User GetUser(string name)
        {
            Contract.Requires(name != null);

            return Datenbank.AsQueryable<User>().FirstOrDefault(x => x.Names.Contains(name));
        }

        public SimpleStorage GetSimpleStorage(string ID)
        {
            return GetSimpleStorage(null, ID);
        }

        public SimpleStorage GetSimpleStorage(object reference, string ID)
        {
            Contract.Ensures(Contract.Result<SimpleStorage>() != null);

            SimpleStorage storage = Query<SimpleStorage>(x => x.Reference == reference && x.ID == ID).FirstOrDefault();
            if (storage == null)
            {
                storage = new SimpleStorage();
                storage.ID = ID;
                storage.Reference = reference;
            }
            return storage;
        }

        public void SaveOrUpdate(object obj)
        {
            Datenbank.Store(obj);
        }

        public void Remove(object obj)
        {
            Datenbank.Delete(obj);
        }

        public void Dispose()
        {
            try
            {
                Datenbank.Commit();
            }
            catch
            {
                Datenbank.Rollback();
            }
        }
    }
}