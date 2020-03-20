using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingHatsuwa.Data
{
    public enum EvaluationItem
    {
        Clear = 1,
        Smile = 2,
        Fair = 3,
        Logical = 4,
        Agreeable = 5,
        Honest = 6,
    }

    public enum MeetingStatus
    {
        Waiting,
        InMeeting,
        Evaluating,
        Closed
    }
}
