using Quartz;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Jobs
{
    class SaverJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return _context.SaveChangesAsync();
        }
    }
}
