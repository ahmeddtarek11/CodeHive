using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeHive.Posts.Domain.Entities;

public class PostTag
{
	public Guid PostId { get; set; }
	public Guid TagId { get; set; }
}
