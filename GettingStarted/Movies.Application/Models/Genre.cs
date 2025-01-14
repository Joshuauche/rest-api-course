using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.Application.Models
{
    public class Genre
    {
        public required Guid Id { get; set; }
        public required string Name { get; set; }
    }
}
