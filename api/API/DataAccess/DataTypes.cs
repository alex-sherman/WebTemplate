using Microsoft.EntityFrameworkCore;
using Replicate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace API.DataAccess {
    [ReplicateType]
    public class UserData {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
        [ReplicateIgnore]
        public string Hash { get; set; }
        public static void Configure(ModelBuilder model) {
            model.Entity<UserData>()
                .HasAlternateKey(ud => ud.Email);
            model.Entity<UserData>()
                .HasAlternateKey(ud => ud.Name);
        }
    }
}
