using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data.SQLite;
using System.Data.Linq;
using SampleSqlLite.Entities;
using System.Diagnostics;
using VirtualCollection;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SampleSqlLite.DataProviders
{
    public class BinaryDataProvider : IVirtualCollectionSource<BinaryData>
    {
        public Type CollectionType => typeof(BinaryData);

        private int? count = null;
        public int? Count
        {
            get
            {
                try
                {
                    if (count == null)
                    {
                        using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
                        {
                            con.Open();
                            using (var context = new DataContext(con))
                            {
                                var data = context.GetTable<BinaryData>();
                                count = data.Count();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                   
                }
                return count;
                
            }
        }

        public event EventHandler<VirtualCollectionSourceChangedEventArgs> CollectionChanged;
        public event EventHandler<EventArgs> CountChanged;

        public ReadOnlyObservableCollection<object> GetGroups(ObservableCollection<GroupDescription> groupDescriptions)
        {
            throw new NotImplementedException();
        }

        public Task<IList> GetPageAsync(int start, int pageSize, IList<SortDescription> sortDescriptions)
        {
            return new Task<IList>(() => {
                try
                {


                    using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
                    {
                        con.Open();
                        using (var context = new DataContext(con))
                        {
                            var data = context.GetTable<BinaryData>();
                            var list = from x in data where x.No >= start && x.No < start + pageSize orderby x.No select x;
                            var ret = new List<BinaryData>();
                            foreach (var b in list)
                            {
                                ret.Add(b);
                            }
                            return ret;
                        }
                    }
                }
                catch (Exception e)
                {
                    return new List<BinaryData>(); ;
                }
            });

        }

        public Task<IList<BinaryData>> GetPageAsyncT(int start, int pageSize, IList<SortDescription> sortDescriptions)
        {
            return new Task<IList<BinaryData>>(() => { 
                using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
                {
                    con.Open();
                    using (var context = new DataContext(con))
                    {
                        var data = context.GetTable<BinaryData>();
                        var list = from x in data where x.No >= start && x.No < start + pageSize orderby x.No select x;
                        var ret = new List<BinaryData>();
                        foreach (var b in list)
                        {
                            ret.Add(b);
                        }
                        return ret;
                    }
                }
            });

        }

        public void Refresh(VirtualCollection.RefreshMode mode)
        {
            count = null;
        }
    }
}
