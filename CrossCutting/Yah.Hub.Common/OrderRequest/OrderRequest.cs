using System;
using Nest;

namespace Yah.Hub.Common.OrderRequest
{
    public class OrderRequest
    {
        public OrderRequest(bool _isScan, DateRange _range, ScrollType _pageType)
        {
            this.IsScan = _isScan;
            this.Range = _range;
            this.PaginateType = _pageType;
        }

        private int ScanStep = 1; // TODO: PASS TO CONFIG FILE

        private int Step = 8; // TODO: PASS TO CONFIG FILE

        public string Key { get; set; }

        public bool IsDone
        {
            get
            {
                return 0 <= this.Range.From.CompareTo(this.Range.To);
            }
        }

        public bool IsScan { get; set; }

        public DateRange Range { get; set; }

        public ScrollType PaginateType { get; set; }

        public void SlideDate()
        {
            this.Range.From = IsScan ? this.Range.From.AddDays(this.ScanStep) : this.Range.From.AddHours(Step);
        }
    }
}

