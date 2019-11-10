using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq.Mapping;

namespace SampleSqlLite.Entities
{
    [Table(Name ="BinaryData")]
    public class BinaryData
    {
        [Column(Name ="id",IsPrimaryKey =true)]
        public int Id
        {
            get; set;
        }

        [Column(Name ="no")]
        public int No
        {
            get; set;
        }

        [Column(Name ="data")]
        public byte[] Data
        {
            get; set;
        }
    }
}
