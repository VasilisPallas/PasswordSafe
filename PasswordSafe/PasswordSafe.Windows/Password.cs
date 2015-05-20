using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace PasswordSafe
{
    [Table("Passwords")]
    class Password
    {
        [NotNull]
        public string username { get; set; }
        [PrimaryKey, NotNull]
        public string name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
