﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Commands
{
    public class CheckCommentResponse
    {
        public Guid CommentId { get; set; }

        public string BranchName { get; set; }
    }
}
