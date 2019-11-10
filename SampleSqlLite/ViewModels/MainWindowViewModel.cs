using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SampleSqlLite.Commands;
using System.Data.SQLite;
using System.ComponentModel;
using System.Diagnostics;
using System.Data.Linq;
using SampleSqlLite.Entities;
using SampleSqlLite.DataProviders;

namespace SampleSqlLite.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        private DelegateCommand dbCreateCommnad;
        public DelegateCommand DbCreateCommand
        {
            get
            {
                return dbCreateCommnad ?? (
                    dbCreateCommnad = new DelegateCommand(
                        a => 
                        {
                            try
                            {
                                CreateDb();
                                Message = "DB Created";
                                OnPropetyChanged("Message");
                            }
                            catch (Exception e)
                            {
                                Message = e.Message;
                                OnPropetyChanged("Message");
                            }
                        },
                        c => 
                        {
                            try
                            {
                                return !IsExistsDb();
                            }
                            catch (Exception e)
                            {
                                Message = e.Message;
                                OnPropetyChanged("Message");
                            }
                            return true;
                        }));
            }
        }

        private DelegateCommand createTestDataCommand;
        public DelegateCommand CreateTestDataCommand
        {
            get
            {
                return createTestDataCommand ?? (
                    createTestDataCommand = new DelegateCommand(
                        a => 
                        {
                            try
                            {
                                CreateTestData();
                            }
                            catch (Exception e)
                            {
                                Message = e.Message;
                                OnPropetyChanged("Message");
                            }
                        },
                        c => 
                        {
                            try
                            {
                                return IsExistsDb();
                            }
                            catch (Exception e)
                            {
                                return false;
                            }
                        })
                    );
            }
        }

        public string Message
        {
            get; set;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropetyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void CreateDb()
        {
            using (var con = new SQLiteConnection("Data Source=sample.db"))
            {
                con.Open();
                using (var command = con.CreateCommand())
                {
                    command.CommandText = "CREATE TABLE BinaryData (id integer primary key asc not null,no integer not null,data blob not null)";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE INDEX Idx_BinaryData_id On BinaryData (id asc)";
                    command.ExecuteNonQuery();
                    command.CommandText = "CREATE INDEX Idx_BinaryData_no On BinaryData (no asc)";
                    command.ExecuteNonQuery();
                }

                    
            }
        }

        private bool IsExistsDb()
        {
            using (var con = new SQLiteConnection("Data Source=sample.db"))
            {
                con.Open();
                using (var command = con.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='BinaryData'";
                    var count = command.ExecuteScalar() as long?;
                    if (count == 1)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void CreateTestData()
        {
            using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
            {
                con.Open();
                using (var command = con.CreateCommand())
                {
                    command.CommandText = "DELETE FROM BinaryData";
                    command.ExecuteNonQuery();

                    command.CommandText = "VACUUM";
                    command.ExecuteNonQuery();



                    var data = new byte[3000];
                    for (int i=0;i<data.Length;i++)
                    {
                        data[i] = 0x30;
                    }
                    var ts = con.BeginTransaction();

                    command.CommandText = "INSERT INTO BinaryData(id,no,data) values (@id,@no,@data)";
                    command.Transaction = ts;
                    var sw = new Stopwatch();
                    sw.Start();
                    for (int i=0;i<1000000;i++)
                    {
                        command.Parameters.Add(new SQLiteParameter("@id", i));
                        command.Parameters.Add(new SQLiteParameter("@no", i));
                        command.Parameters.Add(new SQLiteParameter("@data", data));
                        command.ExecuteNonQuery();
                        if (i%1000==0)
                        {
                            ts.Commit();
                            ts = con.BeginTransaction();
                            command.Transaction = ts;
                        }
                    }
                    ts.Commit();
                    sw.Stop();
                    Message = string.Format("Test Data Created({0:#,0})",sw.ElapsedMilliseconds );
                    OnPropetyChanged("Message");
                }
            }
        }

        public string Count
        {
            get; set;
        }

        private DelegateCommand getCountCommnad;
        public DelegateCommand GetCountCommand
        {
            get
            {
                return getCountCommnad ?? (
                    getCountCommnad = new DelegateCommand(
                        a => 
                        {
                            try
                            {
                                GetCount();
                            }
                            catch (Exception e)
                            {
                                Message = e.Message;
                                OnPropetyChanged("Message");
                            }
                        },
                        c=> 
                        {
                            return true;
                        }));
            }
        }

        private void GetCount()
        {
            using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
            {
                con.Open();
                var sw = new Stopwatch();
                sw.Start();
                using (var context = new DataContext(con))
                {
                    var data = context.GetTable<BinaryData>();
                    Count = string.Format("件数:{0:#,0}",data.Count());
                    OnPropetyChanged("Count");
                }
                sw.Stop();
                Message = string.Format("Table Count({0:#,0})", sw.ElapsedMilliseconds);
                OnPropetyChanged("Message");
            }
        }

        public IEnumerable<BinaryData> SelectData()
        {
            IEnumerable<BinaryData> ret = null;
            using (var con = new SQLiteConnection("Data Source=sample.db;version=3;syncronous=Normal;jounarl mode=Wal;"))
            {
                con.Open();
                var sw = new Stopwatch();
                sw.Start();
                
                using (var context = new DataContext(con))
                {
                    var data = context.GetTable<BinaryData>();
                    var list = from x in data orderby x.Id select x;
                    ret = list.ToList();
                }

                sw.Stop();
                Message = string.Format("Data Select({0:#,0})", sw.ElapsedMilliseconds);
                OnPropetyChanged("Message");
            }

            return ret;
        }

        private VirtualCollection.VirtualCollection binaryDataColleaction;
        public VirtualCollection.VirtualCollection BinaryDataCollection
        {
            get
            {
                return binaryDataColleaction ?? (binaryDataColleaction = new VirtualCollection.VirtualCollection(new BinaryDataProvider(),1000,10));
            }
        }
    }
}
