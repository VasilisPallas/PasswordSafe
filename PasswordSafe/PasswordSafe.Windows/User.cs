using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace PasswordSafe
{
    [Table("Users")]
    class User
    {
        [PrimaryKey, NotNull]
        public string name { get; set; }
        [NotNull]
        public string password { get; set; }
        [Unique, NotNull]
        public string email { get; set; }
    }
}
